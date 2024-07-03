using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using RappleyeLabGUI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace RappleyeLabGUI.Views
{
    public partial class LoadDataView : UserControl
    {
        public LoadDataView()
        {
            InitializeComponent();
            GFFListBox.ItemsSource = new string[] { "", "", "", "", "", "", "" };
            FastaListBox.ItemsSource = new string[] { "", "", "", "", "", "", "" };
        }

        public bool CanContinue()
        {
            return GffDummy.Text != "" && FastaDummy.Text != "" && GffDummy.Text != null && FastaDummy.Text != null;
        }

        public async void ShowFastaDialog(object sender, RoutedEventArgs e)
        {
            FolderPickerOpenOptions folderPicker = new FolderPickerOpenOptions();
            folderPicker.Title = "Select a folder for your fasta file directory: ";
            folderPicker.AllowMultiple = false;

            var top = TopLevel.GetTopLevel(this);


            IReadOnlyList<IStorageFolder> folderPicked = await top.StorageProvider.OpenFolderPickerAsync(folderPicker);
            if (folderPicked.Count != 0)
            {
                string path = folderPicked[0].TryGetLocalPath();
                if (path != null)
                {
                    FastaDummy.Text = path;
                    DirectoryInfo fastaDirectory = new DirectoryInfo(path);
                    FileInfo[] fastaFiles = fastaDirectory.GetFiles("*.fas");
                    string[] fastaFilepaths = new string[fastaFiles.Length];

                    for (int i = 0; i < fastaFiles.Length; i++)
                    {
                        string filepath = fastaFiles[i].Name;
                        fastaFilepaths[i] = Path.GetFileName(filepath);
                    }

                    if (fastaFilepaths.Length < 7)
                    {
                        int emptyElements = 7 - fastaFilepaths.Length;
                        string[] tempFastaFilepaths = new string[7];
                        for (int i = 0; i < 7; i++)
                        {
                            if (i < fastaFilepaths.Length)
                            {
                                tempFastaFilepaths[i] = fastaFilepaths[i];
                            }
                            else
                            {
                                tempFastaFilepaths[i] = "";
                            }
                        }
                        FastaListBox.ItemsSource = tempFastaFilepaths.ToArray();
                    }
                    else
                    {
                        FastaListBox.ItemsSource = fastaFilepaths.ToArray();
                    }
                }
            }

            if (CanContinue())
            {
                string gffDir = GffDummy.Text;
                string fastaDir = FastaDummy.Text;
                DirectoryDummy.ItemsSource = new string[] {gffDir, fastaDir};
                ContinueButton.IsEnabled = true;
            }
        }


        public async void ShowGFFDialog(object sender, RoutedEventArgs e)
        {
            FolderPickerOpenOptions folderPicker = new FolderPickerOpenOptions();
            folderPicker.Title = "Select a folder for your GFF directory: ";
            folderPicker.AllowMultiple = false;

            var top = TopLevel.GetTopLevel(this);

            IReadOnlyList<IStorageFolder> folderPicked = await top.StorageProvider.OpenFolderPickerAsync(folderPicker);
            if (folderPicked.Count != 0)
            {
                string path = folderPicked[0].TryGetLocalPath();
                if (path != null)
                {
                    GffDummy.Text = path;
                    DirectoryInfo gffDirectory = new DirectoryInfo(path);
                    FileInfo[] gffFiles = gffDirectory.GetFiles("*.gff");
                    string[] gffFilepaths = new string[gffFiles.Length];

                    for (int i = 0; i < gffFiles.Length; i++)
                    {
                        string filepath = gffFiles[i].Name;
                        gffFilepaths[i] = Path.GetFileName(filepath);
                    }

                    if (gffFilepaths.Length < 7)
                    {
                        int emptyElements = 7 - gffFilepaths.Length;
                        string[] tempGFFFilepaths = new string[7];
                        for (int i = 0; i < 7; i++)
                        {
                            if (i < gffFilepaths.Length)
                            {
                                tempGFFFilepaths[i] = gffFilepaths[i];
                            }
                            else
                            {
                                tempGFFFilepaths[i] = "";
                            }
                        }
                        GFFListBox.ItemsSource = tempGFFFilepaths.ToArray();
                    }
                    else
                    {
                        GFFListBox.ItemsSource = gffFilepaths.ToArray();
                    }
                }
            }

            if (CanContinue())
            {
                string gffDir = GffDummy.Text;
                string fastaDir = FastaDummy.Text;
                DirectoryDummy.ItemsSource = new string[] { gffDir, fastaDir };
                ContinueButton.IsEnabled = true;
            }
        }
    }
}