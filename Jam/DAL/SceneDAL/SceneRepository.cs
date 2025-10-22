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


    // --------------------------------- INTRO SCENE ---------------------------------

    public async Task<IntroScene?> GetIntroSceneByStoryId(int storyId)
    {
        return await _db.IntroScenes.FirstOrDefaultAsync(i => i.StoryId == storyId);
    }

    public async Task AddIntroScene(IntroScene introScene)
    {
        _db.IntroScenes.Add(introScene);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateIntroScene(IntroScene introScene)
    {
        _db.IntroScenes.Update(introScene);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteIntroScene(int introSceneId)
    {
        var scene = await _db.IntroScenes.FindAsync(introSceneId);
        if (scene == null)
        {
            return false;
        }

        _db.IntroScenes.Remove(scene);
        await _db.SaveChangesAsync();
        return true;
    }




    // --------------------------------- Question SCENE ---------------------------------

    public async Task<IEnumerable<QuestionScene>> GetQuestionScenesByStoryId(int storyId)
    {
        return await _db.QuestionScenes
            .Where(q => q.StoryId == storyId)
            .Include(q => q.AnswerOptions)
            .ToListAsync();
    }

    public async Task<QuestionScene?> GetQuestionSceneById(int id)
    {
        return await _db.QuestionScenes
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.QuestionSceneId == id);
    }


    public async Task AddQuestionScene(QuestionScene scene)
    {
        _db.QuestionScenes.Add(scene);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateQuestionScene(QuestionScene scene)
    {
        _db.QuestionScenes.Update(scene);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteQuestionScene(int id, int? previousSceneId)
    {
        var scene = await _db.QuestionScenes.FindAsync(id);
        if (scene == null)
        {
            return false;
        }

        // if false, it means that we are dealing with the first QuestionScene
        // When removing the first QuestionScene, we do not have to deal with previousSceneId
        // Thats because the IntroScene for a Story does NOT point to the first QuestionScene
        if (previousSceneId != null)
        {
            var previousScene = await _db.QuestionScenes.FindAsync(previousSceneId);
            if (previousScene == null)
                throw new Exception("Previous scene not found");
            previousScene.NextQuestionSceneId = scene.NextQuestionSceneId;
        }

        _db.QuestionScenes.Remove(scene);
        await _db.SaveChangesAsync();
        return true;
    }





    // --------------------------------- Ending SCENE ---------------------------------

    public async Task<IEnumerable<EndingScene>> GetEndingScenesByStoryId(int storyId)
    {
        return await _db.EndingScenes
            .Where(e => e.StoryId == storyId)
            .ToListAsync();
    }

    public async Task<EndingScene?> GetGoodEndingSceneByStoryId(int storyId)
    {
        return await _db.EndingScenes
            .FirstOrDefaultAsync(e => e.StoryId == storyId && e.EndingType == EndingType.Good);
    }

    public async Task<EndingScene?> GetNeutralEndingSceneByStoryId(int storyId)
    {
        return await _db.EndingScenes
            .FirstOrDefaultAsync(e => e.StoryId == storyId && e.EndingType == EndingType.Neutral);
    }

    public async Task<EndingScene?> GetBadEndingSceneByStoryId(int storyId)
    {
        return await _db.EndingScenes
            .FirstOrDefaultAsync(e => e.StoryId == storyId && e.EndingType == EndingType.Bad);
    }

    public async Task AddEndingScene(EndingScene endingScene)
    {
        _db.EndingScenes.Add(endingScene);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateEndingScene(EndingScene endingScene)
    {
        _db.EndingScenes.Update(endingScene);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteEndingScene(int id)
    {
        var scene = await _db.EndingScenes.FindAsync(id);
        if (scene == null)
        {
            return false;
        }

        _db.EndingScenes.Remove(scene);
        await _db.SaveChangesAsync();
        return true;
    }

}




/*
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
        // Optionally include Question + AnswerOptions:
        //     var scenes = await _db.Scenes
        //         .Where(sc => sc.StoryId == storyId)
        //         .Include(sc => sc.Question!)
        //         .ThenInclude(q => q.AnswerOptions)
        //         .ToListAsync();
        

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
        // Optionally include Question + AnswerOptions:
        //      var questionScenes = await _db.Scenes
        //          .Where(sc => sc.StoryId == storyId)
        //          .Include(sc => sc.Question!)
        //              .ThenInclude(q => q.AnswerOptions)
        //          .ToListAsync();

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

    public async Task<Scene?> GetSceneWithDetailsPreloaded(int id)
    {
        return await _db.Scenes
            .Include(sc => sc.Question!)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(s => s.SceneId == id);
    }

    // ======================================================================================
    //   Creation mode
    // ======================================================================================


    // I have moved the if-test and its codeblock to link previousScene --> scene out of DAL
    // This logic is now in StoryCreationController as a private method: AddSceneAsync()
    public async Task CreateScene(Scene scene)
    {
        _db.Scenes.Add(scene);
        await _db.SaveChangesAsync();
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

        if (previousSceneId != null) // if true = Question-scene, if false = Introduction Scene or Ending Scene
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

    // I have updated this method to return a scene without loading related data
    public async Task<Scene?> GetNextScene(int currentSceneId)
    {
        var currentScene = await _db.Scenes.FindAsync(currentSceneId);
        if (currentScene?.NextSceneId == null)
        {
            return null; // the current scene is the last Question-scene, so there is no NextScene
        }

        // Just return the next scene without loading Question and AnswerOptions
        return await _db.Scenes.FirstOrDefaultAsync(s => s.SceneId == currentScene.NextSceneId);

        // return await _db.Scenes
        //    .Include(sc => sc.Question!)
        //    .ThenInclude(q => q.AnswerOptions)
        //    .FirstOrDefaultAsync(s => s.SceneId == currentScene.NextSceneId);
    }

    // This method will no longer be in use
    // I have moved the business logic of choosing the right scene to the controller
    // Instead I have added 3 new methods to fetch each of the 3 Ending Scene types
    public async Task<Scene?> GetEndingSceneForStory(int storyId, int userId)
    {
        //    Instead of accessing the PlayingSession based on storyId && UserId && latest StartTime,
        //    we can just access the PlayingSession directly via the user´s PlayingSessionId, considering 
        //    that we are able to provde the correct PlayingSessionId from the client side

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

    // New method
    public async Task<Scene?> GetGoodEndingScene(int storyId)
    {
        return await _db.Scenes
            .Where(sc => sc.StoryId == storyId && sc.SceneType == SceneType.EndingGood)
            .FirstOrDefaultAsync();
    }

    // New method
    public async Task<Scene?> GetNeutralEndingScene(int storyId)
    {
        return await _db.Scenes
            .Where(sc => sc.StoryId == storyId && sc.SceneType == SceneType.EndingNeutral)
            .FirstOrDefaultAsync();
    }

    // New method
    public async Task<Scene?> GetBadEndingScene(int storyId)
    {
        return await _db.Scenes
            .Where(sc => sc.StoryId == storyId && sc.SceneType == SceneType.EndingBad)
            .FirstOrDefaultAsync();
    }
*/