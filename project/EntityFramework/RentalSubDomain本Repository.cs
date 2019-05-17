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
    public class RentalSubDomain本Repository : I本Repository
    {
        public void Insert(long _eventNumber, 本 _本)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.本一覧.SingleOrDefault(item => item.Id.Equals(_本.GUID)) as 本Entity;

                if (x == null)
                    db.本一覧.Add(_本.Convert(_eventNumber));

                db.SaveChanges();
            }
        }

        public void Update(long _eventNumber, Domain.RentalSubDomain.Events.Book.LendedBookDTOVer100 _dto)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.本一覧.SingleOrDefault(item => item.Id.Equals(Guid.Parse(_dto.id)));

                if (x == null)
                    throw new ArgumentException("LendedBookDTOVer100", "該当するGUIDが存在しません。: " + _dto.id);

                x.Copy(_eventNumber, _dto);

                db.SaveChanges();
            }
        }

        public void Update(long _eventNumber, Domain.RentalSubDomain.Events.Book.ExtendedBookDTOVer100 _dto)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.本一覧.SingleOrDefault(item => item.Id.Equals(Guid.Parse(_dto.id)));

                if (x == null)
                    throw new ArgumentException("ExtendedBookDTOVer100", "該当するGUIDが存在しません。: " + _dto.id);

                x.Copy(_eventNumber, _dto);

                db.SaveChanges();
            }
        }

        public void Update(long _eventNumber, Domain.RentalSubDomain.Events.Book.ReturnedBookDTOVer100 _dto)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.本一覧.SingleOrDefault(item => item.Id.Equals(Guid.Parse(_dto.id)));

                if (x == null)
                    throw new ArgumentException("ReturnedBookDTOVer100", "該当するGUIDが存在しません。: " + _dto.id);

                x.Copy(_eventNumber, _dto);

                db.SaveChanges();
            }
        }

        public void Delete(string id)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                if (db.本一覧.SingleOrDefault(item => item.Id.Equals(Guid.Parse(id))) is 本Entity x)
                    db.本一覧.Remove(x);

                db.SaveChanges();
            }
        }
    }
}
