using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LenteApp.UI.Desktop
{
    public class MainWindow : Window
    {
        private SearchViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.AttachDevTools();

            DataContext = _viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoaderPortableXaml.Load(this);
            _viewModel = new SearchViewModel();
        }
    }
}