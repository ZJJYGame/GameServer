using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol.DTO
{
    public class TokenDto : DataTransferObject
    {
        public long Conv { get; set; }
        public string Token { get; set; }
        public override void Release()
        {
            Conv = 0;
            Token = null;
        }
    }
}
