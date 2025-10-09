using Jam.Models;

namespace Jam.DAL.QuestionDAL;

public interface IQuestionRepository
{
    Task<IEnumerable<Question>> GetAllQuestions();
    Task<Question?> GetQuestionById(int id);
    Task<Question?> GetQuestionBySceneId(int sceneId);

    // Creation mode
    Task CreateQuestion(Question question);
    Task UpdateQuestion(Question question);
    Task<bool> DeleteQuestion(int id);

    /* Playing mode:
        We don't need any specific methods here for now (because we never play an independent question)
        Questions are always accessed through Scenes (see GetNextScene under Playing mode of SceneRepository)
        In the future, if we want to play independent questions, we can add methods here
    */
} 