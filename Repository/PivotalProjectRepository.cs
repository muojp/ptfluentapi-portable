using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Serialization;
using System.Dynamic;
using System.Linq;
using PivotalTracker.FluentAPI.Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PivotalTracker.FluentAPI.Repository
{
    /// <summary>
    /// This repository manages Pivotal Project
    /// </summary>
    /// <see cref="https://www.pivotaltracker.com/help/api?version=v3#manage_projects"/>
    public class PivotalProjectRepository : PivotalTrackerRepositoryBase
    {
        #region DTOs
        [DataContract(Name = "project", Namespace = "")]
        [XmlRoot("project")]
        public class ProjectXmlResponse
        {
            [DataMember()]
            public int id { get; set; }
            [DataMember()]
            public string name { get; set; }
            [DataMember()]
            public int iteration_length { get; set; }
            [DataMember()]
            public DayOfWeek week_start_day { get; set; }
            [DataMember()]
            public string point_scale { get; set; }
            [DataMember()]
            public string account { get; set; }
            [DataMember()]
            public string velocity_scheme { get; set; }
            [DataMember()]
            public int current_velocity { get; set; }
            [DataMember()]
            public int initial_velocity { get; set; }
            [DataMember()]
            public int number_of_done_iterations_to_show { get; set; }
            [DataMember()]
            public string labels { get; set; }
            [DataMember()]
            public bool allow_attachments { get; set; }
            [DataMember()]
            public bool @public { get; set; }
            [DataMember()]
            public bool use_https { get; set; }
            [DataMember()]
            public bool bugs_and_chores_are_estimatable { get; set; }
            [DataMember()]
            public bool commit_mode { get; set; }
            [DataMember()]
            public DateTimeUTC first_iteration_start_time { get; set; }
            [DataMember()]
            public DateTimeUTC last_activity_at { get; set; }

            [XmlArray("memberships")]
            [XmlArrayItem("membership")]
            [DataMember()]
            public Membership[] memberships { get; set; }

            [XmlArray("integrations")]
            [XmlArrayItem("integration")]
            [DataMember()]
            public Integration[] integrations { get; set; }
        }

        [XmlRoot("projects")]
        public class ProjectsXmlResponse
        {
            [XmlElement("project")]
            public Collection<ProjectXmlResponse> projects;
        }

        [XmlRoot("project")]
        public class ProjectXmlRequest
        {
            public string name { get; set; }
            public int iteration_length { get; set; }
            public DateTimeUTC first_iteration_start_time { get; set; }
        }
        #endregion


        public PivotalProjectRepository(Token token) : base(token)
        {   
        }

        public async Task<Project> GetProjectAsync(int id)
        {
            var path = string.Format("/projects/{0}", id);
            var e = await this.RequestPivotalAsync<ProjectXmlResponse>(path, null, "GET");
            return PivotalProjectRepository.CreateProject(e);
        }

        protected static Project CreateProject(ProjectXmlResponse e)
        {
            var lProject = new Project();
            lProject.Account = e.account;
            lProject.CurrentVelocity = e.current_velocity;
            lProject.Id = e.id;
            lProject.InitialVelocity = e.initial_velocity;
            lProject.IsAttachmentAllowed = e.allow_attachments;
            lProject.IsBugAndChoresEstimables = e.bugs_and_chores_are_estimatable;
            lProject.IsCommitModeActive = e.commit_mode;
            lProject.IsPublic = e.@public;
            lProject.IterationLength = e.iteration_length;
            if (e.labels != null)
            {
                foreach (var label in e.labels.Split(','))
                {
                    lProject.Labels.Add(label.Trim());
                }
            }
            lProject.StartDate = e.first_iteration_start_time;
            lProject.LastActivityDate = e.last_activity_at;
            lProject.Name = e.name;
            lProject.NumberOfDoneIterationsToShow = e.number_of_done_iterations_to_show;
            lProject.PointScale = e.point_scale;
            lProject.UseHTTPS = e.use_https;
            lProject.VelocityScheme = e.velocity_scheme;
            lProject.WeekStartDay = e.week_start_day;

            if (e.integrations != null)
            {
                foreach (var i in e.integrations)
                {
                    lProject.Integrations.Add(i);
                }
            }

            if (e.memberships != null)
            {
                foreach (var m in e.memberships)
                {
                    m.ProjectRef.Name = lProject.Name;
                    m.ProjectRef.Id = lProject.Id;
                    lProject.Memberships.Add(m);
                }
            }

            return lProject;
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            const string path = "/projects";
            var e = await this.RequestPivotalAsync<ProjectsXmlResponse>(path, null, "GET");

            return e.projects.Select(p => PivotalProjectRepository.CreateProject(p)).ToList();

        }

        public async Task<Project> CreateProjectAsync(Repository.PivotalProjectRepository.ProjectXmlRequest projectRequest)
       {
            var path = string.Format("/projects");
  

            var e = await this.RequestPivotalAsync<ProjectXmlResponse>(path, projectRequest, "POST");
            return PivotalProjectRepository.CreateProject(e);
            
        }

       
    }
}
