using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Security;
using Tmds.DBus.Protocol;

namespace RappleyeLabGUI.ViewModels
{
    public class LoadDataViewModel : ViewModelBase
    {
        public string gffDir { get; set; }
        public string fastaDir { get; set; }
        public ObservableCollection<GFFFeature> GFFFeatures { get; set; }

        public LoadDataViewModel()
        {
            gffDir = "";
            fastaDir = "";
            GFFFeatures = new ObservableCollection<GFFFeature>();
        }

        public void ChangeDataGrid(string filepath)
        {
            StreamReader inputStream = new StreamReader(gffDir + @"\" + filepath);
            var gfffeatures = new List<GFFFeature>();

            while (!inputStream.EndOfStream)
            {
                var line = inputStream.ReadLine();
                var featureVals = line.Split('\t');
                GFFFeature currFeature = new GFFFeature(featureVals[0], featureVals[1], featureVals[2], int.Parse(featureVals[3]), int.Parse(featureVals[4]), double.Parse(featureVals[5]), featureVals[6], featureVals[7], featureVals[8]);
                gfffeatures.Add(currFeature);
            }

            GFFFeatures = new ObservableCollection<GFFFeature>(gfffeatures);
        }
    }
}
