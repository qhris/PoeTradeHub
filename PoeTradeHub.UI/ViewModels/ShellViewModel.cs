using System.Windows;
using Caliburn.Micro;

namespace PoeTradeHub.UI.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        private readonly IWindowManager _windowManager;
        private readonly ConfigurationViewModel _configurationViewModel;

        public ShellViewModel(IWindowManager windowManager, ConfigurationViewModel configurationViewModel)
        {
            _windowManager = windowManager;
            _configurationViewModel = configurationViewModel;
        }

        public void ShowConfiguration()
        {
            if (!_configurationViewModel.IsActive)
            {
                _windowManager.ShowWindow(_configurationViewModel);
            }
        }

        public void ExitApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
