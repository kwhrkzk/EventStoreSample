using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain.GeneralSubDomain;
using Domain.DeliverySubDomain;
using Microsoft.EntityFrameworkCore;
using DeliveryUsecase;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Query
{

    public class 発送本DTOQuery : I発送本DTOQuery
    {
        public IEnumerable<発送本DTO> 未発送一覧()
        {
            using (var db = new DeliveryProjectorContext())
            {
                return (from b in db.本一覧
                where b.発送状態 == 発送状態.未発送.Enum
                select new 発送本DTO()
                {
                    本のID = 本のID.Create(b.Id),
                    書籍のID = 書籍のID.Create(b.書籍のID),
                    利用者のID = b.利用者のID.HasValue ? 利用者のID.Create(b.利用者のID.Value) : 利用者のID.Empty(),
                    発送期間 = b.発送期間自.HasValue ? 発送期間.Create(b.発送期間自.Value , b.発送期間至.Value) : 発送期間.Empty(),
                    発送状態 = 発送状態.Create(b.発送状態)
                }
                ).ToList();
            }
        }
    }
}
