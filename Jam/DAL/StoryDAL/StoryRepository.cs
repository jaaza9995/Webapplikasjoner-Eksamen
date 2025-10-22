using Microsoft.EntityFrameworkCore;
using Jam.Models;
using Jam.Models.Enums;

namespace Jam.DAL.StoryDAL;

// Remember: should move business-logic to a service layer or Controller 

public class StoryRepository : IStoryRepository
{
    private readonly StoryDbContext _db;

    public StoryRepository(StoryDbContext db)
    {
        _db = db;
    }


    // --------------------------------------- Read ---------------------------------------

    public async Task<IEnumerable<Story>> GetAllStories()
    {
        return await _db.Stories.ToListAsync();
    }
    public async Task<IEnumerable<Story>> GetAllPublicStories()
    {
        return await _db.Stories
            .Where(s => s.Accessible == Accessibility.Public)
            .ToListAsync();
    }

    // Added this new method to access all private stories
    public async Task<IEnumerable<Story>> GetAllPrivateStories()
    {
        return await _db.Stories
            .Where(s => s.Accessible == Accessibility.Private)
            .ToListAsync();
    }

    public async Task<IEnumerable<Story>> GetStoriesByUserId(int userId)
    {
        return await _db.Stories
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    // moved .Take(count) down from var lastSessions = await _db.PlayingSessions... to return lastSessions...
    // updated parameter (int userId, int count) to (int? userId, int count) 
    public async Task<IEnumerable<Story>> GetMostRecentPlayedStories(int? userId, int count)
    {
        if (userId == null)
            return Enumerable.Empty<Story>();

        var lastSessions = await _db.PlayingSessions
            .Where(ps => ps.UserId == userId)
            .OrderByDescending(ps => ps.EndTime ?? ps.StartTime) // use EndTime if available
            .Include(ps => ps.Story) // load related story in the same query
            .ToListAsync();

        return lastSessions
            .Select(ps => ps.Story!)
            .Distinct() // in case the same story appears multiple times
            .Take(count)
            .ToList();
    }

    public async Task<Story?> GetStoryById(int id)
    {
        return await _db.Stories.FindAsync(id);
    }

    public async Task<Story?> GetPublicStoryById(int id)
    {
        return await _db.Stories
            .FirstOrDefaultAsync(s => s.StoryId == id && s.Accessible == Accessibility.Public);
    }

    public async Task<Story?> GetPrivateStoryByCode(string GameCode)
    {
        return await _db.Stories
            .FirstOrDefaultAsync(s => s.GameCode == GameCode && s.Accessible == Accessibility.Private);
    }

    // Created this new method to count the amount of questions in a Story
    public async Task<int?> GetAmountOfQuestionsForStory(int storyId)
    {
        var questionCount = await _db.QuestionScenes
            .Where(s => s.StoryId == storyId)
            .CountAsync();

        return questionCount;
    }

    // Created this new method to get the code for a Story
    public async Task<string?> GetCodeForStory(int storyId)
    {
        var GameCode = await _db.Stories
        .Where(s => s.StoryId == storyId)
        .Select(s => s.GameCode)
        .FirstOrDefaultAsync();

        return GameCode;
    }




    // --------------------------------------- Creation Mode ---------------------------------------

    // I have moved the if-test and its codeblock to create a code out of DAL 
    // This logic is now in StoryCreationController as a private method
    public async Task AddStory(Story story)
    {
        _db.Stories.Add(story);
        await _db.SaveChangesAsync();
    }

    // I created this new method which is used in both StoryCreationController and
    // StoryEditController. It is used by the method GenerateUniqueStoryCodeAsync()
    // DoesCodeExist(string code) checks if the gerenated code is unique or not
    public async Task<bool> DoesCodeExist(string GameCode)
    {
        return await _db.Stories.AnyAsync(s => s.GameCode == GameCode);
    }

    // Have to move business logic (for creating code) out of this method
    public async Task UpdateStory(Story story)
    {
        if (story.Accessible == Accessibility.Private && string.IsNullOrEmpty(story.GameCode))
        {
            string GameCode = Guid.NewGuid().ToString("N")[..8].ToUpper(); // 8-char unique code
            story.GameCode = GameCode;
        }
        else if (story.Accessible == Accessibility.Public)
        {
            story.GameCode = null; // remove code if switching to public
        }

        _db.Stories.Update(story);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteStory(int id)
    {
        var story = await _db.Stories.FindAsync(id);
        if (story == null)
        {
            return false;
        }

        _db.Stories.Remove(story);
        await _db.SaveChangesAsync();
        return true;
    }




    // --------------------------------------- Playing Mode ---------------------------------------
    
    public async Task IncrementPlayed(int storyId)
    {
        await _db.Stories
            .Where(s => s.StoryId == storyId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(st => st.Played, st => st.Played + 1)
                .SetProperty(st => st.Dnf, st => st.Dnf + 1));
    }

    // this method has been updated
    public async Task IncrementFinished(int storyId)
    {
        await _db.Stories
        .Where(s => s.StoryId == storyId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(st => st.Finished, st => st.Finished + 1)
            .SetProperty(st => st.Dnf, st => st.Dnf > 0 ? st.Dnf - 1 : 0));
    }

    // this method has been updated
    public async Task IncrementFailed(int storyId)
    {
        await _db.Stories
        .Where(s => s.StoryId == storyId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(st => st.Failed, st => st.Failed + 1)
            .SetProperty(st => st.Dnf, st => st.Dnf > 0 ? st.Dnf - 1 : 0));
    }
}

