using Jam.Models;

namespace Jam.DAL.AnswerOptionDAL;

public interface IAnswerOptionRepository
{
    Task<IEnumerable<Scene>> GetAllAnswerOptions();
    Task<AnswerOption?> GetAnswerOptionById(int id);
    Task<IEnumerable<AnswerOption>> GetAnswerOptionsByQuestionId(int questionId);

    // Creation mode
    Task CreateAnswerOption(AnswerOption answerOption);
    Task UpdateAnswerOption(AnswerOption answerOption);
    Task<bool> DeleteAnswerOption(int id);

    /* Playing mode:
        We don't need any specific methods here, and probably never will
        AnswerOption is always dependent on Question (Question = parent of 4 AnswerOptions)
        We never access an AnswerOption independently (always related to a Question)
    */
} 