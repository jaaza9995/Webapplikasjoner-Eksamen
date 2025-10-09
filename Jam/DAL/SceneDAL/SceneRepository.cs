using Microsoft.EntityFrameworkCore;
using Jam.Models;
using Jam.Models.Enums;

namespace Jam.DAL.SceneDAL;

// Remember: should move business-logic to a service layer or the StoryController 

public class SceneRepository : ISceneRepository
{
    private readonly StoryDbContext _db;

    public SceneRepository(StoryDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Scene>> GetAllScenes()
    {
        return await _db.Scenes.ToListAsync();
    }

    public async Task<IEnumerable<Scene>> GetScenesByStoryId(int storyId)
    {
        return await _db.Scenes
            .Where(sc => sc.StoryId == storyId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Scene>> GetScenesInOrderByStoryId(int storyId)
    {
        /*
            Optionally include Question + AnswerOptions:
                var scenes = await _db.Scenes
                    .Where(sc => sc.StoryId == storyId)
                    .Include(sc => sc.Question!)
                        .ThenInclude(q => q.AnswerOptions)
                    .ToListAsync();
        */

        // Load all scenes for the story in one query 
        var scenes = await _db.Scenes
            .Where(sc => sc.StoryId == storyId)
            .ToListAsync();

        // Find the intro
        var intro = scenes.FirstOrDefault(sc => sc.SceneType == SceneType.Introduction);
        if (intro == null)
            return new List<Scene>();

        // Build a dictionary for O(1) lookups by id
        var byId = scenes.ToDictionary(sc => sc.SceneId);

        var ordered = new List<Scene>();
        var visited = new HashSet<int>();

        var current = intro;
        while (current != null && !visited.Contains(current.SceneId))
        {
            ordered.Add(current);
            visited.Add(current.SceneId);

            // If no next, break (will add endings manually after loop)
            if (current.NextSceneId == null)
                break;

            // Try get next from dictionary; if missing, break
            if (!byId.TryGetValue(current.NextSceneId.Value, out var next))
                break;

            current = next;
        }

        // Append endings in a deterministic order if they exist
        var good = scenes.FirstOrDefault(sc => sc.SceneType == SceneType.EndingGood);
        var neutral = scenes.FirstOrDefault(sc => sc.SceneType == SceneType.EndingNeutral);
        var bad = scenes.FirstOrDefault(sc => sc.SceneType == SceneType.EndingBad);

        if (good != null) ordered.Add(good);
        if (neutral != null) ordered.Add(neutral);
        if (bad != null) ordered.Add(bad);

        return ordered;
    }

    public async Task<Scene?> GetIntroSceneByStoryId(int storyId)
    {
        return await _db.Scenes
            .FirstOrDefaultAsync(sc => sc.StoryId == storyId && sc.SceneType == SceneType.Introduction);
    }

    public async Task<IEnumerable<Scene>> GetQuestionScenesInOrderByStoryId(int storyId)
    {
        /*
            Optionally include Question + AnswerOptions:
                var questionScenes = await _db.Scenes
                    .Where(sc => sc.StoryId == storyId)
                    .Include(sc => sc.Question!)
                        .ThenInclude(q => q.AnswerOptions)
                    .ToListAsync();
        */

        // Load all scenes for the story 
        var scenes = await _db.Scenes
            .Where(sc => sc.StoryId == storyId)
            .ToListAsync();

        var intro = scenes.FirstOrDefault(sc => sc.SceneType == SceneType.Introduction);
        if (intro == null)
            return new List<Scene>();

        var byId = scenes.ToDictionary(sc => sc.SceneId);
        var orderedQuestions = new List<Scene>();
        var visited = new HashSet<int>();

        var current = intro.NextSceneId.HasValue && byId.ContainsKey(intro.NextSceneId.Value)
            ? byId[intro.NextSceneId.Value]
            : null;

        while (current != null && !visited.Contains(current.SceneId))
        {
            if (current.SceneType == SceneType.Question)
                orderedQuestions.Add(current);

            visited.Add(current.SceneId);

            if (current.NextSceneId == null || !byId.TryGetValue(current.NextSceneId.Value, out var next))
                break;

            current = next;
        }

        return orderedQuestions;
    }

    public async Task<IEnumerable<Scene>> GetEndingScenesByStoryId(int storyId)
    {
        var endings = await _db.Scenes
            .Where(sc => sc.StoryId == storyId &&
                (sc.SceneType == SceneType.EndingGood ||
                sc.SceneType == SceneType.EndingNeutral ||
                sc.SceneType == SceneType.EndingBad))
            .ToListAsync();

        // Return endings in "order": EndingGood, EndingNeutral, EndingBad
        return endings
            .OrderBy(s => s.SceneType == SceneType.EndingGood ? 1 :
                          s.SceneType == SceneType.EndingNeutral ? 2 : 3)
            .ToList();
    }

    public async Task<Scene?> GetSceneById(int id)
    {
        return await _db.Scenes.FindAsync(id);
    }

    public async Task<Scene?> GetSceneWithDetailsById(int id)
    {
        return await _db.Scenes
            .Include(sc => sc.Question!)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(s => s.SceneId == id);
    }

    // ======================================================================================
    //   Creation mode
    // ======================================================================================

    public async Task CreateScene(Scene scene, int? previousSceneId)
    {
        _db.Scenes.Add(scene);
        await _db.SaveChangesAsync();

        if (previousSceneId != null)
        {
            var previousScene = await _db.Scenes.FindAsync(previousSceneId.Value);
            if (previousScene == null)
                throw new Exception("Previous scene not found");

            // Only update NextScene if allowed
            if (previousScene.SceneType == SceneType.Introduction || previousScene.SceneType == SceneType.Question)
            {
                // Ensure we don’t overwrite an existing NextScene
                if (previousScene.NextSceneId != null)
                    throw new Exception("Previous scene already points to a NextScene");

                previousScene.NextSceneId = scene.SceneId;
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task UpdateScene(Scene scene)
    {
        _db.Scenes.Update(scene);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteScene(int id, int? previousSceneId)
    {
        var scene = await _db.Scenes.FindAsync(id);
        if (scene == null)
        {
            return false;
        }

        if (previousSceneId != null) // if true = it is a Question-scene, if false = it is an Introduction-scene or an Ending-scene
        {
            var previousScene = await _db.Scenes.FindAsync(previousSceneId);
            if (previousScene == null)
                throw new Exception("Previous scene not found");
            previousScene.NextSceneId = scene.NextSceneId;
        }

        _db.Scenes.Remove(scene);
        await _db.SaveChangesAsync();
        return true;
    }

    // ======================================================================================
    //   Playing mode
    // ======================================================================================
    public async Task<Scene?> GetIntroductionForStory(int storyId)
    {
        return await _db.Scenes
            .FirstOrDefaultAsync(sc => sc.StoryId == storyId && sc.SceneType == SceneType.Introduction);
    }

    public async Task<Scene?> GetNextScene(int currentSceneId)
    {
        var currentScene = await _db.Scenes.FindAsync(currentSceneId);
        if (currentScene?.NextSceneId == null)
        {
            return null; // the current scene is the last Question-scene, so there is no NextScene
        }

        return await _db.Scenes
            .Include(sc => sc.Question!)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(s => s.SceneId == currentScene.NextSceneId);
    }

    public async Task<Scene?> GetEndingSceneForStory(int storyId, int userId)
    {
        /*
            Instead of accessing the PlayingSession based on storyId && UserId && latest StartTime,
            we can just access the PlayingSession directly via the user´s PlayingSessionId, considering 
            that we are able to provde the correct PlayingSessionId from the client side
        */

        var session = await _db.PlayingSessions
            .Where(ps => ps.UserId == userId && ps.StoryId == storyId)
            .OrderByDescending(ps => ps.StartTime)
            .FirstOrDefaultAsync();

        if (session == null || session.MaxScore == 0)
            return null; // no valid session or invalid data

        var percentage = (double)session.Score / session?.MaxScore * 100;

        SceneType endingType = percentage switch
        {
            >= 80 => SceneType.EndingGood,
            >= 40 => SceneType.EndingNeutral,
            _ => SceneType.EndingBad
        };

        return await _db.Scenes
            .Where(sc => sc.StoryId == storyId && sc.SceneType == endingType)
            .FirstOrDefaultAsync();
    }
}

