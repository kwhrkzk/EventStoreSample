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
    public class 本を返すCommand : I本を返すCommand
    {
        public 利用者のID 利用者のID { get; }
        public long 利用者のEventNumber { get; }
        public 本のID 本のID { get; }
        public long 本のEventNumber { get; }

        [InjectionConstructor]public 本を返すCommand() { }

        public 本を返すCommand(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber)
        {
            利用者のID = _利用者のID;
            利用者のEventNumber = _利用者のEventNumber;
            本のID = _本のID;
            本のEventNumber = _本のEventNumber;
        }
        public I本を返すCommand Create(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber)
        => new 本を返すCommand(_利用者のID, _利用者のEventNumber, _本のID, _本のEventNumber);
    }

    public class 本を返すCommandHandler : I本を返すCommandHandler
    {
        private ILogger<I本を返すCommandHandler> Logger { get; }

        public 本を返すCommandHandler(ILogger<I本を返すCommandHandler> _logger)
        {
            Logger = _logger;
        }

        public async Task HandleAsync(ICommand _command)
        {
            var cmd = _command as I本を返すCommand;
            if (cmd == null)
                throw new ArgumentException(nameof(_command), "I本を返すCommand型ではありません。");

            var 利用者EventData = 
                new EventData(
                    Guid.NewGuid(),
                    Domain.RentalSubDomain.Events.User.ReturnedBookVer100,
                    true,
                    JsonSerializer.Serialize(Domain.RentalSubDomain.Events.User.ReturnedBookDTOVer100.Create(cmd.利用者のID.ID文字列, cmd.本のID.ID文字列)),
                    new byte[] { }
                );

            var 本EventData = 
                new EventData(
                    Guid.NewGuid(),
                    Domain.RentalSubDomain.Events.Book.ReturnedBookVer100,
                    true,
                    JsonSerializer.Serialize(Domain.RentalSubDomain.Events.Book.ReturnedBookDTOVer100.Create(cmd.本のID.ID文字列)),
                    new byte[] { }
                );

            using (var c = EventStoreConnection.Create(
                ConnectionSettings.Create().SetDefaultUserCredentials(Connection.UserCredentials())
                , Connection.EventStoreUri()))
            {
                try
                {
                    await c.ConnectAsync();

                    ConditionalWriteResult 利用者Result = await c.ConditionalAppendToStreamAsync(cmd.利用者のID.ID文字列, ExpectedVersion.Any, new [] {利用者EventData});
                    ConditionalWriteResult 本Result = await c.ConditionalAppendToStreamAsync(cmd.本のID.ID文字列, ExpectedVersion.Any, new [] {本EventData});

                    if (ConditionalWriteStatus.Succeeded.Equals(利用者Result.Status) &&
                        ConditionalWriteStatus.Succeeded.Equals(本Result.Status))
                    {
                        await c.AppendToStreamAsync(cmd.利用者のID.ID文字列, ExpectedVersion.Any, new [] {利用者EventData});
                        await c.AppendToStreamAsync(cmd.本のID.ID文字列, ExpectedVersion.Any, new [] {本EventData});
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
