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
                        container.RegisterType<I書籍Factory, 書籍Factory>();
                        container.RegisterType<I本Factory, 本Factory>();
                        container.RegisterType<I利用者Factory, 利用者Factory>();

                        container.RegisterType<I利用者を登録する, 利用者を登録する>();
                        container.RegisterType<I本を登録する, 本を登録する>();
                        container.RegisterType<Iログイン情報Query, ログイン情報Query>();
                        container.RegisterType<I本の状況Query, 本の状況Query>();
                        container.RegisterType<I本を借りる, 本を借りる>();
                        container.RegisterType<I本を延長する, 本を延長する>();
                        container.RegisterType<I本を返す, 本を返す>();
                        container.RegisterType<I本を破棄する, 本を破棄する>();
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
