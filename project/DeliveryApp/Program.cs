using System;
using MicroBatchFramework;
using MicroBatchFramework.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Unity;
using Unity.Microsoft.DependencyInjection;
using Domain.GeneralSubDomain;
using Domain.DeliverySubDomain;
using DeliveryUsecase;
using Reactive.Bindings.Notifiers;
using Unity.Injection;
using Query;
using EventStore;
using Utf8Json;
using Utf8Json.Resolvers;
using Utf8Json.FSharp;

namespace DeliveryApp
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            CompositeResolver.RegisterAndSetAsDefault(FSharpResolver.Instance, StandardResolver.Default);

            try
            {
                await BatchHost
                    .CreateDefaultBuilder()
                    .UseServiceProviderFactory<IUnityContainer>(new ServiceProviderFactory(new UnityContainer()))
                    .ConfigureContainer<IUnityContainer>((hostContext, container) =>
                    {
                        container.RegisterInstance<IMessageBroker>(Reactive.Bindings.Notifiers.MessageBroker.Default);
                        container.RegisterInstance<IUnityContainer>(container);

                        container.RegisterType<ICommandBus, CommandBus>();
                        container.RegisterType<I発送本DTOQuery, 発送本DTOQuery>();
                        container.RegisterType<I本を発送するCommand, 本を発送するCommand>();
                        container.RegisterType<I本を発送するCommandHandler, 本を発送するCommandHandler>();
                    })
                    .RunBatchEngineAsync<Scenario>(args);

                return Environment.ExitCode;
            }
            catch (Exception ex) when ((ex is ArgumentException) || (ex is ArgumentNullException))
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            catch (Exception)
            {
                return 9;
            }
        }
    }
}
