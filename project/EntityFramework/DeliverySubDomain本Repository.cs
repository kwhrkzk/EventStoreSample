using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain.GeneralSubDomain;
using Domain.DeliverySubDomain;
using Microsoft.EntityFrameworkCore;
using DeliveryUsecase;

namespace EntityFramework
{
    public class DeliverySubDomain本Repository : I本Repository
    {
        public void Insert(本 _本)
        {
            using (var db = new DeliveryProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.本一覧.SingleOrDefault(item => item.Id.Equals(_本.GUID)) as 本Entity;

                if (x == null)
                    db.本一覧.Add(_本.Convert());

                db.SaveChanges();
            }
        }

        public void Update(long _eventNumber, Domain.DeliverySubDomain.Events.Book.ShippedBookDTOVer100 _dto)
        {
            using (var db = new DeliveryProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.本一覧.SingleOrDefault(item => item.Id.Equals(Guid.Parse(_dto.id)));

                if (x == null)
                    throw new ArgumentException("ShippedBookDTOVer100", "該当するGUIDが存在しません。: " + _dto.id);

                x.Copy(_dto);

                db.SaveChanges();
            }
        }

        public void Delete(string id)
        {
            using (var db = new DeliveryProjectorContext())
            {
                db.Database.EnsureCreated();

                if (db.本一覧.SingleOrDefault(item => item.Id.Equals(Guid.Parse(id))) is 本Entity x)
                    db.本一覧.Remove(x);

                db.SaveChanges();
            }
        }
    }
}
