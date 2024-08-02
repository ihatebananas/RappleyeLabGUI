#this import allows a class to be used in itself
from __future__ import annotations

#To stop creation of pycache
import sys

from handler import Request
sys.dont_write_bytecode = True

#Imports
from abc import ABC, abstractmethod
import re
from Bio import SeqIO
from Bio.Seq import Seq
import os
from pathlib import Path

from validators import *
from handler import *
from request import Request
from preprocess import *


#FIXME: ADD CLASS METHODS AND STATIC METHODS


class Chain:

    def __init__(self, next_chain: Chain, file_path: str, validator_chain: list[Validator]):
        #FIXME: force to implement self._handler_type!!!
        self._file_path = file_path
        self._next_chain = next_chain

        #building handler chain
        self._chain = self._build_chain(validator_chain)
        self._chain_name = ""
    
    # @property
    # @abstractmethod
    # def _chain_name(self):
    #     raise NotImplementedError

    def _build_chain(self, validator_chain: list):

        handler_chain = None
        for v in reversed(validator_chain):
            handler_chain = self._handler_type(handler_chain, 
                                               os.path.basename(self._file_path), 
                                               v)         
        return handler_chain
    
    @abstractmethod
    def _assemble_request_list() -> list[Request]:
        pass   

    @abstractmethod
    def _preprocess_data(self):
        pass   

    def run_chain(self, error_dict: dict):
        original_error_size = len(error_dict["filename"])

        request_list = self._preprocess_data()
        for r in request_list:
            self._chain.handle(r, error_dict)

        if len(error_dict["filename"]) > original_error_size:
            return error_dict
        
        elif self._next_chain is None:
            return error_dict
        
        else:
            return self._next_chain.run_chain(error_dict)


#GFF INTEGRITY CHAIN
class GFFIntegrityChain(Chain):

    def __init__(self, next_chain: Chain, file_path: str, validator_chain: list[Validator]):
        self._handler_type = GFFIntegrityHandler
        super().__init__(next_chain, file_path, validator_chain)
        self._chain_name = "GFF Integrity"
    
    def _assemble_request_list(self, gff_lines: list) -> list[Request]:
        #initializing list to contain request objects
        request_list = []

        #splitting each line in list_of_lines by delimitter which is "\t"
        for idx, line in enumerate(gff_lines):
            request_list.append(Request("Line " + str(idx + 1), line.split("\t")))
            
        return request_list
    
    def _preprocess_data (self) -> list[Request]:
        #reading the file
        gff_lines = DataPreprocessor.read_gff_into_list(self._file_path)
        
        return self._assemble_request_list(gff_lines)

    

#GFF STRUCTURE CHAIN
class GFFStructureChain(Chain):

    def __init__(self, next_chain: Chain, file_path: str, validator_chain: list[Validator]):
        self._handler_type = GFFStructureHandler
        super().__init__(next_chain, file_path, validator_chain)
        self._chain_name = "GFF Structure"

    def _get_child_records(child_df: pd.DataFrame, parent_record: pd.DataFrame):
        #getting attributes of all features in child_df whose start or end coordinate is withing the parent_record
        relevant_attributes_bool = (((child_df["start"] >= parent_record["start"]) & (child_df["start"] <= parent_record["end"])) | 
                                    ((child_df["end"] >= parent_record["start"]) & (child_df["end"] <= parent_record["end"])))

        #getting all records that satisfy the condition above and getting their attributes
        relevant_attributes = list(child_df[relevant_attributes_bool]["attributes"])

        #return all child records who have the attributes above
        return child_df.query('attributes in @relevant_attributes')
       
    @classmethod
    def _assemble_request_list(cls, genes, mRNAs, exons, cds) -> list[Request]:

        #preparing request object to be passed on to handlers
        request_list = []
        gene_attribute_list = list(genes["attributes"])

        for gene_id in range(genes.shape[0]):
            #FIXME: make body of for loop into a separate function
            curr_gene = genes.iloc[gene_id, :]

            #for the children mRNA, exon, and CDS, we grab all such features whose start or stop is in the region of the curr_gene
            curr_mRNA = cls._get_child_records(mRNAs, curr_gene)
            curr_exon = cls._get_child_records(exons, curr_gene)
            curr_cds = cls._get_child_records(cds, curr_gene)

            #we also want to get any genes under curr_gene (for overlapping genes)
            curr_gene = cls._get_child_records(genes, curr_gene)

            #creating request object
            curr_request = Request(gene_attribute_list[gene_id], (curr_gene, curr_mRNA, curr_exon, curr_cds))

            request_list.append(curr_request)
        return request_list
            
    #FIXME: CHECK EXISTENCE OF FILE
    def _preprocess_data(self):
        #reading the file
        gff = DataPreprocessor.preprocess_gff(self._file_path)
        genes, mrnas, exons, cds = DataPreprocessor.split_gff_by_feature(gff)
        request_list = self._assemble_request_list(genes, mrnas, exons, cds)
        
        return request_list
    

