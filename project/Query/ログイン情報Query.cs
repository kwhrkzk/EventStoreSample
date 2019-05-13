using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Microsoft.EntityFrameworkCore;
using Application;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Query
{

    public class ログイン情報Query : Iログイン情報Query
    {
        private ログイン情報DTO Convert(利用者Entity item)
        => new ログイン情報DTO
        {
            ID = 利用者のID.Create(item.Id),
            EventNumber = item.EventNumber,
            氏名 = 氏名.Create(item.苗字, item.名前) 
        };

        public ログイン情報DTO First()
        {
            using (var db = new MyContext())
            {
                return db.利用者一覧.Select(Convert).First();
            }
        }
    }
}
