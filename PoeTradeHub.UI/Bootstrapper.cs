using System.Windows;
using Autofac;
using PoeTradeHub.TradeAPI;
using PoeTradeHub.TradeAPI.OfficialTrade;
using PoeTradeHub.UI.ViewModels;

namespace PoeTradeHub.UI
{
    public class Bootstrapper : AutofacBootstrapper
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            builder.RegisterType<OfficialTradeAPI>()
                .As<ITradeAPI>()
                .WithParameter(new TypedParameter(typeof(string), "Metamorph"));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
