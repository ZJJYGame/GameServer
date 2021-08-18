/*
*Author   Don
*Since 	2020-04-17
*Description 玩家用映射模型
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class UserDTO: DataTransferObject
    {
        public virtual string Account { get; set; }
        public virtual string Password { get; set; }
        public virtual string UUID { get; set; }
        public override void Release()
        {
            Account = null;
            Password = null;
            UUID = null;
        }
        public override string ToString()
        {
            return "###### Account : " + Account + " ; Password : " + Password
+ "; UUID: " + UUID + "######";
        }
    }

}
    