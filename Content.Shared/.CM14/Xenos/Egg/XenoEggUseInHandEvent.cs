using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robust.Shared.Serialization;

namespace Content.Shared.CM14.Xenonids.Egg;

public sealed partial class XenoEggUseInHandEvent : HandledEntityEventArgs
{
    public NetEntity UsedEgg;

    public XenoEggUseInHandEvent(NetEntity usedEgg)
    {
        UsedEgg = usedEgg;
    }
}
