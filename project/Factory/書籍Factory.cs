using System;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;


namespace Factory
{
    public class 書籍Factory: I書籍Factory
    {
        public 書籍 Create(string _id, string _title, string _isbn)
        => new 書籍(書籍のID.Create(_id), ISBN.Create(_isbn))
            {
                タイトル = タイトル.Create(_title)
            };
    }
}
