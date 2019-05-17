using System;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using Domain.DeliverySubDomain;


namespace Factory
{
    public class RentalSubDomain本Factory: Domain.RentalSubDomain.I本Factory
    {
        public Domain.RentalSubDomain.本 Create(string _id, string _書籍のID)
        => new Domain.RentalSubDomain.本(本のID.Create(_id), 書籍のID.Create(_書籍のID))
            {
                版数 = 版数.第一版
            };
    }

    public class DeliverySubDomain本Factory: Domain.DeliverySubDomain.I本Factory
    {
        public Domain.DeliverySubDomain.本 Create(string _id, string _書籍のID, string _利用者のID)
        => new Domain.DeliverySubDomain.本(本のID.Create(_id), 書籍のID.Create(_書籍のID))
            {
                利用者のID = 利用者のID.Create(_利用者のID)
            };
    }
}
