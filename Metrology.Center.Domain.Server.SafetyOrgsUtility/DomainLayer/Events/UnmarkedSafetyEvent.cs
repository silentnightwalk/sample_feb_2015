using Metrology.Shared.Core.Server.SafetyOrgsUtility.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Server.SafetyOrgsUtility
{
    internal class UnmarkedSafetyEvent: IEvent
    {
        public Guid OrgId { get; private set; }

        public UnmarkedSafetyEvent(Guid orgId)
        {
            OrgId = orgId;
        }
    }
}
