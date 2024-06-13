using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RappleyeLabGUI.ViewModels
{
    public class RunCheckViewModel : ViewModelBase
    {
        public string gffDir { get; set; }
        public string fastaDir { get; set; }
        public RunCheckViewModel(string gffDirectory, string fastaDirectory)
        {
            gffDir = gffDirectory;
            fastaDir = fastaDirectory;
        }
    }
}
