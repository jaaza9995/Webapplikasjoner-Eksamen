using Jam.Models;

namespace Jam.DAL.SceneDAL;

public interface ISceneRepository
{
    // Methods to handle IntroScene
    Task<IntroScene?> GetIntroSceneByStoryId(int storyId);
    Task AddIntroScene(IntroScene introScene);
    Task UpdateIntroScene(IntroScene introScene);
    Task<bool> DeleteIntroScene(int id);



    // Methods to handle QuestionScenes
    Task<IEnumerable<QuestionScene>> GetQuestionScenesByStoryId(int storyId);
    Task<QuestionScene?> GetQuestionSceneById(int id);
    Task AddQuestionScene(QuestionScene questionScene);
    Task UpdateQuestionScene(QuestionScene questionScene);
    Task<bool> DeleteQuestionScene(int id, int? previousSceneId = null);



    // Methods to handle EndingScenes
    Task<IEnumerable<EndingScene>> GetEndingScenesByStoryId(int storyId);
    Task<EndingScene?> GetGoodEndingSceneByStoryId(int storyId);
    Task<EndingScene?> GetNeutralEndingSceneByStoryId(int storyId);
    Task<EndingScene?> GetBadEndingSceneByStoryId(int storyId);
    Task AddEndingScene(EndingScene endingScene);
    Task UpdateEndingScene(EndingScene EndingScene);
    Task<bool> DeleteEndingScene(int id);



    /*
    Task<IEnumerable<Scene>> GetAllScenes();
    Task<IEnumerable<Scene>> GetScenesByStoryId(int storyId);
    Task<IEnumerable<Scene>> GetScenesInOrderByStoryId(int storyId);
    Task<Scene?> GetIntroSceneByStoryId(int storyId); // same as GetIntroductionForStory
    Task<IEnumerable<Scene>> GetQuestionScenesInOrderByStoryId(int storyId);
    // Task<IEnumerable<Scene>> GetEndingScenesByStoryId(int storyId);
    Task<Scene?> GetGoodEndingScene(int storyId);
    Task<Scene?> GetNeutralEndingScene(int storyId);
    Task<Scene?> GetBadEndingScene(int storyId);
    Task<Scene?> GetSceneById(int id);
    Task<Scene?> GetSceneWithDetailsPreloaded(int id); // changed the name here

    // Creation mode
    Task CreateScene(Scene scene);
    Task UpdateScene(Scene scene);
    Task<bool> DeleteScene(int id, int? previousSceneId = null);

    // Playing mode
    Task<Scene?> GetIntroductionForStory(int storyId);
    Task<Scene?> GetNextScene(int currentSceneId);
    Task<Scene?> GetEndingSceneForStory(int storyId, int userId);
    */

}