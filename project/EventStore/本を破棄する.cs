using System;
using System.Text;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System.Threading.Tasks;
using Utf8Json;
using Microsoft.Extensions.Logging;
using RentalUsecase;
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
        public I本を破棄するCommand Create(本のID _本のID, long _本のEventNumber)
        => new 本を破棄するCommand(_本のID, _本のEventNumber);
    }

    public class 本を破棄するCommandHandler : I本を破棄するCommandHandler
    {
        public async Task HandleAsync(ICommand _command)
        {
            var cmd = _command as I本を破棄するCommand;
            if (cmd == null)
                throw new ArgumentException(nameof(_command), "I本を破棄するCommand型ではありません。");

            using(var c = EventStoreConnection.Create(
                ConnectionSettings.Create().SetDefaultUserCredentials(Connection.UserCredentials())
                , Connection.EventStoreUri()))
            {
                try
                {
                    await c.ConnectAsync();

                    await c.AppendToStreamAsync(
                        cmd.本のID.ID文字列, 
                        ExpectedVersion.StreamExists, 
                        new EventData(
                            Guid.NewGuid(),
                            Domain.RentalSubDomain.Events.Book.DestroyedBookVer100,
                            true,
                            JsonSerializer.Serialize(Domain.RentalSubDomain.Events.Book.DestroyedBookDTOVer100.Create(cmd.本のID.ID文字列)),
                            new byte[]{}
                        ));
                }
                finally
                {
                    c.Close();
                }
            }
        }
    }
}
