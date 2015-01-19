using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using PivotalTracker.FluentAPI.Domain;
using System.Threading.Tasks;

namespace PivotalTracker.FluentAPI.Repository
{

    /// <summary>
    /// Repository that manage Pivotal Membership
    /// </summary>
    /// <see cref="https://www.pivotaltracker.com/help/api?version=v3#get_memberships"/>
    public class PivotalMembershipsRepository : PivotalTrackerRepositoryBase
    {
        public PivotalMembershipsRepository(Token token)
            : base(token)
        {   
        }

        #region DTOs
        /// <summary>
        /// DTO to receive Pivotal memberships list
        /// </summary>
        [XmlRoot("memberships")]
        public class MembershipsXmlResponse
        {
            [XmlElement("membership")]
            public Membership[] memberships;
        }
        #endregion

        public async Task<IEnumerable<Membership>> GetAllMembershipsAsync(int projectId)
        {
            var path = string.Format("/projects/{0}/memberships", projectId);
            var memberships = await this.RequestPivotalAsync<MembershipsXmlResponse>(path, null, "GET");


            return memberships.memberships;

        }

        public async Task<Membership> GetMembershipAsync(int projectId, int membershipId)
        {
            var path = string.Format("/projects/{0}/memberships/{1}", projectId, membershipId);
            var membership = await this.RequestPivotalAsync<Membership>(path, null, "GET");


            return membership;
        }

        public async Task<Membership> AddMembershipAsync(Membership membership)
        {
            var path = string.Format("/projects/{0}/memberships", membership.ProjectRef.Id);
            var result = await this.RequestPivotalAsync<Membership>(path, membership, "POST");


            return result;

        }

        public async Task<Membership> RemoveMembershipAsync(int projectId, int membershipId)
        {
            var path = string.Format("/projects/{0}/memberships/{1}", projectId, membershipId, "DELETE");
            var membership = await this.RequestPivotalAsync<Membership>(path, null, "GET");


            return membership;

        }
        public async Task<Membership> RemoveMembershipAsync(Membership membership)
        {
            return await RemoveMembershipAsync(membership.ProjectRef.Id, membership.Id);
        }
    }
}
