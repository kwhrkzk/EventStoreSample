using System;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RentalUsecase
{
    public class 本Entity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public long EventNumber { get; set; }

        [Required]
        public Guid 書籍EntityId { get; set; }

        public Guid? 利用者EntityId { get; set; }

        public int 版数 { get; set; }

        public DateTime? 貸出期間自 { get; set; }
        public DateTime? 貸出期間至 { get; set; }
    }

    public static class 本EntityExtensions
    {
        public static void Copy(this 本Entity item, long _eventNumber, 本 _本)
        {
            item.EventNumber = _eventNumber;
            item.利用者EntityId = (_本.貸出なし) ? (Guid?)null : _本.利用者のGUID;
            item.貸出期間自 = (_本.貸出なし) ? (DateTime?)null : _本.貸出期間自DateTime;
            item.貸出期間至 = (_本.貸出なし) ? (DateTime?)null : _本.貸出期間至DateTime;
            item.版数 = _本.版数.版数;
        }

        public static void Copy(this 本Entity item, long _eventNumber, Domain.RentalSubDomain.Events.Book.LendedBookDTOVer100 _dto)
        {
            item.EventNumber = _eventNumber;
            item.利用者EntityId = (Guid?)Guid.Parse(_dto.user_id);
            item.貸出期間自 = _dto.lending_start_date;
            item.貸出期間至 = _dto.lending_end_date;
        }

        public static void Copy(this 本Entity item, long _eventNumber, Domain.RentalSubDomain.Events.Book.ExtendedBookDTOVer100 _dto)
        {
            item.EventNumber = _eventNumber;
            item.貸出期間自 = _dto.lending_start_date;
            item.貸出期間至 = _dto.lending_end_date;
        }

        public static void Copy(this 本Entity item, long _eventNumber, Domain.RentalSubDomain.Events.Book.ReturnedBookDTOVer100 _dto)
        {
            item.EventNumber = _eventNumber;
            item.利用者EntityId = (Guid?)null;
            item.貸出期間自 = (DateTime?)null;
            item.貸出期間至 = (DateTime?)null;
        }

        public static 本Entity Convert(this 本 _本, long _eventNumber)
        => new 本Entity
        {
            Id = _本.GUID,
            書籍EntityId = _本.書籍のGUID,
            EventNumber = _eventNumber,
            利用者EntityId = (_本.貸出なし) ? (Guid?)null : _本.利用者のGUID,
            貸出期間自 = (_本.貸出なし) ? (DateTime?)null : _本.貸出期間自DateTime,
            貸出期間至 = (_本.貸出なし) ? (DateTime?)null : _本.貸出期間至DateTime,
            版数 = _本.版数.版数,
        };
    }
}