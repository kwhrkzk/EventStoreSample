using System;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RentalUsecase
{
    public class 書籍Entity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public long EventNumber { get; set; }

        [Required]
        public string タイトル { get; set; }
    }

    public static class 書籍EntityExtensions
    {
        public static void Copy(this 書籍Entity item, long _eventNumber, 書籍 _書籍)
        {
            item.EventNumber = _eventNumber;
            item.タイトル = _書籍.タイトル文字列;
        }

        public static 書籍Entity Convert(this 書籍 _書籍, long _eventNumber)
        => new 書籍Entity
        {
            Id = _書籍.GUID,
            EventNumber = _eventNumber,
            タイトル = _書籍.タイトル文字列,
        };
    }
}