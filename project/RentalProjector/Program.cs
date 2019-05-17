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
using Domain.RentalSubDomain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.PersistentSubscriptions;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using Utf8Json;
using Utf8Json.Resolvers;
using Utf8Json.FSharp;
using EntityFramework;
using Factory;
using RentalProjector;

namespace RentalProjector
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            CompositeResolver.RegisterAndSetAsDefault(FSharpResolver.Instance, StandardResolver.Default);

            await BatchHost
                .CreateDefaultBuilder()
                .UseServiceProviderFactory<IUnityContainer>(new ServiceProviderFactory(new UnityContainer()))
                .ConfigureContainer<IUnityContainer>((hostContext, container) =>
                {
                    
                    container.RegisterType<I本Repository, RentalSubDomain本Repository>();
                    container.RegisterType<I書籍Repository, 書籍Repository>();
                    container.RegisterType<I利用者Repository, 利用者Repository>();
                    container.RegisterType<I利用者Factory, 利用者Factory>();
                    container.RegisterType<I本Factory, RentalSubDomain本Factory>();
                    container.RegisterType<I書籍Factory, 書籍Factory>();
                })
                .RunBatchEngineAsync<Daemon>(args);
        }
    }
}