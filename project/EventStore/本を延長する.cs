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
    public class 本を延長する: I本を延長する
    {
        private ILogger<I本を延長する> Logger { get; }
#if DEBUG
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://localhost:1113");
#else
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://eventstore:1113");
#endif
        public 本を延長する(ILogger<I本を延長する> _logger)
        {
            Logger = _logger;
        }

        private async Task<本> 本を延長するAsync(IEventStoreConnection c, 本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間)
        {
            var result = await c.ReadEventAsync(_本のID.ID文字列, _本のEventNumber, true);

            var _本 = JsonSerializer.Deserialize<本DTO>(result.Event.Value.Event.Data).Convert();
            
            _本.貸出期間 = _貸出期間;

            return _本;
        }

        public async Task ExecuteAsync(本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間)
        {
            using(var c = EventStoreConnection.Create(
                ConnectionSettings.Create()
                    .SetDefaultUserCredentials(UserCredentials)
                    .UseConsoleLogger()
                , EventStoreUri))
            {
                await c.ConnectAsync();

                var _本 = await 本を延長するAsync(c, _本のID, _本のEventNumber, _貸出期間);

                var 本trans = await c.StartTransactionAsync(_本.GUID文字列, _本のEventNumber);

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

                c.Close();
            }
        }
    }
}
