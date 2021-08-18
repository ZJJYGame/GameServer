using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;
using RedisDotNet;
using Cosmos;
using AscensionServer.Model;
using AscensionProtocol;

namespace AscensionServer
{
    public partial class AuctionManager
    {
        /// <summary>
        /// 获取某一种物品的所有物品信息
        /// </summary>
        /// <param name="auctionGoodsID"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<AuctionGoodsDTO>> GetAuctionGoodsList(int auctionGoodsID)
        {
            int goodsCount = 0;
            List<AuctionGoodsIndex> resultIndex;
            List<AuctionGoodsIndex> newResultIndex = new List<AuctionGoodsIndex>();
            List<AuctionGoodsDTO> auctionGoodsDTOList = new List<AuctionGoodsDTO>();
            var isHasValue = await RedisHelper.Hash.HashExistAsync("AuctionIndex", auctionGoodsID.ToString());
            if (!isHasValue)
            {
                Utility.Debug.LogInfo("redis拍卖行索引表不存在该ID");
            }
            else
            {
                Utility.Debug.LogInfo("redis拍卖行索引表存在该ID");
                resultIndex = await RedisHelper.Hash.HashGetAsync<List<AuctionGoodsIndex>>("AuctionIndex", auctionGoodsID.ToString());

                    for (int i = 0; i < resultIndex.Count; i++)
                    {
                        string auctionGoodsJson = await RedisHelper.String.StringGetAsync("AuctionGoods_" + resultIndex[i].RedisKey);
                        if (auctionGoodsJson != null)
                        {
                            auctionGoodsDTOList.Add(await RedisHelper.Hash.HashGetAsync<AuctionGoodsDTO>("AuctionGoodsData", resultIndex[i].RedisKey));
                            newResultIndex.Add(resultIndex[i]);
                        }
                    }
                    await RedisHelper.Hash.HashSetAsync<List<AuctionGoodsIndex>>("AuctionIndex", auctionGoodsID.ToString(), newResultIndex);
            }
            return auctionGoodsDTOList;
        }
        /// <summary>
        /// 获取目标范围的商品信息
        /// </summary>
        /// <returns></returns>
        public List<AuctionGoodsDTO> GetTargetAuctionGoodList(List<AuctionGoodsDTO> allAuctionGoodsDTOs,int startIndex,int count)
        {
            int goodsCount = allAuctionGoodsDTOs.Count;
            List<AuctionGoodsDTO> resultAuctionGoodsDTOList = new List<AuctionGoodsDTO>();
            if (startIndex + count <= goodsCount)
            {
                resultAuctionGoodsDTOList = allAuctionGoodsDTOs.GetRange(startIndex, count);
            }
            else
            {
                resultAuctionGoodsDTOList = allAuctionGoodsDTOs.GetRange(startIndex, goodsCount - startIndex);
            }
            return resultAuctionGoodsDTOList;
        }

