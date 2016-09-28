using Metrology.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Server.SafetyOrgsUtility
{
    internal interface ISafetyOrgRepository
    {
        void AddSafetyOrg(Guid orgId, string markCodes, Guid? roadId);
        void UpdateSafetyOrg(Guid orgId, string markCodes, Guid? roadId);
        void RemoveSafetyOrg(Guid orgId);
        IEnumerable<SafetyOrgLink> GetSafetyOrgsLinks();
        SafetyOrgAggregate GetSafetyOrgAggregate(Guid orgId);
    }
}
