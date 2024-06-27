#To stop creation of pycache
import sys
sys.dont_write_bytecode = True
from pathlib import Path

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

def output_error_df(error_dict: dict, save_path: Path):
    error_df = pd.DataFrame.from_dict(error_dict)
    error_df["filename"] = error_df["filename"].apply(os.path.basename)

    timestamp = time.time()
    curr_time = time.ctime(timestamp)
    curr_time = curr_time[4:].replace(" ", "_")
    curr_time = curr_time.replace(":", ";")

    filename = save_path / f"error_file_{curr_time}.csv"
    error_df.to_csv(filename, index = False, sep = ",")

def main(gff_file, fas_dir, save_dir):
    gff_path = Path(gff_file)
    fas_path = Path(fas_dir)
    save_path = Path(save_dir)

    error_dict = {"filename": [],
            "error_type": [],
            "identifier": [],
            "error_message": []}
    
    analyze_files(gff_path, fas_path, error_dict)
    output_error_df(error_dict, save_path)
    
if __name__ == "__main__":

    gff_dir = Path("input/gff/BP_Contig457_20231226.gff")
    fas_dir = Path("input/fas/")
    save_dir = Path()

    main(gff_dir, fas_dir, save_dir)


