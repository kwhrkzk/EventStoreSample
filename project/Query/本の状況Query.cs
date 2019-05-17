using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using Microsoft.EntityFrameworkCore;
using RentalUsecase;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Query
{

    public class 本の状況Query : I本の状況Query
    {
        public IEnumerable<本の状況DTO> All()
        {
            using (var db = new RentalProjectorContext())
            {
                return (from b in db.本一覧
                join s in db.書籍一覧 on b.書籍EntityId equals s.Id
                select new 本の状況DTO {
                    本のID = 本のID.Create(b.Id),
                    本のEventNumber = b.EventNumber,
                    書籍のID = 書籍のID.Create(s.Id),
                    書籍のEventNumber = s.EventNumber,
                    タイトル = タイトル.Create(s.タイトル),
                    貸出期間 = (b.貸出期間自.HasValue) ? 貸出期間.Create(b.貸出期間自.Value, b.貸出期間至.Value) : 貸出期間.Empty(),
                    版数 = 版数.Create(b.版数)
                }).ToList();
            }
        }

        public IEnumerable<本の状況DTO> 借りてる本(利用者のID _利用者のID)
        {
            using (var db = new RentalProjectorContext())
            {
                return (from b in db.本一覧
                join s in db.書籍一覧 on b.書籍EntityId equals s.Id
                where b.利用者EntityId.HasValue && b.利用者EntityId.Value == _利用者のID.ID
                select new 本の状況DTO {
                    本のID = 本のID.Create(b.Id),
                    本のEventNumber = b.EventNumber,
                    書籍のID = 書籍のID.Create(s.Id),
                    書籍のEventNumber = s.EventNumber,
                    タイトル = タイトル.Create(s.タイトル),
                    貸出期間 = (b.貸出期間自.HasValue) ? 貸出期間.Create(b.貸出期間自.Value, b.貸出期間至.Value) : 貸出期間.Empty(),
                    版数 = 版数.Create(b.版数)
                }).ToList();
            }
        }

        public IEnumerable<本の状況DTO> 借りてない本()
        {
            using (var db = new RentalProjectorContext())
            {
                return (from b in db.本一覧
                join s in db.書籍一覧 on b.書籍EntityId equals s.Id
                where b.利用者EntityId.HasValue == false
                select new 本の状況DTO {
                    本のID = 本のID.Create(b.Id),
                    本のEventNumber = b.EventNumber,
                    書籍のID = 書籍のID.Create(s.Id),
                    書籍のEventNumber = s.EventNumber,
                    タイトル = タイトル.Create(s.タイトル),
                    貸出期間 = (b.貸出期間自.HasValue) ? 貸出期間.Create(b.貸出期間自.Value, b.貸出期間至.Value) : 貸出期間.Empty(),
                    版数 = 版数.Create(b.版数)
                }).ToList();
            }
        }
    }
}
