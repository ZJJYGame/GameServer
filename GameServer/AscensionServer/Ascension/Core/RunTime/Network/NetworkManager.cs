using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Cosmos;
namespace AscensionServer
{
    [Module]
    public class NetworkManager: Module ,INetworkManager
    {
        INetworkMessageHelper messageHelper;

        public object EncodeMessage(object message)
        {
            return messageHelper.EncodeMessage(message);
        }
        protected override void OnInitialization()
        {
            InitHelper();
        }
        void InitHelper()
        {
            var helper = Utility.Assembly.GetInstanceByAttribute<ImplementerAttribute, INetworkMessageHelper>(GetType().Assembly, true);
            messageHelper = helper;
        }
    }
}


