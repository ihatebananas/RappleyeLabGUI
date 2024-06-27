#this import allows a class to be used in itself
from __future__ import annotations
from abc import ABC, abstractmethod

from validators import *
from request import Request
        

class Handler:

    def __init__ (self, nextHandler: Handler, filename : str, validators: list[Validator]):
        self._next_handler = nextHandler
        self._filename = filename
        self._validators = validators
        self._proceed = True

    #returns true if check was passed
    #FIXME: hardcode run check to include for loop (boilerplate) and then a check function for each line?
    @abstractmethod
    def _log_error(self, error_dict: dict, identifier: str, error_message: str):
        pass

    def _run_check(self, request, error_dict: dict):
        for validator in self._validators:
            if not validator.check_element(request.element):
                # print("Error: " + validator.get_error_message())
                self._log_error(error_dict, request.id, validator.get_error_message())
                self._proceed = False

    def handle(self, request, error_dict: dict):

        #run the check
        self._run_check(request, error_dict)

        if (self._next_handler is not None) and (self._proceed == True):
            #pass request to next handler in chain
            return self._next_handler.handle(request, error_dict)
        

#GFF INTEGRITY HANDLER
class GFFIntegrityHandler(Handler):

    def __init__ (self, nextHandler : Handler, filename : str, validators: list[Validator]):
        super().__init__(nextHandler, filename, validators)

    def _log_error(self, error_dict: dict, identifier: str, error_message: str):
        #NOTE: Change filename to contig_name?
        error_dict["filename"].append(self._filename)
        error_dict["error_type"].append("GFF Integrity")
        error_dict["identifier"].append(identifier)
        error_dict["error_message"].append(error_message)


#GFF STRUCTURE HANDLER
class GFFStructureHandler(Handler):
    def __init__ (self, nextHandler : Handler, filename : str, validators: list):
        super().__init__(nextHandler, filename, validators)

    def _log_error(self, error_dict: dict, identifier: str, error_message: str):
        #NOTE: Change filename to contig_name?
        error_dict["filename"].append(self._filename)
        error_dict["error_type"].append("GFF Structure")
        error_dict["identifier"].append(identifier)
        error_dict["error_message"].append(error_message)


#INTRON HANDLER
class IntronHandler(Handler):

    def __init__ (self, nextHandler : Handler, filename : str, validators: list[Validator]):
        super().__init__(nextHandler, filename, validators)

    def _log_error(self, error_dict: dict, identifier: str, error_message: str):
        error_dict["filename"].append(self._filename)
        error_dict["error_type"].append("Splice Site")
        error_dict["identifier"].append(identifier)
        error_dict["error_message"].append(error_message)


#CDS HANDLER
class CDSHandler(Handler):
    def __init__ (self, nextHandler : Handler, filename : str, validators: list[Validator]):
        super().__init__(nextHandler, filename, validators)

    def _log_error(self, error_dict: dict, identifier: str, error_message: str):
        #NOTE: Change filename to contig_name?
        error_dict["filename"].append(self._filename)
        error_dict["error_type"].append("CDS")
        error_dict["identifier"].append(identifier)
        error_dict["error_message"].append(error_message)