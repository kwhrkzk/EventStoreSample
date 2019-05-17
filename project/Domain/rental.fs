namespace Domain.RentalSubDomain

open System
open System.Threading.Tasks
open Utf8Json
open Domain.GeneralSubDomain
open System.Diagnostics.Tracing
open System.Runtime.Serialization

type 貸出期間自 =
    {
        _date: DateTime
    }
    static member Empty() = { _date = DateTime.MinValue }
    static member Create(_date:string) =
        match _date with "" -> 貸出期間自.Empty() | _date -> { _date = JsonSerializer.Deserialize(_date) }
    static member Create(_date:DateTime) = { _date = _date }    
    member this.日付文字列 = 
        match this with
            |  d when d <> (貸出期間自.Empty()) -> System.Text.Encoding.UTF8.GetString(JsonSerializer.Serialize(d._date))
            | _ -> ""

type 貸出期間至 =
    {
        _date: DateTime
    }
    static member Empty() = { _date = DateTime.MinValue }
    static member Create(_date:string) =
        match _date with "" -> 貸出期間至.Empty() | _date -> { _date = JsonSerializer.Deserialize(_date) }
    static member Create(_date:DateTime) = { _date = _date }    
    member this.日付文字列 = 
        match this with
            |  d when d <> 貸出期間至.Empty() -> System.Text.Encoding.UTF8.GetString(JsonSerializer.Serialize(d._date))
            | _ -> ""
    static member デフォルト期間(_自:貸出期間自) = { _date = _自._date.AddDays(14.) }
    member this.延長(span) = { _date = this._date.Add(span) }
    static member op_LessThan (d1:貸出期間至, d2:貸出期間至) = d1._date < d2._date
    static member op_GreaterThan (d1:貸出期間至, d2:貸出期間至) = d1._date > d2._date

type 貸出期間 =
    {
        _貸出期間自: 貸出期間自
        _貸出期間至: 貸出期間至
    }
    static member Empty() = { _貸出期間自 = 貸出期間自.Empty(); _貸出期間至 = 貸出期間至.Empty() }
    static member Create(_自:貸出期間自, _至:貸出期間至) = { _貸出期間自 = _自; _貸出期間至 = _至 }
    static member Create(_自:DateTime, _至:DateTime) = { _貸出期間自 = 貸出期間自.Create(_自); _貸出期間至 = 貸出期間至.Create(_至) }
    static member Create(_自:string, _至:string) = { _貸出期間自 = 貸出期間自.Create(_自); _貸出期間至 = 貸出期間至.Create(_至) }
    static member デフォルト期間(_自:DateTime) = { _貸出期間自 = 貸出期間自.Create(_自); _貸出期間至 = 貸出期間至.デフォルト期間(貸出期間自.Create(_自)) }
    member this.延長(span) = { _貸出期間自 = this._貸出期間自; _貸出期間至 = this._貸出期間至.延長(span) }
    member this.貸出期間自日付文字列 = this._貸出期間自.日付文字列
    member this.貸出期間至日付文字列 = this._貸出期間至.日付文字列
    member this.貸出期間自DateTime = this._貸出期間自._date
    member this.貸出期間至DateTime = this._貸出期間至._date
    member this.より貸出期間至が後(_貸出期間:貸出期間) = this._貸出期間至 < _貸出期間._貸出期間至

type 本 (_id, _書籍のID) =
    member this.ID: 本のID = _id
    member this.GUID = this.ID.ID
    member this.GUID文字列 = this.ID.ID.ToString()
    member this.書籍のID: 書籍のID = _書籍のID
    member this.書籍のGUID = this.書籍のID.ID
    member this.書籍のGUID文字列 = this.書籍のGUID.ToString()
    member val 利用者のID = 利用者のID.Empty() with get, set
    member this.利用者のGUID = this.利用者のID.ID
    member this.利用者のGUID文字列 = this.利用者のGUID.ToString()
    member val 貸出期間 = 貸出期間.Empty() with get, set
    member this.貸出期間自 = this.貸出期間._貸出期間自
    member this.貸出期間至 = this.貸出期間._貸出期間至
    member this.貸出期間自日付文字列 = this.貸出期間.貸出期間自日付文字列
    member this.貸出期間至日付文字列 = this.貸出期間.貸出期間至日付文字列
    member this.貸出なし = 貸出期間.Empty().Equals(this.貸出期間)
    member this.貸出期間自DateTime = this.貸出期間.貸出期間自DateTime
    member this.貸出期間至DateTime = this.貸出期間.貸出期間至DateTime
    member val 版数 = 版数.Empty with get, set
    member this.版数Int = this.版数.版数

