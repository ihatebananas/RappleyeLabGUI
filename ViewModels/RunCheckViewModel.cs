﻿using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Security.AccessControl;
using Avalonia;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace RappleyeLabGUI.ViewModels
{
    public class RunCheckViewModel : ViewModelBase
    {
        private string _gffDir;
        private string _fastaDir;
        private string _pythonExe;
        private bool _canRun;
        private bool _canBack;
        private bool _canEditTextBox;
        private bool _canSave;
        private int _progressVal;
        private string _statusText;
        private ObservableCollection<ErrorLine> _errorLines;

        public ReactiveCommand<Unit, Unit> RunCommand { get; }

        public RunCheckViewModel(string gffDirectory, string fastaDirectory)
        {

            _canSave = false;
            _gffDir = gffDirectory;
            _fastaDir = fastaDirectory;
            _canRun = false;
            _errorLines = new ObservableCollection<ErrorLine>();
            _progressVal = 0;
            _canBack = true;
            _canEditTextBox = true;
            _statusText = "";

            var lines = File.ReadAllLines("pythonexepath.txt");

            if (lines.Length > 0)
            {
                _pythonExe = lines[0];
                _canRun = true;
            }
            else
            {
                _pythonExe = "";
            }

            RunCommand = ReactiveCommand.Create(RunChecks);

        }

        private string RunPython(string gffFilepath)
        {
            try
            {
                string relative_path = Path.Combine("python_scripts", "main.py");

                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(PythonExe)
                {
                    // RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p.StartInfo.ArgumentList.Add(relative_path);
                p.StartInfo.ArgumentList.Add(gffFilepath);
                p.StartInfo.ArgumentList.Add(FastaDir);

                p.Start();

                // Console.WriteLine(p.StandardOutput.ReadToEnd());

                p.WaitForExit();

                if (p.ExitCode == 0)
                {
                    return "success";
                } 
                else
                {
                    return "python exception raised";
                }
            }
            catch
            {
                return "python exe filepath is incorrect";
            }

        }

        // C:\Users\itz_s\AppData\Local\Programs\Python\Python311\python.exe
        public async void RunChecks()
        {
            try
            {
                StatusText = "";
                ProgressVal = 0;
                CanRun = false;
                CanBack = false;
                CanSave = false;
                CanEditTextBox = false;

                string error_header = "filename,error_type,identifier,error_message";
                File.WriteAllText("output_file.csv", error_header);

                DirectoryInfo gffFolder = new DirectoryInfo(GffDir);
                FileInfo[] gffFiles = gffFolder.GetFiles("*.gff");

                int count = 0;
                foreach (FileInfo gffFile in gffFiles)
                {
                    string result = await Task.Run(() => RunPython(gffFile.FullName));
                    if (result == "python exception raised")
                    {
                        StreamWriter outputWriter = new StreamWriter("output_file.csv", true);

                        outputWriter.Write("\n" + Path.GetFileName(gffFile.FullName) + ",Analysis,None,Error checks failed--see Help for possible reasons.");

                        outputWriter.Close();
                    }
                    else if (result == "python exe filepath is incorrect")
                    {
                        StatusText = "python.exe path is incorrect";
                        ProgressVal = 100;
                        break;
                    }

                    count++;
                    ProgressVal = (int)((count * 100) / gffFiles.Length);

                }

                CanRun = true;
                CanBack = true;
                CanEditTextBox = true;

                ChangeDataGrid("output_file.csv");
            }
            catch
            {

            }

        }

        public void ChangeDataGrid(string filepath)
        {
            try
            {
                StreamReader inputStream = new StreamReader(filepath);
                var errorLines = new List<ErrorLine>();
                inputStream.ReadLine();
                int count = 1;
                bool can_save = false;

                while (!inputStream.EndOfStream)
                {
                    var line = inputStream.ReadLine();
                    if (line != null)
                    {
                        var errorVals = line.Split(',');
                        ErrorLine currError = new ErrorLine(count, errorVals[0], errorVals[1], errorVals[2], errorVals[3]);
                        errorLines.Add(currError);
                        count++;

                        can_save = true;
                    }
                }

                if (can_save)
                {
                    CanSave = true;
                }
                else
                {
                    CanSave = false;
                }

                ErrorLines = new ObservableCollection<ErrorLine>(errorLines);

                inputStream.Close();
            }
            catch
            {

            }
            
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
                else
                {
                    CanRun = false;
                }

                File.WriteAllText("pythonexepath.txt", _pythonExe);
            }
        }

        public bool CanRun
        {
            get => _canRun;
            set => this.RaiseAndSetIfChanged(ref _canRun, value);
        }

        public bool CanBack
        {
            get => _canBack;
            set => this.RaiseAndSetIfChanged(ref _canBack, value);
        }

        public bool CanEditTextBox
        {
            get => _canEditTextBox;
            set => this.RaiseAndSetIfChanged(ref _canEditTextBox, value);
        }

        public bool CanSave
        {
            get => _canSave;
            set => this.RaiseAndSetIfChanged(ref _canSave, value);
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

        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

    }
}
