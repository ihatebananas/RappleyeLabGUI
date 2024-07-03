using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RappleyeLabGUI.ViewModels
{
    public class RunCheckViewModel : ViewModelBase
    {
        private string _gffDir;
        private string _fastaDir;

        public RunCheckViewModel(string gffDirectory, string fastaDirectory)
        {
            _gffDir = gffDirectory;
            _fastaDir = fastaDirectory;
        }

        public string GffDir
        {
            get => _gffDir;
            set => this.RaiseAndSetIfChanged(ref _gffDir, value);
        }

        public string FastaDir
        {
            get => _fastaDir;
            set => this.RaiseAndSetIfChanged(ref _fastaDir, value);
        }
    }
}
