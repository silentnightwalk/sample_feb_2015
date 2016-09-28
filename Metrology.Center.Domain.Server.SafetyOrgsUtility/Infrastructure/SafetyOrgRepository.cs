using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Metrology.Shared.Core.Server.SafetyOrgsUtility.Kernel;
using Metrology.Server.Persistence;
using Metrology.Shared.Core;

namespace Metrology.Server.SafetyOrgsUtility
{
    internal class SafetyOrgRepository: DbRepositoryBase, ISafetyOrgRepository        
    {
        private readonly IDistributedContext _context;

        public SafetyOrgRepository(IDistributedContext context)
        {
            _context = context;
        }

        /// <summary>
        /// добавить строку в таблицу SafetyOrgLinks
        /// </summary>
        public void AddSafetyOrg(Guid orgId, string markCodes, Guid? roadId)
        {
            Session.Execute(
                @"INSERT INTO SafetyOrgLinks (Id, Mark, RoadServer_Id) VALUES (@orgId, @markCodes, @roadId)",
                new
                {
                    orgId,
                    markCodes,
                    roadId
                }
            );
        }

        /// <summary>
        /// Изменить строку в SafetyOrgLinks
        /// </summary>
        public void UpdateSafetyOrg(Guid orgId, string markCodes, Guid? roadId)
        {
            Session.Execute(
                @"UPDATE SafetyOrgLinks SET Mark = @markCodes, RoadServer_Id = @roadId WHERE Id = @orgId",
                new
                {
                    orgId,
                    markCodes,
                    roadId
                }
            );
        }

        /// <summary>
        /// Удалить строку из SafetyOrgLinks
        /// </summary>
        public void RemoveSafetyOrg(Guid orgId)
        {
            Session.Execute(@"DELETE FROM SafetyOrgLinks WHERE Id = @orgId", new { orgId });
        }

        public IEnumerable<SafetyOrgLink> GetSafetyOrgsLinks()
        {
            return Session.Query<SafetyOrgLink>("SELECT Id, Mark, RoadServer_Id FROM SafetyOrgLinks");
        }

        /// <summary>
        /// формируем агрегат по организации
        /// </summary>
        public SafetyOrgAggregate GetSafetyOrgAggregate(Guid orgId)
        {
            //достаем все организации
            var allOrgsQuery = new WebQuery_GetOrganizations();
            var rootOrgs = _context.Execute(allOrgsQuery);
            if (rootOrgs == null) throw new Exception("No organizations");
            var allOrgs = Organization.Traverse(rootOrgs);

            //выбираем нашу организацию
            var org = Organization.FindFirst(rootOrgs,(x)=>x.Id == orgId);
            if (org == null) throw new Exception("Organization not found");

            //достаем все аккредитованные организации
            var links = GetSafetyOrgsLinks().ToArray();
            if (links == null) links = new SafetyOrgLink[0];

            //аккредитованна ли наша организация
            var targetSafetyOrgLink = links.FirstOrDefault(x=>x.Id == orgId);
            var isSafetyOrganization = targetSafetyOrgLink != null;

            //достаем реальную организацию, чтобы узнать id сервера
            var realOrganizationsQuery = new DistributedQuery_GetRealOrganizationsById(orgId);
            var realOrganizations = _context.Execute(realOrganizationsQuery);
            if (realOrganizations == null) throw new Exception("Organization not found");
            if (realOrganizations.Count() > 1) throw new Exception("Organization should not be distributed");
            var targetRealOrganization = realOrganizations.FirstOrDefault();

            //группировка списка организаций по родителю
            var groupedOrgs = allOrgs.GroupBy(x=>x.ParentId);
            var targetGroup = groupedOrgs.FirstOrDefault(x=>x.Key == org.ParentId);
            if (targetGroup == null) throw new Exception("Organization not found");

            //есть ли аккредитованные родители
            var hasSertifiedParents = false;
            var loopGroup = targetGroup;
            do
	        {
                if (loopGroup == null) break;

	            if (links.FirstOrDefault(x => loopGroup.Key.HasValue && x.Id == loopGroup.Key) != null)
                {
                    hasSertifiedParents = true;
                }
                loopGroup = groupedOrgs.FirstOrDefault(x => x.FirstOrDefault(y => y.Id == loopGroup.Key) != null);
	        }
            while(!hasSertifiedParents || loopGroup != null && loopGroup.Key != null);

            //есть ли аккредитованные дети
            var hasSertifiedChildren = false;
            var levelOrgs = targetGroup.AsEnumerable();
            do
            {
                foreach (var levelOrg in levelOrgs)
                {
                    if (links.FirstOrDefault(x => x.Id == levelOrg.Id) != null)
                    {
                        hasSertifiedChildren = true;
                    }
                }
                levelOrgs = levelOrgs.SelectMany(x => x.Children != null ? x.Children : new Organization[0]);
            }
            while (!hasSertifiedChildren && levelOrgs != null && levelOrgs.Count() > 0);

            //результирующий аггрегат
            var aggregate = new SafetyOrgAggregate(
                org.Id,
                isSafetyOrganization,
                org.Name,
                targetSafetyOrgLink != null ? targetSafetyOrgLink.Mark : null,
                targetRealOrganization.ServerId,
                hasSertifiedParents,
                hasSertifiedChildren
                );
            return aggregate;
        }

    }
}
