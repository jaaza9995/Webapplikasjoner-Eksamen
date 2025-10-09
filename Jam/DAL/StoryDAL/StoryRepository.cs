using Microsoft.EntityFrameworkCore;
using Jam.Models;
using Jam.Models.Enums;

namespace Jam.DAL.StoryDAL;

// Remember: should move business-logic to a service layer or the StoryController 

public class StoryRepository : IStoryRepository
{
    private readonly StoryDbContext _db;

    public StoryRepository(StoryDbContext db)
    {
        _db = db;
    }

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

    public async Task<IEnumerable<Story>> GetStoriesByUserId(int userId)
    {
        return await _db.Stories
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Story>> GetMostRecentPlayedStories(int userId, int count)
    {
        var lastSessions = await _db.PlayingSessions
            .Where(ps => ps.UserId == userId)
            .OrderByDescending(ps => ps.EndTime ?? ps.StartTime) // use EndTime if available
            .Take(count)
            .Include(ps => ps.Story) // load related story in the same query
            .ToListAsync();

        return lastSessions
            .Select(ps => ps.Story!)
            .Distinct() // in case the same story appears multiple times
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

    public async Task<Story?> GetPrivateStoryByCode(string code)
    {
        return await _db.Stories
            .FirstOrDefaultAsync(s => s.Code == code && s.Accessible == Accessibility.Private);
    }

    // ======================================================================================
    //   Creation mode
    // ======================================================================================

    public async Task CreateStory(Story story)
    {
        if (story.Accessible == Accessibility.Private)
        {
            string code = Guid.NewGuid().ToString("N")[..8].ToUpper(); // 8-char unique code
            while (await _db.Stories.AnyAsync(s => s.Code == code)) // ensure uniqueness
                code = Guid.NewGuid().ToString("N")[..8].ToUpper();
            story.Code = code;
        }
        _db.Stories.Add(story);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateStory(Story story)
    {
        if (story.Accessible == Accessibility.Private && string.IsNullOrEmpty(story.Code))
        {
            string code = Guid.NewGuid().ToString("N")[..8].ToUpper(); // 8-char unique code
            story.Code = code;
        }
        else if (story.Accessible == Accessibility.Public)
        {
            story.Code = null; // remove code if switching to public
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

    // ======================================================================================
    //   Playing mode
    // ======================================================================================
    public async Task IncrementPlayed(int id)
    {
        await _db.Stories
            .Where(s => s.StoryId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(st => st.Played, st => st.Played + 1)
                .SetProperty(st => st.Dnf, st => st.Dnf + 1));
    }

    public async Task IncrementFinished(int id)
    {
        var story = await _db.Stories.FindAsync(id);
        if (story != null)
        {
            story.Finished += 1;
            if (story.Dnf > 0) story.Dnf -= 1; // ensure Dnf doesn't go negative
            await _db.SaveChangesAsync();
        }
    }

    public async Task IncrementFailed(int id)
    {
        var story = await _db.Stories.FindAsync(id);
        if (story != null)
        {
            story.Failed += 1;
            if (story.Dnf > 0) story.Dnf -= 1; // ensure Dnf doesn't go negative
            await _db.SaveChangesAsync();
        }
    }
}

