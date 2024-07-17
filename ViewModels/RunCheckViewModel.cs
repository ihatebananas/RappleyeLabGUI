using ReactiveUI;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System.Security.AccessControl;
using Avalonia;
using System.Reflection;
using System.Diagnostics;

namespace RappleyeLabGUI.ViewModels
{
    public class RunCheckViewModel : ViewModelBase
    {
        private string _gffDir;
        private string _fastaDir;
        private string _pythonExe;
        private bool _canRun;
        private int _progressVal;
        private ObservableCollection<ErrorLine> _errorLines;

        public ReactiveCommand<Unit, Unit> RunCommand { get; }

        public RunCheckViewModel(string gffDirectory, string fastaDirectory)
        {
            _gffDir = gffDirectory;
            _fastaDir = fastaDirectory;
            _pythonExe = "";
            _canRun = false;
            _errorLines = new ObservableCollection<ErrorLine>();
            _progressVal = 0;

            RunCommand = ReactiveCommand.Create(RunChecks);
        }

        public async void RunChecks()
        {
            string error_header = "filename,error_type,identifier,error_message";
            string relative_path = Path.Combine("python_scripts", "main.py");

            File.WriteAllText("output_file.csv", error_header);

            DirectoryInfo gffFolder = new DirectoryInfo(GffDir);
            FileInfo[] gffFiles = gffFolder.GetFiles("*.gff");

            int count = 0;
            foreach (FileInfo gffFile in gffFiles)
            {
                count++;
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(PythonExe)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p.StartInfo.ArgumentList.Add(relative_path);
                p.StartInfo.ArgumentList.Add(gffFile.FullName);
                p.StartInfo.ArgumentList.Add(FastaDir);

                p.Start();

                Console.WriteLine(p.StandardOutput.ReadToEnd());

                await p.WaitForExitAsync();

                ProgressVal = (int)((count / gffFiles.Length) * 100);
            }

            // update CanRun maybe?

            ChangeDataGrid("output_file.csv");
        }

        public void ChangeDataGrid(string filepath)
        {
            StreamReader inputStream = new StreamReader(filepath);
            var errorLines = new List<ErrorLine>();
            inputStream.ReadLine();

            while (!inputStream.EndOfStream)
            {
                var line = inputStream.ReadLine();
                if (line != null)
                {
                    var errorVals = line.Split(',');
                    ErrorLine currError = new ErrorLine(errorVals[0], errorVals[1], errorVals[2], errorVals[3]);
                    errorLines.Add(currError);
                }

            }

            ErrorLines = new ObservableCollection<ErrorLine>(errorLines);
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

        public string PythonExe
        {
            get => _pythonExe;
            set
            {
                this.RaiseAndSetIfChanged(ref _pythonExe, value);
                if (_pythonExe != "" && _pythonExe != null)
                {
                    CanRun = true;
                }
            }
        }

        public bool CanRun
        {
            get => _canRun;
            set => this.RaiseAndSetIfChanged(ref _canRun, value);
        }

        public ObservableCollection<ErrorLine> ErrorLines
        {
            get => _errorLines;
            set => this.RaiseAndSetIfChanged(ref _errorLines, value);
        }

        public int ProgressVal
        {
            get => _progressVal;
            set => this.RaiseAndSetIfChanged(ref _progressVal, value);
        }

    }
}
