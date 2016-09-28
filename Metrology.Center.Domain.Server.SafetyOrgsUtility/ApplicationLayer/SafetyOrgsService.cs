using Metrology.Server.Persistence;
using Metrology.Shared.Core.Server.SafetyOrgsUtility.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Metrology.Server.SafetyOrgsUtility;
using Metrology.Shared.Core;

namespace Metrology.Server.SafetyOrgsUtility
{
    internal class SafetyOrgsService : ISafetyOrgsService
    {
        private readonly ISafetyOrgRepository _repository; 
        private readonly DomainEventsHandler _domainEventsHandler;
        private readonly IHeadDbUnitOfWorkFactory _HeadDbUnitOfWorkFactory;
        private readonly IDistributedContext _DistributedContext;

        public SafetyOrgsService(IDistributedContext ctx, IHeadDbUnitOfWorkFactory factory, ISafetyOrgRepository repository)
        {
            _DistributedContext = ctx;
            _repository = repository;
            _domainEventsHandler = new DomainEventsHandler(_repository);
            _HeadDbUnitOfWorkFactory = factory;
        }

        public void MarkOrgAsSafety(Guid organizationGuid, string markCodes)
        {
            using (var uow = _HeadDbUnitOfWorkFactory.Create())
            {
                using (ServiceBus.Instance.RegisterInstance(_domainEventsHandler))
                {
                    SafetyOrgAggregate safetyOrg = _repository.GetSafetyOrgAggregate(organizationGuid);
                    safetyOrg.MarkAsSafety(markCodes);
                }

                uow.Commit();
            }
        }

        public void UpdateSafetyOrg(Guid organizationGuid, string markCodes)
        {
            using (var uow = _HeadDbUnitOfWorkFactory.Create())
            {
                using (ServiceBus.Instance.RegisterInstance(_domainEventsHandler))
                {
                    SafetyOrgAggregate safetyOrg = _repository.GetSafetyOrgAggregate(organizationGuid);
                    safetyOrg.UpdateSafetyOrg(markCodes);
                }

                uow.Commit();
            }
        }


        public void UnmarkSafetyOrg(Guid organizationGuid)
        {
            using (var uow = _HeadDbUnitOfWorkFactory.Create())
            {
                using (ServiceBus.Instance.RegisterInstance(_domainEventsHandler))
                {
                    SafetyOrgAggregate safetyOrg = _repository.GetSafetyOrgAggregate(organizationGuid);
                    safetyOrg.UnmarkSafetyOrg();
                }

                uow.Commit();
            }
        }

        public IEnumerable<SafetyOrgTreeItem> GetSafetyOrganizationsTree()
        {
            var allOrgsQuery = new WebQuery_GetOrganizations();
            var allOrgs = _DistributedContext.Execute(allOrgsQuery);
            var organizationArray = Organization.Traverse(allOrgs).ToArray();

            IEnumerable<SafetyOrgLink> links = null;

            using (var uow = _HeadDbUnitOfWorkFactory.Create())
            {
                links = _repository.GetSafetyOrgsLinks();

                if (links.Count() == 0) 
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

        private class DomainEventsHandler : IEventHandler<MarkedSafetyEvent>
            , IEventHandler<SafetyOrgUpdatedEvent>
            , IEventHandler<UnmarkedSafetyEvent>
        {
            protected ISafetyOrgRepository _repository;

            public DomainEventsHandler(ISafetyOrgRepository repo)
            {
                _repository = repo;
            }

            public void Handle(MarkedSafetyEvent @event)
            {
                _repository.AddSafetyOrg(@event.OrgId, @event.MarkCodes, @event.RoadId);
            }

            public void Handle(SafetyOrgUpdatedEvent @event)
            {
                _repository.UpdateSafetyOrg(@event.OrgId, @event.MarkCodes, @event.RoadId);
            }

            public void Handle(UnmarkedSafetyEvent @event)
            {
                _repository.RemoveSafetyOrg(@event.OrgId);
            }
        }
    }
}
