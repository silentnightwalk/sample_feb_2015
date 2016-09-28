using Metrology.Shared.Core.Server.SafetyOrgsUtility.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Server.SafetyOrgsUtility
{
    internal class SafetyOrgUpdatedEvent: IEvent
    {
        public Guid OrgId { get; private set; }
        public string MarkCodes { get; private set; }
        public Guid? RoadId { get; private set; }

        public SafetyOrgUpdatedEvent(Guid orgId, string markCodes, Guid? roadId)
        {
            OrgId = orgId;
            MarkCodes = markCodes;
            RoadId = roadId;
        }
    }
}
