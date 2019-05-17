namespace DeliveryUsecase

open System
open System.Threading.Tasks
open Microsoft.EntityFrameworkCore
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open System.Runtime.Serialization
open Pomelo.EntityFrameworkCore.MySql.Infrastructure
open Domain.GeneralSubDomain
open Domain.DeliverySubDomain
open System.Runtime.CompilerServices

type I本を発送するCommand =
        inherit ICommand
        abstract 本のID: 本のID with get
        abstract 発送期間: 発送期間 with get

        abstract Create: 本のID * 発送期間 -> I本を発送するCommand

type I本を発送するCommandHandler =
     inherit ICommandHandler

[<CLIMutable>]
type 本Entity = 
    {
        [<Key>]Id: Guid
        [<Required>]書籍のID: Guid
        mutable 利用者のID: System.Nullable<Guid>
        mutable 発送期間自: System.Nullable<DateTime>
        mutable 発送期間至: System.Nullable<DateTime>
        [<Required>]mutable 発送状態: 発送状態Enum
    }

[<CLIMutable>]
type 発送本DTO = 
    {
        本のID:本のID
        書籍のID:書籍のID
        利用者のID:利用者のID
        発送期間:発送期間
        発送状態:発送状態
    }

type DeliveryProjectorContext() =
  inherit DbContext()

  [<DefaultValue>]
  val mutable _本一覧 : DbSet<本Entity>

  static member val ConnectionString = "" with get, set

  member this.本一覧
    with get() = this._本一覧
    and set v = this._本一覧 <- v

  override this.OnConfiguring(optionsBuilder: DbContextOptionsBuilder) =
    optionsBuilder
#if DEBUG
                .UseMySql("Server=localhost;Port=3307;Username=root;Password=root;Database=snapshots")
#else
                .UseMySql("Server=delivery_projection;Port=3306;Username=root;Password=root;Database=snapshots")
#endif
    |> ignore

type I発送本DTOQuery =
    abstract 未発送一覧: unit -> seq<発送本DTO>

[<Extension>]
type 本Extensions() =
    [<Extension>]
    static member Convert(_本:本) =
      {
         本Entity.Id = _本.GUID
         書籍のID = _本.書籍のGUID
         利用者のID = Nullable<Guid>(_本.利用者のGUID)
         発送期間自 = match _本.発送なし with true -> Nullable<DateTime>() | false -> Nullable<DateTime>(_本.発送期間自DateTime)
         発送期間至 = match _本.発送なし with true -> Nullable<DateTime>() | false -> Nullable<DateTime>(_本.発送期間至DateTime)
         発送状態 = 発送状態Enum.未発送
      }


[<Extension>]
type 本EntityExtensions() =
    [<Extension>]
    static member Copy(_本:本Entity, _dto:Domain.DeliverySubDomain.Events.Book.ShippedBookDTOVer100) =
        _本.発送期間自 <- _dto.shipping_start_date
        _本.発送期間至 <- _dto.shipping_end_date
        _本.発送状態 <- 発送状態Enum.発送済
        _本

