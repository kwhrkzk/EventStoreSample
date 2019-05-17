using System;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;

namespace Factory
{
    public class 利用者Factory: I利用者Factory
    {
        public 利用者 Create(string _利用者のID, string _苗字, string _名前) => new 利用者(利用者のID.Create(_利用者のID)) { 氏名 = 氏名.Create(_苗字, _名前) };
    }
}