#INTRON CHAIN
class IntronChain(Chain):

    def __init__(self, next_chain: Chain, file_path: str, validator_chain: list[Validator], fas_dir: Path):
        self._handler_type = IntronHandler
        super().__init__(next_chain, file_path, validator_chain)
        self._chain_name = "Intron Chain"
        self._fas_dir = fas_dir
        
    def _assemble_request_list(self, exon_groups,fas_seq) -> list[Request]:
        request_list = []

        for _, group in exon_groups:
            if group.shape[0] > 1:
                for i in range(group.shape[0] - 1):
                    curr_element = {"contig_name": group.iloc[i, 0],
                                    "intron_seq": fas_seq[group.iloc[i, 4] : group.iloc[i + 1, 3] - 1],
                                    "strand": group.iloc[i, 6]}

                    curr_id = f"{group.iloc[i, 8]}:{group.iloc[i, 3]} - {group.iloc[i, 4]}"

                    request_list.append(Request(curr_id, curr_element))
                # print(f"{attribute}\n: {group}")

        return request_list

    def _preprocess_data (self):

        gff = DataPreprocessor.preprocess_gff(self._file_path)

        #loading in fasta file
        fas_seq = SeqIO.read(self._fas_dir / f"{gff.iloc[0,0]}.fas", "fasta")

        exons = gff[gff["feature"] == "exon"]
        # intron_df = {"contig_name": [], "intron_start": [], "intron_end": [], "strand": [], "attributes": []}

        #group by attributes
        exon_groups = exons.groupby(["attributes"])
        request_list = self._assemble_request_list(exon_groups, fas_seq)

        return request_list
    

#CDS CHAIN
class CDSChain(Chain):

    def __init__(self, next_chain: Chain, file_path: str, validator_chain: list[Validator], fas_dir: Path):
        self._handler_type = CDSHandler
        super().__init__(next_chain, file_path, validator_chain)
        self._chain_name = "CDS Chain"
        self._fas_dir = fas_dir

    def _assemble_request_list(self, cds_groups, fas_seq) -> list[Request]:
        #preparing request list
        request_list = []
        for _, group in cds_groups:
            cds_seq = DataPreprocessor.construct_sequence(group, fas_seq)
            id = f"{group.iloc[0,0]}: {group.iloc[0, 3]} - {group.iloc[group.shape[0] - 1, 4]}"
            element = {"group": group, "cds_seq": cds_seq}

            request_list.append(Request(id, element))

        return request_list
    
    def _preprocess_data (self):
        gff = DataPreprocessor.preprocess_gff(self._file_path)
        # gff = pd.read_csv(self._file_path, sep = "\t", names = ["contig_name", "source", "feature", "start", "stop", "zeroes", "direction", "dots", "attribute"])
        cds = gff.query('feature == "CDS"')
        # cds = cds.sort_values(by = "start")
        cds_groups = cds.groupby('attributes')

        fas_seq = str(SeqIO.read(self._fas_dir / f"{gff.iloc[0,0]}.fas", "fasta").seq)


        #preparing request list
        request_list = self._assemble_request_list(cds_groups, fas_seq)

        return request_list
    

#For prototyping/testing purposes
if __name__ == '__main__':
    gff_path = "input/gff/ZZ_Contig127_20230731.gff"
    # gff_path = "input/gff/ZL_Contig1131_20230731.gff"
    fas_dir = "input/fas/"
    error_dict = {"filename": [],
                "error_type": [],
                "identifier": [],
                "error_message": []}  
    
    # #Running GFF Integrity chain
    # gff_integrity_chain = GFFIntegrityChain(gff_path)
    # gff_integrity_chain.run_chain(error_dict)

    # #Running GFF Structure chain
    # gff_structure_chain = GFFStructureChain(gff_path)
    # gff_structure_chain.run_chain(error_dict)

    #Running Intron chain
    intron_chain = IntronChain(gff_path, fas_dir)
    intron_chain.run_chain(error_dict)

    # #Running CDS chain
    # cds_chain = CDSErrorChain(gff_path, fas_dir)
    # cds_chain.run_chain(error_dict)

    print(error_dict)

    