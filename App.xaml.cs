using System.Configuration;
using System.Data;
using System.Windows;
using MomotetsuGame.Application.DependencyInjection;

namespace MomotetsuGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // DIコンテナを構成
            ServiceContainer.Configure();

            // モード選択画面から開始
            var navigationService = ServiceContainer.GetService<Infrastructure.Services.SimpleNavigationService>();
            navigationService.NavigateToAsync("ModeSelectionView").Wait();
        }
    }
}