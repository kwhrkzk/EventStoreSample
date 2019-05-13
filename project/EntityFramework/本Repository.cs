using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Microsoft.EntityFrameworkCore;
using Application;

namespace EntityFramework
{
    public class 本Repository : I本Repository
    {
        public void Upsert(long _eventNumber, 本 _本)
        {
            using (var db = new MyContext())
            {
                db.Database.EnsureCreated();

                if (db.本一覧.SingleOrDefault(item => item.Id.Equals(_本.GUID)) is 本Entity x)
                    x.Copy(_eventNumber, _本);
                else
                    db.本一覧.Add(_本.Convert(_eventNumber));

                db.SaveChanges();
            }
        }

        public void Delete(本のID _本のID)
        {
            using (var db = new MyContext())
            {
                if (db.本一覧.SingleOrDefault(item => item.Id.Equals(_本のID.ID)) is 本Entity x)
                {
                    db.本一覧.Remove(x);

                    db.SaveChanges();
                }
            }
        }
    }
}
