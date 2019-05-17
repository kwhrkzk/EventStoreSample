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
        public async Task HandleAsync(ICommand _command)
        {
            var cmd = _command as I本を延長するCommand;
            if (cmd == null)
                throw new ArgumentException(nameof(_command), "I本を延長するCommand型ではありません。");

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
                            Domain.RentalSubDomain.Events.Book.ExtendedBookVer100,
                            true,
                            JsonSerializer.Serialize(
                                Domain.RentalSubDomain.Events.Book.ExtendedBookDTOVer100.Create(
                                    cmd.本のID.ID文字列, 
                                    cmd.貸出期間.貸出期間自DateTime, 
                                    cmd.貸出期間.貸出期間至DateTime
                                )
                            ),
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
