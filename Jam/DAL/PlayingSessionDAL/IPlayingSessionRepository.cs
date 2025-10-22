using Jam.Models;

namespace Jam.DAL.PlayingSessionDAL;

public interface IPlayingSessionRepository
{
    // Read
    Task<IEnumerable<PlayingSession>> GetAllPlayingSessions();
    Task<PlayingSession?> GetPlayingSessionById(int id);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserId(int userId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByStoryId(int storyId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserIdAndStoryId(int userId, int storyId);
    Task<int> GetUserHighScoreForStory(int userId, int storyId);



    // Create
    Task AddPlayingSession(PlayingSession playingSession);
    // Task<PlayingSession> CreatePlayingSession(int userId, int storyId); no longer needed



    // Update
    Task MoveToNextScene(int sessionId, int nextSceneId);
    Task AnswerQuestion(int sessionId, int nextSceneId, int newScore, int newLevel);
    Task FinishSession(int sessionId, int finalScore, int newLevel);



    // Delete
    Task<bool> DeletePlayingSession(int id);
} 