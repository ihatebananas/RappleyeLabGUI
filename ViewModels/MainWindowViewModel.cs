using HarfBuzzSharp;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reactive;
using Tmds.DBus.Protocol;

namespace RappleyeLabGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<string[], Unit> NextCommand { get; }
        private ViewModelBase _contentViewModel;

        public MainWindowViewModel()
        {
            _contentViewModel = new LoadDataViewModel();
            NextCommand = ReactiveCommand.Create<string[]>(NextScreen);
        }

        public ViewModelBase ContentViewModel
        {
            get => _contentViewModel;
            private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
        }

        public void NextScreen(string[] directories)
        {
            ContentViewModel = new RunCheckViewModel(directories[0], directories[1]);
        }

        public void BackScreen()
        {
            ContentViewModel = new LoadDataViewModel();
        }
    }
}
