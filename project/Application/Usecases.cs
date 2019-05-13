using System;
using Domain;
using System.Threading.Tasks;

namespace Application
{
    public interface I利用者を登録する
    {
        Task ExecuteAsync(string _苗字, string _名前);
    }

    public interface I本を登録する
    {
        Task<書籍のID> ExecuteAsync(タイトル _タイトル, ISBN _isbn);
        Task ExecuteAsync(書籍のID _書籍のID);
    }

    public interface I本を借りる
    {
        Task ExecuteAsync(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間);
    }

    public interface I本を延長する
    {
        Task ExecuteAsync(本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間);
    }

    public interface I本を返す
    {
        Task ExecuteAsync(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber);
    }

    public interface I本を破棄する
    {
        Task ExecuteAsync(本のID _本のID, long _本のEventNumber);
    }
}