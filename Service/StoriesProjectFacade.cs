using System;
using System.Collections.Generic;
using PivotalTracker.FluentAPI.Domain;
using PivotalTracker.FluentAPI.Repository;
using System.Threading.Tasks;

namespace PivotalTracker.FluentAPI.Service
{
    /// <summary>
    /// Facade that manages stories of a project
    /// </summary>
    public class StoriesProjectFacade : Facade<StoriesProjectFacade, ProjectFacade>
    {
        private readonly PivotalStoryRepository _storyRepository;
        

        public StoriesProjectFacade(ProjectFacade parent)
            : base(parent)
        {
            _storyRepository = new Repository.PivotalStoryRepository(this.RootFacade.Token);
        }


        /// <summary>
        /// Retrieve stories that match the filter
        /// </summary>
        /// <param name="filter">filter to be matched (ex: state:unstarted)</param>
        /// <returns>a StoriesFacade that manages the result</returns>
        public async Task<StoriesFacade> FilterAsync(string filter)
        {
            var list = await _storyRepository.GetSomeStoriesAsync(this.ParentFacade.Item.Id, filter);
            return new StoriesFacade(this, list);
            
        }

        //TODO: Not tested
        /// <summary>
        /// Retrieve paginated stories 
        /// </summary>
        /// <param name="offset">offset of the page</param>
        /// <param name="limit">max number of stories</param>
        /// <returns>a StoriesFacade that manages the result</returns>
        public async Task<StoriesFacade> Some(int offset, int limit)
        {
            var list = await _storyRepository.GetLimitedStoriesAsync(this.ParentFacade.Item.Id, offset, limit);
            return new StoriesFacade(this, list);

        }

        /// <summary>
        /// Get a facade to create a story
        /// </summary>
        /// <returns>StoryCreationFacade that manages the creation</returns>
        public StoryCreationFacade Create()
        {
            return new StoryCreationFacade(this, new PivotalStoryRepository.StoryCreationXmlRequest());

        }

        /// <summary>
        /// Get a story facade for a specific story 
        /// </summary>
        /// <param name="id">the story id</param>
        /// <returns>a facade that manages the loaded Story</returns>
        public async Task<StoryFacade<StoriesProjectFacade>> Get(int id)
        {
            
            var lStory = await _storyRepository.GetStoryAsync(this.ParentFacade.Item.Id, id);

            return new StoryFacade<StoriesProjectFacade>(this, lStory);

        }
        
        /// <summary>
        /// Get a facade that manages all the stories of this project
        /// </summary>
        /// <returns>facade that manages the result</returns>
        public async Task<StoriesFacade> All()
        {
            return new StoriesFacade(this, await _storyRepository.GetStoriesAsync(this.ParentFacade.Item.Id));
        }

    }
}