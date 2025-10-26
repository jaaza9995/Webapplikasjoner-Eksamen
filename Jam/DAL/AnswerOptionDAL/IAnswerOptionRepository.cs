using Jam.Models;

namespace Jam.DAL.AnswerOptionDAL;

public interface IAnswerOptionRepository
{
    // Read
    Task<IEnumerable<AnswerOption>?> GetAllAnswerOptions();
    Task<IEnumerable<AnswerOption>?> GetAnswerOptionByQuestionSceneId(int questionSceneId);
    Task<AnswerOption?> GetAnswerOptionById(int id);


    // Create
    Task AddAnswerOption(AnswerOption answerOption);


    // Update
    Task UpdateAnswerOption(AnswerOption answerOption);

    // Delete
    Task<bool> DeleteAnswerOption(int id);
} 