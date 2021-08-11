using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    [Serializable]
    public class School : DataObject
    {
        public virtual int ID { get; set; }
        public virtual int SchoolID { get; set; }
        public virtual int ContributionNow { get; set; }
        public virtual int Hatred { get; set; }
        public virtual int TreasureAtticID { get; set; }
        public virtual int SutrasAtticID { get; set; }
        public virtual int RankingListID { get; set; }
        public virtual int SchoolJob { get; set; }
        public virtual int GetContributions { get; set; }
        public virtual int ContributionHistory { get; set; }
        public virtual bool IsSignin { get; set; }
        public School()
        {
            SchoolID = 900;
            ContributionNow = 0;
            Hatred = 0;
            TreasureAtticID = 0;
            SutrasAtticID = 0;
            RankingListID = 0;
            SchoolJob = 0;
            GetContributions = 0;
            ContributionHistory = 0;
            IsSignin = false;
        }

        public override void Release()
        {
            ID = -1;
            SchoolID = 0;
            ContributionNow = 0;
            Hatred = 0;
            TreasureAtticID = 0;
            SutrasAtticID = 0;
            RankingListID = 0;
            SchoolJob = 0;
            GetContributions = 0;
            ContributionHistory = 0;
            IsSignin = false;
        }
    }
}