        /// <summary>
        /// 玩家购买物品的事件处理
        /// </summary>
        /// <param name="buyAuctionGoodsDTO"></param>
        /// 0=》别人购买中
        /// 1=>商品不存在
        /// 2=》数量不足
        /// 3=》购买成功
        /// 4=》意外情况
        public async Task<byte> BuyAuctionGoods(AuctionGoodsDTO buyAuctionGoodsDTO,int buyerId)
        {
            if (occupyGuidHash.Contains(buyAuctionGoodsDTO.GUID))//别人购买中
            {
                return 0;
            }
            else
            {
                occupyGuidHash.Add(buyAuctionGoodsDTO.GUID);
                if(!await IsAuctionGoodsExist(buyAuctionGoodsDTO.GUID))//商品不存在，返回
                {
                    Utility.Debug.LogInfo("商品不存在=>"+ buyAuctionGoodsDTO.GUID);
                    return 1;
                }
                else//商品存在，尝试去处理拍卖品，扣除金钱，添加背包
                {
                    //拍卖行对应拍卖品
                    AuctionGoodsDTO auctionGoodsObj = null;
                    //AuctionGoodsDTO resultAuctionGoodsDTO = null;
                    auctionGoodsObj = await RedisHelper.Hash.HashGetAsync<AuctionGoodsDTO>("AuctionGoodsData", buyAuctionGoodsDTO.GUID);
                    if (buyAuctionGoodsDTO.Count <= auctionGoodsObj.Count)
                    {
                        //resultAuctionGoodsDTO = new AuctionGoodsDTO()
                        //{
                        //    GUID = auctionGoodsObj.GUID,
                        //    RoleID = auctionGoodsObj.RoleID,
                        //    GlobalID = auctionGoodsObj.GlobalID,
                        //    Price = auctionGoodsObj.Price,
                        //    ItemData = auctionGoodsObj.ItemData,
                        //    Count = buyAuctionGoodsDTO.Count
                        //};
                        auctionGoodsObj.Count -= buyAuctionGoodsDTO.Count;
                        if (auctionGoodsObj.Count == 0)//如果商品被买完了
                        {
                            await RedisHelper.KeyDeleteAsync("AuctionGoods_" + auctionGoodsObj.GUID);
                            await RedisHelper.Hash.HashDeleteAsync("AuctionGoodsData", buyAuctionGoodsDTO.GUID);
                            List<string> roleAuctionGuidList =await RedisHelper.Hash.HashGetAsync<List<string>>("RoleAuctionItems", auctionGoodsObj.RoleID.ToString());
                            string guidStr = roleAuctionGuidList.Find((p) => p == auctionGoodsObj.GUID);
                            roleAuctionGuidList.Remove(guidStr);
                            if (roleAuctionGuidList.Count != 0)
                            {
                                await RedisHelper.Hash.HashSetAsync("RoleAuctionItems", auctionGoodsObj.RoleID.ToString(), roleAuctionGuidList);
                            }
                            else
                            {
                                await RedisHelper.Hash.HashDeleteAsync("RoleAuctionItems", auctionGoodsObj.RoleID.ToString());
                            }
                            //处理同种商品的拍卖品列表
                            List<AuctionGoodsIndex> tempAuctionGoodIndexs = await RedisHelper.Hash.HashGetAsync<List<AuctionGoodsIndex>>("AuctionIndex", auctionGoodsObj.GlobalID.ToString());
                            AuctionGoodsIndex auctionGoodsIndex = tempAuctionGoodIndexs.Find((p) => p.RedisKey == auctionGoodsObj.GUID);
                            tempAuctionGoodIndexs.Remove(auctionGoodsIndex);
                            if (tempAuctionGoodIndexs.Count != 0)//购买后当前种物品索引数量 不为0
                            {
                                await RedisHelper.Hash.HashSetAsync("AuctionIndex", auctionGoodsObj.GlobalID.ToString(), tempAuctionGoodIndexs);
                            }
                            else
                            {
                                await RedisHelper.Hash.HashDeleteAsync("AuctionIndex", auctionGoodsObj.GlobalID.ToString());
                            }
                        }
                        else
                        {
                            await RedisHelper.Hash.HashSetAsync("AuctionGoodsData", buyAuctionGoodsDTO.GUID, auctionGoodsObj);
                        }
                    }
                    else//购买数量多于商品数量，返回
                    {
                        return 2;
                    }

                    //扣除买家金钱
                    int spiritStonePrice = auctionGoodsObj.Price * buyAuctionGoodsDTO.Count * (-100);
                    await ChangeRoleAssets(spiritStonePrice, 0, buyerId);
                    //增加卖家金钱
                    spiritStonePrice = auctionGoodsObj.Price * buyAuctionGoodsDTO.Count * (100);
                    await ChangeRoleAssets(spiritStonePrice, 0, auctionGoodsObj.RoleID);
                    //告知卖家
                    OperationData opdata = new OperationData();
                    opdata.OperationCode = (byte)OperationCode.SyncAuction;
                    Dictionary<byte, object> subResponseParametersDict = new Dictionary<byte, object>();
                    subResponseParametersDict.Add((byte)ParameterCode.SoldOutAuctionGoods, (byte)SyncAuctionType.AuctionGoodsBeBought);
                    opdata.DataMessage = subResponseParametersDict;
                    GameEntry.RoleManager.SendMessage(auctionGoodsObj.RoleID, opdata);
                    //物品赋予买家
                    InventoryManager.AddNewItem(buyerId, auctionGoodsObj.GlobalID, buyAuctionGoodsDTO.Count);
                    //todo
                    //将商品移除关注列表
                    return 3;
                }
            }
            return 4;
        }

