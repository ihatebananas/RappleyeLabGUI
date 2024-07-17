using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ErrorLine
{
    
    public string FileName { get; set; }
    public string ErrorType { get; set; }
    public string Identifier { get; set; }
    public string ErrorMessage { get; set; }

    public ErrorLine(string FileName, string ErrorType, string Identifier, string ErrorMessage)
    {
        this.FileName = FileName;
        this.ErrorType = ErrorType;
        this.Identifier = Identifier;
        this.ErrorMessage = ErrorMessage;
    }
}

