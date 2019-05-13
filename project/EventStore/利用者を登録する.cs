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
    public class 利用者を登録する: I利用者を登録する
    {
        private ILogger<I利用者を登録する> Logger { get; }
        private I利用者Factory 利用者Factory { get; }
#if DEBUG
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://localhost:1113");
#else
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://eventstore:1113");
#endif

        public 利用者を登録する(
            ILogger<I利用者を登録する> _logger,
            I利用者Factory _利用者Factory
            )
        {
            Logger = _logger;
            利用者Factory = _利用者Factory;
        }

        public async Task ExecuteAsync(string _苗字, string _名前)
        {
            var _利用者 = 利用者Factory.Create(_苗字, _名前);

            using(var c = EventStoreConnection.Create(
                ConnectionSettings.Create()
                    .SetDefaultUserCredentials(UserCredentials)
                    .UseConsoleLogger()
                , EventStoreUri))
            {
                await c.ConnectAsync();

                var 利用者trans = await c.StartTransactionAsync(_利用者.GUID文字列, ExpectedVersion.NoStream);

                try
                {
                    await 利用者trans.WriteAsync(new EventData(
                        Guid.NewGuid(),
                        typeof(利用者DTO).FullName + "," + typeof(利用者DTO).Assembly.FullName,
                        true,
                        JsonSerializer.Serialize(_利用者.Convert()),
                        new byte[]{}
                    ));

                    await 利用者trans.CommitAsync();
                }
                catch (System.Exception ex)
                {
                    Logger.LogError(ex.Message);

                    利用者trans.Rollback();
                }

                await c.CreatePersistentSubscriptionAsync(_利用者.GUID文字列, typeof(利用者DTO).FullName + "," + typeof(利用者DTO).Assembly.FullName, PersistentSubscriptionSettings.Create(), UserCredentials);

                c.Close();
            }
        }
    }
}
