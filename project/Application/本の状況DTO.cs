
using System;
using Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Utf8Json;

namespace Application
{
    public class 本の状況DTO
    {
        public 本のID 本のID { get; set; }
        public long 本のEventNumber { get; set; }
        public 書籍のID 書籍のID { get; set; }
        public long 書籍のEventNumber { get; set; }
        public タイトル タイトル { get; set; }
        public 貸出期間 貸出期間 { get; set; }
        public bool 貸し出しされている => Domain.貸出期間.Empty().Equals(貸出期間) == false;
        public 版数 版数 { get; set; }

        public string ToJson() => System.Text.Encoding.UTF8.GetString((JsonSerializer.Serialize(this)));
    }

    public interface I本の状況Query
    {
        IEnumerable<本の状況DTO> All();
        IEnumerable<本の状況DTO> 借りてる本(利用者のID _利用者のID);
    }
}
