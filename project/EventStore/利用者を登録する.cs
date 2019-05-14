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
    public class 利用者を登録するCommand: I利用者を登録するCommand
    {
        public string 苗字 { get; }
        public string 名前 { get; }
        [InjectionConstructor]public 利用者を登録するCommand(){}

        public 利用者を登録するCommand(string _苗字, string _名前)
        {
                苗字 = _苗字;
                名前 = _名前;
        }
        public I利用者を登録するCommand Create(string _苗字, string _名前) => new 利用者を登録するCommand(_苗字, _名前);
    }

    public class 利用者を登録するCommandHandler: I利用者を登録するCommandHandler
    {
        private ILogger<I利用者を登録するCommandHandler> Logger { get; }
        private I利用者Factory 利用者Factory { get; }

        public 利用者を登録するCommandHandler(
            ILogger<I利用者を登録するCommandHandler> _logger,
            I利用者Factory _利用者Factory
            )
        {
            Logger = _logger;
            利用者Factory = _利用者Factory;
        }

        public async Task HandleAsync(ICommand _command)
        {
            if (_command is I利用者を登録するCommand cmd)
            {
                var _利用者 = 利用者Factory.Create(cmd.苗字, cmd.名前);

                using(var c = EventStoreConnection.Create(
                    ConnectionSettings.Create()
                        .SetDefaultUserCredentials(Connection.UserCredentials())
                        .UseConsoleLogger()
                    , Connection.EventStoreUri()))
                {
                    await c.ConnectAsync();

                    var 利用者trans = await c.StartTransactionAsync(_利用者.GUID文字列, ExpectedVersion.NoStream);

                    try
                    {
                        await 利用者trans.WriteAsync(new EventData(
                            Guid.NewGuid(),
                            typeof(利用者DTO).FullName + "," + typeof(利用者DTO).Assembly.FullName,
                            true,
                            JsonSerializer.Serialize(_利用者.Convert()),
                            new byte[]{}
                        ));

                        await 利用者trans.CommitAsync();
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogError(ex.Message);

                        利用者trans.Rollback();
                    }

                    await c.CreatePersistentSubscriptionAsync(_利用者.GUID文字列, typeof(利用者DTO).FullName + "," + typeof(利用者DTO).Assembly.FullName, PersistentSubscriptionSettings.Create(), Connection.UserCredentials());

                    c.Close();
                }
            }
        }
    }
}
