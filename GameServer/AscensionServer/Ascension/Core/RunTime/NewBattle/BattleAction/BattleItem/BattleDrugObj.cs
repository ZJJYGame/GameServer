using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol.DTO;

namespace AscensionServer
{
    public class BattleDrugObj : BattleItemObj
    {

        DrugData drugData;

        public override bool ActionCost(out ActionCost actionCost)
        {
            throw new NotImplementedException();
        }

        public override bool CanUseAction(BattleCharacterEntity target)
        {
            throw new NotImplementedException();
        }
        public BattleDrugObj(int actionId, BattleCharacterEntity battleCharacterEntity) : base(actionId, battleCharacterEntity)
        {
            GameEntry.DataManager.TryGetValue<Dictionary<int, DrugData>>(out var drugDataDict);
            drugData = drugDataDict[actionId];
            NowCold = 0;
            MaxCold = drugData.Drug_CD;
        }
    }
}
