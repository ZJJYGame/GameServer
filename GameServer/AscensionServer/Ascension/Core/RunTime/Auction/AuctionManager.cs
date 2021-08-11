using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol.DTO;
using RedisDotNet;

namespace AscensionServer
{
    [Module]
    public partial class AuctionManager: Cosmos. Module,IAuctionManager
    {
        public HashSet<string> occupyGuidHash;
        protected override void OnInitialization()
        {
            occupyGuidHash = new HashSet<string>();
        }
        public async Task<bool> IsAuctionGoodsExist(string Guid)
        {
            bool isExist;
            isExist = await RedisHelper.Hash.HashExistAsync("AuctionGoodsData", Guid);
            isExist=((await RedisHelper.String.StringGetAsync("AuctionGoods_" + Guid))!=null)&&isExist;
            return isExist;
        }
        //public async Task<bool> TryGetAuctionAuctionGoods()
        //{

        //}
    }
}


