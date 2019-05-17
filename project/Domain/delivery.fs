namespace Domain.DeliverySubDomain

open System
open System.Threading.Tasks
open Utf8Json
open Domain.GeneralSubDomain

type 発送状態Enum =
    | 未発送 = 0
    | 発送済 = 1

type 発送状態 = 
    {
        _発送状態: 発送状態Enum
    }
    static member 発送済 = { _発送状態 = 発送状態Enum.発送済 }
    static member 未発送 = { _発送状態 = 発送状態Enum.未発送 }
    static member Create(_発送状態Enum:発送状態Enum) =
        match _発送状態Enum with
            | 発送状態Enum.未発送 -> 発送状態.未発送
            | 発送状態Enum.発送済 -> 発送状態.発送済
            | _ -> invalidArg "_発送状態Enum" ("規定されていないEnumです。" + Enum.GetName(typeof<発送状態Enum>, _発送状態Enum))
    member this.Enum = this._発送状態        

type 発送期間自 =
    {
        _date: DateTime
    }
    static member Empty() = { _date = DateTime.MinValue }
    static member Create(_date:string) =
        match _date with "" -> 発送期間自.Empty() | _date -> { _date = JsonSerializer.Deserialize(_date) }
    static member Create(_date:DateTime) = { _date = _date }    
    member this.日付文字列 = 
        match this with
            |  d when d <> (発送期間自.Empty()) -> System.Text.Encoding.UTF8.GetString(JsonSerializer.Serialize(d._date))
            | _ -> ""

type 発送期間至 =
    {
        _date: DateTime
    }
    static member Empty() = { _date = DateTime.MinValue }
    static member Create(_date:string) =
        match _date with "" -> 発送期間至.Empty() | _date -> { _date = JsonSerializer.Deserialize(_date) }
    static member Create(_date:DateTime) = { _date = _date }    
    member this.日付文字列 = 
        match this with
            |  d when d <> 発送期間至.Empty() -> System.Text.Encoding.UTF8.GetString(JsonSerializer.Serialize(d._date))
            | _ -> ""
    static member デフォルト期間(_自:発送期間自) = { _date = _自._date.AddDays(2.) }
    member this.延長(span) = { _date = this._date.Add(span) }
    static member op_LessThan (d1:発送期間至, d2:発送期間至) = d1._date < d2._date
    static member op_GreaterThan (d1:発送期間至, d2:発送期間至) = d1._date > d2._date

type 発送期間 =
    {
        _発送期間自: 発送期間自
        _発送期間至: 発送期間至
    }
    static member Empty() = { _発送期間自 = 発送期間自.Empty(); _発送期間至 = 発送期間至.Empty() }
    static member Create(_自:発送期間自, _至:発送期間至) = { _発送期間自 = _自; _発送期間至 = _至 }
    static member Create(_自:DateTime, _至:DateTime) = { _発送期間自 = 発送期間自.Create(_自); _発送期間至 = 発送期間至.Create(_至) }
    static member Create(_自:string, _至:string) = { _発送期間自 = 発送期間自.Create(_自); _発送期間至 = 発送期間至.Create(_至) }
    static member デフォルト期間(_自:DateTime) = { _発送期間自 = 発送期間自.Create(_自); _発送期間至 = 発送期間至.デフォルト期間(発送期間自.Create(_自)) }
    member this.延長(span) = { _発送期間自 = this._発送期間自; _発送期間至 = this._発送期間至.延長(span) }
    member this.発送期間自日付文字列 = this._発送期間自.日付文字列
    member this.発送期間至日付文字列 = this._発送期間至.日付文字列
    member this.発送期間自DateTime = this._発送期間自._date
    member this.発送期間至DateTime = this._発送期間至._date
    member this.より発送期間至が後(_発送期間:発送期間) = this._発送期間至 < _発送期間._発送期間至

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
    member val 発送期間 = 発送期間.Empty() with get, set
    member this.発送期間自 = this.発送期間._発送期間自
    member this.発送期間至 = this.発送期間._発送期間至
    member this.発送期間自日付文字列 = this.発送期間.発送期間自日付文字列
    member this.発送期間至日付文字列 = this.発送期間.発送期間至日付文字列
    member this.発送なし = 発送期間.Empty().Equals(this.発送期間)
    member this.発送期間自DateTime = this.発送期間.発送期間自DateTime
    member this.発送期間至DateTime = this.発送期間.発送期間至DateTime
    member val 発送状態 = 発送状態.未発送 with get, set

module Events =
    module Book =
        [<Literal>]
        [<CompiledName "ShippedBookVer100">]
        let shippedBookVer100 = "book.shippedBookVer1.0.0"
        type ShippedBookDTOVer100 = 
                {
                    id: string
                    shipping_start_date: Nullable<DateTime>
                    shipping_end_date: Nullable<DateTime>
                }
                static member Create(a,b,c) = { id = a; shipping_start_date = b; shipping_end_date = c }

type I本Factory =
    abstract Create: string * string * string -> 本

type I本Repository =
    abstract Insert: 本 -> unit
    abstract Update: int64 * Events.Book.ShippedBookDTOVer100 -> unit
    abstract Delete: string -> unit
