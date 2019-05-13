using System;
using Domain;

namespace Factory
{
    public class 利用者Factory: I利用者Factory
    {
        public 利用者 Create(string _苗字, string _名前) => new 利用者(利用者のID.New()) { 氏名 = 氏名.Create(_苗字, _名前) };
    }
}
