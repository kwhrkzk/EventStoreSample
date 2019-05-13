using System;
using Domain;

namespace Factory
{
    public class 本Factory: I本Factory
    {
        public 本 Create(書籍のID _id)
        => new 本(本のID.New(), _id)
            {
                版数 = 版数.第一版
            };
    }
}
