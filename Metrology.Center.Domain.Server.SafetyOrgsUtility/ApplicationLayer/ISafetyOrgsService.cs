using Metrology.Shared.Core;
using System;
using System.Collections.Generic;
namespace Metrology.Server.SafetyOrgsUtility
{
    public interface ISafetyOrgsService
    {
        void MarkOrgAsSafety(Guid organizationGuid, string markCodes);
        void UnmarkSafetyOrg(Guid organizationGuid);
        void UpdateSafetyOrg(Guid organizationGuid, string markCodes);
        IEnumerable<SafetyOrgTreeItem> GetSafetyOrganizationsTree();
    }
}
