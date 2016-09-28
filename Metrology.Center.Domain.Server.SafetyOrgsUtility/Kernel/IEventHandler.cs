using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Shared.Core.Server.SafetyOrgsUtility.Kernel
{
    internal interface IEventHandler<T>
        where T:IEvent
    {
        void Handle(T @event);
    }

}
