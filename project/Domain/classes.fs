namespace Domain

open System
open System.Threading.Tasks
open Utf8Json

type 書籍のID =
    {
        _id: Guid
    }
    member this.ID = this._id
    static member Create(_id:string) = { _id = Guid.Parse(_id) }
    static member Create(_id:Guid) = { _id = _id }
    static member New() = { _id = Guid.NewGuid() }

type タイトル =
    {
        _タイトル: string
    }
    member this.タイトル = match this._タイトル with "" -> "タイトル未設定" | _ -> this._タイトル
    static member Empty() = { _タイトル = "" }
    static member Create x = { _タイトル = x }

type ISBN =
    {
        _isbn: string
    }
    member this.ISBN = this._isbn
    static member Empty() = { _isbn = "" }
    static member Create x = { _isbn = x }

type 書籍 (_id, _isbn) =
    member this.ID: 書籍のID = _id
    member this.GUID = this.ID.ID
    member this.GUID文字列 = this.GUID.ToString()
    member val タイトル = タイトル.Empty() with get, set
    member this.タイトル文字列 = this.タイトル.タイトル
    member this.ISBN: ISBN = _isbn
    member this.ISBN文字列 = this.ISBN.ISBN

type 本のID =
    {
        _id: Guid
    }
    member this.ID = this._id
    static member Create(_id:string) = { _id = Guid.Parse(_id) }
    static member Create(_id:Guid) = { _id = _id }
    static member New() = { _id = Guid.NewGuid() }
    member this.ID文字列 = this.ID.ToString()

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

type 版数 =
    {
        _版数: int
    }
    member this.版数 = this._版数
    static member Empty = { _版数 = 0 }
    static member Create x = { _版数 = x }
    static member 第一版 = { _版数 = 1 }

type 利用者のID =
    {
        _id: Guid
    }
    member this.ID = this._id
    member this.ID文字列 = this.ID.ToString()
    static member Create(_id:string) = match _id with "" -> 利用者のID.Empty() | _id -> { _id = Guid.Parse(_id) }
    static member Create(_id:Guid) = { _id = _id }
    static member New() = { _id = Guid.NewGuid() }
    static member Empty() = { _id = Guid.Empty }

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

type 苗字 =
    {
        苗字: string
    }
    static member Create(_name) = { 苗字 = _name }
    static member Empty = { 苗字 = "" }

type 名前 =
    {
        名前: string
    }
    static member Create(_name) = { 名前 = _name }
    static member Empty = { 名前 = "" }
type 氏名 =
    {
        _苗字: 苗字
        _名前: 名前
    }
    static member Create(_苗字, _名前) = { _苗字 = 苗字.Create(_苗字); _名前 = 名前.Create(_名前) }
    static member Empty = { _苗字 = 苗字.Empty; _名前 = 名前.Empty }
    member this.苗字文字列 = this._苗字.苗字
    member this.名前文字列 = this._名前.名前

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

type I書籍Factory =
    abstract Create: タイトル * ISBN -> 書籍

type I本Factory =
    abstract Create: 書籍のID -> 本

type I利用者Factory =
    abstract Create: string * string -> 利用者

type I本Repository =
    abstract Upsert: int64 * 本 -> unit
    abstract Delete: 本のID -> unit

type I書籍Repository =
    abstract Upsert: int64 * 書籍 -> unit

type I利用者Repository =
    abstract Upsert: int64 * 利用者 -> unit
