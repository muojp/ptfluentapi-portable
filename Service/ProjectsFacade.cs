using System;
using System.Collections.Generic;
using System.Net;
using PivotalTracker.FluentAPI.Domain;
using PivotalTracker.FluentAPI.Repository;
using System.Threading.Tasks;

namespace PivotalTracker.FluentAPI.Service
{
    /// <summary>
    /// Facade that manage all projects of the account
    /// </summary>
    public class ProjectsFacade : Facade<ProjectsFacade, PivotalTrackerFacade>
    {
        private PivotalProjectRepository _projectRepository;

        public ProjectsFacade(PivotalTrackerFacade parent)
            : base(parent)
        {
        }

        //TODO: Not tested
        /// <summary>
        /// Get a Project facade for the first project that match the predicate
        /// </summary>
        /// <param name="predicate">predicate to match</param>
        /// <returns>a facade that manage the found project or null if no project was found</returns>
        public async Task<ProjectFacade> FindProjectAsync(Predicate<Project> predicate)
        {
            _projectRepository = new PivotalProjectRepository(this.RootFacade.Token);
            var list = await _projectRepository.GetProjectsAsync();
            foreach (var project in list)
            {
                if (predicate(project))
                    return new ProjectFacade(this, project);
            }
            
            return null;
        }

        /// <summary>
        /// Get all projects
        /// </summary>
        /// <returns>a list of facades those manages the retrieved projects or null if no one is found</returns>
        public async Task<List<ProjectFacade>> GetAllAsync()
        {
            _projectRepository = new PivotalProjectRepository(this.RootFacade.Token);
            var lProjects = await _projectRepository.GetProjectsAsync();
            if (lProjects == null)
                return null;

            List<ProjectFacade> projectFacadeList = new List<ProjectFacade>();
            foreach (var lProject in lProjects)
            {
                projectFacadeList.Add(new ProjectFacade(this, lProject));
            }

            return projectFacadeList;
        }

        /// <summary>
        /// Get a specific project by its id
        /// </summary>
        /// <param name="projectId">id of the project to retrieve</param>
        /// <returns>a facade that manages the retrieved project or null if no one is found</returns>
        public async Task<ProjectFacade> GetAsync(int projectId)
        {
            _projectRepository = new PivotalProjectRepository(this.RootFacade.Token);
            var lProject = await _projectRepository.GetProjectAsync(projectId);
            if (lProject == null)
                return null;

            var lFacade = new ProjectFacade(this, lProject);

            return lFacade;
        }

        /// <summary>
        /// Get a facade for the creation of a project
        /// </summary>
        /// <returns>a facade that will manage the project creation</returns>
        public ProjectCreateFacade Create()
        {            
            return new ProjectCreateFacade(this, new PivotalProjectRepository.ProjectXmlRequest());
        }
    }
}
