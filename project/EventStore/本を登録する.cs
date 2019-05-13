using System;
using System.Text;
using Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System.Threading.Tasks;
using Utf8Json;
using Microsoft.Extensions.Logging;
using Application;

namespace EventStore
{
    public class 本を登録する: I本を登録する
    {
        private ILogger<I本を登録する> Logger { get; }
        private I本Factory 本Factory { get; }
        private I書籍Factory 書籍Factory { get; }
#if DEBUG
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://localhost:1113");
#else
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://eventstore:1113");
#endif
        public 本を登録する(
            ILogger<I本を登録する> _logger,
            I本Factory _本Factory,
            I書籍Factory _書籍Factory
            )
        {
            Logger = _logger;
            本Factory = _本Factory;
            書籍Factory = _書籍Factory;
        }

        public async Task<書籍のID> ExecuteAsync(タイトル _タイトル, ISBN _isbn)
        {
            var _書籍 = 書籍Factory.Create(_タイトル, _isbn);
            var _本 = 本Factory.Create(_書籍.ID);

            using(var c = EventStoreConnection.Create(
                ConnectionSettings.Create()
                    .SetDefaultUserCredentials(UserCredentials)
                    .UseConsoleLogger()
                , EventStoreUri))
            {
                await c.ConnectAsync();

                var 本trans = await c.StartTransactionAsync(_本.GUID文字列, ExpectedVersion.NoStream);
                var 書籍trans = await c.StartTransactionAsync(_書籍.GUID文字列, ExpectedVersion.NoStream);

                try
                {
                    await 本trans.WriteAsync(new EventData(
                        Guid.NewGuid(),
                        typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName,
                        true,
                        JsonSerializer.Serialize(_本.Convert()),
                        new byte[]{}
                    ));

                    await 書籍trans.WriteAsync(new EventData(
                        Guid.NewGuid(),
                        typeof(書籍DTO).FullName + "," + typeof(書籍DTO).Assembly.FullName,
                        true,
                        JsonSerializer.Serialize(_書籍.Convert()),
                        new byte[]{}
                    ));

                    await 本trans.CommitAsync();
                    await 書籍trans.CommitAsync();
                }
                catch (System.Exception ex)
                {
                    Logger.LogError(ex.Message);

                    本trans.Rollback();
                    書籍trans.Rollback();
                }

                await c.CreatePersistentSubscriptionAsync(_本.GUID文字列, typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName, PersistentSubscriptionSettings.Create(), UserCredentials);
                await c.CreatePersistentSubscriptionAsync(_書籍.GUID文字列, typeof(書籍DTO).FullName + "," + typeof(書籍DTO).Assembly.FullName, PersistentSubscriptionSettings.Create(), UserCredentials);

                c.Close();
            }

            return _書籍.ID;
        }

        public async Task ExecuteAsync(書籍のID _書籍のID)
        {
            var _本 = 本Factory.Create(_書籍のID);

            using(var c = EventStoreConnection.Create(
                ConnectionSettings.Create()
                    .SetDefaultUserCredentials(UserCredentials)
                    .UseConsoleLogger()
                , EventStoreUri))
            {
                await c.ConnectAsync();

                var 本trans = await c.StartTransactionAsync(_本.GUID文字列, ExpectedVersion.NoStream);

                try
                {
                    await 本trans.WriteAsync(new EventData(
                        Guid.NewGuid(),
                        typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName,
                        true,
                        JsonSerializer.Serialize(_本.Convert()),
                        new byte[]{}
                    ));

                    await 本trans.CommitAsync();
                }
                catch (System.Exception ex)
                {
                    Logger.LogError(ex.Message);

                    本trans.Rollback();
                }

                await c.CreatePersistentSubscriptionAsync(_本.GUID文字列, typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName, PersistentSubscriptionSettings.Create(), UserCredentials);

                c.Close();
            }
        }
    }
}
