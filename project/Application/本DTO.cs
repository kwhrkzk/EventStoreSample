using System;
using Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Application
{
    public class 本DTO
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "book_id")]
        public string BookID { get; set; }

        [DataMember(Name = "user_id")]
        public string UserID { get; set; }

        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "from_date")]
        public string FromDate { get; set; }

        [DataMember(Name = "to_date")]
        public string ToDate { get; set; }

        public void Copy(本DTO _dto)
        {
            this.BookID = _dto.BookID;
            this.UserID = _dto.UserID;
            this.Version = _dto.Version;
            this.FromDate = _dto.FromDate;
            this.ToDate = _dto.ToDate;
        }
    }

    public static class 本DTOExtensions
    {
        public static 本 Convert(this 本DTO _dto)
        => new 本(本のID.Create(_dto.ID), Domain.書籍のID.Create(_dto.BookID))
            {
                利用者のID = Domain.利用者のID.Create(_dto.UserID),
                版数 = Domain.版数.Create(_dto.Version),
                貸出期間 = 貸出期間.Create(_dto.FromDate, _dto.ToDate),
            };

        public static 本DTO Convert(this 本 _本)
        => new 本DTO
            {
                ID = _本.GUID文字列,
                BookID = _本.書籍のGUID文字列,
                UserID = (_本.貸出なし) ? "" : _本.利用者のGUID文字列,
                FromDate = (_本.貸出なし) ? "" : _本.貸出期間自日付文字列,
                ToDate = (_本.貸出なし) ? "" : _本.貸出期間至日付文字列,
                Version = _本.版数Int
            };
    }
}