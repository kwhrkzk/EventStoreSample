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
using Domain.GeneralSubDomain;
using Domain.DeliverySubDomain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.PersistentSubscriptions;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using Utf8Json;
using DeliveryUsecase;
using System.Diagnostics;
using DeliveryProjector;

namespace DeliveryProjector
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

        private IEventStoreConnection Conn { get; set; }

        private I本Repository 本Repository { get; }
        private I本Factory 本Factory { get; }

        public Daemon(
            I本Repository _本Repository,
            I本Factory _本Factory
            )
        {
            本Repository = _本Repository;
            本Factory = _本Factory;
        }

        private List<(EventStorePersistentSubscriptionBase b, PersistentSubscriptionDetails d)> SubscribeList { get; } = new List<(EventStorePersistentSubscriptionBase b, PersistentSubscriptionDetails d)>();

        [Command(new [] { "run", "help", "list" })]
        public async Task Run()
        {
#if DEBUG
#else
            EventStoreIPEndPoint = new IPEndPoint((System.Net.Dns.GetHostEntry("eventstore").AddressList)[0], 2113);
#endif

            Conn = await GetConnectionAsync();

            try
            {
                var ps = new PersistentSubscriptionsManager(new Mylogger(Context.Logger), EventStoreIPEndPoint, TimeSpan.FromMinutes(1));

                while (!Context.CancellationToken.IsCancellationRequested)
                {
                    List<PersistentSubscriptionDetails> list = await ps.List(UserCredentials);

                    foreach (PersistentSubscriptionDetails detail in list)
                    {
                        if (Judge(detail.GroupName) == false)
                            continue;

                        if (SubscribeList.Exists(tpl => tpl.d.EventStreamId.Equals(detail.EventStreamId)))
                            continue;

                        Context.Logger.LogInformation(detail.EventStreamId);

                        EventStorePersistentSubscriptionBase _base =
                            Conn.ConnectToPersistentSubscription(
                                detail.EventStreamId, 
                                detail.GroupName, 
                                Subscribed,
                                (_base, _reason, ex) => Context.Logger.LogError($"Reason = {Enum.GetName(typeof(SubscriptionDropReason), _reason)}: {ex.Message}"),
                                UserCredentials,
                                1,
                                false
                                );
                        SubscribeList.Add((_base, detail));

                        StreamEventsSlice ret = await Conn.ReadStreamEventsForwardAsync(detail.EventStreamId, 0, (int)(detail.TotalItemsProcessed + 1) , false);
                        foreach (ResolvedEvent _event in ret.Events)
                        {
                            await ExecuteAsync(_event.Event);
                        }
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
                Conn.Close();
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

        private async Task Subscribed(EventStorePersistentSubscriptionBase _base, ResolvedEvent _event, int? _)
        {
            Context.Logger.LogInformation(_event.Event.EventStreamId);

            await ExecuteAsync(_event.Event);
        }

        private bool Judge(string _groupname)
        => _groupname switch {
            Domain.GeneralSubDomain.Aggregate.Book => true,
            _ => false
        };

        private async Task ExecuteAsync(RecordedEvent _event)
        {
            var x = _event.EventType switch {
                Domain.RentalSubDomain.Events.Book.LendedBookVer100 => await LendedBookVer100Async(_event),
                Domain.RentalSubDomain.Events.Book.ReturnedBookVer100 => ReturnedBookVer100(_event),
                Domain.DeliverySubDomain.Events.Book.ShippedBookVer100 => ShippedBookVer100(_event),
                _ => 0
            };
        }

        private async Task<int> LendedBookVer100Async(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.Book.LendedBookDTOVer100>(_event.Data);

            EventReadResult ret = await Conn.ReadEventAsync(dto.id, 0, false);

            var first = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.Book.AddedBookDTOVer100>(ret.Event.Value.Event.Data);
            
            var _本 = 本Factory.Create(dto.id, first.book_id, dto.user_id);

            本Repository.Insert(_本);

            using (var db = new DeliveryProjectorContext())
            {
                if (db.本一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 本Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }
            
            return 0;
        }

        private int ShippedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.DeliverySubDomain.Events.Book.ShippedBookDTOVer100>(_event.Data);

            本Repository.Update(_event.EventNumber, dto);

            using (var db = new DeliveryProjectorContext())
            {
                if (db.本一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 本Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }
            
            return 0;
        }

        private int ReturnedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.Book.ReturnedBookDTOVer100>(_event.Data);

            本Repository.Delete(dto.id);

            using (var db = new DeliveryProjectorContext())
            {
                if (db.本一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 本Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }
    }
}