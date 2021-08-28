using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
namespace AscensionServer
{
    //========================================
    //采集：
    //         玩家采集流程，在LevelResEntity中，是直接判断当
    //前资源是否在可采集的列表中。若可采集，则将可采集容器中
    //的对象移至不可采集容器中，并返回true。否则就返回false。
    //
    //战斗：
    //          1、玩家与怪物进行战斗时，会对怪进行占用。
    //          2、当怪被占用时其他玩家则无法与这个被占用的怪进行战斗。
    //          3、被占用的怪会进入pending容器中，直到战斗成功、
    //          失败的结果返回，再通知玩家显示怪的状态。
    //          4、战斗失败则玩家触发死亡，怪进行保留。
    //          5、战斗成功则玩家/获得奖励，怪被移至不可战斗容器中。
    //========================================
    public class LevelResEntity : IDisposable
    {
        static ConcurrentPool<LevelResEntity> levelResEntityPool;
        static LevelResEntity()
        {
            levelResEntityPool = new ConcurrentPool<LevelResEntity>(() => new LevelResEntity(), lre => { lre.Dispose(); });
        }

        public int LevelId { get; private set; }
        public LevelTypeEnum LevelType { get; private set; }
        LevelResEntityDataProxy levelResEntityDataProxy = new LevelResEntityDataProxy();

        /// <summary>
        /// RoleId---LevelResObject
        /// </summary>
        Dictionary<int, LevelResObject> roleResObjDict = new Dictionary<int, LevelResObject>();
        /// <summary>
        /// index---collectable
        /// </summary>
        public Dictionary<int, FixCollectable> CollectableDict { get { return levelResEntityDataProxy.CollectableDict; } }
        public Dictionary<int, FixCombatable> CombatableDict { get { return levelResEntityDataProxy.CombatableDict; } }
        Action<int, LevelResObject> combatSuccess;
        Action<int, LevelResObject> combatFailure;
        public void InitEntityRes()
        {
            GameEntry.DataManager.TryGetValue<MapResSpanwInfoData>(out var resSpawnInfoData);
            levelResEntityDataProxy.InitEntityRes(resSpawnInfoData);
        }
        public void SetCallback(Action<int,LevelResObject>combatSuccess, Action<int, LevelResObject> combatFailure)
        {
            this.combatSuccess = combatSuccess;
            this.combatFailure = combatFailure;
        }
        /// <summary>
        /// 表示进入战斗是否成功；
        /// 如果进入战斗成功，则这只怪会进入pending状态，其他玩家就无法再与之战斗；
        /// </summary>
        /// <param name="index">资源生成时的id</param>
        /// <param name="gId">全局id</param>
        /// <param name="eleId">元素id</param>
        /// <returns>是否进入战斗成功</returns>
        public bool Combat(int roleId, int index, int gId, int eleId)
        {
            var enter = levelResEntityDataProxy.Combat(index, gId, eleId);
            if (enter)
            {
                StartCombat(roleId, index, gId, eleId);
            }
            return enter;
        }
        /// <summary>
        /// 采集；
        /// </summary>
        /// <param name="index">资源生成时的id</param>
        /// <param name="gId">全局id</param>
        /// <param name="eleId">元素id</param>
        /// <returns>是否采集成功</returns>
        public bool Gather(int index, int gId, int eleId)
        {
            return levelResEntityDataProxy.Gather(index, gId, eleId);
        }
        public void BroadCast2AllS2C(OperationData opData)
        {
            GameEntry.LevelManager.SendMessageToLevelS2C(LevelType, LevelId, opData);
        }
        public void Dispose()
        {
            LevelId = 0;
            LevelType = LevelTypeEnum.None;
        }
        public static LevelResEntity Create(LevelTypeEnum levelType, int levelId)
        {
            var lre = levelResEntityPool.Spawn();
            lre.LevelId = levelId;
            lre.LevelType = levelType;
            return lre;
        }
        public static void Release(LevelResEntity levelResEntity)
        {
            levelResEntityPool.Despawn(levelResEntity);
        }
        void StartCombat(int roleId, int index, int gId, int eleId)
        {
            var levelResObj = new LevelResObject() { EleId = eleId, Index = index, GId = gId };
            roleResObjDict.Add(roleId, levelResObj);
            var roleInfo = GameEntry.BattleRoomManager.CreateRoom(roleId, new List<int>() { gId });
            roleInfo.OnComplete(OnCombatComplete);
        }
        void OnCombatComplete(Dictionary<BattleCharacterType, List<BattleResultInfo>> rstInfos)
        {
            if (rstInfos == null)
            {
                Utility.Debug.LogInfo("OnCombatComplete rstInfos is invalid !");
                return;
            }
            var playerInfo = rstInfos[BattleCharacterType.Player];
            //var aiInfo = rstInfos[BattleCharacterType.AI];
            var battleRst = playerInfo[0];
            var roleId = battleRst.CharacterId;
            var levelResObj = roleResObjDict[roleId];
            if (battleRst.IsWin)
            {
                OnCombatSuccess(roleId, levelResObj);
            }
            else
            {
                OnCombatFailure(roleId, levelResObj);
            }
        }
        void OnCombatSuccess(int roleId, LevelResObject levelResObject)
        {
             levelResEntityDataProxy.OnCombatSuccess(levelResObject.Index, levelResObject.GId, levelResObject.EleId);
            combatSuccess.Invoke(roleId, levelResObject);
        }
        void OnCombatFailure(int roleId, LevelResObject levelResObject)
        {
            levelResEntityDataProxy.OnCombatFailure(levelResObject.Index, levelResObject.GId, levelResObject.EleId);
            combatFailure.Invoke(roleId, levelResObject);
        }
    }
}
