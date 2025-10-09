using Jam.Models;

namespace Jam.DAL.PlayingSessionDAL;

public interface IPlayingSessionRepository
{
    // -------------------------------------------- READ --------------------------------------------
    Task<IEnumerable<PlayingSession>> GetAllPlayingSessions();
    Task<PlayingSession?> GetPlayingSessionById(int id);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserId(int userId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByStoryId(int storyId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserIdAndStoryId(int userId, int storyId);

    // ---------------------------- CREATE ----------------------------
    Task CreatePlayingSession(PlayingSession playingSession);
    Task<PlayingSession> CreatePlayingSession(int userId, int storyId);

    // ------------------------------------ UPDATE ------------------------------------
    Task MoveToNextScene(int sessionId, int nextSceneId);
    Task AnswerQuestion(int sessionId, int nextSceneId, int newScore, int newLevel);
    Task FinishSession(int sessionId, int finalScore, int newLevel);

    // ------------------ DELETE ------------------
    Task<bool> DeletePlayingSession(int id);
} 