using Jam.Models;

namespace Jam.DAL.StoryDAL;

public interface IStoryRepository
{
    Task<IEnumerable<Story>> GetAllStories();
    Task<IEnumerable<Story>> GetAllPublicStories();
    Task<IEnumerable<Story>> GetStoriesByUserId(int userId);
    Task<IEnumerable<Story>> GetMostRecentPlayedStories(int userId, int count = 5);
    Task<Story?> GetStoryById(int id);
    Task<Story?> GetPublicStoryById(int id);
    Task<Story?> GetPrivateStoryByCode(string code);


    // Creation mode
    Task CreateStory(Story story);
    Task UpdateStory(Story story);
    Task<bool> DeleteStory(int id);

    // Playing mode
    Task IncrementPlayed(int id);
    Task IncrementFinished(int id);
    Task IncrementFailed(int id);
} 