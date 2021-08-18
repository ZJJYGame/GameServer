//================================================
// 所有模型映射对象的抽象基类，使用了IReference接口，实现引用池生成
//
//================================================
using Cosmos;
namespace AscensionServer.Model
{
    public abstract class DataObject : IReference
    {
        public abstract void Release();
    }
}


