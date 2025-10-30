using Jam.Models;

namespace Jam.DAL.SceneDAL;

public interface ISceneRepository
{
    // Methods to handle IntroScene
    Task<IntroScene?> GetIntroSceneByStoryId(int storyId);
    Task<IntroScene?> GetIntroSceneById(int introSceneId); // new method
    Task<bool> AddIntroScene(IntroScene introScene);
    Task<bool> UpdateIntroScene(IntroScene introScene);
    Task<bool> DeleteIntroScene(int introSceneId);



    // Methods to handle QuestionScenes
    Task<IEnumerable<QuestionScene>> GetQuestionScenesByStoryId(int storyId);
    Task<QuestionScene?> GetFirstQuestionSceneByStoryId(int storyId); // new method (very important method)
    Task<QuestionScene?> GetQuestionSceneById(int questionSceneId);
    Task<QuestionScene?> GetQuestionSceneWithAnswerOptionsById(int questionSceneId); // new method to get QuestionScene with AnswerOptions
    Task<bool> AddQuestionScene(QuestionScene questionScene);
    Task<bool> UpdateQuestionScene(QuestionScene questionScene);
    Task<bool> DeleteQuestionScene(int questionSceneId, int? previousSceneId = null);



    // Methods to handle EndingScenes
    Task<IEnumerable<EndingScene>> GetEndingScenesByStoryId(int storyId);
    Task<EndingScene?> GetEndingSceneById(int endingSceneId); // new method
    Task<EndingScene?> GetGoodEndingSceneByStoryId(int storyId);
    Task<EndingScene?> GetNeutralEndingSceneByStoryId(int storyId);
    Task<EndingScene?> GetBadEndingSceneByStoryId(int storyId);
    Task<bool> AddEndingScene(EndingScene endingScene);
    Task<bool> UpdateEndingScene(EndingScene EndingScene);
    Task<bool> DeleteEndingScene(int endingSceneId);
}

