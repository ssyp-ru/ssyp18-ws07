using Avalonia;
using Avalonia.Markup.Xaml;

namespace LenteApp.UI.Desktop
{
    class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        static void Main(string[] args)
        {
            AppBuilder.Configure<App>().UsePlatformDetect().Start<MainWindow>();
        }
    }
}