using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RappleyeLabGUI.ViewModels;


namespace RappleyeLabGUI.Views
{
    public partial class RunCheckView : UserControl
    {
        public RunCheckView()
        {
            InitializeComponent();
        }

        public void RunHandler(object sender, RoutedEventArgs args)
        {
            Test.Text = "Hi";
        }
    }
}

