using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    [Serializable]
    public class UserInfoDto : DataTransferObject
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public string Date{ get; set; }
        public string DeviceUID { get; set; }
        public override void Release()
        {
            Account = null;
            Password = null;
            Date = null;
            DeviceUID = null;
        }
        public override string ToString()
        {
            string str = $"Account:{Account};Password:{Password};Date:{Date};DeviceUID:{DeviceUID};";
            return str;
        }
    }
}
