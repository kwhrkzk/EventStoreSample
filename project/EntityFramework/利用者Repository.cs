using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Microsoft.EntityFrameworkCore;
using Application;

namespace EntityFramework
{
    public class 利用者Repository : I利用者Repository
    {
        public void Upsert(long _eventNumber, 利用者 _利用者)
        {
            using (var db = new MyContext())
            {
                db.Database.EnsureCreated();

                if (db.利用者一覧.SingleOrDefault(item => item.Id.Equals(_利用者.GUID)) is 利用者Entity x)
                    x.Copy(_eventNumber, _利用者);
                else
                    db.利用者一覧.Add(_利用者.Convert(_eventNumber));

                db.SaveChanges();
            }
        }
    }
}
