using Microsoft.EntityFrameworkCore;
using Jam.Models;
using Jam.Models.Enums;

namespace Jam.DAL.PlayingSessionDAL;

public class PlayingSessionRepository : IPlayingSessionRepository
{
    private readonly StoryDbContext _db;

    public PlayingSessionRepository(StoryDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<PlayingSession>> GetAllPlayingSessions()
    {
        return await _db.PlayingSessions.ToListAsync();
    }

    public async Task<PlayingSession?> GetPlayingSessionById(int id)
    {
        return await _db.PlayingSessions.FindAsync(id);
    }

    public async Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserId(int userId)
    {
        return await _db.PlayingSessions
            .Where(ps => ps.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlayingSession>> GetPlayingSessionsByStoryId(int storyId)
    {
        return await _db.PlayingSessions
            .Where(ps => ps.StoryId == storyId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserIdAndStoryId(int userId, int storyId)
    {
        return await _db.PlayingSessions
            .Where(ps => ps.UserId == userId && ps.StoryId == storyId)
            .ToListAsync();
    }








    public async Task CreatePlayingSession(PlayingSession playingSession)
    {
        _db.PlayingSessions.Add(playingSession);
        await _db.SaveChangesAsync();
    }

    // This is a mock version of the CreatePlayingSession above (which is the correct one)
    // In this method, I combine business logic from PlayingSessionController in the DAL
    public async Task<PlayingSession> CreatePlayingSession(int userId, int storyId)
    {
        var scenes = await _db.Scenes
            .Where(sc => sc.StoryId == storyId)
            .ToListAsync();

        var intro = scenes.FirstOrDefault(sc => sc.SceneType == SceneType.Introduction);
        if (intro == null)
            throw new Exception("No introduction scene found for this story.");

        var questionCount = scenes.Count(sc => sc.SceneType == SceneType.Question);

        var session = new PlayingSession
        {
            StartTime = DateTime.UtcNow,
            Score = 0,
            MaxScore = questionCount * 10,
            CurrentLevel = 3,
            CurrentSceneId = intro.SceneId,
            StoryId = storyId,
            UserId = userId
        };

        _db.PlayingSessions.Add(session);
        await _db.SaveChangesAsync();

        return session;
    }






    // this is called from controller when user goes from Introduction to first Question-scene
    public async Task MoveToNextScene(int sessionId, int nextSceneId)
        => await UpdateSessionProgressAsync(sessionId, nextSceneId);

    // this is called from controller when user goes from a Question-scene to another Question-scene
    // this is also called from controller when user goes from the last Question-scene to an Ending-scene
    public async Task AnswerQuestion(int sessionId, int nextSceneId, int newScore, int newLevel)
        => await UpdateSessionProgressAsync(sessionId, nextSceneId, newScore, newLevel);

    // this is called when the user finished the Ending-scene
    public async Task FinishSession(int sessionId, int finalScore, int newLevel)
        => await UpdateSessionProgressAsync(sessionId, null, finalScore, newLevel);

    private async Task UpdateSessionProgressAsync(
        int sessionId,
        int? nextSceneId,
        int? newScore = null,
        int? newCurrentLevel = null)
    {
        var session = await _db.PlayingSessions.FindAsync(sessionId);
        if (session == null)
            throw new Exception("Session not found");

        if (nextSceneId.HasValue)
            session.CurrentSceneId = nextSceneId;

        if (newScore.HasValue)
            session.Score = newScore.Value;

        if (newCurrentLevel.HasValue)
            session.CurrentLevel = newCurrentLevel.Value;

        if (nextSceneId == null)
            session.EndTime = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }







    public async Task<bool> DeletePlayingSession(int id)
    {
        var playingSession = await _db.PlayingSessions.FindAsync(id);
        if (playingSession == null)
        {
            return false;
        }

        _db.PlayingSessions.Remove(playingSession);
        await _db.SaveChangesAsync();
        return true;
    }
}