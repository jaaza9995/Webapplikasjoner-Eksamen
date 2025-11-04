using Microsoft.EntityFrameworkCore;
using Jam.Models;
using Jam.Models.Enums;

namespace Jam.DAL.SceneDAL;

// Might want to consider avoiding logging entire EF entities
// Consider adding AsNoTracking() to read-only queries for performance boost

// I am including IntroScene as Entities in some of the log messages, that is
// fine, but for QuestionScene I think I will avoid this for now, due to its 
// potentially large size (?), especially when including AnswerOptions 

public class SceneRepository : ISceneRepository
{
    private readonly StoryDbContext _db;
    private readonly ILogger<SceneRepository> _logger;

    public SceneRepository(StoryDbContext db, ILogger<SceneRepository> logger)
    {
        _db = db;
        _logger = logger;
    }


    // --------------------------------- INTRO SCENE ---------------------------------

    public async Task<IntroScene?> GetIntroSceneByStoryId(int storyId)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetIntroSceneByStoryId] Invalid storyId provided: {storyId}", storyId);
            return null;
        }

        try
        {
            var intro = await _db.IntroScenes
                .Where(s => s.StoryId == storyId)
                .FirstOrDefaultAsync();

            if (intro == null)
            {
                _logger.LogInformation("[SceneRepository -> GetIntroSceneByStoryId] No IntroScene found for storyId {storyId}", storyId);
            }

            return intro;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetIntroSceneByStoryId] Error retrieving IntroScene for storyId {storyId}", storyId);
            return null;
        }
    }

    public async Task<IntroScene?> GetIntroSceneById(int introSceneId)
    {
        if (introSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetIntroSceneById] Invalid IntroSceneId provided: {introSceneId}", introSceneId);
            return null;
        }

        try
        {
            var intro = await _db.IntroScenes.FindAsync(introSceneId);
            if (intro == null)
            {
                _logger.LogInformation("[SceneRepository -> GetIntroSceneById] No IntroScene found with id {introSceneId}", introSceneId);
            }

            return intro;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetIntroSceneById] Error retrieving IntroScene with id {introSceneId}", introSceneId);
            return null;
        }
    }

    public async Task<bool> AddIntroScene(IntroScene introScene)
    {
        if (introScene == null)
        {
            _logger.LogWarning("[SceneRepository -> AddIntroScene] Cannot add a null IntroScene");
            return false;
        }

        try
        {
            _db.IntroScenes.Add(introScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> AddIntroScene] Error adding IntroScene {introScene}", introScene);
            return false;
        }
    }

    public async Task<bool> UpdateIntroScene(IntroScene introScene)
    {
        if (introScene == null)
        {
            _logger.LogWarning("[SceneRepository -> UpdateIntroScene] Cannot update a null IntroScene");
            return false;
        }

        try
        {
            _db.IntroScenes.Update(introScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> UpdateIntroScene] Error updating IntroScene {introScene}", introScene);
            return false;
        }
    }

    public async Task<bool> DeleteIntroScene(int introSceneId)
    {
        if (introSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> DeleteIntroScene] Invalid IntroScene id provided: {introSceneId}", introSceneId);
            return false;
        }

        try
        {
            var intro = await _db.IntroScenes.FindAsync(introSceneId);
            if (intro == null)
            {
                _logger.LogInformation("[SceneRepository -> DeleteIntroScene] No IntroScene found with id {introSceneId}", introSceneId);
                return false;
            }

            _db.IntroScenes.Remove(intro);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> DeleteIntroScene] Error deleting IntroScene with id {introSceneId}", introSceneId);
            return false;
        }
    }




    // --------------------------------- Question SCENE ---------------------------------

    public async Task<IEnumerable<QuestionScene>> GetQuestionScenesByStoryId(int storyId)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetQuestionScenesByStoryId] Invalid storyId provided: {storyId}", storyId);
            return Enumerable.Empty<QuestionScene>(); // not returning null to avoid null reference exceptions
        }

        try
        {
            return await _db.QuestionScenes
                .Where(q => q.StoryId == storyId)
                .Include(q => q.AnswerOptions) // eager load AnswerOptions (not sure if wanted here)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetQuestionScenesByStoryId] Error retrieving QuestionScenes for storyId {storyId}", storyId);
            return Enumerable.Empty<QuestionScene>(); ; // not returning null to avoid null reference exceptions
        }
    }

    public async Task<QuestionScene?> GetFirstQuestionSceneByStoryId(int storyId)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetFirstQuestionSceneByStoryId] Invalid storyId provided: {storyId}", storyId);
            return null;
        }

        try
        {
            // Step 1: Collect all NextQuestionSceneIds for the given story
            var nonStartingIds = await _db.QuestionScenes
                .Where(qs => qs.StoryId == storyId && qs.NextQuestionSceneId.HasValue)
                .Select(qs => qs.NextQuestionSceneId!.Value)
                .Distinct()
                .ToListAsync();

            // Step 2: Find the single QuestionScene that belongs to the story 
            // AND whose ID is NOT in the list of non-starting IDs collected above
            var firstQuestionScene = await _db.QuestionScenes
                .Where(qs => qs.StoryId == storyId && !nonStartingIds.Contains(qs.QuestionSceneId))
                .FirstOrDefaultAsync();

            return firstQuestionScene;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetFirstQuestionSceneByStoryId] Error retrieving first QuestionScene for storyId {storyId}", storyId);
            return null;
        }
    }

    public async Task<QuestionScene?> GetQuestionSceneById(int questionSceneId)
    {
        if (questionSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetQuestionSceneById] Invalid QuestionSceneId provided: {questionSceneId}", questionSceneId);
            return null;
        }

        try
        {
            var questionScene = await _db.QuestionScenes.FindAsync(questionSceneId);
            if (questionScene == null)
            {
                _logger.LogWarning("[SceneRepository -> GetQuestionSceneById] No QuestionScene found with id {questionSceneId}", questionSceneId);
            }

            return questionScene;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetQuestionSceneById] Error retrieving QuestionScene with id {questionSceneId}", questionSceneId);
            return null;
        }
    }

    // New method to get QuestionScene with its AnswerOptions eagerly loaded
    public async Task<QuestionScene?> GetQuestionSceneWithAnswerOptionsById(int questionSceneId)
    {
        if (questionSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetQuestionSceneWithAnswerOptionsById] Invalid QuestionSceneId provided: {questionSceneId}", questionSceneId);
            return null;
        }

        try
        {
            var questionScene = await _db.QuestionScenes
                .Include(q => q.AnswerOptions) // eager load AnswerOptions
                .FirstOrDefaultAsync(q => q.QuestionSceneId == questionSceneId);

            if (questionScene == null)
            {
                _logger.LogInformation("[SceneRepository -> GetQuestionSceneWithAnswerOptionsById] No QuestionScene found with id {questionSceneId}", questionSceneId);
            }

            return questionScene;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetQuestionSceneWithAnswerOptionsById] Error retrieving QuestionScene with id {questionSceneId}", questionSceneId);
            return null;
        }
    }


    public async Task<bool> AddQuestionScene(QuestionScene scene)
    {
        if (scene == null)
        {
            _logger.LogWarning("[SceneRepository -> AddQuestionScene] Cannot add a null QuestionScene");
            return false;
        }

        try
        {
            _db.QuestionScenes.Add(scene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> AddQuestionScene] Error adding QuestionScene for storyId {storyId}", scene.StoryId);
            return false;
        }
    }

    public async Task<bool> UpdateQuestionScene(QuestionScene scene)
    {
        if (scene == null)
        {
            _logger.LogWarning("[SceneRepository -> UpdateQuestionScene] Cannot update a null QuestionScene");
            return false;
        }

        try
        {
            _db.QuestionScenes.Update(scene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> UpdateQuestionScene] Error updating QuestionScene with id {questionSceneId}", scene.QuestionSceneId);
            return false;
        }
    }

    // This might be considered business logic and should be moved to the service layer or controller
    public async Task<bool> DeleteQuestionScene(int questionSceneId, int? previousSceneId)
    {
        if (questionSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> DeleteQuestionScene] Invalid QuestionSceneId provided: {questionSceneId}", questionSceneId);
            return false;
        }

        if (previousSceneId != null && previousSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> DeleteQuestionScene] Invalid previousSceneId provided: {previousSceneId}", previousSceneId);
            return false;
        }

        try
        {
            var scene = await _db.QuestionScenes.FindAsync(questionSceneId);
            if (scene == null)
            {
                _logger.LogWarning("[SceneRepository -> DeleteQuestionScene] No QuestionScene found with id {questionSceneId}", questionSceneId);
                return false;
            }

            // if false, it means that we are dealing with the first QuestionScene
            // When removing the first QuestionScene, we do not have to deal with previousSceneId
            // Thats because the IntroScene for a Story does NOT point to the first QuestionScene
            if (previousSceneId != null)
            {
                var previousScene = await _db.QuestionScenes.FindAsync(previousSceneId);
                if (previousScene == null)
                {
                    _logger.LogWarning("[SceneRepository -> DeleteQuestionScene] No previous QuestionScene found with id {previousSceneId}", previousSceneId);
                    return false;
                }

                // TODO: Move scene-link update logic to service layer when refactoring business logic
                previousScene.NextQuestionSceneId = scene.NextQuestionSceneId;
            }

            _db.QuestionScenes.Remove(scene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> DeleteQuestionScene] Error deleting QuestionScene with id {questionSceneId}", questionSceneId);
            return false;
        }
    }





    // --------------------------------- Ending SCENE ---------------------------------

    public async Task<IEnumerable<EndingScene>> GetEndingScenesByStoryId(int storyId)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetEndingScenesByStoryId] Invalid storyId provided: {storyId}", storyId);
            return Enumerable.Empty<EndingScene>(); // not returning null to avoid null reference exceptions
        }

        try
        {
            return await _db.EndingScenes
                .Where(e => e.StoryId == storyId)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetEndingScenesByStoryId] Error retrieving EndingScenes for storyId {storyId}", storyId);
            return Enumerable.Empty<EndingScene>(); // not returning null to avoid null reference exceptions
        }
    }

    public async Task<EndingScene?> GetEndingSceneById(int endingSceneId)
    {
        if (endingSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetEndingSceneById] Invalid EndingSceneId provided: {endingSceneId}", endingSceneId);
            return null;
        }

        try
        {
            var ending = await _db.EndingScenes.FindAsync(endingSceneId);
            if (ending == null)
            {
                _logger.LogWarning("[SceneRepository -> GetEndingSceneById] No EndingScene found with id {endingSceneIdid}", endingSceneId);
            }

            return ending;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetEndingSceneById] Error retrieving EndingScene with id {endingSceneId}", endingSceneId);
            return null;
        }
    }

    public async Task<EndingScene?> GetGoodEndingSceneByStoryId(int storyId)
    {
        return await GetEndingSceneByTypeAsync(storyId, EndingType.Good, "Good");
    }

    public async Task<EndingScene?> GetNeutralEndingSceneByStoryId(int storyId)
    {
        return await GetEndingSceneByTypeAsync(storyId, EndingType.Neutral, "Neutral");
    }

    public async Task<EndingScene?> GetBadEndingSceneByStoryId(int storyId)
    {
        return await GetEndingSceneByTypeAsync(storyId, EndingType.Bad, "Bad");
    }

    // Added this private method to reduce redundancy in the three methods above
    private async Task<EndingScene?> GetEndingSceneByTypeAsync(int storyId, EndingType type, string typeName)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetEndingSceneByTypeAsync] Invalid storyId provided: {storyId}", storyId);
            return null;
        }

        try
        {
            var ending = await _db.EndingScenes
                .FirstOrDefaultAsync(e => e.StoryId == storyId && e.EndingType == type);

            if (ending == null)
            {
                _logger.LogInformation("[SceneRepository -> GetEndingSceneByTypeAsync] No {typeName} EndingScene found for storyId {storyId}", typeName, storyId);
            }

            return ending;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetEndingSceneByTypeAsync] Error retrieving {typeName} EndingScene for storyId {storyId}", typeName, storyId);
            return null;
        }
    }

    public async Task<bool> AddEndingScene(EndingScene endingScene)
    {
        if (endingScene == null)
        {
            _logger.LogWarning("[SceneRepository -> AddEndingScene] Cannot add a null EndingScene");
            return false;
        }

        try
        {
            _db.EndingScenes.Add(endingScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> AddEndingScene] Error adding EndingScene {endingScene}", endingScene);
            return false;
        }
    }

    public async Task<bool> UpdateEndingScene(EndingScene endingScene)
    {
        if (endingScene == null)
        {
            _logger.LogWarning("[SceneRepository -> UpdateEndingScene] Cannot update a null EndingScene");
            return false;
        }

        try
        {
            _db.EndingScenes.Update(endingScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> UpdateEndingScene] Error updating EndingScene {endingScene}", endingScene);
            return false;
        }
    }

    public async Task<bool> DeleteEndingScene(int endingSceneId)
    {
        if (endingSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> DeleteEndingScene] Invalid EndingSceneId provided: {endingSceneId}", endingSceneId);
            return false;
        }

        try
        {
            var ending = await _db.EndingScenes.FindAsync(endingSceneId);
            if (ending == null)
            {
                _logger.LogWarning("[SceneRepository -> DeleteEndingScene] No EndingScene found with id {endingSceneId}", endingSceneId);
                return false;
            }

            _db.EndingScenes.Remove(ending);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> DeleteEndingScene] Error deleting EndingScene with id {endingSceneId}", endingSceneId);
            return false;
        }
    }
}

