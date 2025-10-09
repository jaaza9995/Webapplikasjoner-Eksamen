using Microsoft.EntityFrameworkCore;
using Jam.Models;

namespace Jam.DAL.QuestionDAL;

public class QuestionRepository : IQuestionRepository
{
    private readonly StoryDbContext _db;

    public QuestionRepository(StoryDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Question>> GetAllQuestions()
    {
        return await _db.Questions.ToListAsync();
    }

    public async Task<Question?> GetQuestionById(int id)
    {
        return await _db.Questions.FindAsync(id);
    }

    public async Task<Question?> GetQuestionBySceneId(int sceneId)
    {
        return await _db.Questions
            .FirstOrDefaultAsync(q => q.SceneId == sceneId);
    }

    // ======================================================================================
    //   Creation mode
    // ======================================================================================

    public async Task CreateQuestion(Question question)
    {
        _db.Questions.Add(question);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateQuestion(Question question)
    {
        _db.Questions.Update(question);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteQuestion(int id)
    {
        var question = await _db.Questions.FindAsync(id);
        if (question == null)
        {
            return false;
        }

        _db.Questions.Remove(question);
        await _db.SaveChangesAsync();
        return true;
    }
}

