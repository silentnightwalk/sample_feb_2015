using Metrology.Shared.Core.Server.SafetyOrgsUtility.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metrology.Server.SafetyOrgsUtility
{
    internal class SafetyOrgAggregate
    {
        public Guid Id { get; private set; }
        public bool IsSafetyOrganization { get; private set; }
        public string Name { get; private set; }
        public string MarkCodes { get; private set; }
        public Guid? RoadId { get; private set;}
        public bool HasSafetyAncestors { get; private set; }
        public bool HasSafetyChildren { get; private set; }

        public SafetyOrgAggregate(
            Guid   id, 
            bool isSafetyOrganization,
            string name, 
            string markCodes, 
            Guid?   roadId, 
            bool hasSafetyAncestors,
            bool hasSafetyChildren
            )
        {
            Id = id;
            IsSafetyOrganization = isSafetyOrganization;
            Name = name; 
            MarkCodes = markCodes; 
            RoadId = roadId; 
            HasSafetyAncestors = hasSafetyAncestors;
            HasSafetyChildren = hasSafetyChildren;
        }


        public void MarkAsSafety(string newMarkCodes)
        {
            //if (HasSafetyAncestors)
            //    throw new Exception("Cannot be safety because it has safety ancestors");
            //if (HasSafetyChildren)
            //    throw new Exception("Cannot be safety because it has safety children");

            if (!this.IsSafetyOrganization)
            {
                this.IsSafetyOrganization = true;
                this.MarkCodes = newMarkCodes;
                ServiceBus.Instance.Raise<MarkedSafetyEvent>(new MarkedSafetyEvent(this.Id, this.MarkCodes, this.RoadId));
            }
            else if (this.MarkCodes != newMarkCodes)
            {
                //UpdateSafetyOrg(newMarkCodes);
                throw new Exception("Organization is already a safety one");
            }
        }


        public void UpdateSafetyOrg(string newMarkCodes, Guid? roadId)
        {
            if (!this.IsSafetyOrganization)
            {
                throw new Exception("Cannot update because organization is not a safety one");
            }

            this.RoadId = roadId;
            this.MarkCodes = newMarkCodes;
            ServiceBus.Instance.Raise<SafetyOrgUpdatedEvent>(new SafetyOrgUpdatedEvent(this.Id, this.MarkCodes, this.RoadId));
        }


        public void UpdateSafetyOrg(string newMarkCodes)
        {
            UpdateSafetyOrg(newMarkCodes, this.RoadId);
        }


        public void UnmarkSafetyOrg()
        {
            if (this.IsSafetyOrganization == false)
                throw new Exception("Org is already not a sefety one");
            else
            {
                this.IsSafetyOrganization = false;
                ServiceBus.Instance.Raise<UnmarkedSafetyEvent>(new UnmarkedSafetyEvent(this.Id));
            }
        }
    }
}