        /// <summary>
        /// 改变玩家金钱
        /// </summary>
        /// <param name="spiritStonePrice"></param>
        /// <param name="xianYuPrice"></param>
        /// <param name="targetRoleId"></param>
        /// <returns></returns>
        public async Task ChangeRoleAssets(int spiritStonePrice,int xianYuPrice,int targetRoleId)
        {
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", targetRoleId);
            bool roleExist = NHibernateQuerier.Verify<Role>(nHCriteriaRoleID);
            bool roleAssetsExist = NHibernateQuerier.Verify<RoleAssets>(nHCriteriaRoleID);
            if (roleExist && roleAssetsExist)
            {
                var assetsServer = NHibernateQuerier.CriteriaSelect<RoleAssets>(nHCriteriaRoleID);
                assetsServer.SpiritStonesLow += spiritStonePrice;
                assetsServer.XianYu += xianYuPrice;
                NHibernateQuerier.Update<RoleAssets>(new RoleAssets() { RoleID = targetRoleId, SpiritStonesLow = assetsServer.SpiritStonesLow, XianYu = assetsServer.XianYu });
                await RedisHelper.Hash.HashSetAsync<RoleAssets>("RoleAssets", targetRoleId.ToString(), new RoleAssets() { RoleID = targetRoleId, SpiritStonesLow = assetsServer.SpiritStonesLow, XianYu = assetsServer.XianYu });
            }
        }

        /// <summary>
        /// 玩家上架拍卖品的事件
        /// </summary>
        public async Task PutAwayAuctionGoods(AuctionGoodsDTO putAwayGoods,int itemId)
        {
            putAwayGoods.GUID= Guid.NewGuid().ToString("N");
            string redisKey = "AuctionGoods_" + putAwayGoods.GUID;
            RedisHelper.String.StringSet(redisKey, "");

            if (await RedisHelper.Hash.HashExistAsync("AuctionIndex", putAwayGoods.GlobalID.ToString()))
            {
                List<AuctionGoodsIndex> tempAuctionGoods = await RedisHelper.Hash.HashGetAsync<List<AuctionGoodsIndex>>("AuctionIndex", putAwayGoods.GlobalID.ToString());
                tempAuctionGoods.Add(new AuctionGoodsIndex()
                {
                    RedisKey = putAwayGoods.GUID,
                    Price = putAwayGoods.Price
                });
                tempAuctionGoods.Sort();
                await RedisHelper.Hash.HashSetAsync("AuctionIndex", putAwayGoods.GlobalID.ToString(), tempAuctionGoods);
            }
            else
            {
                List<AuctionGoodsIndex> tempAuctionGoods = new List<AuctionGoodsIndex>();
                tempAuctionGoods.Add(new AuctionGoodsIndex()
                {
                    RedisKey = putAwayGoods.GUID,
                    Price = putAwayGoods.Price
                });
                await RedisHelper.Hash.HashSetAsync("AuctionIndex", putAwayGoods.GlobalID.ToString(), tempAuctionGoods);
            }
            //添加拍卖品数据
            if (!await RedisHelper.Hash.HashExistAsync("AuctionGoodsData", putAwayGoods.GUID.ToString()))
                await RedisHelper.Hash.HashSetAsync("AuctionGoodsData", putAwayGoods.GUID.ToString(), putAwayGoods);

            //更新玩家个人拍卖表数据
            List<string> roleAuctionItemList = new List<string>();
            if (await RedisHelper.Hash.HashExistAsync("RoleAuctionItems", putAwayGoods.RoleID.ToString()))
            {
                roleAuctionItemList = RedisHelper.Hash.HashGetAsync<List<string>>("RoleAuctionItems", putAwayGoods  .RoleID.ToString()).Result;
                roleAuctionItemList.Add(putAwayGoods.GUID);
                Utility.Debug.LogInfo("玩家拍卖列表存在key");
                await RedisHelper.Hash.HashSetAsync("RoleAuctionItems", putAwayGoods.RoleID.ToString(), roleAuctionItemList);
            }
            else
            {
                Utility.Debug.LogInfo("玩家拍卖列表不存在key!!!!");
                roleAuctionItemList.Add(putAwayGoods.GUID);
                await RedisHelper.Hash.HashSetAsync("RoleAuctionItems", putAwayGoods.RoleID.ToString(), roleAuctionItemList);
            }
            //移除玩家背包物品
            InventoryManager.Remove(putAwayGoods.RoleID, itemId);
            //扣除玩家手续费
            int spiritStonePrice = -putAwayGoods.Price * putAwayGoods.Count;
            await ChangeRoleAssets(spiritStonePrice, 0, putAwayGoods.RoleID);
        }
    }
}


