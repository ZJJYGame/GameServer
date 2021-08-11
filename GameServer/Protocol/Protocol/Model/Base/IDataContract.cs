using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol
{
    [Union(0, typeof(CmdInput))]
    [Union(1, typeof(SessionRoleIdPair))]
    [Union(3, typeof(C2SContainer))]
    [Union(4, typeof(S2CInput))]
    [Union(5, typeof(S2CContainer))]
    [Union(6, typeof(DataParameters))]
    [Union(7, typeof(SessionRoleIds))]
    [Union(8, typeof(C2SSkillInput))]
    //[Union(9, typeof(C2STransformInput))]
    [Union(10, typeof(C2SFlyMagicToolInput))]
    [Union(12, typeof(C2SMapRes))]
    [Union(13, typeof(S2CMapResource))]
    //[Union(14, typeof(ICmdDataContract))]
    public interface IDataContract { }
}
