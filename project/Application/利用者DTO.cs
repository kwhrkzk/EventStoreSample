
using System;
using Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Application
{
    public class 利用者DTO
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "book_list")]
        public List<string> BookList { get; set; }

        public void Copy(利用者DTO _dto)
        {
            this.LastName = _dto.LastName;
            this.FirstName = _dto.FirstName;
            this.BookList = _dto.BookList;
        }
    }

    public static class 利用者DTOExtensions
    {
        public static 利用者 Convert(this 利用者DTO _dto)
        => new 利用者(利用者のID.Create(_dto.ID))
            {
                氏名 = 氏名.Create(_dto.LastName, _dto.FirstName),
                貸出本一覧 = new List<本のID>(_dto.BookList.Select(本のID.Create))
            };
        
        public static 利用者DTO Convert(this 利用者 _利用者)
        => new 利用者DTO
            {
                ID = _利用者.ID.ID.ToString(),
                LastName = _利用者.苗字文字列,
                FirstName = _利用者.名前文字列,
                BookList = new List<string>(_利用者.貸出本ID文字列一覧)
            };
    }
}