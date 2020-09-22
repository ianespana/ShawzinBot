using System.Windows;
using Caliburn.Micro;
using ShawzinBot.ViewModels;

namespace ShawzinBot
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }
    }
}
