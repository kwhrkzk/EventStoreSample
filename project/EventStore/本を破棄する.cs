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
    public class 本を破棄する: I本を破棄する
    {
        private ILogger<I本を破棄する> Logger { get; }
#if DEBUG
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://localhost:1113");
#else
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://eventstore:1113");
#endif
        public 本を破棄する(ILogger<I本を破棄する> _logger)
        {
            Logger = _logger;
        }

        public async Task ExecuteAsync(本のID _本のID, long _本のEventNumber)
        {
            using(var c = EventStoreConnection.Create(
                ConnectionSettings.Create()
                    .SetDefaultUserCredentials(UserCredentials)
                    .UseConsoleLogger()
                , EventStoreUri))
            {
                await c.ConnectAsync();

                var 本trans = await c.StartTransactionAsync(_本のID.ID文字列, _本のEventNumber);

                try
                {
                    await 本trans.WriteAsync(new EventData(
                        Guid.NewGuid(),
                        typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName,
                        true,
                        new byte[]{},
                        new byte[]{}
                    ));

                    await 本trans.CommitAsync();
                }
                catch (System.Exception ex)
                {
                    Logger.LogError(ex.Message);

                    本trans.Rollback();
                }

                c.Close();
            }
        }
    }
}
