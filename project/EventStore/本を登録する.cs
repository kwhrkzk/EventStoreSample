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
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;


namespace EventStore
{
    public class 本を登録するCommand: I本を登録するCommand
    {
        public タイトル タイトル { get; }
        public ISBN ISBN { get; }

        [InjectionConstructor]public 本を登録するCommand(){}

        public 本を登録するCommand(タイトル _タイトル, ISBN _isbn)
        {
            タイトル = _タイトル;
            ISBN = _isbn;
        }

        public I本を登録するCommand Create(タイトル _タイトル, ISBN _isbn) => new 本を登録するCommand(_タイトル, _isbn);
    }

    public class 本が登録されたEvent: I本が登録されたEvent
    {
        public 書籍のID 書籍のID { get; }

        [InjectionConstructor]public 本が登録されたEvent(){}

        public 本が登録されたEvent(書籍のID _書籍のID)
        {
            書籍のID = _書籍のID;
        }
        public I本が登録されたEvent Create(書籍のID _書籍のID) => new 本が登録されたEvent(_書籍のID);
    }

    public class 本を登録するCommandHandler: I本を登録するCommandHandler
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
            if (_command is I本を登録するCommand cmd)
            {
                var _書籍 = 書籍Factory.Create(cmd.タイトル, cmd.ISBN);
                var _本 = 本Factory.Create(_書籍.ID);

                using(var c = EventStoreConnection.Create(
                    ConnectionSettings.Create()
                        .SetDefaultUserCredentials(Connection.UserCredentials())
                        .UseConsoleLogger()
                    , Connection.EventStoreUri()))
                {
                    await c.ConnectAsync();

                    var 本trans = await c.StartTransactionAsync(_本.GUID文字列, ExpectedVersion.NoStream);
                    var 書籍trans = await c.StartTransactionAsync(_書籍.GUID文字列, ExpectedVersion.NoStream);

                    try
                    {
                        await 本trans.WriteAsync(new EventData(
                            Guid.NewGuid(),
                            typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName,
                            true,
                            JsonSerializer.Serialize(_本.Convert()),
                            new byte[]{}
                        ));

                        await 書籍trans.WriteAsync(new EventData(
                            Guid.NewGuid(),
                            typeof(書籍DTO).FullName + "," + typeof(書籍DTO).Assembly.FullName,
                            true,
                            JsonSerializer.Serialize(_書籍.Convert()),
                            new byte[]{}
                        ));

                        await 本trans.CommitAsync();
                        await 書籍trans.CommitAsync();
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogError(ex.Message);

                        本trans.Rollback();
                        書籍trans.Rollback();
                    }

                    await c.CreatePersistentSubscriptionAsync(_本.GUID文字列, typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName, PersistentSubscriptionSettings.Create(), Connection.UserCredentials());
                    await c.CreatePersistentSubscriptionAsync(_書籍.GUID文字列, typeof(書籍DTO).FullName + "," + typeof(書籍DTO).Assembly.FullName, PersistentSubscriptionSettings.Create(), Connection.UserCredentials());

                    c.Close();
                }

                MessageBroker.Publish<I本が登録されたEvent>(本が登録されたEvent.Create(_書籍.ID));
            }
        }
    }
    public class 本を登録する2Command: I本を登録する2Command
    {
        public 書籍のID 書籍のID { get; }

        [InjectionConstructor]public 本を登録する2Command(){}

        public 本を登録する2Command(書籍のID _書籍のID)
        {
            書籍のID = _書籍のID;
        }

        public I本を登録する2Command Create(書籍のID _書籍のID) => new 本を登録する2Command(_書籍のID);
    }

    public class 本を登録する2CommandHandler: I本を登録する2CommandHandler
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
            if (_command is I本を登録する2Command cmd)
            {
                var _本 = 本Factory.Create(cmd.書籍のID);

                using(var c = EventStoreConnection.Create(
                    ConnectionSettings.Create()
                        .SetDefaultUserCredentials(Connection.UserCredentials())
                        .UseConsoleLogger()
                    , Connection.EventStoreUri()))
                {
                    await c.ConnectAsync();

                    var 本trans = await c.StartTransactionAsync(_本.GUID文字列, ExpectedVersion.NoStream);

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

                    await c.CreatePersistentSubscriptionAsync(_本.GUID文字列, typeof(本DTO).FullName + "," + typeof(本DTO).Assembly.FullName, PersistentSubscriptionSettings.Create(), Connection.UserCredentials());

                    c.Close();
                }
            }
        }
    }
}
