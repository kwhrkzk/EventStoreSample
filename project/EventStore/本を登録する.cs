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
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;


namespace EventStore
{
    public class 本を登録するCommand : I本を登録するCommand
    {
        public タイトル タイトル { get; }
        public ISBN ISBN { get; }

        [InjectionConstructor] public 本を登録するCommand() { }

        public 本を登録するCommand(タイトル _タイトル, ISBN _isbn)
        {
            タイトル = _タイトル;
            ISBN = _isbn;
        }

        public I本を登録するCommand Create(タイトル _タイトル, ISBN _isbn) => new 本を登録するCommand(_タイトル, _isbn);
    }

    public class 本が登録されたEvent : I本が登録されたEvent
    {
        public 書籍のID 書籍のID { get; }

        [InjectionConstructor] public 本が登録されたEvent() { }

        public 本が登録されたEvent(書籍のID _書籍のID)
        {
            書籍のID = _書籍のID;
        }
        public I本が登録されたEvent Create(書籍のID _書籍のID) => new 本が登録されたEvent(_書籍のID);
    }

    public class 本を登録するCommandHandler : I本を登録するCommandHandler
    {
        private ILogger<I本を登録するCommandHandler> Logger { get; }
        private I本Factory 本Factory { get; }
        private I書籍Factory 書籍Factory { get; }
        private IMessageBroker MessageBroker { get; }
        private I本が登録されたEvent 本が登録されたEvent { get; }

        public 本を登録するCommandHandler(
            ILogger<I本を登録するCommandHandler> _logger,
            I本Factory _本Factory,
            I書籍Factory _書籍Factory,
            IMessageBroker _messageBroker,
            I本が登録されたEvent _本が登録されたEvent
            )
        {
            Logger = _logger;
            本Factory = _本Factory;
            書籍Factory = _書籍Factory;
            MessageBroker = _messageBroker;
            本が登録されたEvent = _本が登録されたEvent;
        }

        public async Task HandleAsync(ICommand _command)
        {
            var cmd = _command as I本を登録するCommand;
            if (cmd == null)
                throw new ArgumentException(nameof(_command), "I本を登録するCommand型ではありません。");

            var _書籍のID = 書籍のID.New();
            var _本のID = 本のID.New();

            var 書籍のEventData =
                new EventData(
                    Guid.NewGuid(),
                    Domain.RentalSubDomain.Events.BookInfo.AddedBookInfoVer100,
                    true,
                    JsonSerializer.Serialize(Domain.RentalSubDomain.Events.BookInfo.AddedBookInfoDTOVer100.Create(_書籍のID.ID文字列, cmd.タイトル.タイトル, cmd.ISBN.ISBN)),
                    new byte[] { }
                );

            var 本のEventData = 
                new EventData(
                    Guid.NewGuid(),
                    Domain.RentalSubDomain.Events.Book.AddedBookVer100,
                    true,
                    JsonSerializer.Serialize(Domain.RentalSubDomain.Events.Book.AddedBookDTOVer100.Create(_本のID.ID文字列, _書籍のID.ID文字列)),
                    new byte[] { }
                );

            using (var c = EventStoreConnection.Create(
                ConnectionSettings.Create().SetDefaultUserCredentials(Connection.UserCredentials())
                , Connection.EventStoreUri()))
            {
                try
                {
                    await c.ConnectAsync();

                    ConditionalWriteResult 書籍Result = await c.ConditionalAppendToStreamAsync(_書籍のID.ID文字列, ExpectedVersion.NoStream, new []{書籍のEventData});
                    ConditionalWriteResult 本Result = await c.ConditionalAppendToStreamAsync(_本のID.ID文字列, ExpectedVersion.NoStream, new []{本のEventData});

                    if (ConditionalWriteStatus.Succeeded.Equals(書籍Result.Status) &&
                        ConditionalWriteStatus.Succeeded.Equals(本Result.Status))
                    {
                        await c.AppendToStreamAsync(_書籍のID.ID文字列, ExpectedVersion.NoStream, 書籍のEventData);
                        await c.AppendToStreamAsync(_本のID.ID文字列, ExpectedVersion.NoStream, 本のEventData);

                        await c.CreatePersistentSubscriptionAsync(_書籍のID.ID文字列, Domain.GeneralSubDomain.Aggregate.BookInfo, PersistentSubscriptionSettings.Create(), Connection.UserCredentials());
                        await c.CreatePersistentSubscriptionAsync(_本のID.ID文字列, Domain.GeneralSubDomain.Aggregate.Book, PersistentSubscriptionSettings.Create(), Connection.UserCredentials());

                        MessageBroker.Publish<I本が登録されたEvent>(本が登録されたEvent.Create(_書籍のID));
                    }
                    else
                    {
                        Logger.LogError($"書籍Result.Status = {Enum.GetName(typeof(ConditionalWriteStatus), 書籍Result.Status)}, 本Result.Status = {Enum.GetName(typeof(ConditionalWriteStatus), 本Result.Status)}");
                    }
                }
                finally
                {
                    c.Close();
                }
            }
        }
    }

    public class 本を登録する2Command : I本を登録する2Command
    {
        public 書籍のID 書籍のID { get; }

        [InjectionConstructor] public 本を登録する2Command() { }

        public 本を登録する2Command(書籍のID _書籍のID)
        {
            書籍のID = _書籍のID;
        }

        public I本を登録する2Command Create(書籍のID _書籍のID) => new 本を登録する2Command(_書籍のID);
    }

    public class 本を登録する2CommandHandler : I本を登録する2CommandHandler
    {
        private ILogger<I本を登録する2CommandHandler> Logger { get; }
        private I本Factory 本Factory { get; }

        public 本を登録する2CommandHandler(
            ILogger<I本を登録する2CommandHandler> _logger,
            I本Factory _本Factory
            )
        {
            Logger = _logger;
            本Factory = _本Factory;
        }

        public async Task HandleAsync(ICommand _command)
        {
            var cmd = _command as I本を登録する2Command;
            if (cmd == null)
                throw new ArgumentException(nameof(_command), "I本を登録する2Command型ではありません。");

            using (var c = EventStoreConnection.Create(
                ConnectionSettings.Create().SetDefaultUserCredentials(Connection.UserCredentials())
                , Connection.EventStoreUri()))
            {
                try
                {
                    await c.ConnectAsync();

                    var _本のID = 本のID.New();

                    var 本trans = await c.AppendToStreamAsync(_本のID.ID文字列, ExpectedVersion.NoStream, new EventData(
                            Guid.NewGuid(),
                            Domain.RentalSubDomain.Events.Book.AddedBookVer100,
                            true,
                            JsonSerializer.Serialize(Domain.RentalSubDomain.Events.Book.AddedBookDTOVer100.Create(_本のID.ID文字列, cmd.書籍のID.ID文字列)),
                            new byte[] { }
                        ));

                    await c.CreatePersistentSubscriptionAsync(_本のID.ID文字列, Domain.GeneralSubDomain.Aggregate.Book, PersistentSubscriptionSettings.Create(), Connection.UserCredentials());
                }
                finally
                {
                    c.Close();
                }
            }
        }
    }
}
