using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using RappleyeLabGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;


namespace RappleyeLabGUI.Views
{
    public partial class RunCheckView : UserControl
    {
        public RunCheckView()
        {
            InitializeComponent();
        }

        public void HelpHandler(object sender, RoutedEventArgs args)
        {
            var helpWindow = new HelpWindow();
            helpWindow.Show();
        }

        public string[] GetErrorLines()
        {
            return File.ReadAllLines("output_file.csv");
        }

        public bool WriteErrorLines(string[] lines, string path)
        {
            try
            {
                File.WriteAllLines(path, lines);

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public async void SaveHandler(object sender, RoutedEventArgs args)
        {
            var redBrush = RedButton.Foreground;
            var greenBrush = GreenButton.Foreground;

            var topLevel = TopLevel.GetTopLevel(this);
            string[] extensions = { "csv" };

            FilePickerFileType csvFileType = new FilePickerFileType("CSV File")
            {
                Patterns = new[] { "*.csv" }
            };

            List<FilePickerFileType> acceptableFiletypes = new List<FilePickerFileType>();
            acceptableFiletypes.Add(csvFileType);

            FilePickerSaveOptions saveOptions = new FilePickerSaveOptions()
            {
                Title = "Save error file",
                SuggestedFileName = "error_file.csv",
                DefaultExtension = "csv",
                FileTypeChoices = acceptableFiletypes,
                ShowOverwritePrompt = true
            };

            if (topLevel is not null)
            {
                try
                {
                    var savedFile = await topLevel.StorageProvider.SaveFilePickerAsync(saveOptions);

                    if (savedFile is not null)
                    {
                        string[] lines = await Task.Run(GetErrorLines);
                        var savedFilepath = savedFile.TryGetLocalPath();
                        if (savedFilepath != null)
                        {
                            try
                            {
                                bool saveStatus = await Task.Run(() => WriteErrorLines(lines, savedFilepath));

                                SaveStatusText.Text = "success";
                                if (greenBrush is not null)
                                {
                                    SaveStatusText.Foreground = greenBrush;
                                }
                            }
                            catch
                            {
                                SaveStatusText.Text = "failed";
                                if (redBrush is not null)
                                {
                                    SaveStatusText.Foreground = redBrush;
                                }
                            }
                        }
                        else
                        {
                            SaveStatusText.Text = "failed";
                            if (redBrush is not null)
                            {
                                SaveStatusText.Foreground = redBrush;
                            }
                        }
                    }
                    else
                    {
                        SaveStatusText.Text = "failed";
                        if (redBrush is not null)
                        {
                            SaveStatusText.Foreground = redBrush;
                        }
                    }
                }
                catch
                {
                    SaveStatusText.Text = "failed";
                    if (redBrush is not null)
                    {
                        SaveStatusText.Foreground = redBrush;
                    }
                }
                
            }
            else
            {
                SaveStatusText.Text = "failed";
                if (redBrush is not null)
                {
                    SaveStatusText.Foreground = redBrush;
                }
            }

            await Task.Delay(10000);

            SaveStatusText.Text = "";
        }
    }
}

