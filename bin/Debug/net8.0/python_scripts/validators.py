import pandas as pd
from Bio.Seq import Seq


class Validator:

    def __init__(self):
        self._reference_id = ""
        self._error_message = ""
    
    def check_element(self, element) -> bool:
        pass

    def _generate_error_message(self, element) -> None:
        pass

    def get_error_message(self) -> str:
        return self._error_message
    

#GFF INTEGRITY VALIDATORS
class NumColValidator(Validator):

    def __init__(self):
        super().__init__()

    def check_element(self, element: list) -> bool:
        # print("Checking Number of Columns...")
        num_col = len(element)

        if num_col == 9:
            return True
        else:
            self._generate_error_message(num_col)
            return False
        
    def _generate_error_message(self, num_col: int) -> None:
        self._error_message = f"There are {num_col} fields but there should be 9 fields"


class NullValueValidator(Validator):

    def __init__(self):
        super().__init__()
        
        #relevant fields to check for null values
        self._indices_to_check = [0, 2, 3, 4, 6, 8]

        #defining possible null values
        self._null_values = {"null", "Null", "Nan", "nan", ""} 

    def check_element(self, element):
        # print("Checking Null Values...")

        no_null_values = True

        #checking if relevant fields are null
        for null_idx in self._indices_to_check:

            if element[null_idx] in self._null_values:
                # print(f"There are null values in line {idx} in file f{self._filename}")
                self._generate_error_message(null_idx)
                no_null_values = False

        return no_null_values
    
    def _generate_error_message(self, col_idx: int) -> None:
        self._error_message = f"Field/Column {col_idx} should not be null"


class FeatureTypeValidator(Validator):
        
    def __init__(self):
        super().__init__()

        #defining valid feature types
        self._feature_types = {"gene", "mRNA", "exon", "CDS"}
    
    def check_element(self, element) -> bool:
        feature_type = element[2]

        if feature_type in self._feature_types:
            return True
        
        else:
            self._generate_error_message(feature_type)
            return False
    
    def _generate_error_message(self, feature_type: str) -> None:
        self._error_message = f"{feature_type} is an invalid feature type"


class StrandTypeValidator(Validator):

    def __init__(self):
        super().__init__()

        #defining valid strand types
        self._strand_types = {"+", "-", "."}
    
    def check_element(self, element) -> bool:
        strand_type = element[6]

        if strand_type in self._strand_types:
            return True
        
        else:
            self._generate_error_message(strand_type)
        
    def _generate_error_message(self, strand_type: str) -> None:
        self._error_message = f"{strand_type} is an invalid strand type"


class BoundaryStartValidator(Validator):

    def __init__(self):
        super().__init__()        
    
    def check_element(self, element) -> bool:
        boundary_start = element[3]

        if boundary_start.isnumeric():
            return True
        else:
            self._generate_error_message(boundary_start)
            return False
               
    def _generate_error_message(self, boundary_start: str) -> None:
        self._error_message = f"start boundary value ({boundary_start}) is not numeric"


class BoundaryEndValidator(Validator):

    def __init__(self):
        super().__init__()
    
    def check_element(self, element) -> bool:
        boundary_end = element[4]

        if boundary_end.isnumeric():
            return True
        else:
            self._generate_error_message(boundary_end)
            return False
    
    def _generate_error_message(self, boundary_end: str) -> None:
        self._error_message = f"end boundary value ({boundary_end}) is not numeric"



#GFF STRUCTURE VALIDATORS
        
class OverlappingFeaturesValidator(Validator):

    def __init__(self):
        super().__init__()

    def check_element(self, element) -> bool:

        features_not_overlapping =  True
        for curr_feature in element:
            # print(curr_feature)

            #if curr_feature is a single row or empty, it only has 1 row -> it cannot have more than one gene_id
            if curr_feature.shape[0] <=1:
                continue

            #getting np array of unique gene ids in curr_feature
            unique_attributes_in_feature = curr_feature["attributes"].unique()
            # print("unique attributes: ", unique_attributes_in_feature)

            #checking for more than 1 gene id in curr_feature == checking if curr_feature overlaps with another
            if len(unique_attributes_in_feature) > 1:
                features_not_overlapping = False
                self._generate_error_message(unique_attributes_in_feature)
                # print(f"features {unique_attributes_in_feature} have overlapping {curr_feature['feature']}")

        return features_not_overlapping

    def _generate_error_message(self, attributes_in_feature) -> None:
        self._error_message = f"The following features are overlapping {attributes_in_feature}"


