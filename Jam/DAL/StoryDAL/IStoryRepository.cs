using Jam.Models;

namespace Jam.DAL.StoryDAL;

public interface IStoryRepository
{
    // Read
    Task<IEnumerable<Story>> GetAllStories();
    Task<IEnumerable<Story>> GetAllPublicStories();
    Task<IEnumerable<Story>> GetAllPrivateStories(); // added this
    Task<IEnumerable<Story>> GetStoriesByUserId(int userId);
    Task<IEnumerable<Story>> GetMostRecentPlayedStories(int? userId, int count = 5);
    Task<Story?> GetStoryById(int id);
    Task<Story?> GetPublicStoryById(int id);
    Task<Story?> GetPrivateStoryByCode(string code);
    Task<int?> GetAmountOfQuestionsForStory(int storyId); // added this
    Task<string?> GetCodeForStory(int storyId); // added this



    // Creation mode
    Task AddStory(Story story);
    Task UpdateStory(Story story);
    Task<bool> DeleteStory(int id);
    Task<bool> DoesCodeExist(string code);


    // Playing mode
    Task IncrementPlayed(int id);
    Task IncrementFinished(int id);
    Task IncrementFailed(int id);
} 