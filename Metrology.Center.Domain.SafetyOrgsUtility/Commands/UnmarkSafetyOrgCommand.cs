using Metrology.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Shared.Core.SafetyOrgsUtility
{
    [WebApiPath("api/orgs/UnmarkSafetyOrg")]
    public class UnmarkSafetyOrgCommand: IDataCommand
    {
        public Guid OrgId { get; private set; }

        public UnmarkSafetyOrgCommand(Guid orgId)
        {
            OrgId = orgId;
        }
    }
}
