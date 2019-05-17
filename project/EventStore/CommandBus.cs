using System;
using System.Text;
using System.Collections.Generic;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using Domain.DeliverySubDomain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System.Threading.Tasks;
using Utf8Json;
using Unity;
using Microsoft.Extensions.Logging;
using RentalUsecase;
using DeliveryUsecase;

namespace EventStore
{
    public class CommandBus: ICommandBus
    {
        private Dictionary<Type, Type> TypeConverter { get; } = new Dictionary<Type, Type>()
        {
            { typeof(I利用者を登録するCommand), typeof(I利用者を登録するCommandHandler) },
            { typeof(I本を登録するCommand), typeof(I本を登録するCommandHandler) },
            { typeof(I本を登録する2Command), typeof(I本を登録する2CommandHandler) },
            { typeof(I本を借りるCommand), typeof(I本を借りるCommandHandler) },
            { typeof(I本を延長するCommand), typeof(I本を延長するCommandHandler) },
            { typeof(I本を返すCommand), typeof(I本を返すCommandHandler) },
            { typeof(I本を破棄するCommand), typeof(I本を破棄するCommandHandler) },
            { typeof(I本を発送するCommand), typeof(I本を発送するCommandHandler) },
        };

        private byte[] Serialize(ICommand _command)
        =>  _command switch
            {
                I利用者を登録するCommand cmd => JsonSerializer.Serialize(cmd),
                I本を登録するCommand cmd => JsonSerializer.Serialize(cmd),
                I本を登録する2Command cmd => JsonSerializer.Serialize(cmd),
                I本を借りるCommand cmd => JsonSerializer.Serialize(cmd),
                I本を延長するCommand cmd => JsonSerializer.Serialize(cmd),
                I本を返すCommand cmd => JsonSerializer.Serialize(cmd),
                I本を破棄するCommand cmd => JsonSerializer.Serialize(cmd),
                I本を発送するCommand cmd => JsonSerializer.Serialize(cmd),
                _ => throw new ArgumentException(nameof(_command), "ICommandに対応するICommandHandlerが登録されていません。")
            };
            
        private IUnityContainer Container { get; }
        private Guid StreamId { get; }
        private ILogger<ICommandBus> Logger { get; }

        public CommandBus(IUnityContainer _container, ILogger<ICommandBus> _logger)
        {
            Container = _container;
            StreamId = Guid.Parse("6d1acd3a-9dff-41c2-a448-c7a093c0fda5");
            Logger = _logger;
            Logger.LogInformation("Command Stream Id is " + StreamId.ToString());
        }

        private Type GetHandler(ICommand _command)
        {
            Type value = null;
            foreach(var type in _command.GetType().GetInterfaces())
            {
                if (TypeConverter.TryGetValue(type, out value))
                    return value;
            }

            throw new ArgumentException(nameof(_command), "ICommandに対応するICommandHandlerが登録されていません。");
        }

        public async Task ExecuteAsync(ICommand _command)
        {

            var handlerType = GetHandler(_command);

            var handler = Container.Resolve(handlerType) as ICommandHandler;

             if (handler == null)
                throw new ArgumentException(nameof(_command), "ICommandに対応するICommandHandlerが登録されていません。");

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            await handler.HandleAsync(_command);
            sw.Stop();

            Logger.LogInformation(handlerType.FullName + ":" + System.Text.Encoding.UTF8.GetString((Serialize(_command))));

            using(var c = EventStoreConnection.Create(
                ConnectionSettings.Create()
                    .SetDefaultUserCredentials(Connection.UserCredentials())
                , Connection.EventStoreUri()))
            {
                await c.ConnectAsync();

                await c.AppendToStreamAsync(StreamId.ToString(), ExpectedVersion.Any, new EventData(
                        Guid.NewGuid(),
                        handlerType.FullName + "," + handlerType.Assembly.FullName,
                        true,
                        Serialize(_command),
                        JsonSerializer.Serialize((sw.Elapsed))
                    ));

                c.Close();
            }
        }
    }
}