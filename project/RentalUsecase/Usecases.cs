using System;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using System.Threading.Tasks;

namespace RentalUsecase
{
    public interface I利用者を登録するCommand : ICommand
    {
        string 苗字 { get; }
        string 名前 { get; }

        I利用者を登録するCommand Create(string _苗字, string _名前);
    }
    public interface I利用者を登録するCommandHandler : ICommandHandler { }

    public interface I本を登録するCommand: ICommand
    {
        タイトル タイトル { get; }
        ISBN ISBN { get; }

        I本を登録するCommand Create(タイトル _タイトル, ISBN _isbn);
    }
    public interface I本を登録するCommandHandler: ICommandHandler {}

    public interface I本が登録されたEvent
    {
        書籍のID 書籍のID { get; }
        I本が登録されたEvent Create(書籍のID _書籍のID);
    }

    public interface I本を登録する2Command: ICommand
    {
        書籍のID 書籍のID { get; }
        I本を登録する2Command Create(書籍のID _書籍のID);
    }
    public interface I本を登録する2CommandHandler: ICommandHandler {}

    public interface I本を借りるCommand: ICommand
    {
        利用者のID 利用者のID { get; }
        long 利用者のEventNumber { get; }
        本のID 本のID { get; }
        long 本のEventNumber { get; }
        貸出期間 貸出期間 { get; }
        I本を借りるCommand Create(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間);
    }
    public interface I本を借りるCommandHandler: ICommandHandler {}

    public interface I本を延長するCommand: ICommand
    {
        本のID 本のID { get; }
        long 本のEventNumber { get; }
        貸出期間 貸出期間 { get; }
        I本を延長するCommand Create(本のID _本のID, long _本のEventNumber, 貸出期間 _貸出期間);
    }
    public interface I本を延長するCommandHandler: ICommandHandler {}

    public interface I本を返すCommand: ICommand
    {
        利用者のID 利用者のID { get; }
        long 利用者のEventNumber { get; }
        本のID 本のID { get; }
        long 本のEventNumber { get; }
        I本を返すCommand Create(利用者のID _利用者のID, long _利用者のEventNumber, 本のID _本のID, long _本のEventNumber);
    }

    public interface I本を返すCommandHandler: ICommandHandler { }

    public interface I本を破棄するCommand: ICommand
    {
        本のID 本のID { get; }
        long 本のEventNumber { get; }
        I本を破棄するCommand Create(本のID _本のID, long _本のEventNumber);
    }

    public interface I本を破棄するCommandHandler: ICommandHandler {}
}