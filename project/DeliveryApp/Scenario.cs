using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using MicroBatchFramework;
using MicroBatchFramework.Logging;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Domain.GeneralSubDomain;
using Domain.DeliverySubDomain;
using DeliveryUsecase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Utf8Json;

namespace DeliveryApp
{
    public class Scenario: BatchBase
    {
        private IMessageBroker MessageBroker { get; }
        private ICommandBus CommandBus { get; }
        private I発送本DTOQuery 発送本DTOQuery { get; }
        private I本を発送するCommand 本を発送するCommand { get; }

        public Scenario(
            IMessageBroker _messageBroker,
            ICommandBus _commandBus,
            I発送本DTOQuery _発送本DTOQuery,
            I本を発送するCommand _本を発送するCommand
            )
        {
            MessageBroker = _messageBroker;
            CommandBus = _commandBus;
            発送本DTOQuery = _発送本DTOQuery;
            本を発送するCommand = _本を発送するCommand;
        }

        static List<Type> GetBatchTypes()
        {
            List<Type> batchBaseTypes = new List<Type>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.FullName.StartsWith("System") || asm.FullName.StartsWith("Microsoft.Extensions")) continue;

                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                foreach (var item in types)
                {
                    if (typeof(BatchBase).IsAssignableFrom(item) && item != typeof(BatchBase))
                    {
                        batchBaseTypes.Add(item);
                    }
                }
            }

            return batchBaseTypes;
        }

        [Command(new [] { "-ls", "--list", "list" })]
        public void リスト()
        {
            var list = GetBatchTypes();
            foreach (var item in list)
            {
                foreach (var item2 in item.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if ("リスト".Equals(item2.Name)) continue;
                    if ("ヘルプ".Equals(item2.Name)) continue;

                    Console.WriteLine(item2.Name);
                }
            }
        }

        [Command(new [] { "-h", "--help", "help" })]
        public void ヘルプ() => リスト();

        [Command("未発送一覧")]
        public void 未発送一覧()
        {
            var list = 発送本DTOQuery.未発送一覧();

            foreach(var item in list)
                Context.Logger.LogInformation(System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(item))));
        }

        [Command("発送する")]
        public async Task 発送する()
        {
            var list = 発送本DTOQuery.未発送一覧();

            if (list.Any() == false)
                Context.Logger.LogInformation("未発送な本はありません。");
            
            var item = list.First();
            var _発送期間 = 発送期間.デフォルト期間(DateTime.UtcNow);

            await CommandBus.ExecuteAsync(本を発送するCommand.Create(item.本のID, _発送期間));
        }
    }
}
