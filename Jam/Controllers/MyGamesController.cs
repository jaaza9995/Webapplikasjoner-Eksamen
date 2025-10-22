/* forslag for deler av controller for både create og edit av story

using Microsoft.AspNetCore.Mvc;
using Jam.DAL.UserDAL;
using Jam.DAL.StoryDAL;
using Jam.Models.Enums;



[HttpPost]
public IActionResult CreateOrEdit(CreateStoryViewModel model, string action)
{
    if (action == "Next")
    {
        model.Step++;
    }
    else if (action == "Back")
    {
        model.Step--;
    }
    else if (action == "Finish")
    {
        if (model.IsEditMode)
        {
            // 🔹 Oppdater eksisterende story
            var existingStory = _context.Stories.FirstOrDefault(s => s.Id == model.StoryId);
            if (existingStory == null)
                return NotFound();

            existingStory.Title = model.Title;
            existingStory.Description = model.Description;
            existingStory.Intro = model.Intro;
            existingStory.DifficultyLevel = model.DifficultyLevel;
            existingStory.Accessibility = model.Accessibility;
            // Oppdater andre felter her …

            _context.Update(existingStory);
            _context.SaveChanges();

            return RedirectToAction("Details", "Story", new { id = existingStory.Id });
        }
        else
        {
            // 🔹 Opprett ny story
            var newStory = new Story
            {
                Title = model.Title,
                Description = model.Description,
                Intro = model.Intro,
                DifficultyLevel = model.DifficultyLevel,
                Accessibility = model.Accessibility
                // legg til spørsmål, endings osv.
            };

            _context.Add(newStory);
            _context.SaveChanges();

            return RedirectToAction("Details", "Story", new { id = newStory.Id });
        }
    }

    // Hvis du går frem/tilbake, last samme view igjen med oppdatert steg
    return View("Create", model);
}
