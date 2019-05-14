using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using MicroBatchFramework;
using MicroBatchFramework.Logging;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Domain;
using Application;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;

namespace MainApp
{
    public class Scenario: BatchBase
    {
        private I本Factory 本Factory { get; }
        private I書籍Factory 書籍Factory { get; }
        private I利用者Factory 利用者Factory { get; }
        private Iログイン情報Query ログイン情報Query { get; }
        private I本の状況Query 本の状況Query { get; }
        private ICommandBus CommandBus { get; }
        private I利用者を登録するCommand 利用者を登録するCommand { get; }
        private I本を登録するCommand 本を登録するCommand { get; }
        private I本を登録する2Command 本を登録する2Command { get; }
        private I本を借りるCommand 本を借りるCommand { get; }
        private I本を延長するCommand 本を延長するCommand { get; }
        private I本を返すCommand 本を返すCommand { get; }
        private I本を破棄するCommand 本を破棄するCommand { get; }
        private IMessageBroker MessageBroker { get; }

        public Scenario(
            I本Factory _本Factory,
            I書籍Factory _書籍Factory,
            I利用者Factory _利用者Factory,
            Iログイン情報Query _ログイン情報Query,
            I本の状況Query _本の状況Query,
            ICommandBus _commandBus,
            I利用者を登録するCommand _利用者を登録するCommand,
            I本を登録するCommand _本を登録するCommand,
            I本を登録する2Command _本を登録する2Command,
            I本を借りるCommand _本を借りるCommand,
            I本を延長するCommand _I本を延長するCommand,
            I本を返すCommand _本を返すCommand,
            I本を破棄するCommand _本を破棄するCommand,
            IMessageBroker _messageBroker
            )
        {
            本Factory = _本Factory;
            書籍Factory = _書籍Factory;
            利用者Factory = _利用者Factory;
            ログイン情報Query = _ログイン情報Query;
            本の状況Query = _本の状況Query;
            CommandBus = _commandBus;
            利用者を登録するCommand = _利用者を登録するCommand;
            本を登録するCommand = _本を登録するCommand;
            本を登録する2Command = _本を登録する2Command;
            本を借りるCommand = _本を借りるCommand;
            本を延長するCommand = _I本を延長するCommand;
            本を返すCommand = _本を返すCommand;
            本を破棄するCommand = _本を破棄するCommand;
            MessageBroker = _messageBroker;
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

        [Command("初期値入力")]
        public async Task 初期値入力()
        {
            await CommandBus.ExecuteAsync(利用者を登録するCommand.Create("田中", "太郎"));
            await CommandBus.ExecuteAsync(利用者を登録するCommand.Create("山田", "花子"));

            var prop = MessageBroker.ToObservable<I本が登録されたEvent>().ToReadOnlyReactivePropertySlim(null);

            await CommandBus.ExecuteAsync(本を登録するCommand.Create(タイトル.Create(".NETのエンタープライズアプリケーションアーキテクチャ 第２版"), ISBN.Create("9784822298487")));

            if (prop.Value == null)
                await prop;

            await CommandBus.ExecuteAsync(本を登録する2Command.Create(prop.Value.書籍のID));
        }

        [Command("本を借りる")]
        public async Task 本を借りる()
        {
            var ログイン情報 = ログイン情報Query.First();

            var item = 本の状況Query.All().First();

            if (item.貸し出しされている)
            {
                Context.Logger.LogInformation("貸し出しされているため、本を借りることができません。");
                return;
            }

            var _貸出期間 = 貸出期間.デフォルト期間(DateTime.UtcNow);
            await CommandBus.ExecuteAsync(本を借りるCommand.Create(ログイン情報.ID, ログイン情報.EventNumber, item.本のID, item.本のEventNumber, _貸出期間));
        }

        [Command("本を延長する")]
        public async Task 本を延長する()
        {
            var ログイン情報 = ログイン情報Query.First();

            try
            {
                var item = 本の状況Query.借りてる本(ログイン情報.ID).First();

                var _貸出期間 = item.貸出期間.延長(TimeSpan.FromDays(14));

                await CommandBus.ExecuteAsync(本を延長するCommand.Create(item.本のID, item.本のEventNumber, _貸出期間));
            }
            catch (System.Exception)
            {
                Context.Logger.LogInformation("借りている本がありません。");
            }
        }

        [Command("本を返す")]
        public async Task 本を返す()
        {
            var ログイン情報 = ログイン情報Query.First();

            try
            {
                var item = 本の状況Query.借りてる本(ログイン情報.ID).First();

                await CommandBus.ExecuteAsync(本を返すCommand.Create(ログイン情報.ID, ログイン情報.EventNumber, item.本のID, item.本のEventNumber));
            }
            catch (System.Exception)
            {
                Context.Logger.LogInformation("借りている本がありません。");
            }
        }

        [Command("本を破棄する")]
        public async Task 本を破棄する()
        {
            var item = 本の状況Query.All().First();

            if (item.貸し出しされている)
            {
                Context.Logger.LogInformation("貸し出しされているため、本を破棄できません。");
                return;
            }

            await CommandBus.ExecuteAsync(本を破棄するCommand.Create(item.本のID, item.本のEventNumber));
        }
    }
}
