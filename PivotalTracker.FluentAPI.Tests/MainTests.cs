using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PivotalTracker.FluentAPI.Service;
using PivotalTracker.FluentAPI.Domain;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;

namespace PivotalTracker.FluentAPI.Tests
{
    [TestClass]
    public class MainTests
    {

        #region Properties
        static private PivotalTrackerFacade Pivotal { get; set; }
        public static Project Project { get; private set; }
        private static Story Story { get; set; }
        public static TestContext Context { get; set; }

        #endregion


        #region Helpers

        private static void CreateNewProject(string name)
        {
            CreateNewProjectAsync(name).Wait();
        }

        private static async Task<Project> CreateNewProjectAsync(string name)
        {
           
            return (await Pivotal
                .Projects()
                    .Create()
                        .SetName(name)
                        .SetIterationLength(3)
                    .SaveAsync()).Item;
        }

        private static Story CreateNewStory(string name, StoryTypeEnum type, string description)
        {
            var task = CreateNewStoryAsync(name, type, description);
            task.Wait();
            return task.Result;
        }

        private static async Task<Story> CreateNewStoryAsync(string name, StoryTypeEnum type, string description)
        {
            return (await
                (await Pivotal.Projects().GetAsync(Project.Id))
                        .Stories()
                            .Create()
                                .SetName(name)
                                .SetType(type)
                                .SetDescription(description)
                            .SaveAsync()).Item;
        }
        #endregion


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ClassInitializeAsync(context).Wait();
        }

        public static async System.Threading.Tasks.Task ClassInitializeAsync(TestContext context)
        {
            Context = context;
            Pivotal = new PivotalTrackerFacade(new Token(Properties.Settings.Default.ApiKey));

            Project = Properties.Settings.Default.TestProjectId > 0 ? 
                (await Pivotal.Projects().GetAsync(Properties.Settings.Default.TestProjectId)).Item : 
                await CreateNewProjectAsync("test" + DateTime.Now.Ticks.ToString());

            Story = CreateNewStory("test story", StoryTypeEnum.Feature, "Story test");

            //Uncomment to trace request in fiddler2
            //System.Net.WebRequest.DefaultWebProxy = new WebProxy("localhost", 8888);

        }



        [ClassCleanup]
        public static void ClassCleanup()
        {
            ClassCleanupAsync().Wait();
        }

        public static async System.Threading.Tasks.Task ClassCleanupAsync()
        {
            (await
                (await Pivotal.Projects().GetAsync(Project.Id))
                        .Stories()
                            .AllAsync())
                                .Each(async f =>
                                {
                                    await f.DeleteAsync();
                                });
                            

            //Do not exists, clean up must be done manually
            //or choose a project id in the settings to avoid creation

            if (Properties.Settings.Default.TestProjectId == 0)
            {
                Assert.Inconclusive("You must delete the project {0} with id={1} because PT do not allow project deletion. Prefer to launch tests in a specific project (cf. Settings)", Project.Name, Project.Id);
            }
            //    Pivotal
            //        .Projects()
            //            .Get(Project.Id)
            //                .Delete();

        }


        #region Stories Tests

        [TestMethod]
        public void GetAllStories()
        {
            GetAllStoriesAsync().Wait();
        }

