using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using IContainer = Autofac.IContainer;

namespace PoeTradeHub.UI
{
    public class AutofacBootstrapper : BootstrapperBase
    {
        protected override void Configure()
        {
            var builder = new ContainerBuilder();

            ConfigureContainer(builder);

            Container = builder.Build();
        }

        /// <summary>
        /// Get the IoC container.
        /// </summary>
        public IContainer Container { get; private set; }

        protected override object GetInstance(Type serviceType, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                if (Container.IsRegistered(serviceType))
                {
                    return Container.Resolve(serviceType);
                }
            }
            else
            {
                if (Container.IsRegisteredWithKey(key, serviceType))
                {
                    return Container.ResolveKeyed(key, serviceType);
                }
            }

            throw new Exception($"Could not locate any instances of contract {key ?? serviceType.Name}.");
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return Container.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            Container.InjectProperties(instance);
        }

        protected virtual void ConfigureContainer(ContainerBuilder builder)
        {
            // Register view models.
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(type => type.Name.EndsWith("ViewModel"))
                .Where(type => !(string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.EndsWith("ViewModels")))
                .Where(type => type.GetInterface(typeof(INotifyPropertyChanged).Name) != null)
                .AsSelf()
                .InstancePerDependency();

            // Register views.
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(type => type.Name.EndsWith("View"))
                .Where(type => !(string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.EndsWith("Views")))
                .AsSelf()
                .InstancePerDependency();

            builder.Register<IWindowManager>(c => new WindowManager())
                .InstancePerLifetimeScope();

            builder.Register<IEventAggregator>(c => new EventAggregator())
                .InstancePerLifetimeScope();
        }
    }
}
