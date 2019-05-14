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
    public class 本を破棄するCommand: I本を破棄するCommand
    {
        public 本のID 本のID { get; }
        public long 本のEventNumber { get; }

        [InjectionConstructor]public 本を破棄するCommand(){}

        public 本を破棄するCommand(本のID _本のID, long _本のEventNumber)
        {
            本のID = _本のID;
            本のEventNumber = _本のEventNumber;
        }
        public I本を破棄するCommand Create(本のID _本のID, long _本のEventNumber) => new 本を破棄するCommand(_本のID, _本のEventNumber);
    }

    public class 本を破棄するCommandHandler : I本を破棄するCommandHandler
    {
        private ILogger<I本を破棄するCommandHandler> Logger { get; }

        public 本を破棄するCommandHandler(ILogger<I本を破棄するCommandHandler> _logger)
        {
            Logger = _logger;
        }

        public async Task HandleAsync(ICommand _command)
        {
            if (_command is I本を破棄するCommand cmd)
            {
                using(var c = EventStoreConnection.Create(
                    ConnectionSettings.Create()
                        .SetDefaultUserCredentials(Connection.UserCredentials())
                        .UseConsoleLogger()
                    , Connection.EventStoreUri()))
                {
                    await c.ConnectAsync();

                    var 本trans = await c.StartTransactionAsync(cmd.本のID.ID文字列, cmd.本のEventNumber);

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
}
