namespace Domain.GeneralSubDomain

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
    member this.ID文字列 = this.ID.ToString()

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

type I書籍Factory =
    abstract Create: string * string * string -> 書籍

type I書籍Repository =
    abstract Upsert: int64 * 書籍 -> unit

type ICommand = interface
    end

type ICommandHandler =
    abstract HandleAsync: ICommand -> Task

type ICommandBus =
    abstract ExecuteAsync: ICommand -> Task

module Aggregate =
    [<Literal>]
    [<CompiledName "User">]
    let user = "user"
    [<Literal>]
    [<CompiledName "Book">]
    let book = "book"
    [<Literal>]
    [<CompiledName "BookInfo">]
    let book_info = "book_info"
