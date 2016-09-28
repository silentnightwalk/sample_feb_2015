using Metrology.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Shared.Core.SafetyOrgsUtility
{
    [WebApiPath("api/orgs/MarkAsSafetyOrg")]
    public class MarkAsSafetyOrgCommand : IDataCommand
    {
        public Guid OrgId { get; protected set; }
        public string MarkCode { get; protected set; }

        public MarkAsSafetyOrgCommand(Guid orgId, string markCode)
        {
            OrgId = orgId;
            MarkCode = markCode;
        }
    }
}
