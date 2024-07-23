using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ErrorLine
{
    public int n { get; set; }
    public string filename { get; set; }
    public string error_type { get; set; }
    public string identifier { get; set; }
    public string error_message { get; set; }

    public ErrorLine(int Num, string FileName, string ErrorType, string Identifier, string ErrorMessage)
    {
        this.n = Num;
        this.filename = FileName;
        this.error_type = ErrorType;
        this.identifier = Identifier;
        this.error_message = ErrorMessage;
    }
}

