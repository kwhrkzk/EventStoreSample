using System;
using Domain;

namespace Factory
{
    public class 書籍Factory: I書籍Factory
    {
        public 書籍 Create(タイトル _タイトル, ISBN _isbn)
        => new 書籍(書籍のID.New(), _isbn)
            {
                タイトル = _タイトル
            };
    }
}