        public async System.Threading.Tasks.Task GetAllStoriesAsync()
        {
            (await
            (await Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Stories()
                            .AllAsync())
                                .Do((f, s) =>
                                {
                                    Assert.IsTrue(true);
                                })
                                .Done()
                            .Done()
                        .Done()
                    .Done()
                .Done();
        }

        [TestMethod]
        public async System.Threading.Tasks.Task UpdateStoryAsync()
        {
            const string DESCRIPTION = "test updated successfully";

            (await
            (await
            (await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Stories()
                            .GetAsync(Story.Id))
                                .UpdateAsync(s =>
                                {
                                    s.Description = DESCRIPTION;
                                }))
                            .Done()
                            .GetAsync(Story.Id))
                                .Do((f, s) =>
                                {
                                    Assert.AreEqual(s.Description, DESCRIPTION);
                                })
                            .Done()
                        .Done()
                    .Done()
                .Done()
            .Done();

        }

        [TestMethod]
        [Ignore]
        public async System.Threading.Tasks.Task CreateStoryAsync()
        {
            (await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Stories()
                            .Create()
                                .SetName("Im famous")
                                .SetType(StoryTypeEnum.Chore)
                                .SetDescription("test description")
                            .SaveAsync())
                            .Do(s =>
                            {
                                Assert.AreEqual(s.Name, "Im famous");
                                Assert.AreEqual(s.Type, StoryTypeEnum.Chore);
                                Assert.AreEqual(s.Description, "test description");
                                Assert.IsTrue(s.Id > 0);
                            })
                        .Done()
                    .Done()
                .Done()
            .Done();
        }

        [TestMethod]
        public async System.Threading.Tasks.Task AddNoteToStoryAsync()
        {
            (await
            (await
            (await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Stories()
                            .GetAsync(Story.Id))
                                .AddNoteAsync("YOUPI"))
                            .Done()
                            .GetAsync(Story.Id))
                                .Do(s =>
                                {
                                    Assert.AreEqual(1, s.Notes.Count(n => n.Description == "YOUPI"));
                                })
                            .Done()
                        .Done()
                    .Done()
                .Done()
            .Done();
        }

        [TestMethod]
        public async System.Threading.Tasks.Task DeleteStoryAsync()
        {
            Story s = await CreateNewStoryAsync("to  be deleted", StoryTypeEnum.Feature, "delete me!");

            try
            {
                await
                (await
                (await
                (await
                Pivotal
                    .Projects()
                        .GetAsync(Project.Id))
                            .Stories()
                                .GetAsync(s.Id))
                                .DeleteAsync())
                            .Done()
                            .Stories()
                                .GetAsync(s.Id);
                Assert.Fail("request to deleted stories need to cause 404 error");
            }
            catch (HttpRequestException ex)
            {
            }
        }

        [TestMethod]
        public async System.Threading.Tasks.Task FilterStoriesAsync()
        {
            var i = 0;
            (await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Stories()
                            .FilterAsync("state:unstarted"))
                                .Do(stories =>
                                {
                                    i++;
                                })
                            .Done()
                        .Done()
                    .Done()
                .Done()
            .Done();

            Assert.IsTrue(i > 0);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task GetOneStoryAsync()
        {
            (await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Stories()
                           .GetAsync(Story.Id))
                           .Do(s => {
                               Assert.AreEqual(Story.Id, s.Id);
                               Assert.AreEqual(Story.Name, s.Name);                                
                           })
                          .Done()
                    .Done()
                .Done();
        }

        #endregion

        #region Project Tests

        [TestMethod]
        public async System.Threading.Tasks.Task EntireTestAsync()
        {
            byte[] someBytes = System.Text.Encoding.ASCII.GetBytes("Hello World"); //Some bytes

            (await
            (await
            (await
            (await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id)) //ProjectId
                        .Stories()
                            .Create()
                                .SetName("This is my first story")
                                .SetType(StoryTypeEnum.Feature)
                                .SetDescription("i'am happy it's so easy !")
                                .SaveAsync())
                                    .AddNoteAsync("this is really amazing"))
                                    .UploadAttachment(someBytes, "attachment.txt", "text/plain")
                                    .UpdateAsync(story =>
                                    {
                                        story.Estimate = 3;
                                        story.OwnedBy = story.RequestedBy;
                                        story.CurrentState = StoryStateEnum.Started;
                                    }))
                        .Done()
                        .FilterAsync("state:started"))
                            .Do(stories =>
                            {
                                foreach (var s in stories)
                                {
                                    Console.WriteLine("{0}: {1} ({2})", s.Id, s.Name, s.Type);
                                    foreach (var n in s.Notes)
                                        Console.WriteLine("\tNote {0} ({1}): {2}", n.Id, n.Description, n.NoteDate);
                                }
                            })
                        .Done()
                    .Done()
                .Done()
            .Done();
        }

        [TestMethod]
        [Ignore]
        public async System.Threading.Tasks.Task CreateProjectAsync()
        {
            const string projectName = "test project creation";
            int id = 0;
            DateTime startDate = new DateTime(2011, 03, 01);
            (await
            (await
            Pivotal
               .Projects()
                   .Create()
                       .SetName(projectName)
                       .SetIterationLength(3)
                       .SetPublic(true)
                   .SaveAsync())
                   .Do(p =>
                   {
                       Assert.AreNotEqual(0, p.Id);
                       id = p.Id;
                       Assert.AreEqual(p.Name, projectName);
                   })
                   .Done()
                   .GetAsync(id)) //reload
                       .Do(p =>
                       {
                           Assert.AreNotEqual(0, p.Id);
                           Assert.AreEqual(p.Name, projectName);
                       })
                   .Done()
              .Done()
           .Done();
        }

