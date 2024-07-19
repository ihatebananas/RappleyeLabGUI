using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RappleyeLabGUI.ViewModels;
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
    }
}

