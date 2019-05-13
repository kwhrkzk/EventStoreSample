
using System;
using Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Application
{
    public class 書籍DTO
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "title")]
        public string タイトル { get; set; }

        [DataMember(Name = "isbn")]
        public string ISBN { get; set; }

        public void Copy(書籍DTO _dto)
        {
            this.タイトル = _dto.タイトル;
            this.ISBN = _dto.ISBN;
        }
    }

    public static class 書籍DTOExtensions
    {
        public static 書籍 Convert(this 書籍DTO _dto)
        => new 書籍(書籍のID.Create(_dto.ID), Domain.ISBN.Create(_dto.ISBN))
            {
                タイトル = Domain.タイトル.Create(_dto.タイトル),
            };

        public static 書籍DTO Convert(this 書籍 _本)
        => new 書籍DTO
            {
                ID = _本.GUID文字列,
                タイトル = _本.タイトル文字列,
                ISBN = _本.ISBN文字列
            };
    }
}