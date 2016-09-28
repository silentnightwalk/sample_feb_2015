using Metrology.Shared.Core.Server.SafetyOrgsUtility.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Server.SafetyOrgsUtility
{
    internal class MarkedSafetyEvent: IEvent
    {
        public Guid OrgId { get; private set; }
        public string MarkCodes { get; private set; }
        public Guid? RoadId { get; private set; }

        public MarkedSafetyEvent(Guid orgId, string markCodes, Guid? roadId)
        {
            OrgId = orgId;
            MarkCodes = markCodes;
            RoadId = roadId;
        }
    }
}
