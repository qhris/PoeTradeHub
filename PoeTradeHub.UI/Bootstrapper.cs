using System.Reflection;
using System.Windows;
using Autofac;
using PoeTradeHub.TradeAPI;
using PoeTradeHub.TradeAPI.OfficialTrade;
using PoeTradeHub.UI.ViewModels;
using WindowsInput;

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

            // Register services.
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(type => type.Name.EndsWith("Service"))
                .Where(type => !(string.IsNullOrWhiteSpace(type.Namespace)) && type.Namespace.EndsWith("Services"))
                .Where(type => !type.IsInterface)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<OfficialTradeAPI>()
                .As<ITradeAPI>()
                .WithParameter(new TypedParameter(typeof(string), "Metamorph"));

            builder.RegisterType<InputSimulator>()
                .As<IInputSimulator>()
                .SingleInstance();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
