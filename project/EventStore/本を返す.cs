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
    public class 本を返す: I本を返す
    {
        private ILogger<I本を返す> Logger { get; }
#if DEBUG
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://localhost:1113");
#else
        private UserCredentials UserCredentials { get; } = new UserCredentials("admin", "changeit");
        private Uri EventStoreUri { get; } = new Uri("tcp://eventstore:1113");
#endif
        public 本を返す(ILogger<I本を返す> _logger)
        {
            Logger = _logger;
        }

        private async Task<本> 本を返すAsync(IEventStoreConnection c, 本のID _本のID, long _本のEventNumber)
        {
            var result = await c.ReadEventAsync(_本のID.ID文字列, _本のEventNumber, true);

            var _本 = JsonSerializer.Deserialize<本DTO>(result.Event.Value.Event.Data).Convert();
            
            _本.貸出期間 = 貸出期間.Empty();
            _本.利用者のID = 利用者のID.Empty();

            return _本;
        }

        private async Task<利用者> 本を返すAsync(IEventStoreConnection c, 利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID)
        {
            var result = await c.ReadEventAsync(_利用者のID.ID文字列, _利用者のEventNumber, true);

            var _利用者 = JsonSerializer.Deserialize<利用者DTO>(result.Event.Value.Event.Data).Convert();

            _利用者.本を返す(_本のID);

            return _利用者;
        }

        public async Task ExecuteAsync(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber)
        {
            using(var c = EventStoreConnection.Create(
                ConnectionSettings.Create()
                    .SetDefaultUserCredentials(UserCredentials)
                    .UseConsoleLogger()
                , EventStoreUri))
            {
                await c.ConnectAsync();

                var _利用者 = await 本を返すAsync(c, _利用者のID, _利用者のEventNumber, _本のID);
                var _本 = await 本を返すAsync(c, _本のID, _本のEventNumber);

                var 利用者trans = await c.StartTransactionAsync(_利用者.GUID文字列, _利用者のEventNumber);
                var 本trans = await c.StartTransactionAsync(_本.GUID文字列, _本のEventNumber);

                try
                {
                    await 利用者trans.WriteAsync(new EventData(
                        Guid.NewGuid(),
                        typeof(利用者DTO).FullName + "," + typeof(利用者DTO).Assembly.FullName,
                        true,
                        JsonSerializer.Serialize(_利用者.Convert()),
                        new byte[]{}
                    ));

                    await 本trans.WriteAsync(new EventData(
                        Guid.NewGuid(),
                        typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName,
                        true,
                        JsonSerializer.Serialize(_本.Convert()),
                        new byte[]{}
                    ));

                    await 利用者trans.CommitAsync();
                    await 本trans.CommitAsync();
                }
                catch (System.Exception ex)
                {
                    Logger.LogError(ex.Message);

                    利用者trans.Rollback();
                    本trans.Rollback();
                }

                c.Close();
            }
        }
    }
}
