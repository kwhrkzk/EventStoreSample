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
using RentalUsecase;
using System.Diagnostics;
using RentalProjector;

namespace RentalProjector
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
        private I利用者Factory 利用者Factory { get; }
        private I本Factory 本Factory { get; }
        private I書籍Factory 書籍Factory { get; }

        public Daemon(
            I本Repository _本Repository,
            I書籍Repository _書籍Repository,
            I利用者Repository _利用者Repository,
            I利用者Factory _利用者Factory,
            I本Factory _本Factory,
            I書籍Factory _書籍Factory
            )
        {
            本Repository = _本Repository;
            書籍Repository = _書籍Repository;
            利用者Repository = _利用者Repository;
            利用者Factory = _利用者Factory;
            本Factory = _本Factory;
            書籍Factory = _書籍Factory;
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
                        if (Judge(detail.GroupName) == false)
                            continue;

                        if (SubscribeList.Exists(tpl => tpl.d.EventStreamId.Equals(detail.EventStreamId)))
                            continue;

                        Context.Logger.LogInformation(detail.EventStreamId);

                        EventStorePersistentSubscriptionBase _base =
                            conn.ConnectToPersistentSubscription(
                                detail.EventStreamId, 
                                detail.GroupName, 
                                Subscribed,
                                (_base, _reason, ex) => Context.Logger.LogError($"Reason = {Enum.GetName(typeof(SubscriptionDropReason), _reason)}: {ex.Message}"),
                                UserCredentials,
                                1,
                                false
                                );

                        SubscribeList.Add((_base, detail));

                        StreamEventsSlice ret = await conn.ReadStreamEventsForwardAsync(detail.EventStreamId, 0, (int)(detail.TotalItemsProcessed + 1) , false);
                        foreach (ResolvedEvent _event in ret.Events)
                        {
                            Execute(_event.Event);
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
                conn.Close();
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

        private bool Judge(string _groupname)
        => _groupname switch {
            Domain.GeneralSubDomain.Aggregate.User => true,
            Domain.GeneralSubDomain.Aggregate.Book => true,
            Domain.GeneralSubDomain.Aggregate.BookInfo => true,
            _ => false
        };

        private void Execute(RecordedEvent _event)
        {
            var x = _event.EventType switch {
                Domain.RentalSubDomain.Events.User.AddedUserVer100 => AddedUserVer100(_event),
                Domain.RentalSubDomain.Events.User.LendedBookVer100 => UserLendedBookVer100(_event),
                Domain.RentalSubDomain.Events.User.ReturnedBookVer100 => UserReturnedBookVer100(_event),
                Domain.RentalSubDomain.Events.BookInfo.AddedBookInfoVer100 => AddedBookInfoVer100(_event),
                Domain.RentalSubDomain.Events.Book.AddedBookVer100 => AddedBookVer100(_event),
                Domain.RentalSubDomain.Events.Book.LendedBookVer100 => BookLendedBookVer100(_event),
                Domain.RentalSubDomain.Events.Book.ExtendedBookVer100 => ExtendedBookVer100(_event),
                Domain.RentalSubDomain.Events.Book.ReturnedBookVer100 => BookReturnedBookVer100(_event),
                Domain.RentalSubDomain.Events.Book.DestroyedBookVer100 => DestroyedBookVer100(_event),
                _ => 0
            };
        }

        private int AddedBookInfoVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.BookInfo.AddedBookInfoDTOVer100>(_event.Data);
            var _書籍 = 書籍Factory.Create(dto.id, dto.title, dto.isbn);

           書籍Repository.Upsert(_event.EventNumber, _書籍);

            using (var db = new RentalProjectorContext())
            {
                if (db.書籍一覧.SingleOrDefault(x => x.Id.Equals(_書籍.GUID)) is 書籍Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }

        private int AddedUserVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.User.AddedUserDTOVer100>(_event.Data);
            var _利用者 = 利用者Factory.Create(dto.id, dto.last_name, dto.first_name);

            利用者Repository.Insert(_event.EventNumber, _利用者);

            using (var db = new RentalProjectorContext())
            {
                if (db.利用者一覧.SingleOrDefault(x => x.Id.Equals(_利用者.GUID)) is 利用者Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }

        private int UserLendedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.User.LendedBookDTOVer100>(_event.Data);

            利用者Repository.Update(_event.EventNumber, dto);

            using (var db = new RentalProjectorContext())
            {
                if (db.利用者一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 利用者Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }

        private int UserReturnedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.User.ReturnedBookDTOVer100>(_event.Data);

            利用者Repository.Update(_event.EventNumber, dto);

            using (var db = new RentalProjectorContext())
            {
                if (db.利用者一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 利用者Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }

        private int AddedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.Book.AddedBookDTOVer100>(_event.Data);
            var _本 = 本Factory.Create(dto.id, dto.book_id);

            本Repository.Insert(_event.EventNumber, _本);

            using (var db = new RentalProjectorContext())
            {
                if (db.本一覧.SingleOrDefault(x => x.Id.Equals(_本.GUID)) is 本Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }
            
            return 0;
        }

        private int BookLendedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.Book.LendedBookDTOVer100>(_event.Data);

            本Repository.Update(_event.EventNumber, dto);

            using (var db = new RentalProjectorContext())
            {
                if (db.本一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 本Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }
            
            return 0;
        }

        private int ExtendedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.Book.ExtendedBookDTOVer100>(_event.Data);

            本Repository.Update(_event.EventNumber, dto);

            using (var db = new RentalProjectorContext())
            {
                if (db.本一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 本Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }
            
            return 0;
        }

        private int BookReturnedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.Book.ReturnedBookDTOVer100>(_event.Data);

            本Repository.Update(_event.EventNumber, dto);

            using (var db = new RentalProjectorContext())
            {
                if (db.本一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 本Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }

        private int DestroyedBookVer100(RecordedEvent _event)
        {
            var dto = JsonSerializer.Deserialize<Domain.RentalSubDomain.Events.Book.DestroyedBookDTOVer100>(_event.Data);

            本Repository.Delete(dto.id);

            using (var db = new RentalProjectorContext())
            {
                if (db.本一覧.SingleOrDefault(x => x.Id.Equals(Guid.Parse(dto.id))) is 本Entity x)
                    Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(x))));
            }

            return 0;
        }
    }
}