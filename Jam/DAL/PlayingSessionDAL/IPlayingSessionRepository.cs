using Jam.Models;
using Jam.Models.Enums;

namespace Jam.DAL.PlayingSessionDAL;

public interface IPlayingSessionRepository
{
    // Read / GET
    Task<IEnumerable<PlayingSession>> GetAllPlayingSessions();
    Task<PlayingSession?> GetPlayingSessionById(int playingSessionId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserId(int userId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByStoryId(int storyId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserIdAndStoryId(int userId, int storyId);
    Task<int?> GetUserHighScoreForStory(int userId, int storyId);



    // Create
    Task<bool> AddPlayingSession(PlayingSession playingSession);



    // Update
    Task<bool> MoveToNextScene(int playingSessionId, int nextSceneId, SceneType newSceneType); // added SceneType newSceneType
    Task<bool> AnswerQuestion(int playingSessionId, int nextSceneId, SceneType newSceneType, int newScore, int newLevel); // added SceneType newSceneType
    Task<bool> FinishSession(int playingSessionId, int finalScore, int finalLevel);



    // Delete
    Task<bool> DeletePlayingSession(int id);
}