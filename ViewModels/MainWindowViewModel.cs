using HarfBuzzSharp;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Security;
using Tmds.DBus.Protocol;

namespace RappleyeLabGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _contentViewModel;
        public LoadDataViewModel LoadVM { get; }
        public MainWindowViewModel()
        {
            LoadVM = new LoadDataViewModel();
            _contentViewModel = LoadVM;
        }

        public ViewModelBase ContentViewModel
        {
            get => _contentViewModel;
            private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
        }

        public void NextScreen()
        {
            ContentViewModel = new RunCheckViewModel(LoadVM.gffDir, LoadVM.fastaDir);
        }

        public void BackScreen()
        {
            ContentViewModel = new LoadDataViewModel();
        }
    }
}
