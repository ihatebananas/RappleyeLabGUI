using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HarfBuzzSharp;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace RappleyeLabGUI.ViewModels
{
    public class LoadDataViewModel : ViewModelBase
    {
        private string _gffDir;
        private string _fastaDir;
        private string _selectedGFF;
        private string[] _directories;
        private ObservableCollection<GFFFeature> _gffFeatures;

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

        public string SelectedGFF
        {
            get => _selectedGFF;
            set => this.RaiseAndSetIfChanged(ref _selectedGFF, value);
        }
        public string[] Directories
        {
            get => _directories;
            set => this.RaiseAndSetIfChanged(ref _directories, value);
        }

        public ObservableCollection<GFFFeature> GffFeatures 
        {
            get => _gffFeatures;
            set => this.RaiseAndSetIfChanged(ref _gffFeatures, value);
        }

        public ReactiveCommand<Unit, Unit> GffPreviewCommand { get; }


        public LoadDataViewModel()
        {
            _gffDir = "";
            _fastaDir = "";
            _gffFeatures = new ObservableCollection<GFFFeature>();
            _directories = new string[] { "", "" };
            _selectedGFF = "";

            GffPreviewCommand = ReactiveCommand.Create(GFFPreview);

        }

        public void ChangeDataGrid(string filepath)
        {
            StreamReader inputStream = new StreamReader(filepath);
            var gfffeatures = new List<GFFFeature>();

            while (!inputStream.EndOfStream)
            {
                var line = inputStream.ReadLine();
                if (line != null)
                {
                    var featureVals = line.Split('\t');
                    GFFFeature currFeature = new GFFFeature(featureVals[0], featureVals[1], featureVals[2], int.Parse(featureVals[3]), int.Parse(featureVals[4]), double.Parse(featureVals[5]), featureVals[6], featureVals[7], featureVals[8]);
                    gfffeatures.Add(currFeature);
                }
            
            }

            GffFeatures = new ObservableCollection<GFFFeature>(gfffeatures);
        }

        public void GFFPreview()
        {
            var selectedFile = _selectedGFF;
            if (selectedFile != null)
            {
                string filepath = selectedFile.ToString();

                if (filepath != "" && filepath != null)
                {
                    try
                    {
                        ChangeDataGrid(System.IO.Path.Combine(GffDir, filepath));
                    } 
                    catch
                    {
                        // give message to user that file can't be opened
                    }
                    
                }
            }
        }

        public void HelpHandler()
        {
            // implement this later
        }

    }
}
