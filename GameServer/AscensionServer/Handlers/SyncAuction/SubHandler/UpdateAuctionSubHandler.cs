using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol.DTO;
using Photon.SocketServer;
using AscensionServer.Model;
using Cosmos;
using RedisDotNet;
using StackExchange.Redis;

namespace AscensionServer
{
    public class UpdateAuctionSubHandler : SyncAuctionSubHandler
    {
        public override byte SubOpCode { get; protected set; } = (byte)SubOperationCode.Update;

        public override OperationResponse EncodeMessage(OperationRequest operationRequest)
        {
            BuyAuctionTypeEnum buyAuctionTypeEnum = BuyAuctionTypeEnum.Default;
            Utility.Debug.LogInfo("进入更新拍卖品事件");
            var dict = operationRequest.Parameters;
            string buyAuctionGoodsJson = Convert.ToString(Utility.GetValue(dict, (byte)ParameterCode.AddAuctionGoods));
            Dictionary<string, string> dataDict = Utility.Json.ToObject<Dictionary<string, string>>(buyAuctionGoodsJson);
            var buyAuctionGoodsObj = Utility.Json.ToObject<AuctionGoodsDTO>(dataDict["Data"]);
            int startIndex = Convert.ToInt32(dataDict["Start"]);
            int count = Convert.ToInt32(dataDict["Count"]);
            int roleID = Convert.ToInt32(dataDict["RoleID"]);
            int allGoodsCount = 0;

            string auctionGoodsJson = RedisHelper.String.StringGetAsync("AuctionGoods_" + buyAuctionGoodsObj.GUID).Result;
            Utility.Debug.LogInfo(roleID + "AuctionGoods_" + buyAuctionGoodsObj.GUID);
            Utility.Debug.LogInfo(roleID + auctionGoodsJson);

            AuctionGoodsDTO resultAuctionGoodsDTO = new AuctionGoodsDTO();

            if (auctionGoodsJson == null)//没有数据。失败
            {
                Utility.Debug.LogInfo(roleID + "没有数据。失败");
                buyAuctionTypeEnum = BuyAuctionTypeEnum.Empty;
            }
            else//获得数据，继续判断
            {
                Utility.Debug.LogInfo(roleID + "找到该商品");
                AuctionGoodsDTO auctionGoodsObj = null;
                try
                {
                    auctionGoodsObj = RedisHelper.Hash.HashGetAsync<AuctionGoodsDTO>("AuctionGoodsData", buyAuctionGoodsObj.GUID).Result;
                }
                catch
                {
                    Utility.Debug.LogInfo(roleID + "59行中断");
                }
                if (auctionGoodsObj == null)
                {
                    Utility.Debug.LogInfo(roleID + "63行中断");
                    buyAuctionTypeEnum = BuyAuctionTypeEnum.Empty;
                }
                else
                {
                    if (buyAuctionGoodsObj.Count <= auctionGoodsObj.Count)//购买数量足够
                    {
                        Utility.Debug.LogInfo(roleID + "购买数量足够");
                        resultAuctionGoodsDTO = new AuctionGoodsDTO()
                        {
                            GUID = auctionGoodsObj.GUID,
                            RoleID = auctionGoodsObj.RoleID,
                            GlobalID = auctionGoodsObj.GlobalID,
                            Price = auctionGoodsObj.Price,
                            ItemData = auctionGoodsObj.ItemData,
                            Count = buyAuctionGoodsObj.Count
                        };
                        auctionGoodsObj.Count -= buyAuctionGoodsObj.Count;
                        if (auctionGoodsObj.Count == 0)//买完了
                        {
                            Utility.Debug.LogInfo(roleID + "买完了");
                            RedisHelper.KeyDelete("AuctionGoods_" + auctionGoodsObj.GUID);
                            RedisHelper.Hash.HashDelete("AuctionGoodsData", buyAuctionGoodsObj.GUID);
                            List<string> roleAuctionGuidList = RedisHelper.Hash.HashGetAsync<List<string>>("RoleAuctionItems", auctionGoodsObj.RoleID.ToString()).Result;
                            string guidStr = roleAuctionGuidList.Find((p) => p == auctionGoodsObj.GUID);
                            roleAuctionGuidList.Remove(guidStr);
                            if (roleAuctionGuidList.Count != 0)
                            {
                                RedisHelper.Hash.HashSet("RoleAuctionItems", auctionGoodsObj.RoleID.ToString(), roleAuctionGuidList);
                            }
                            else
                            {
                                RedisHelper.Hash.HashDelete("RoleAuctionItems", auctionGoodsObj.RoleID.ToString());
                            }
                            List<AuctionGoodsIndex> tempAuctionGoodIndexs = RedisHelper.Hash.HashGetAsync<List<AuctionGoodsIndex>>("AuctionIndex", auctionGoodsObj.GlobalID.ToString()).Result;
                            AuctionGoodsIndex auctionGoodsIndex = tempAuctionGoodIndexs.Find((p) => p.RedisKey == auctionGoodsObj.GUID);
                            tempAuctionGoodIndexs.Remove(auctionGoodsIndex);
                            if (tempAuctionGoodIndexs.Count != 0)//购买后当前种物品索引数量 不为0
                            {
                                RedisHelper.Hash.HashSet("AuctionIndex", auctionGoodsObj.GlobalID.ToString(), tempAuctionGoodIndexs);
                            }
                            else
                            {
                                RedisHelper.Hash.HashDelete("AuctionIndex", auctionGoodsObj.GlobalID.ToString());
                            }
                        }
                        else
                        {
                            RedisHelper.Hash.HashSet("AuctionGoodsData", buyAuctionGoodsObj.GUID, auctionGoodsObj);
                        }
                        Utility.Debug.LogInfo(roleID + "主备发送成功信息");
                        buyAuctionTypeEnum = BuyAuctionTypeEnum.Success;
                    }
                    else//购买数量不足
                    {
                        buyAuctionTypeEnum = BuyAuctionTypeEnum.NotEnougth;
                    }
                }

            }

            List<AuctionGoodsDTO> tempAuctionGoodsDTOs = GetReturnGoodsList(buyAuctionGoodsObj.GlobalID, ref startIndex, count, ref allGoodsCount);
            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            resultDict.Add("Data", Utility.Json.ToJson(resultAuctionGoodsDTO));
            resultDict.Add("List", Utility.Json.ToJson(tempAuctionGoodsDTOs));
            resultDict.Add("StartIndex", startIndex.ToString());
            resultDict.Add("Count", allGoodsCount.ToString());
            Utility.Debug.LogInfo(roleID + Utility.Json.ToJson(resultDict));

            switch (buyAuctionTypeEnum)
            {
                case BuyAuctionTypeEnum.Success:

                    Utility.Debug.LogError(roleID + "添加");
                    Utility.Debug.LogInfo(roleID + "购买物品成功");
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.AddAuctionGoods, Utility.Json.ToJson(resultDict));
                        operationResponse.ReturnCode = (short)ReturnCode.Success;
                    });
                    break;
                case BuyAuctionTypeEnum.Empty:
                    Utility.Debug.LogError(roleID + "添加");
                    Utility.Debug.LogInfo(roleID + "找不到该商品");
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.Inventory, Utility.Json.ToJson(resultDict));
                        operationResponse.ReturnCode = (short)ReturnCode.Empty;
                    });
                    break;
                case BuyAuctionTypeEnum.NotEnougth:
                    Utility.Debug.LogError(roleID + "添加");
                    Utility.Debug.LogInfo(roleID + "物品数量不足");
                    SetResponseParamters(() =>
                    {
                        subResponseParameters.Add((byte)ParameterCode.Auction, Utility.Json.ToJson(resultDict));
                        operationResponse.ReturnCode = (short)ReturnCode.Fail;
                    });
                    break;
                case BuyAuctionTypeEnum.Default:
                    Utility.Debug.LogInfo("拍卖行判断出现异常");
                    break;
            }
            Utility.Debug.LogInfo(roleID + "更新拍卖行事件结束");
            return operationResponse;
        }

        void AddRoleAssets(AuctionGoodsDTO buyAuctionGoodsDTO)
        {
            NHCriteria nHCriteriaRoleID =ReferencePool.Accquire<NHCriteria>().SetValue("RoleID", buyAuctionGoodsDTO.RoleID);
            bool roleExist = NHibernateQuerier.Verify<Role>(nHCriteriaRoleID);
            bool roleAssetsExist = NHibernateQuerier.Verify<RoleAssets>(nHCriteriaRoleID);
            if (roleExist && roleAssetsExist)
            {
                var assetsServer = NHibernateQuerier.CriteriaSelect<RoleAssets>(nHCriteriaRoleID);
                assetsServer.SpiritStonesLow += buyAuctionGoodsDTO.Price * buyAuctionGoodsDTO.Count * 100;
                NHibernateQuerier.Update<RoleAssets>(new RoleAssets() { RoleID = buyAuctionGoodsDTO.RoleID, SpiritStonesLow = assetsServer.SpiritStonesLow, XianYu = assetsServer.XianYu });
                RedisHelper.Hash.HashSet<RoleAssets>("RoleAssets", buyAuctionGoodsDTO.RoleID.ToString(), new RoleAssets() { RoleID = buyAuctionGoodsDTO.RoleID, SpiritStonesLow = assetsServer.SpiritStonesLow, XianYu = assetsServer.XianYu });
            }
        }

        List<AuctionGoodsDTO> GetReturnGoodsList(int id, ref int startIndex, int count, ref int allGoodsCount)
        {
            List<AuctionGoodsDTO> auctionGoodsDTOList = new List<AuctionGoodsDTO>();
            if (!RedisHelper.Hash.HashExistAsync("AuctionIndex", id.ToString()).Result)
            {
                Utility.Debug.LogInfo("当前种类商品不存在");
                return auctionGoodsDTOList;
            }

            //todo 表不存在，直接返回
            List<AuctionGoodsIndex> result = RedisHelper.Hash.HashGet<List<AuctionGoodsIndex>>("AuctionIndex", id.ToString());
            allGoodsCount = result.Count;
            if (result.Count == 0)
            {
                return auctionGoodsDTOList;
            }
            if (startIndex >= result.Count)
            {
                startIndex = ((result.Count - 1) / count) * count;
            }
            for (int i = 0; i < result.Count; i++)
            {
                auctionGoodsDTOList.Add(RedisHelper.Hash.HashGetAsync<AuctionGoodsDTO>("AuctionGoodsData", result[i].RedisKey).Result);
            }
            if (startIndex + count <= result.Count)
            {
                auctionGoodsDTOList = auctionGoodsDTOList.GetRange(startIndex, count);
            }
            else
            {
                auctionGoodsDTOList = auctionGoodsDTOList.GetRange(startIndex, result.Count - startIndex);
            }
            return auctionGoodsDTOList;
        }

        enum BuyAuctionTypeEnum
        {
            //购买成功
            Success = 0,
            //商品不存在
            Empty = 1,
            //商品数量不足
            NotEnougth = 2,
            Default = 3,
        }
    }
}