class StrandConsistencyValidator(Validator):

    def __init__(self):
        super().__init__()

    #checks if possibly multi-row features like exons and CDS have the same direction on all of their rows
    def _check_strand_consistency(curr_feature):
        strand_consistency = True

        #if curr_feature is empty or a series (one row), then it cannot have consistency issues!
        # if not (isinstance(curr_feature, pd.Series) or curr_feature.empty):
        if curr_feature.shape[0] > 1:
            unique_strand_values = curr_feature["strand"].unique()
            strand_consistency = (len(unique_strand_values) == 1)
        
        return strand_consistency

    def check_element(self, element) -> bool:
        strands_consistent = True

        for child_feature in element[1:]:

            if not StrandConsistencyValidator._check_strand_consistency(child_feature):
                # print(f"Strand consistency failed for f{child_feature}")
                self._generate_error_message(child_feature)
                strands_consistent = False

        return strands_consistent

    def _generate_error_message(self, child_feature) -> None:
        self._error_message = f"{child_feature.iloc[0, 2]} strand entries are not consistent"


class ParentChildStrandValidator(Validator):
    
    def __init__(self):
        super().__init__()

    def _get_child_strand(child_feature, gene_strand) -> str:
            
            if child_feature.shape[0] > 0:
                return child_feature.iloc[0, 6]
            else:
                # print(child_feature)
                return gene_strand
        
    def check_element(self, element) -> bool:
        strands_match = True
        gene = element[0]
        gene_strand = gene.iloc[0, 6]

        for child_feature in element[1:]:
            #getting strand of child feature
            child_strand = ParentChildStrandValidator._get_child_strand(child_feature, gene_strand)

            if not gene_strand == child_strand:
                self._generate_error_message(child_feature, gene)
                strands_match = False
        # print("strands match value:", strands_match)
        return strands_match
    
    def _generate_error_message(self, child_feature, gene) -> None:
        self._error_message = f"Strands don't match for gene and {child_feature.iloc[0, 2]} for feature {gene.iloc[0,8]}"


#TODO: Add BoundaryConsistency Validator



class ParentChildBoundaryValidator(Validator):

    def __init__(self):
        super().__init__()

    def _compare_boundaries(gene: pd.DataFrame, child_feature: pd.DataFrame) -> bool:

        if child_feature.empty:
            return True
        
        gene_start = gene.iloc[0,3]
        gene_end = gene.iloc[0,4]

        child_start = child_feature.iloc[0,3]
        child_end = child_feature.iloc[child_feature.shape[0] - 1, 4]

        if child_feature.iloc[0,2] == "CDS":
            return (gene_start <= child_start and gene_end >= child_end)
        else:
            return(gene_start == child_start and gene_end == child_end)

    def check_element(self, element) -> bool:
        boundaries_match = True
        gene = element[0]

        for child_feature in element[1:]:

            if not ParentChildBoundaryValidator._compare_boundaries(gene, child_feature):
                self._generate_error_message(child_feature, gene)
                boundaries_match = False
                #FIXME: Include Debug Message?
                # print(f"Boundaries don't match for gene and {child_record.iloc[0, 2]} for feature {gene_record.iloc[0,8]} with gene coordinates {gene_record.iloc[0,3]}:{gene_record.iloc[0,4]}")

        return boundaries_match

    def _generate_error_message(self, child_feature, gene) -> None:
        self._error_message = f"Boundaries don't match for gene and {child_feature.iloc[0, 2]} for feature {gene.iloc[0,8]} with gene coordinates {gene.iloc[0,3]}:{gene.iloc[0,4]}"


    
#INTRON VALIDATORS
        
class IntronStrandValidator(Validator):
    
    def __init__(self):
        super().__init__()

    def check_element(self, element) -> bool:
        intron_strand = element["strand"]

        if not (intron_strand == "+" or intron_strand == "-"):
            self._generate_error_message(intron_strand)
            return False
        else:
            return True

    def _generate_error_message(self, intron_strand: str) -> None:
        self._error_message = f"Intron strand ({intron_strand}) is not + or -"

