using System;
using System.Text;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System.Threading.Tasks;
using Utf8Json;
using Utf8Json.Resolvers;
using Utf8Json.FSharp;
using Microsoft.Extensions.Logging;
using RentalUsecase;
using Unity;

namespace EventStore
{
    public class 利用者を登録するCommand : I利用者を登録するCommand
    {
        public string 苗字 { get; }
        public string 名前 { get; }
        [InjectionConstructor] public 利用者を登録するCommand() { }

        public 利用者を登録するCommand(string _苗字, string _名前)
        {
            苗字 = _苗字;
            名前 = _名前;
        }
        public I利用者を登録するCommand Create(string _苗字, string _名前)
        => new 利用者を登録するCommand(_苗字, _名前);
    }

    public class 利用者を登録するCommandHandler : I利用者を登録するCommandHandler
    {
        public async Task HandleAsync(ICommand _command)
        {
            var cmd = _command as I利用者を登録するCommand;
            if (cmd == null)
                throw new ArgumentException(nameof(_command), "I利用者を登録するCommand型ではありません。");

            using (var c = EventStoreConnection.Create(
                ConnectionSettings.Create().SetDefaultUserCredentials(Connection.UserCredentials())
                , Connection.EventStoreUri()))
            {
                try
                {
                    var id = 利用者のID.New();

                    await c.ConnectAsync();

                    await c.AppendToStreamAsync(
                        id.ID文字列,
                        ExpectedVersion.NoStream,
                        new EventData(
                            Guid.NewGuid(),
                            Domain.RentalSubDomain.Events.User.AddedUserVer100,
                            true,
                            JsonSerializer.Serialize(Domain.RentalSubDomain.Events.User.AddedUserDTOVer100.Create(id, cmd.苗字, cmd.名前)),
                            new byte[] { }
                        ));

                    await c.CreatePersistentSubscriptionAsync(id.ID文字列, Domain.GeneralSubDomain.Aggregate.User, PersistentSubscriptionSettings.Create(), Connection.UserCredentials());
                }
                finally
                {
                    c.Close();
                }
            }
        }
    }
}
