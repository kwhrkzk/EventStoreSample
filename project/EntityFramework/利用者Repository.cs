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
    public class 利用者Repository : I利用者Repository
    {
        public void Insert(long _eventNumber, 利用者 _利用者)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.利用者一覧.SingleOrDefault(item => item.Id.Equals(_利用者.GUID)) as 利用者Entity;

                if (x == null)
                    db.利用者一覧.Add(_利用者.Convert(_eventNumber));

                db.SaveChanges();
            }
        }

        public void Update(long _eventNumber, Domain.RentalSubDomain.Events.User.LendedBookDTOVer100 _dto)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.利用者一覧.SingleOrDefault(item => item.Id.Equals(Guid.Parse(_dto.id))) as 利用者Entity;

                if (x == null)
                    throw new ArgumentException("LendedBookDTOVer100", "該当するGUIDが存在しません。: " + _dto.id);

                x.Copy(_eventNumber, _dto);

                db.SaveChanges();
            }
        }

        public void Update(long _eventNumber, Domain.RentalSubDomain.Events.User.ReturnedBookDTOVer100 _dto)
        {
            using (var db = new RentalProjectorContext())
            {
                db.Database.EnsureCreated();

                var x = db.利用者一覧.SingleOrDefault(item => item.Id.Equals(Guid.Parse(_dto.id))) as 利用者Entity;

                if (x == null)
                    throw new ArgumentException("ReturnedBookDTOVer100", "該当するGUIDが存在しません。: " + _dto.id);

                x.Copy(_eventNumber, _dto);

                db.SaveChanges();
            }
        }
    }
}
