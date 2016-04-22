using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using PivotalTracker.FluentAPI.Domain;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PivotalTracker.FluentAPI.Repository
{
    /// <summary>
    /// This repository manages Pivotal stories
    /// </summary>
    /// <see cref="https://www.pivotaltracker.com/help/api?version=v3#getting_stories"/>
    /// <see cref="https://www.pivotaltracker.com/help/api?version=v3#manage_stories"/>
    public class PivotalStoryRepository : PivotalTrackerRepositoryBase
    {



        #region DTOs
        [DataContract(Name = "story", Namespace = "")]
        [XmlRoot("story")]
        public class StoryXmlResponse
        {
            public bool idSpecified { get { return id != 0; } }
            [DataMember()]
            public int id { get; set; }
            public bool project_idSpecified { get { return project_id != 0; } }
            [DataMember()]
            public int project_id { get; set; }
            [DataMember()]
            public string story_type { get; set; }
            [DataMember()]
            public string url { get; set; }

            public bool estimateSpecified { get { return estimate > 0; } }
            [DataMember()]
            public int estimate { get; set; }
            [DataMember()]
            public string current_state { get; set; }
            [DataMember()]
            public string description { get; set; }
            [DataMember()]
            public string name { get; set; }
            [DataMember()]
            public string requested_by { get; set; }
            public string owned_by { get; set; }
            [DataMember()]
            public DateTime created_at { get; set; }
            [DataMember()]
            public DateTime updated_at { get; set; }
            public DateTime accepted_at { get; set; }

            [XmlArray("attachments")]
            [XmlArrayItem("attachment")]
            public Attachment[] attachments { get; set; }

            [XmlArray("notes")]
            [XmlArrayItem("note")]
            public StoryNoteXmlResponse[] notes { get; set; }

            [XmlArray("labels")]
            [XmlArrayItem]
            [DataMember()]
            public StoryLabelXmlResponse[] labels { get; set; }

            [XmlArray("tasks")]
            [XmlArrayItem("task")]
            public PivotalTracker.FluentAPI.Domain.Task[] tasks { get; set; }


        }

        [XmlRoot("story")]
        public class StoryXmlRequest : StoryXmlResponse
        {

        }

        [DataContract]
        public class StoryCreationLabel
        {
            public static implicit operator StoryCreationLabel(string s)
            {
                return new StoryCreationLabel { name = s };
            }

            [DataMember(EmitDefaultValue = false)]
            public int id;

            [DataMember(EmitDefaultValue = false)]
            public string name;
        }

        [DataContract]
        public class StoryCreationRequest
        {
            [DataMember]
            public string story_type { get; set; }
            [DataMember]
            public string name { get; set; }

            public string requested_by { get; set; }
            [DataMember]
            public string description { get; set; }

            public string current_state { get; set; }

            public string owned_by { get; set; }

            public bool estimateSpecified { get { return estimate > 0; } }
            [DataMember]
            public int estimate { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public List<StoryCreationLabel> labels { get; set; }
        }

        [XmlRoot("note")]
        public class StoryNoteXmlRequest
        {
            public string text { get; set; }
        }

        [XmlRoot("note")]
        public class StoryNoteXmlResponse
        {
            public int id { get; set; }
            public string text { get; set; }
            public string author { get; set; }
            public DateTime noted_at { get; set; }
        }

        public class StoryLabelXmlResponse
        {
            public DateTime created_at { get; set; }
            public int id { get; set; }
            public string kind { get; set; }
            public string name { get; set; }
            public int project_id { get; set; }
            public DateTime updated_at { get; set; }
        }

        #endregion
      

        public PivotalStoryRepository(Token token) : base(token)
        {
        }

        internal static Story CreateStory(StoryXmlResponse e)
        {
            if (e == null)
            {
                return new Story();
            }
            var lStory = new Story()
                             {
                                 AcceptedDate = e.accepted_at,
                                 //Attachments = 
                                 CreatedDate = e.created_at,
                                 UpdatedDate = e.updated_at,
                                 CurrentState = (StoryStateEnum) Enum.Parse(typeof (StoryStateEnum), e.current_state, true),
                                 Description = e.description,
                                 Estimate =  e.estimate,
                                 Id = e.id,
                                 Name = e.name,
                                 //Notes = 
                                 OwnedBy = e.owned_by,
                                 ProjectId = e.project_id,
                                 RequestedBy = e.requested_by,
                                 //Tasks = 
                                 Type = (StoryTypeEnum)Enum.Parse(typeof(StoryTypeEnum), e.story_type, true),
                                 Url = new Uri(e.url)
                             };

            if (e.labels != null)
            {
                foreach (var label in e.labels)
                {
                    lStory.Labels.Add(new Label
                    {
                        CreatedDate = label.created_at,
                        Id = label.id,
                        Name = label.name,
                        UpdatedDate = label.updated_at
                    });
                }
            }

            if (e.attachments != null)
            {
                foreach (var attachment in e.attachments)
                {
                    lStory.Attachments.Add(attachment);
                }
            }

            if (e.notes != null)
            {
                foreach (var note in e.notes)
                {
                    lStory.Notes.Add(new Note()
                        {
                            Id = note.id,
                            Author = note.author,
                            Description = note.text,
                            NoteDate = note.noted_at
                        });
                }
            }

            if (e.tasks != null)
            {
                foreach (var task in e.tasks)
                {
                    lStory.Tasks.Add(task);
                }
            }

            return lStory;
        }

        public async Task<IEnumerable<Story>> GetStoriesAsync(string url, string method="GET")
        {
            var e = await this.RequestPivotalAsync<StoryXmlResponse[]>(url, null, method);
            if (e.Length != 0)
            {
                return e.ToList().Select(CreateStory).ToList();
            }
            return new List<Story>();
        }
     
        
        public async Task<Story> GetStoryAsync(int projectId, int storyId)
        {
            var path = string.Format("/projects/{0}/stories/{1}", projectId, storyId);
            var e = await this.RequestPivotalAsync<StoryXmlResponse>(path, null, "GET");

            return PivotalStoryRepository.CreateStory(e);
        }

        public async Task<IEnumerable<Story>> GetStoriesAsync(int projectId)
        {
            var path = string.Format("/projects/{0}/stories", projectId);

            return await GetStoriesAsync(path);

        }

        //TODO: can GetSomeStories & GetLimitedStories be an unique method ?
        public async Task<IEnumerable<Story>> GetSomeStoriesAsync(int projectId, string filter)
        {
            var path = string.Format("/projects/{0}/stories?filter={1}", projectId, Uri.EscapeDataString(filter));
            return await GetStoriesAsync(path);
            
        }

        public async Task<IEnumerable<Story>> GetLimitedStoriesAsync(int projectId, int offset, int limit)
        {
            var path = string.Format("/projects/{0}/stories?limit={1}&offset={2}", projectId, limit, offset);
            return await GetStoriesAsync(path);
            
        }

        public async Task<Story> AddStoryAsync(int projectId, StoryCreationRequest storyCreationRequest)
        {
            var path = string.Format("/projects/{0}/stories", projectId);
            var e = await this.RequestPivotalAsync<StoryXmlResponse>(path, storyCreationRequest, "POST");
            return CreateStory(e);
            
        }

        public async Task<Story> UpdateStoryAsync(Story story)
        {
            var path = string.Format("/projects/{0}/stories/{1}", story.ProjectId, story.Id);
            var s = new StoryXmlRequest
                        {
                            current_state = story.CurrentState.ToString().ToLowerInvariant(),
                            description = story.Description,                            
                            name = story.Name,
                            owned_by = story.OwnedBy,
                            story_type = story.Type.ToString().ToLowerInvariant(),
                            requested_by = story.RequestedBy,                            
                        };
            if (story.Estimate > 0)
                s.estimate = story.Estimate;

            var e = await this.RequestPivotalAsync<StoryXmlResponse>(path, s, "PUT");
            return CreateStory(e);

        }

        public async Task<Note> AddNoteAsync(int projectId, int storyId, string text)
        {
            var path = string.Format("/projects/{0}/stories/{1}/notes", projectId, storyId);
            var noteReq = new Repository.PivotalStoryRepository.StoryNoteXmlRequest { text = text };

            var noteResp = await this.RequestPivotalAsync<StoryNoteXmlResponse>(path, noteReq, "POST");
            var note = new Note
                           {
                               Author = noteResp.author,
                               Description = noteResp.text,
                               NoteDate = noteResp.noted_at,
                               Id = noteResp.id,
                               StoryId = storyId
                           };
            return note;

        }

        public async Task<Story> DeleteStoryAsync(int projectId, int storyId)
        {
                var path = string.Format("/projects/{0}/stories/{1}", projectId, storyId);
                var e = await this.RequestPivotalAsync<StoryXmlResponse>(path, null, "DELETE");

            return CreateStory(e);
        }

        public async Task<IEnumerable<Story>> DeliverAllFinishedStoriesAsync(int projectId)
        {
            var path = string.Format("/projects/{0}/stories/deliver_all_finished", projectId);
            return await GetStoriesAsync(path, "PUT");
        }

        public enum MovePositionEnum
        {
            After,
            Before
        } ;

        public async Task<Story> MoveStoryAsync(int projectId, int storyId, MovePositionEnum move, int targetStoryId)
        {
            var path = string.Format("/projects/{0}/stories/{1}/moves?move\\[move\\]={2}&move\\[target\\]={3}", 
                projectId,
                storyId,
                move,
                targetStoryId);
            var e = await this.RequestPivotalAsync<StoryXmlResponse>(path, null, "POST");

            return CreateStory(e);
        }

        public Story LinkStoryToExternal(int projectId, int storyId, dynamic integrationInfo)
        {
            //TODO: to be implemented
            throw new NotImplementedException();

        }
    }
}