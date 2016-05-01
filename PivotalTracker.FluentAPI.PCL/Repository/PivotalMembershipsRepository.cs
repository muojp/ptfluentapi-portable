using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using PivotalTracker.FluentAPI.Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PivotalTracker.FluentAPI.Repository
{

    /// <summary>
    /// Repository that manage Pivotal Membership
    /// </summary>
    /// <see cref="https://www.pivotaltracker.com/help/api?version=v3#get_memberships"/>
    public class PivotalMembershipsRepository : PivotalTrackerRepositoryBase
    {
        [DataContract]
        class MembershipAddRequest
        {
            [DataMember(EmitDefaultValue = false)]
            public int person_id { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string role { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string email { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string name { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string initials { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string project_color { get; set; }
        }

        public PivotalMembershipsRepository(Token token)
            : base(token)
        {   
        }

        public class MembershipResponse
        {
            public class MembershipPersonResponse
            {
                public string kind { get; set; }
                public int id { get; set; }
                public string name { get; set; }
                public string email { get; set; }
                public string initials { get; set; }
                public string username { get; set; }
            }

            public string kind { get; set; }
            public int id { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }

            public MembershipPersonResponse person { get; set; }

            public int project_id { get; set; }
            public string role { get; set; }
            public string project_color { get; set; }
            public DateTime last_viewed_at { get; set; }
            public bool wants_comment_notification_emails { get; set; }
            public bool will_receive_mention_notifications_or_emails { get; set; }
        }

        public async Task<IEnumerable<Membership>> GetAllMembershipsAsync(int projectId)
        {
            var path = string.Format("/projects/{0}/memberships", projectId);
            var memberships = await this.RequestPivotalAsync<List<MembershipResponse>>(path, null, "GET");
            var personPath = string.Format("/projects/{0}/memberships", projectId);
            return memberships.Select(o => new Membership {
                Id = o.id,
                Person = new Person { Email = o.person.email, Initials = o.person.initials, Name = o.person.name },
                ProjectRef = new ProjectRef { Id = projectId }
            });
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
            var m = new MembershipAddRequest()
            {
                role = membership.MembershipRole.ToString().ToLowerInvariant()
            };
            if (!string.IsNullOrEmpty(membership.Person.Email))
            {
                m.email = membership.Person.Email;
            }
            if (!string.IsNullOrEmpty(membership.Person.Initials))
            {
                m.initials = membership.Person.Initials;
            }
            if (!string.IsNullOrEmpty(membership.Person.Name))
            {
                m.name = membership.Person.Name;
            }
            var result = await this.RequestPivotalAsync<Membership>(path, m, "POST");

            return result;
        }

        public async Task<Membership> RemoveMembershipAsync(int projectId, int membershipId)
        {
            var path = string.Format("/projects/{0}/memberships/{1}", projectId, membershipId);
            var membership = await this.RequestPivotalAsync<Membership>(path, null, "DELETE");

            return membership;

        }
        public async Task<Membership> RemoveMembershipAsync(Membership membership)
        {
            return await RemoveMembershipAsync(membership.ProjectRef.Id, membership.Id);
        }
    }
}
