using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using RappleyeLabGUI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RappleyeLabGUI.Views
{

    public partial class LoadDataView : UserControl
    {

        public LoadDataViewModel LoadVM { get; }
        public LoadDataView()
        {
            InitializeComponent();
            LoadVM = new LoadDataViewModel();
            GFFListBox.ItemsSource = new string[] { "", "", "", "", "", "", "" }.OrderBy(x => x);
            FastaListBox.ItemsSource = new string[] { "", "", "", "", "", "", "" }.OrderBy(x => x);
            GFFDataGrid.ItemsSource = LoadVM.GFFFeatures;
        }

        public void ResetHandler(object sender, RoutedEventArgs args)
        {
            GFFListBox.ItemsSource = new string[] { "", "", "", "", "", "", "" }.OrderBy(x => x);
            FastaListBox.ItemsSource = new string[] { "", "", "", "", "", "", "" }.OrderBy(x => x);
        }

        public void GFFPreviewHandler(object sender, RoutedEventArgs args)
        {

            var selectedFile = GFFListBox.SelectedItem;
            if (selectedFile != null)
            {
                if (selectedFile.ToString() != null)
                {
                    string filepath = selectedFile.ToString();
                    LoadVM.ChangeDataGrid(filepath);

                    GFFDataGrid.ItemsSource = LoadVM.GFFFeatures;
                }
            }
        }

        public void HelpHandler(object sender, RoutedEventArgs args)
        {
            // implement this later
        }

        public async void LoadFastaHandler(object sender, RoutedEventArgs args)
        {
            FolderPickerOpenOptions folderPicker = new FolderPickerOpenOptions();
            folderPicker.Title = "Select a folder for your fasta file directory: ";
            folderPicker.AllowMultiple = false;

            var top = TopLevel.GetTopLevel(this);


            IReadOnlyList<IStorageFolder> folderPicked = await top.StorageProvider.OpenFolderPickerAsync(folderPicker);
            if (folderPicked.Count != 0)
            {
                string path = folderPicked[0].TryGetLocalPath();
                if (path == null)
                {
                    // disable the load button and tell user that something is wrong with picked file
                }
                else
                {
                    LoadVM.fastaDir = path;
                    DirectoryInfo fastaDirectory = new DirectoryInfo(path);
                    FileInfo[] fastaFiles = fastaDirectory.GetFiles("*.fas");
                    string[] fastaFilepaths = new string[fastaFiles.Length];

                    for (int i = 0; i < fastaFiles.Length; i++)
                    {
                        string filepath = fastaFiles[i].Name;
                        int lastIndexOfSlash = filepath.LastIndexOf(@"\");
                        if (lastIndexOfSlash == -1)
                        {
                            lastIndexOfSlash = filepath.LastIndexOf("/");
                            if (lastIndexOfSlash == filepath.Length - 1)
                            {
                                lastIndexOfSlash--;
                            }
                        }

                        fastaFilepaths[i] = filepath.Substring(lastIndexOfSlash + 1);
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
        }


        public async void LoadGFFHandler(object sender, RoutedEventArgs args)
        {
            FolderPickerOpenOptions folderPicker = new FolderPickerOpenOptions();
            folderPicker.Title = "Select a folder for your GFF directory: ";
            folderPicker.AllowMultiple = false;

            var top = TopLevel.GetTopLevel(this);


            IReadOnlyList<IStorageFolder> folderPicked = await top.StorageProvider.OpenFolderPickerAsync(folderPicker);
            if (folderPicked.Count != 0)
            {
                string path = folderPicked[0].TryGetLocalPath();
                if (path == null)
                {
                    // disable the load button and tell user that something is wrong with picked file
                }
                else
                {
                    LoadVM.gffDir = path;
                    DirectoryInfo gffDirectory = new DirectoryInfo(path);
                    FileInfo[] gffFiles = gffDirectory.GetFiles("*.gff");
                    string[] gffFilepaths = new string[gffFiles.Length];

                    for (int i = 0; i < gffFiles.Length; i++)
                    {
                        string filepath = gffFiles[i].Name;
                        int lastIndexOfSlash = filepath.LastIndexOf(@"\");
                        if (lastIndexOfSlash == -1)
                        {
                            lastIndexOfSlash = filepath.LastIndexOf("/");
                            if (lastIndexOfSlash == filepath.Length - 1)
                            {
                                lastIndexOfSlash--;
                            }
                        }

                        gffFilepaths[i] = filepath.Substring(lastIndexOfSlash + 1);
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
        }
    }
}