type 利用者 (_id) =
    member this.ID: 利用者のID = _id
    member this.GUID = this.ID.ID
    member this.GUID文字列 = this.ID.ID.ToString()
    member val 氏名: 氏名 = 氏名.Empty with get, set
    member val 貸出本一覧: System.Collections.Generic.List<本のID> = new System.Collections.Generic.List<本のID>() with get, set
    member this.本を借りる(_本のID) = this.貸出本一覧.Add(_本のID)
    member this.本を返す(_本のID) = this.貸出本一覧.Remove(_本のID)
    member this.貸出本GUID一覧 = new System.Collections.Generic.List<Guid>(Seq.map (fun (x:本のID) -> x.ID) this.貸出本一覧)
    member this.貸出本ID文字列一覧 = new System.Collections.Generic.List<string>(Seq.map (fun (x:本のID) -> x.ID文字列) this.貸出本一覧)
    member this.苗字文字列 = this.氏名.苗字文字列
    member this.名前文字列 = this.氏名.名前文字列

module Events =
    module User =
        [<Literal>]
        [<CompiledName "AddedUserVer100">]
        let addedUserVer100 = "user.addedUserVer1.0.0"
        type AddedUserDTOVer100 = 
                {
                    id: string
                    last_name: string
                    first_name: string
                }
                static member Create(_id:利用者のID, _苗字:string, _名前:string) = { id = _id.ID文字列; last_name = _苗字; first_name = _名前 }

        [<Literal>]
        [<CompiledName "LendedBookVer100">]
        let lendedBookVer100 = "user.lendedBookVer1.0.0"
        type LendedBookDTOVer100 = 
                {
                    id: string
                    book_id: string
                }
                static member Create(a,b) = { id = a; book_id = b }

        [<Literal>]
        [<CompiledName "ReturnedBookVer100">]
        let returnedBookVer100 = "user.returnedBookVer1.0.0"
        type ReturnedBookDTOVer100 = 
                {
                    id: string
                    book_id: string
                }
                static member Create(a,b) = { id = a; book_id = b }

    module BookInfo =
        [<Literal>]
        [<CompiledName "AddedBookInfoVer100">]
        let addedBookInfoVer100 = "bookinfo.addedBookInfoVer1.0.0"
        type AddedBookInfoDTOVer100 = 
                {
                    id: string
                    title: string
                    isbn: string
                }
                static member Create(_id, _title, _isbn) = { id = _id; title = _title; isbn = _isbn }

    module Book =
        [<Literal>]
        [<CompiledName "AddedBookVer100">]
        let addedBookVer100 = "book.addedBookVer1.0.0"
        type AddedBookDTOVer100 = 
                {
                    id: string
                    book_id: string
                }
                static member Create(_id, _書籍のID) = { id = _id; book_id = _書籍のID }

        [<Literal>]
        [<CompiledName "LendedBookVer100">]
        let lendedBookVer100 = "book.lendedBookVer1.0.0"
        type LendedBookDTOVer100 = 
                {
                    id: string
                    user_id: string
                    lending_start_date: Nullable<DateTime>
                    lending_end_date: Nullable<DateTime>
                }
                static member Create(a,b,c,d) = { id = a; user_id = b; lending_start_date = c; lending_end_date = d }

        [<Literal>]
        [<CompiledName "ExtendedBookVer100">]
        let extendedBookVer100 = "book.extendedBookVer1.0.0"
        type ExtendedBookDTOVer100 = 
                {
                    id: string
                    lending_start_date: Nullable<DateTime>
                    lending_end_date: Nullable<DateTime>
                }
                static member Create(a,b,c) = { id = a; lending_start_date = b; lending_end_date = c }

        [<Literal>]
        [<CompiledName "ReturnedBookVer100">]
        let returnedBookVer100 = "book.returnedBookVer1.0.0"
        type ReturnedBookDTOVer100 = 
                {
                    id: string
                }
                static member Create(a) = { id = a }

        [<Literal>]
        [<CompiledName "DestroyedBookVer100">]
        let destroyedBookVer100 = "book.destroyedBookVer1.0.0"
        type DestroyedBookDTOVer100 = 
                {
                    id: string
                }
                static member Create(a) = { id = a }

type I本Factory =
    abstract Create: string * string -> 本

type I利用者Factory =
    abstract Create: string * string * string -> 利用者

type I利用者Repository =
    abstract Insert: int64 * 利用者 -> unit
    abstract Update: int64 * Events.User.LendedBookDTOVer100 -> unit
    abstract Update: int64 * Events.User.ReturnedBookDTOVer100 -> unit

type I本Repository =
    abstract Insert: int64 * 本 -> unit
    abstract Update: int64 * Events.Book.LendedBookDTOVer100 -> unit
    abstract Update: int64 * Events.Book.ExtendedBookDTOVer100 -> unit
    abstract Update: int64 * Events.Book.ReturnedBookDTOVer100 -> unit
    abstract Delete: string -> unit
