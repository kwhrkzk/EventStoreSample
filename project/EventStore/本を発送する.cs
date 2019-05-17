using System;
using System.Text;
using Domain.GeneralSubDomain;
using Domain.DeliverySubDomain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System.Threading.Tasks;
using Utf8Json;
using Microsoft.Extensions.Logging;
using DeliveryUsecase;
using Unity;

namespace EventStore
{
    public class 本を発送するCommand : I本を発送するCommand
    {
        public 本のID 本のID { get; }
        public 発送期間 発送期間 { get; }
        [InjectionConstructor]public 本を発送するCommand() { }

        public 本を発送するCommand(本のID _本のID, 発送期間 _発送期間)
        {
            本のID = _本のID;
            発送期間 = _発送期間;
        }
        public I本を発送するCommand Create(本のID _本のID, 発送期間 _発送期間)
        => new 本を発送するCommand(_本のID, _発送期間);
    }

    public class 本を発送するCommandHandler : I本を発送するCommandHandler
    {
        public async Task HandleAsync(ICommand _command)
        {
            var cmd = _command as I本を発送するCommand;
            if (cmd == null)
                throw new ArgumentException(nameof(_command), "I本を発送するCommand型ではありません。");

            using (var c = EventStoreConnection.Create(
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
                            Domain.DeliverySubDomain.Events.Book.ShippedBookVer100,
                            true,
                            JsonSerializer.Serialize(
                                Domain.DeliverySubDomain.Events.Book.ShippedBookDTOVer100.Create(
                                    cmd.本のID.ID文字列, 
                                    cmd.発送期間.発送期間自DateTime, 
                                    cmd.発送期間.発送期間至DateTime
                                )
                            ),
                            new byte[] { }
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
