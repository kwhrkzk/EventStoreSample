using System;
using System.Text;
using Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System.Threading.Tasks;
using Utf8Json;
using Microsoft.Extensions.Logging;
using Application;
using Unity;

namespace EventStore
{
    public class 本を延長するCommand: I本を延長するCommand
    {
        public 本のID 本のID { get; }
        public long 本のEventNumber { get; }
        public 貸出期間 貸出期間 { get; }

        [InjectionConstructor]public 本を延長するCommand(){}
        public 本を延長するCommand(本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間)
        {
            本のID = _本のID;
            本のEventNumber = _本のEventNumber;
            貸出期間 = _貸出期間;
        }
        public I本を延長するCommand Create(本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間)
        => new 本を延長するCommand(_本のID, _本のEventNumber, _貸出期間);
    }

    public class 本を延長するCommandHandler: I本を延長するCommandHandler
    {
        private ILogger<I本を延長するCommandHandler> Logger { get; }
        public 本を延長するCommandHandler(ILogger<I本を延長するCommandHandler> _logger)
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

        public async Task HandleAsync(ICommand _command)
        {
            if (_command is I本を延長するCommand cmd)
            {
                using(var c = EventStoreConnection.Create(
                    ConnectionSettings.Create()
                        .SetDefaultUserCredentials(Connection.UserCredentials())
                        .UseConsoleLogger()
                    , Connection.EventStoreUri()))
                {
                    await c.ConnectAsync();

                    var _本 = await 本を延長するAsync(c, cmd.本のID, cmd.本のEventNumber, cmd.貸出期間);

                    var 本trans = await c.StartTransactionAsync(cmd.本のID.ID文字列, cmd.本のEventNumber);

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
}
