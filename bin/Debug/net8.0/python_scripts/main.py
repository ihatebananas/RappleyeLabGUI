#To stop creation of pycache
from ast import Raise
import sys
sys.dont_write_bytecode = True
from pathlib import Path
import os
from glob import glob

import time
from chain import *

#Initializing Validator Chains
gff_integrity_validator_chain = [[NumColValidator()], 
                                [NullValueValidator()], 
                                [FeatureTypeValidator(), StrandTypeValidator(), 
                                BoundaryStartValidator(), BoundaryEndValidator()]]

gff_structure_validator_chain = [[OverlappingFeaturesValidator()], 
                                [ParentChildBoundaryValidator(), StrandConsistencyValidator()],
                                [ParentChildStrandValidator()]]

intron_validator_chain = [[IntronStrandValidator()],
                        [StartSpliceValidator(), EndSpliceValidator()]]

cds_validator_chain = [[CDSStrandValidator()], 
                    [StartCodonValidator(), StopCodonValidator(), PrematureStopCodonValidator()],
                    [CDSLengthValidator()]]

def analyze_files(gff_path: Path, fas_dir: Path, error_dict: dict):
    print(f"\n===========Analyzing {gff_path}===========\n")
        
    #Creating chain
    cds_chain = CDSChain(None, gff_path, cds_validator_chain, fas_dir)
    intron_chain = IntronChain(cds_chain, gff_path, intron_validator_chain, fas_dir)
    gff_structure_chain = GFFStructureChain(intron_chain, gff_path, gff_structure_validator_chain)
    gff_integrity_chain = GFFIntegrityChain(gff_structure_chain, gff_path, gff_integrity_validator_chain)

    #Running chain
    gff_integrity_chain.run_chain(error_dict)

def output_error_df(error_dict: dict):
    prev_error_df = pd.read_csv("output_file.csv")
    curr_error_df = pd.DataFrame.from_dict(error_dict)

    error_df = pd.concat([prev_error_df, curr_error_df])
        
    error_df.to_csv("output_file.csv", index=False)

def checkFasta(gff_name, fasta_dir):
    try:
        fas_seq = SeqIO.read(fasta_dir / f"{gff_name}.fas", "fasta")

        return True
    except:
        return False


def checkGFF(gff_path):
    try:
        col_names = ["contig_name", "source", "feature", "start", "end", 
                        "score", "strand", "phase", "attributes"]
        gff = pd.read_csv(gff_path, sep = "\t", names = col_names)

        return gff.iloc[0, 0]
    except:
        return ""

def main(gff_file, fas_dir):
    gff_path = Path(gff_file)
    fas_path = Path(fas_dir)
    
    error_dict = {"filename": [],
            "error_type": [],
            "identifier": [],
            "error_message": []}
    
    gff_name = checkGFF(gff_path)

    if (gff_name != ""):
        if (checkFasta(gff_name, fas_path)):
            analyze_files(gff_path, fas_path, error_dict)
        else:
            error_dict['filename'].append(os.path.basename(gff_path))
            error_dict['error_type'].append("File I/O")
            error_dict['identifier'].append("None")
            error_dict['error_message'].append(f"{fas_path} could not be processed. See Help for possible reasons.")
    else:
        error_dict['filename'].append(os.path.basename(gff_path))
        error_dict['error_type'].append("File I/O")
        error_dict['identifier'].append("None")
        error_dict['error_message'].append(f"{os.path.basename(gff_path)} could not be processed. See Help for possible reasons.")
    
    output_error_df(error_dict)

main(sys.argv[1], sys.argv[2])
    


'''
if __name__ == "__main__":
    header = "filename,error_type,identifier,error_message"

    f = open("output/output_file.csv", "w")
    f.truncate()
    f.write(header)
    f.close()

    gff_path = Path("input/labelled_gffs/")
    fasta_dir = Path("input/fas/")

    for gff_file in gff_path.glob("*.gff"):
        main(gff_file, fasta_dir)
'''