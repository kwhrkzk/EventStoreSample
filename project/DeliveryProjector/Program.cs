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
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.PersistentSubscriptions;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using Utf8Json;
using EntityFramework;
using Factory;
using DeliveryProjector;

namespace DeliveryProjector
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await BatchHost
                .CreateDefaultBuilder()
                .UseServiceProviderFactory<IUnityContainer>(new ServiceProviderFactory(new UnityContainer()))
                .ConfigureContainer<IUnityContainer>((hostContext, container) =>
                {
                    container.RegisterType<I本Repository, DeliverySubDomain本Repository>();
                    container.RegisterType<I本Factory, DeliverySubDomain本Factory>();
                })
                .RunBatchEngineAsync<Daemon>(args);
        }
    }
}