﻿using PivotalTracker.FluentAPI.Domain;
using System.Threading.Tasks;

namespace PivotalTracker.FluentAPI.Service
{
    /// <summary>
    /// Facade that manages the project creation
    /// </summary>
    public class ProjectCreateFacade : FacadeItem<ProjectCreateFacade, ProjectsFacade, Repository.PivotalProjectRepository.ProjectXmlRequest>
    {
        public ProjectCreateFacade(ProjectsFacade parent, Repository.PivotalProjectRepository.ProjectXmlRequest project)
            : base(parent, project)
        {
            project.iteration_length = 3;
        }

        /// <summary>
        /// Set the project name
        /// </summary>
        /// <param name="name">project name</param>
        /// <returns>this</returns>
        public ProjectCreateFacade SetName(string name)
        {
            this.Item.name = name;

            return this;
        }

        /// <summary>
        /// Set the iteration length
        /// </summary>
        /// <param name="length">iteration length</param>
        /// <returns></returns>
        public ProjectCreateFacade SetIterationLength(int length)
        {
            this.Item.iteration_length = length;
            return this;
        }

        public ProjectCreateFacade SetPublic(bool isPublic)
        {
            this.Item.@public = isPublic;
            return this;
        }

        /// <summary>
        /// Save the project into Pivotal
        /// </summary>
        /// <returns>a facade that manage the new project</returns>
        public async Task<ProjectFacade> SaveAsync()
        {
            var repo = new Repository.PivotalProjectRepository(this.RootFacade.Token);
            var p = await repo.CreateProjectAsync(this.Item);

            return new ProjectFacade(this.ParentFacade, p);
        }

        
    }
}