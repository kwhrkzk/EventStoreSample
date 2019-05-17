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
    public class 本を借りるCommand : I本を借りるCommand
    {
        public 利用者のID 利用者のID { get; }
        public long 利用者のEventNumber { get; }
        public 本のID 本のID { get; }
        public long 本のEventNumber { get; }
        public 貸出期間 貸出期間 { get; }

        [InjectionConstructor] public 本を借りるCommand() { }

        public 本を借りるCommand(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間)
        {
            利用者のID = _利用者のID;
            利用者のEventNumber = 利用者のEventNumber;
            本のID = _本のID;
            本のEventNumber = _本のEventNumber;
            貸出期間 = _貸出期間;
        }
        public I本を借りるCommand Create(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間)
        => new 本を借りるCommand(_利用者のID, _利用者のEventNumber, _本のID, _本のEventNumber, _貸出期間);
    }

    public class 本を借りるCommandHandler : I本を借りるCommandHandler
    {
        private ILogger<I本を借りるCommandHandler> Logger { get; }
        public 本を借りるCommandHandler(ILogger<I本を借りるCommandHandler> _logger)
        {
            Logger = _logger;
        }

        public async Task HandleAsync(ICommand _command)
        {
            var cmd = _command as I本を借りるCommand;
            if (cmd == null)
                throw new ArgumentException(nameof(_command), "I本を借りるCommand型ではありません。");

            var 利用者EventData =
                new EventData(
                    Guid.NewGuid(),
                    Domain.RentalSubDomain.Events.User.LendedBookVer100,
                    true,
                    JsonSerializer.Serialize(Domain.RentalSubDomain.Events.User.LendedBookDTOVer100.Create(cmd.利用者のID.ID文字列, cmd.本のID.ID文字列)),
                    new byte[] { }
                );

            var 本EventData =
                new EventData(
                    Guid.NewGuid(),
                    Domain.RentalSubDomain.Events.Book.LendedBookVer100,
                    true,
                    JsonSerializer.Serialize(
                        Domain.RentalSubDomain.Events.Book.LendedBookDTOVer100.Create(
                            cmd.本のID.ID文字列, 
                            cmd.利用者のID.ID文字列, 
                            cmd.貸出期間.貸出期間自DateTime, 
                            cmd.貸出期間.貸出期間至DateTime
                        )
                    ),
                    new byte[] { }
                );

            using (var c = EventStoreConnection.Create(
                ConnectionSettings.Create()
                    .SetDefaultUserCredentials(Connection.UserCredentials())
                    // .UseConsoleLogger()
                , Connection.EventStoreUri()))
            {
                try
                {
                    await c.ConnectAsync();

                    ConditionalWriteResult 利用者Result = await c.ConditionalAppendToStreamAsync(cmd.利用者のID.ID文字列, ExpectedVersion.Any, new []{利用者EventData});
                    ConditionalWriteResult 本Result = await c.ConditionalAppendToStreamAsync(cmd.本のID.ID文字列, ExpectedVersion.Any, new []{本EventData});

                    if (ConditionalWriteStatus.Succeeded.Equals(利用者Result.Status) &&
                        ConditionalWriteStatus.Succeeded.Equals(本Result.Status))
                    {
                        await c.AppendToStreamAsync(cmd.利用者のID.ID文字列, ExpectedVersion.Any, new []{利用者EventData});
                        await c.AppendToStreamAsync(cmd.本のID.ID文字列, ExpectedVersion.Any, new []{本EventData});
                    }
                    else
                    {
                        Logger.LogError($"利用者Result.Status = {Enum.GetName(typeof(ConditionalWriteStatus), 利用者Result.Status)}, 本Result.Status = {Enum.GetName(typeof(ConditionalWriteStatus), 本Result.Status)}");
                    }
                }
                finally
                {
                    c.Close();
                }
            }
        }
    }
}
