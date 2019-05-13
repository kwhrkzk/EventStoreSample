using System;
using System.Linq;
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
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.PersistentSubscriptions;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using Utf8Json;
using Application;
using System.Diagnostics;

namespace WatchApp
{
    public class Daemon : BatchBase
    {
#if DEBUG
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://localhost:1113");
        private IPEndPoint EventStoreIPEndPoint { get; } = new IPEndPoint(
                    IPAddress.Parse("127.0.0.1")/*Dns.GetHostAddresses("localhost")[0]*/
                    , 2113);
#else
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://eventstore:1113");
        private IPEndPoint EventStoreIPEndPoint { get; set; }
#endif

        private I本Repository 本Repository { get; }
        private I書籍Repository 書籍Repository { get; }
        private I利用者Repository 利用者Repository { get; }

        public Daemon(
            I本Repository _本Repository,
            I書籍Repository _書籍Repository,
            I利用者Repository _利用者Repository
            )
        {
            本Repository = _本Repository;
            書籍Repository = _書籍Repository;
            利用者Repository = _利用者Repository;
        }

        private List<(EventStorePersistentSubscriptionBase b, PersistentSubscriptionDetails d)> SubscribeList { get; } = new List<(EventStorePersistentSubscriptionBase b, PersistentSubscriptionDetails d)>();

        [Command(new [] { "run", "help", "list" })]
        public async Task Run()
        {
#if DEBUG
#else
            EventStoreIPEndPoint = new IPEndPoint((System.Net.Dns.GetHostEntry("eventstore").AddressList)[0], 2113);
#endif

            IEventStoreConnection conn = await GetConnectionAsync();

            try
            {
                var ps = new PersistentSubscriptionsManager(new Mylogger(Context.Logger), EventStoreIPEndPoint, TimeSpan.FromMinutes(1));

                while (!Context.CancellationToken.IsCancellationRequested)
                {
                    List<PersistentSubscriptionDetails> list = await ps.List(UserCredentials);

                    foreach (var detail in list)
                    {
                        if (SubscribeList.Exists(tpl => tpl.d.EventStreamId.Equals(detail.EventStreamId)))
                            continue;

                        Context.Logger.LogInformation(detail.EventStreamId);

                        EventStorePersistentSubscriptionBase _base = conn.ConnectToPersistentSubscription(detail.EventStreamId, detail.GroupName, Subscribed);
                        SubscribeList.Add((_base, detail));

                        EventReadResult ret = await conn.ReadEventAsync(detail.EventStreamId, detail.LastKnownEventNumber, false);

                         Execute(ret.Event.Value.Event);
                    }

                    // wait for next time
                    await Task.Delay(TimeSpan.FromSeconds(1), Context.CancellationToken);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Context.Logger.LogError(ex.Message);
            }
            finally
            {
                // you can write cleanup code here.
            }
        }

        private async Task<IEventStoreConnection> GetConnectionAsync()
        {
            while (!Context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), Context.CancellationToken);

                try
                {
                    var conn = EventStoreConnection.Create(
                            ConnectionSettings.Create()
                            .SetDefaultUserCredentials(UserCredentials)
                            .UseConsoleLogger(),
                            EventStoreUri);

                    await conn.ConnectAsync();

                    return conn;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Context.Logger.LogError(ex.Message);
                }
            }

            return null;
        }

        private Task Subscribed(EventStorePersistentSubscriptionBase _base, ResolvedEvent _event, int? _)
        {
            Context.Logger.LogInformation(_event.Event.EventStreamId);

            Execute(_event.Event);

            return Task.CompletedTask;
        }

        private void Execute(RecordedEvent _event)
        {
            var obj = JsonSerializer.NonGeneric.Deserialize(Type.GetType(_event.EventType, true), _event.Data);

            var x = obj switch {
                本DTO dto => Execute本DTO(_event, dto.Convert()),
                書籍DTO dto => Execute書籍DTO(_event, dto.Convert()),
                利用者DTO dto => Execute利用者DTO(_event, dto.Convert()),
                _ => throw new ArgumentException("ArgumentException: " + _event.EventType)
            };
        }
        private int Execute本DTO(RecordedEvent _event, 本 _本)
        {
            if (_event.Data.Any() == false)
                本Repository.Delete(本のID.Create(_event.EventStreamId));
            else
            {
                本Repository.Upsert(_event.EventNumber, _本);

                using (var db = new MyContext())
                {
                    if (db.本一覧.SingleOrDefault(x => x.Id.Equals(_本.GUID)) is 本Entity x)
                        Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
                }
            }
            
            return 0;
        }
        private int Execute書籍DTO(RecordedEvent _event, 書籍 _書籍)
        {
            書籍Repository.Upsert(_event.EventNumber, _書籍);

            using (var db = new MyContext())
            {
                if (db.書籍一覧.SingleOrDefault(x => x.Id.Equals(_書籍.GUID)) is 書籍Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }
        private int Execute利用者DTO(RecordedEvent _event, 利用者 _利用者)
        {
            利用者Repository.Upsert(_event.EventNumber, _利用者);

            using (var db = new MyContext())
            {
                if (db.利用者一覧.SingleOrDefault(x => x.Id.Equals(_利用者.GUID)) is 利用者Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }
    }
}