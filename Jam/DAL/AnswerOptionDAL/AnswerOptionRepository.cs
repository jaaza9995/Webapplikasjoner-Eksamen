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


    // --------------------------------------- Read ---------------------------------------

    public async Task<IEnumerable<AnswerOption>> GetAllAnswerOptions()
    {
        return await _db.AnswerOptions.ToListAsync();
    }

    public async Task<IEnumerable<AnswerOption>> GetAnswerOptionByQuestionSceneId(int questionSceneId)
    {
        return await _db.AnswerOptions
            .Where(ao => ao.QuestionSceneId == questionSceneId)
            .ToListAsync();
    }

    public async Task<AnswerOption?> GetAnswerOptionById(int id)
    {
        return await _db.AnswerOptions.FindAsync(id);
    }




    // --------------------------------------- Create ---------------------------------------

    public async Task AddAnswerOption(AnswerOption answerOption)
    {
        _db.AnswerOptions.Add(answerOption);
        await _db.SaveChangesAsync();
    }



    // --------------------------------------- Update ---------------------------------------

    public async Task UpdateAnswerOption(AnswerOption answerOption)
    {
        _db.AnswerOptions.Update(answerOption);
        await _db.SaveChangesAsync();
    }



    // --------------------------------------- Delete ---------------------------------------

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