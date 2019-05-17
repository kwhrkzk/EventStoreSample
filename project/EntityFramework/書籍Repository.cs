using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using Microsoft.EntityFrameworkCore;
using RentalUsecase;

namespace EntityFramework
{
    public class 書籍Repository : I書籍Repository
    {
        public void Upsert(long _eventNumber, 書籍 _書籍)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                if (db.書籍一覧.SingleOrDefault(item => item.Id.Equals(_書籍.GUID)) is 書籍Entity x)
                    x.Copy(_eventNumber, _書籍);
                else
                    db.書籍一覧.Add(_書籍.Convert(_eventNumber));

                db.SaveChanges();
            }
        }
    }
}
