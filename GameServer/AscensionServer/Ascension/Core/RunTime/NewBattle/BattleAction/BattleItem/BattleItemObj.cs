using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleItemObj : BattleActionObjBase
    {
        //物品需消耗背包中的物品
        public override bool ActionCost(out ActionCost actionCost)
        {
            throw new NotImplementedException();
        }

        public override bool CanUseAction(BattleCharacterEntity target)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 检查背包物品数量或耐久是否足够
        /// </summary>
        /// <returns></returns>
        protected bool CheckCountEnougth()
        {
            return true;
        }
        /// <summary>
        ///减少背包中道具的数量或者耐久
        /// </summary>
        protected void ReduceBagCount()
        {

        }

        public BattleItemObj(int actionId, BattleCharacterEntity battleCharacterEntity) : base(actionId, battleCharacterEntity)
        {

        }
    }
}
