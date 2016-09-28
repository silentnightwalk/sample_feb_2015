using Metrology.Server.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Center.Domain
{

    //класс перенесен из другого проекта, просто для приведения примера кода

    public class HeadDb_GetSafetyOrganizationsTree
    {
        private readonly IDistributedContext DistributedContext;
        private readonly IHeadDbUnitOfWorkFactory HeadDbFactory;

        public HeadDb_GetSafetyOrganizationsTree(IDistributedContext distributedContext, IHeadDbUnitOfWorkFactory headDbFactory)
        {
            DistributedContext = distributedContext;
            HeadDbFactory = headDbFactory;
        }

        public SafetyOrgTreeItem[] Execute()
        {
            var allOrgsQuery = new WebQuery_GetOrganizations();
            var allOrgs = DistributedContext.Execute(allOrgsQuery);
            var organizationArray = Organization.Traverse(allOrgs).ToArray();

            IEnumerable<SafetyOrgLink> links = null;

            using (var db = HeadDbFactory.Create())
            {
                var query = new HeadDB_GetSafetyOrgsLinks();
                links = db.Execute(query).ToArray();
            }

            if (links.Count() == 0)
            {
                return null;
            }

            var safetyOrgs = links.Join(organizationArray, x => x.Id, x => x.Id, (l, r) => r).ToArray();
            var targets = new HashSet<Organization>(safetyOrgs);

            foreach (var org in safetyOrgs)
            {
                Organization current = org;
                Organization parent = null;
                do
                {
                    parent = organizationArray.FirstOrDefault(x => x.Id == current.ParentId);

                    if (parent != null)
                    {
                        targets.Add(parent);
                    }

                    current = parent;
                }
                while (parent != null);
            }

            var wrappedQuery = from x in targets
                               let safetyLink = links.Where(l => l.Id == x.Id).FirstOrDefault()
                               select new SafetyOrgTreeItem()
                               {
                                   Id = x.Id,
                                   Mark = safetyLink == null ? null : safetyLink.Mark,
                                   OrganizationType = x.OrganizationType,
                                   Address = x.Address,
                                   Name = x.Name,
                                   Children = null,
                                   ParentId = x.ParentId,
                                   AssistanceCount = x.AssistanceCount,
                                   DeviceCount = x.DeviceCount,
                                   IsSafetyOrganization = (safetyLink != null)
                               };

            var wrapped = wrappedQuery.ToArray();

            var groupped = wrapped.GroupBy(x => wrapped.FirstOrDefault(p => p.Id == x.ParentId)).ToArray();

            Array.ForEach(
                groupped,
                g =>
                {
                    var children = g.ToArray();
                    if (g.Key != null)
                    { 
                        g.Key.Children = children;
                    }
                    else
                    {
                        Array.ForEach(children, x => x.ParentId = null);
                    }
                }
            );

            var result = wrapped.Where(x => x.ParentId == null).ToArray();
            return result;
        }
    }
}