class StartSpliceValidator(Validator):

    def __init__(self):
        super().__init__()

    def _get_start_splice(intron_strand: str, intron_seq: Seq) -> Seq:
        # print("intron row:\n", intron_row)

        if intron_strand == "+":

            # return fas_seq[intron_row.iloc[1] : intron_row.iloc[1] + 5]
            return intron_seq[ : 5]
        
        elif intron_strand == "-":

            # return (fas_seq[intron_row.iloc[2] - 6 : intron_row.iloc[2] - 1]).reverse_complement()
            return (intron_seq[-5 : ]).reverse_complement()
        

    def _check_start_splice(start_splice: str) -> bool:
        #include check to see if last character is a G?
        if start_splice[: 2] not in {"GT", "GC"}:
            return False
        return True
    
    def check_element(self, element):

        start_splice = StartSpliceValidator._get_start_splice(element["strand"], element["intron_seq"])
        start_splice_str = str(start_splice.seq)

        if not StartSpliceValidator._check_start_splice(start_splice_str):
            self._generate_error_message(start_splice_str)
            return False
        
        else:
            return True

    def _generate_error_message(self, start_splice_str: str) -> None:
        self._error_message = f"Start splice site {start_splice_str} does not start with GT or GC"


class EndSpliceValidator(Validator):
    
    def __init__(self):
        super().__init__()

    def _get_end_splice(intron_strand: str, intron_seq: Seq) -> Seq:
        # print("intron row:\n", intron_row)

        if intron_strand == "+":

            # return fas_seq[intron_row.iloc[2] - 3: intron_row.iloc[2] - 1]
            return intron_seq[-2 : ]
        
        elif intron_strand == "-":

            # return (fas_seq[intron_row.iloc[1]: intron_row.iloc[1] + 2]).reverse_complement()
            return (intron_seq[ : 2]).reverse_complement()

    def _check_end_splice(end_splice_str: str) -> bool:
        #include check to see if last character is a G?
        if end_splice_str != "AG":
            return False
        return True

    def check_element(self, element):

        end_splice = EndSpliceValidator._get_end_splice(element["strand"], element["intron_seq"])
        end_splice_str = str(end_splice.seq)


        # print("End Splice: ", end_splice.seq)
        if not EndSpliceValidator._check_end_splice(end_splice_str):
            self._generate_error_message(end_splice_str)
            return False
        else:
            return True
    
    def _generate_error_message(self, end_splice_str: str) -> None:
        self._error_message = f"The splice site end {end_splice_str} is not AG"



#CDS VALIDATORS
        
class CDSStrandValidator(Validator):
    def __init__(self):
        super().__init__()

    def check_element(self, element) -> bool:
        group = element["group"]

        if group.iloc[0, 6] == ".":
            self._generate_error_message()
            return False
        else:
            return True
    
    def _generate_error_message(self) -> None:
        self._error_message = "CDS has unkown direction"

class StartCodonValidator(Validator):
    
    def __init__(self):
        super().__init__()

    def check_element(self, element) -> bool:

        cds_seq = element["cds_seq"]
        start_codon = cds_seq[:3]
        if start_codon != "ATG":
            self._generate_error_message(start_codon)
            return False
        else:
            return True
    
    def _generate_error_message(self, start_codon: str) -> None:
        self._error_message = f"The start codon is {start_codon} and not ATG"


class StopCodonValidator(Validator):
    
    def __init__(self):
        super().__init__()

    def check_element(self, element) -> bool:
        
        cds_seq = element["cds_seq"]
        stop_codon = cds_seq[-3:]
        valid_stop_codons = {"TAG", "TAA", "TGA"} 
        if stop_codon not in valid_stop_codons:
            self._generate_error_message(stop_codon)
            return False
        else:
            return True
    
    def _generate_error_message(self, stop_codon: str) -> None:
        self._error_message = f"The stop codon is {stop_codon} and not one of TAG/TAA/TGA"


class PrematureStopCodonValidator(Validator):
    
    def __init__(self):
        super().__init__()

    def check_element(self, element) -> bool:
        
        cds_seq = element["cds_seq"]
        group = element["group"]
        valid_stop_codons = {"TAG", "TAA", "TGA"} 

        for i in range(3, len(cds_seq) - 3, 3):
            codon = cds_seq[i : i + 3]
            if codon in valid_stop_codons:
                premature_stop_coordinate = group.iloc[0, 3] + i
                self._generate_error_message(premature_stop_coordinate)
                return False

        return True
    
    def _generate_error_message(self, premature_stop_coordinate: str) -> None:
        self._error_message = f"There is a premature stop codon at coordinate {premature_stop_coordinate}"


class CDSLengthValidator(Validator):
    def __init__(self):
        super().__init__()

    def check_element(self, element) -> bool:
        
        cds_seq = element["cds_seq"]

        if not (len(cds_seq) % 3) == 0:
            self._generate_error_message()
            return False
        
        else:
            return True
    
    def _generate_error_message(self) -> None:
        self._error_message = f"CDS has a partial codon (length is not divisible by 3)"