using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PivotalTracker.FluentAPI.Domain;
using System.Threading.Tasks;

namespace PivotalTracker.FluentAPI.Service
{
    /// <summary>
    /// Facade that manages project memberships 
    /// </summary>
    public class MembershipsFacade : Facade<MembershipsFacade, ProjectFacade>
    {
        private Repository.PivotalMembershipsRepository _repository;
        public MembershipsFacade(ProjectFacade parent)
            :base(parent)
        {
            _repository = new Repository.PivotalMembershipsRepository(RootFacade.Token);
        }


        /// <summary>
        /// Apply an action on the loaded membership list
        /// </summary>
        /// <param name="action">action that accept membeship list</param>
        /// <returns>This</returns>
        public async Task<MembershipsFacade> AllAsync(Action<IEnumerable<Membership>> action)
        {
            action(await _repository.GetAllMembershipsAsync(this.ParentFacade.Item.Id));
            return this;
        }

        /// <summary>
        /// Add a new membership into the project
        /// </summary>
        /// <param name="membership">a initialized membership (email is mandatory)</param>
        /// <returns>This</returns>
        public async Task<MembershipsFacade> Add(Membership membership)
        {

            membership.ProjectRef.Name = this.ParentFacade.Item.Name;
            membership.ProjectRef.Id = this.ParentFacade.Item.Id;
            await _repository.AddMembershipAsync(membership);

            return this;
        }
        
        /// <summary>
        /// Add a new membership that is return by a Factory
        /// </summary>
        /// <param name="creator">factory that accept the project object and create the membership</param>
        /// <returns>This</returns>
        public async Task<MembershipsFacade> Add(Func<Project, Membership> creator)
        {
            var m = creator(this.ParentFacade.Item);
            return await Add(m);
        }

        /// <summary>
        /// Remove a membership with a selector
        /// </summary>
        /// <param name="selector">Selector that accept a projet and returns a membership</param>
        /// <returns></returns>
        public async Task<MembershipsFacade> RemoveAsync(Func<Project, Membership> selector)
        {
            await _repository.RemoveMembershipAsync(selector(this.ParentFacade.Item));
            return this;
        }
    }
}
