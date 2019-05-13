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

namespace MainApp
{
    public class Scenario: BatchBase
    {
        private I本Factory 本Factory { get; }
        private I書籍Factory 書籍Factory { get; }
        private I利用者Factory 利用者Factory { get; }
        private I利用者を登録する 利用者を登録するCommand { get; }
        private I本を登録する 本を登録するCommand { get; }
        private Iログイン情報Query ログイン情報Query { get; }
        private I本の状況Query 本の状況Query { get; }
        private I本を借りる 本を借りるCommand { get; }
        private I本を延長する 本を延長するCommand { get; }
        private I本を返す 本を返すCommand { get; }
        private I本を破棄する 本を破棄するCommand { get; }

        public Scenario(
            I本Factory _本Factory,
            I書籍Factory _書籍Factory,
            I利用者Factory _利用者Factory,
            I利用者を登録する _利用者を登録する,
            I本を登録する _本を登録する,
            Iログイン情報Query _ログイン情報Query,
            I本の状況Query _本の状況Query,
            I本を借りる _本を借りる,
            I本を延長する _本を延長する,
            I本を返す _本を返す,
            I本を破棄する _本を破棄する
            )
        {
            本Factory = _本Factory;
            書籍Factory = _書籍Factory;
            利用者Factory = _利用者Factory;
            利用者を登録するCommand = _利用者を登録する;
            本を登録するCommand = _本を登録する;
            ログイン情報Query = _ログイン情報Query;
            本の状況Query = _本の状況Query;
            本を借りるCommand = _本を借りる;
            本を延長するCommand = _本を延長する;
            本を返すCommand = _本を返す;
            本を破棄するCommand = _本を破棄する;
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
            await 利用者を登録するCommand.ExecuteAsync("田中", "太郎");
            await 利用者を登録するCommand.ExecuteAsync("山田", "花子");

            var _書籍のID = await 本を登録するCommand.ExecuteAsync(タイトル.Create(".NETのエンタープライズアプリケーションアーキテクチャ 第２版"), ISBN.Create("9784822298487"));
            await 本を登録するCommand.ExecuteAsync(_書籍のID);
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

            await 本を借りるCommand.ExecuteAsync(ログイン情報.ID, ログイン情報.EventNumber, item.本のID, item.本のEventNumber, _貸出期間);
        }

        [Command("本を延長する")]
        public async Task 本を延長する()
        {
            var ログイン情報 = ログイン情報Query.First();

            try
            {
                var item = 本の状況Query.借りてる本(ログイン情報.ID).First();

                var _貸出期間 = item.貸出期間.延長(TimeSpan.FromDays(14));

                await 本を延長するCommand.ExecuteAsync(item.本のID, item.本のEventNumber, _貸出期間);
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

                await 本を返すCommand.ExecuteAsync(ログイン情報.ID, ログイン情報.EventNumber, item.本のID, item.本のEventNumber);
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

            await 本を破棄するCommand.ExecuteAsync(item.本のID, item.本のEventNumber);
        }
    }
}
