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
using Domain;
using Application;
using Factory;
using EventStore;
using Query;
using Reactive.Bindings.Notifiers;

namespace MainApp
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                await BatchHost
                    .CreateDefaultBuilder()
                    // .ConfigureServices((context, services) => { })
                    .UseServiceProviderFactory<IUnityContainer>(new ServiceProviderFactory(new UnityContainer()))
                    .ConfigureContainer<IUnityContainer>((hostContext, container) =>
                    {
                        
                        container.RegisterInstance<IMessageBroker>(Reactive.Bindings.Notifiers.MessageBroker.Default);
                        container.RegisterInstance<IUnityContainer>(container);
                        container.RegisterType<I書籍Factory, 書籍Factory>();
                        container.RegisterType<I本Factory, 本Factory>();
                        container.RegisterType<I利用者Factory, 利用者Factory>();

                        container.RegisterType<ICommandBus, CommandBus>();
                        container.RegisterType<I利用者を登録するCommandHandler, 利用者を登録するCommandHandler>();
                        container.RegisterType<I利用者を登録するCommand, 利用者を登録するCommand>();
                        container.RegisterType<I本を登録するCommandHandler, 本を登録するCommandHandler>();
                        container.RegisterType<I本を登録するCommand, 本を登録するCommand>();
                        container.RegisterType<I本が登録されたEvent, 本が登録されたEvent>();
                        container.RegisterType<I本を登録する2CommandHandler, 本を登録する2CommandHandler>();
                        container.RegisterType<I本を登録する2Command, 本を登録する2Command>();
                        container.RegisterType<I本を借りるCommandHandler, 本を借りるCommandHandler>();
                        container.RegisterType<I本を借りるCommand, 本を借りるCommand>();
                        container.RegisterType<I本を延長するCommandHandler, 本を延長するCommandHandler>();
                        container.RegisterType<I本を延長するCommand, 本を延長するCommand>();
                        container.RegisterType<I本を返すCommandHandler, 本を返すCommandHandler>();
                        container.RegisterType<I本を返すCommand, 本を返すCommand>();
                        container.RegisterType<I本を破棄するCommandHandler, 本を破棄するCommandHandler>();
                        container.RegisterType<I本を破棄するCommand, 本を破棄するCommand>();

                        container.RegisterType<Iログイン情報Query, ログイン情報Query>();
                        container.RegisterType<I本の状況Query, 本の状況Query>();
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
