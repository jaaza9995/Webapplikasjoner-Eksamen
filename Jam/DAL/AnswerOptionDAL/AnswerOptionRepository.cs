using Microsoft.EntityFrameworkCore;
using Jam.Models;

namespace Jam.DAL.AnswerOptionDAL;

public class AnswerOptionRepository : IAnswerOptionRepository
{
    private readonly StoryDbContext _db;

    public AnswerOptionRepository(StoryDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Scene>> GetAllAnswerOptions()
    {
        return await _db.Scenes.ToListAsync();
    }

    public async Task<AnswerOption?> GetAnswerOptionById(int id)
    {
        return await _db.AnswerOptions.FindAsync(id);
    }

    public async Task<IEnumerable<AnswerOption>> GetAnswerOptionsByQuestionId(int questionId)
    {
        return await _db.AnswerOptions
            .Where(ao => ao.QuestionId == questionId)
            .ToListAsync();
    }

    // ======================================================================================
    //   Creation mode
    // ======================================================================================

    public async Task CreateAnswerOption(AnswerOption answerOption)
    {
        _db.AnswerOptions.Add(answerOption);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAnswerOption(AnswerOption answerOption)
    {
        _db.AnswerOptions.Update(answerOption);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAnswerOption(int id)
    {
        var answerOption = await _db.AnswerOptions.FindAsync(id);
        if (answerOption == null)
        {
            return false;
        }

        _db.AnswerOptions.Remove(answerOption);
        await _db.SaveChangesAsync();
        return true;
    }
}