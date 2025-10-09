using Jam.Models;

namespace Jam.DAL.SceneDAL;

public interface ISceneRepository
{
    Task<IEnumerable<Scene>> GetAllScenes();
    Task<IEnumerable<Scene>> GetScenesByStoryId(int storyId);
    Task<IEnumerable<Scene>> GetScenesInOrderByStoryId(int storyId);
    Task<Scene?> GetIntroSceneByStoryId(int storyId); // same as GetIntroductionForStory
    Task<IEnumerable<Scene>> GetQuestionScenesInOrderByStoryId(int storyId);
    Task<IEnumerable<Scene>> GetEndingScenesByStoryId(int storyId);
    Task<Scene?> GetSceneById(int id);
    Task<Scene?> GetSceneWithDetailsById(int id);

    // Creation mode
    Task CreateScene(Scene scene, int? previousSceneId = null);
    Task UpdateScene(Scene scene);
    Task<bool> DeleteScene(int id, int? previousSceneId = null);

    // Playing mode
    Task<Scene?> GetIntroductionForStory(int storyId);
    Task<Scene?> GetNextScene(int currentSceneId);
    Task<Scene?> GetEndingSceneForStory(int storyId, int userId);

} 