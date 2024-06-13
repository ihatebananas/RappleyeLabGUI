using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RappleyeLabGUI.Views
{
    public partial class RunCheckView : UserControl
    {
        public RunCheckView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

