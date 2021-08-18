using AscensionProtocol;
using Cosmos;
using Photon.SocketServer;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace AscensionServer
{
    [Implementer]
    public class PhotonMessageHelper : INetworkMessageHelper
    {
        ConcurrentDictionary<byte, IHandler> handlerDict;
        public PhotonMessageHelper()
        {
            handlerDict = new ConcurrentDictionary<byte, IHandler>();
            InitHandler();
        }
        public object EncodeMessage(object message)
        {
            OperationRequest request = message as OperationRequest;
            IHandler handler;
            var result = handlerDict.TryGetValue(request.OperationCode, out handler);
            if (!result)
                return handlerDict[0].EncodeMessage(request);//使用default handler
            else
                return handler.EncodeMessage(request);
        }
        void InitHandler()
        {
            var handlers = Utility.Assembly.GetInstancesByAttribute<InheritedAttribute, IHandler>(true);
            int length = handlers.Length;
            for (int i = 0; i < length; i++)
            {
                handlers[i].OnInitialization();
                handlerDict.TryAdd(handlers[i].OpCode, handlers[i]);
            }
        }
    }
}