        [TestMethod]
        public async System.Threading.Tasks.Task DeliverAllFinishedStoriesAsync()
        {
            Story s = await CreateNewStoryAsync("to  be finished", StoryTypeEnum.Feature, "finish me!");
           
            await
            (await
            (await
            (await
            (await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Stories()
                            .GetAsync(s.Id))
                            .UpdateAsync(story =>
                            {
                                story.Estimate = 1;
                                story.OwnedBy = story.RequestedBy;
                                story.CurrentState = StoryStateEnum.Finished;
                            }))
                            .Done()
                        .Done()
                        .DeliverAllFinishedStoriesAsync())
                        .Stories()
                            .GetAsync(s.Id))
                                .Do(story =>
                                {
                                    Assert.AreEqual(StoryStateEnum.Delivered, story.CurrentState);
                                })
                                .DeleteAsync();
        }

        #endregion

        #region AttachmentsTest

        private static async Task<StoryFacade<StoriesProjectFacade>> UploadAttachment(string DATA)
        {
            return
                await
                (await
                (await
                Pivotal
                    .Projects()
                        .GetAsync(Project.Id))
                            .Stories()
                                .GetAsync(Story.Id))
                                    .UploadAttachment((s, stream) =>
                                    {
                                        using (var writer = new StreamWriter(stream, Encoding.ASCII))
                                        {
                                            writer.WriteLine(DATA);
                                        }
                                    })
                                .Done()
                                .GetAsync(Story.Id);
        }
        [TestMethod]
        [Ignore]
        public async System.Threading.Tasks.Task AddAttachmentThenDownloadThenCheckContentAsync()
        {
            const string DATA = "This is an attachment";

            (await UploadAttachment(DATA))
                .Do(async (f, s) =>
                {
                    Assert.AreEqual(1, s.Attachments.Count);
                    Assert.AreNotEqual(0, s.Attachments[0].Id);

                    //Download and check content
                    var data = await f.DownloadAttachmentAsync(s.Attachments[0]);
                    string value = System.Text.Encoding.ASCII.GetString(data);

                    Assert.AreEqual(DATA, value);
                });
        }

        [TestMethod]
        [Ignore]
        public async System.Threading.Tasks.Task AddAttachmentButDoNotCheckContentAsync()
        {
            const string DATA = "This is an attachment";

            (await UploadAttachment(DATA))
                                .Do((f, s) =>
                                {
                                    Assert.AreEqual(1, s.Attachments.Count);
                                    Assert.AreNotEqual(0, s.Attachments[0].Id);
                                });
        }
        #endregion

        #region Membership
        [TestMethod]
        public async System.Threading.Tasks.Task GetAllMembershipsAsync()
        {
            await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Membership()
                            .AllAsync(members =>
                            {
                                Assert.IsNotNull(members);
                                Assert.AreEqual(1, members.Count());
                                Assert.IsNotNull(members.First().Person);
                                Assert.IsNotNull(members.First().Person.Name);
                            });
        }

        [TestMethod]
        public async System.Threading.Tasks.Task AddMembershipAsync()
        {
            await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Membership()
                            .Add(p =>
                            {
                                var m = new Membership();
                                m.MembershipRole = MembershipRoleEnum.Member;
                                m.Person.Name = Properties.Settings.Default.NewMemberName;
                                m.Person.Email = Properties.Settings.Default.NewMemberEmail;

                                return m;
                            });
        }

        [TestMethod]
        public async System.Threading.Tasks.Task RemoveMembership()
        {
            await AddMembershipAsync();

            await
            (await
            (await
            Pivotal
                .Projects()
                    .GetAsync(Project.Id))
                        .Membership()
                            .RemoveAsync(p =>
                            {
                                return p.Memberships.Where(m => m.Person.Email == Properties.Settings.Default.NewMemberEmail).First();
                            }))
                        .Done()
                        .Membership()
                            .AllAsync(members =>
                            {
                                Assert.AreEqual(0, members.Where(m => m.Person.Email == Properties.Settings.Default.NewMemberEmail).Count());
                            });
        }
        
        #endregion

    }
}
