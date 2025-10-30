using Jam.Models;

namespace Jam.DAL.StoryDAL;

public interface IStoryRepository
{
    // Read / GET
    Task<IEnumerable<Story>> GetAllStories();
    Task<IEnumerable<Story>> GetAllPublicStories();
    Task<IEnumerable<Story>> GetAllPrivateStories();
    Task<IEnumerable<Story>> GetStoriesByUserId(int userId);
    Task<IEnumerable<Story>> GetMostRecentPlayedStories(int userId, int count = 5);
    Task<Story?> GetStoryById(int storyId);
    Task<Story?> GetPublicStoryById(int storyId);
    Task<Story?> GetPrivateStoryByCode(string code);
    Task<int?> GetAmountOfQuestionsForStory(int storyId);
    Task<string?> GetCodeForStory(int storyId);



    // Creation mode
    Task<bool> AddStory(Story story);
    Task<bool> UpdateStory(Story story);
    Task<bool> DeleteStory(int storyId);
    Task<bool> DoesCodeExist(string code);


    // Playing mode
    Task<bool> IncrementPlayed(int storyId);
    Task<bool> IncrementFinished(int storyId);
    Task<bool> IncrementFailed(int storyId);
}