using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
namespace AscensionServer
{
    /// <summary>
    /// SQL Colum对应类，一个column对应一个传入数值
    /// </summary>
    public  class NHCriteria:IReference
    {
        public NHCriteria() { }
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public NHCriteria SetValue(string propertyName,object value)
        {
            this.PropertyName = propertyName;
            this.Value = value;
            return this;
        }
        public void Release()
        {
            PropertyName = string.Empty;
            Value = null;
        }
        public override string ToString()
        {
            return "PropertyName : " + PropertyName + " ;  Value" + Value.ToString();
        }
    }
}


