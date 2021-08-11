using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer.Model
{
    public class PetAbilityPoint : DataObject
    {
        public virtual int ID { get; set; }
        public virtual int SlnNow { get; set; }
        public virtual bool IsUnlockSlnThree { get; set; }
        public virtual string AbilityPointSln { get; set; }
        public PetAbilityPoint()
        {
            SlnNow = 0;
            IsUnlockSlnThree = false;
            AbilityPointSln = "{}";
        }
        public override void Release()
        {
            ID = -1;
            SlnNow = 0;
            IsUnlockSlnThree = false;
            AbilityPointSln ="{}";
        }
    }
}


