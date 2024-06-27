#Static class that bundles together common data operations


#Imports
import re
import pandas as pd
from Bio.Seq import Seq

class DataPreprocessor:
    
    #GFF PreProcessing METHODS
    @staticmethod
    def _read_gff_into_df(filepath: str) -> pd.DataFrame:
        col_names = ["contig_name", "source", "feature", "start", "end", 
                     "score", "strand", "phase", "attributes"]
        gff = pd.read_csv(filepath, sep = "\t", names = col_names)
        gff = gff.sort_values(by = "start")
        return gff

    @staticmethod
    def read_gff_into_list(filepath: str) -> list:
        #reading the file
        gff_file = open(filepath, "r")

        #splitting file into a list of lines (splitting by "\n")
        gff_lines = gff_file.readlines()

        return gff_lines

    #extracting feature id from attributes in gff. NOTE: feature id must have "ID=" before it and a ";" after it
    @staticmethod
    def _extract_feature_id(attribute_str: str) -> str:

        #extracting feature_id using regex from attribute_str
        reg_exp = "ID=(.*?);"
        feature_id = re.search(reg_exp, attribute_str)

        #If a match is found, then the matched string is returned (which is element 1 in feature_id)
        if feature_id:
            return feature_id[1]
        else:
            return "null"
        
    #function to modify attributes column in gff to only contain ID value of feature
    @classmethod
    def _change_attributes_to_ids(cls, gff: pd.DataFrame):
        gff.sort_values(by = ["feature", "start"], inplace = True)
        #changing attributes to only contain ID field for easier filtering
        gff["attributes"] = gff["attributes"] + ";"
        gff["attributes"] = gff["attributes"].apply(cls._extract_feature_id)
        
        return gff
    
    
    @staticmethod
    def _get_feature_df(gff: pd.DataFrame, feature: str):
        return gff.query(f'feature == "{feature}"')
    
    @classmethod
    def split_gff_by_feature(cls, gff: pd.DataFrame) -> tuple:
        genes = cls._get_feature_df(gff, "gene")
        mrnas = cls._get_feature_df(gff, "mRNA")
        exons = cls._get_feature_df(gff, "exon")
        cds = cls._get_feature_df(gff, "CDS")

        return genes, mrnas, exons, cds
    
    @staticmethod
    def construct_sequence(group, sequence):
        curr_sequence = ""

        #construct the sequence
        for i in range(group.shape[0]):
            curr_sequence += sequence[group.iloc[i, 3]-1 : group.iloc[i, 4]]
        # reverse complement the sequence if the direction is negative - this is done easiest using the Bio.Seq module
        if group.iloc[0, 6] == "-":
            temp = Seq(curr_sequence)
            curr_sequence = str(temp.reverse_complement())

        elif group.iloc[0, 6] == ".":
            raise ValueError("Error in Constructing CDS Sequence: CDS cannot have unkown direction")
        
        return curr_sequence

    @classmethod        
    def preprocess_gff(cls, file_path: str):
        gff = cls._read_gff_into_df(file_path)
        gff = cls._change_attributes_to_ids(gff)

        return gff
