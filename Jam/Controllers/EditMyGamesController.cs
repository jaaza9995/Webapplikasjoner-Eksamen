using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Jam.DAL;
using Jam.Models;
using Jam.Models.Enums;
using Jam.ViewModels;

namespace Jam.Controllers
{
    public class EditGameController : Controller
    {
        private readonly StoryDbContext _db;
        public EditGameController(StoryDbContext db) => _db = db;

        // ---------- INTRO ----------
        [HttpGet]
        public async Task<IActionResult> EditIntro(int storyId)
        {
            var story = await _db.Stories.FindAsync(storyId);
            if (story == null) return NotFound();

            var vm = new EditStoryViewModel
            {
                IsEditMode = true,
                StoryId = story.StoryId,
                Title = story.Title,
                Description = story.Description,
                DifficultyLevel = story.DifficultyLevel,
                Accessibility = story.Accessible,
                // dropdowns
                DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                    .Cast<DifficultyLevel>()
                    .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString(), Selected = d == story.DifficultyLevel })
                    .ToList(),
                AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                    .Cast<Accessibility>()
                    .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString(), Selected = a == story.Accessible })
                    .ToList()
            };

            return View(viewName: "CreateIntro", model: vm); // gjenbruk view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIntro(EditStoryViewModel vm)
        {
            var story = await _db.Stories.FindAsync(vm.StoryId);
            if (story == null) return NotFound();

            if (!ModelState.IsValid)
            {
                // repopulate dropdowns
                vm.IsEditMode = true;
                vm.DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                    .Cast<DifficultyLevel>().Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString(), Selected = d == vm.DifficultyLevel }).ToList();
                vm.AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                    .Cast<Accessibility>().Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString(), Selected = a == vm.Accessibility }).ToList();
                return View(viewName: "CreateIntro", model: vm);
            }

            // Oppdater story-felt
            story.Title = vm.Title ?? "";
            story.Description = vm.Description ?? "";
            story.DifficultyLevel = vm.DifficultyLevel;

            // Håndter bytte mellom Public/Private + kode
            if (story.Accessible != vm.Accessibility)
            {
                story.Accessible = vm.Accessibility;
                if (vm.Accessibility == Accessibility.Private && string.IsNullOrEmpty(story.GameCode))
                    story.GameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
                if (vm.Accessibility == Accessibility.Public)
                    story.GameCode = null; // valgfritt: fjern kode når den blir public
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(EditMultipleQuestion), new { storyId = story.StoryId, questionIndex = 1 });
        }

        // ---------- QUESTIONS ----------
        [HttpGet]
        public async Task<IActionResult> EditMultipleQuestion(int storyId, int questionIndex = 1)
        {
            var questions = await _db.QuestionScenes
                .Where(q => q.StoryId == storyId)
                .Include(q => q.AnswerOptions)
                .OrderBy(q => q.QuestionSceneId)
                .ToListAsync();

            var vm = new EditStoryViewModel
            {
                IsEditMode = true,
                StoryId = storyId,
                Step = questionIndex,
                Questions = questions.Select(q => new CreateQuestionViewModel
                {
                    QuestionId = q.QuestionSceneId,
                    QuestionText = q.Question,
                    Answers = q.AnswerOptions.OrderBy(a => a.AnswerOptionId).Select(a => a.Answer).ToList(),
                    Feedbacks = q.AnswerOptions.OrderBy(a => a.AnswerOptionId).Select(a => a.FeedbackText ?? "").ToList(),
                    IsCorrect = q.AnswerOptions.OrderBy(a => a.AnswerOptionId).Select(a => a.IsCorrect).ToList()
                }).ToList()
            };

            // sørg for 4 svar pr spørsmål i UI
            foreach (var q in vm.Questions)
            {
                q.Answers = (q.Answers ?? new()).Concat(Enumerable.Repeat("", Math.Max(0, 4 - (q.Answers?.Count ?? 0)))).Take(4).ToList();
                q.Feedbacks = (q.Feedbacks ?? new()).Concat(Enumerable.Repeat("", Math.Max(0, 4 - (q.Feedbacks?.Count ?? 0)))).Take(4).ToList();
                q.IsCorrect = (q.IsCorrect ?? new()).Concat(Enumerable.Repeat(false, Math.Max(0, 4 - (q.IsCorrect?.Count ?? 0)))).Take(4).ToList();
            }

            return View(viewName: "CreateMultipleQuestion", model: vm); // gjenbruk view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMultipleQuestion(EditStoryViewModel model, string? submit)
        {
            if (model == null) return BadRequest("Ugyldig skjema.");

            // (samme validering som i Create)
            for (int i = 0; i < model.Questions.Count; i++)
            {
                var q = model.Questions[i];
                if (string.IsNullOrWhiteSpace(q.QuestionText))
                    ModelState.AddModelError($"Questions[{i}].QuestionText", "Spørsmål er påkrevd.");
                if (q.Answers == null || q.Answers.Count != 4)
                    ModelState.AddModelError($"Questions[{i}].Answers", "Du må ha nøyaktig 4 svaralternativer.");
                if (q.IsCorrect == null || q.IsCorrect.Count != 4)
                    ModelState.AddModelError($"Questions[{i}].IsCorrect", "Du må ha 4 verdier i IsCorrect-listen.");
                if (q.Feedbacks == null || q.Feedbacks.Count != 4)
                    ModelState.AddModelError($"Questions[{i}].Feedbacks", "Du må ha 4 feedback-tekster.");

                if (q.IsCorrect?.Count(c => c) != 1)
                    ModelState.AddModelError($"Questions[{i}].IsCorrect", "Nøyaktig ett alternativ må være riktig.");
                for (int j = 0; j < (q.Answers?.Count ?? 0); j++)
                {
                    if (string.IsNullOrWhiteSpace(q.Answers[j]))
                        ModelState.AddModelError($"Questions[{i}].Answers[{j}]", "Svartekst er påkrevd.");
                    if (string.IsNullOrWhiteSpace(q.Feedbacks[j]))
                        ModelState.AddModelError($"Questions[{i}].Feedbacks[{j}]", "Feedback-tekst er påkrevd.");
                }
            }
            if (!ModelState.IsValid) return View(viewName: "CreateMultipleQuestion", model: model);

            // Synk databasen: oppdater, legg til, fjern
            var existing = await _db.QuestionScenes
                .Where(x => x.StoryId == model.StoryId)
                .Include(x => x.AnswerOptions)
                .ToListAsync();

            // Fjern spørsmål som ikke finnes i modellen lenger
            var keepIds = model.Questions.Where(q => q.QuestionId > 0).Select(q => q.QuestionId).ToHashSet();
            var toRemove = existing.Where(x => !keepIds.Contains(x.QuestionSceneId));
            _db.QuestionScenes.RemoveRange(toRemove);

            // Oppdater/legg til
            foreach (var qvm in model.Questions)
            {
                QuestionScene entity;
                if (qvm.QuestionId > 0)
                {
                    entity = existing.First(x => x.QuestionSceneId == qvm.QuestionId);
                    entity.Question = qvm.QuestionText;
                    entity.SceneText = qvm.QuestionText;

                    // Oppdater 4 svar i rekkefølge (for enkelhet)
                    var opts = entity.AnswerOptions.OrderBy(a => a.AnswerOptionId).ToList();
                    for (int i = 0; i < 4; i++)
                    {
                        if (i < opts.Count)
                        {
                            opts[i].Answer = qvm.Answers[i];
                            opts[i].FeedbackText = qvm.Feedbacks[i];
                            opts[i].IsCorrect = qvm.IsCorrect[i];
                        }
                        else
                        {
                            entity.AnswerOptions.Add(new AnswerOption
                            {
                                Answer = qvm.Answers[i],
                                FeedbackText = qvm.Feedbacks[i],
                                IsCorrect = qvm.IsCorrect[i]
                            });
                        }
                    }
                    // dersom det lå flere enn 4 fra før, kutt dem:
                    if (entity.AnswerOptions.Count > 4)
                    {
                        var extra = entity.AnswerOptions.OrderBy(a => a.AnswerOptionId).Skip(4).ToList();
                        _db.RemoveRange(extra);
                    }
                }
                else
                {
                    entity = new QuestionScene
                    {
                        StoryId = model.StoryId,
                        Question = qvm.QuestionText,
                        SceneText = qvm.QuestionText,
                        AnswerOptions = Enumerable.Range(0, 4).Select(i => new AnswerOption
                        {
                            Answer = qvm.Answers[i],
                            FeedbackText = qvm.Feedbacks[i],
                            IsCorrect = qvm.IsCorrect[i]
                        }).ToList()
                    };
                    _db.QuestionScenes.Add(entity);
                }
            }

            await _db.SaveChangesAsync();

            // Navigasjon
            model.Step++;
            if (string.Equals(submit, "next", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction(nameof(EditMultipleQuestion), new { storyId = model.StoryId, questionIndex = model.Step });

            return RedirectToAction(nameof(EditEndings), new { storyId = model.StoryId });
        }

        // ---------- ENDINGS ----------
        [HttpGet]
        public async Task<IActionResult> EditEndings(int storyId)
        {
            var endings = await _db.EndingScenes.Where(e => e.StoryId == storyId).ToListAsync();

            string get(EndingType t) => endings.FirstOrDefault(e => e.EndingType == t)?.EndingText ?? "";

            var vm = new EditStoryViewModel
            {
                IsEditMode = true,
                StoryId = storyId,
                HighEnding = get(EndingType.Good),
                MediumEnding = get(EndingType.Neutral),
                LowEnding = get(EndingType.Bad)
            };
            return View(viewName: "CreateEndings", model: vm); // gjenbruk view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEndings(EditStoryViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.HighEnding))
                ModelState.AddModelError(nameof(vm.HighEnding), "Good ending er påkrevd.");
            if (string.IsNullOrWhiteSpace(vm.MediumEnding))
                ModelState.AddModelError(nameof(vm.MediumEnding), "Neutral ending er påkrevd.");
            if (string.IsNullOrWhiteSpace(vm.LowEnding))
                ModelState.AddModelError(nameof(vm.LowEnding), "Bad ending er påkrevd.");

            if (!ModelState.IsValid)
            {
                vm.IsEditMode = true;
                return View(viewName: "CreateEndings", model: vm);
            }

            // Oppdater/lag endings
            await UpsertEnding(vm.StoryId, EndingType.Good, vm.HighEnding);
            await UpsertEnding(vm.StoryId, EndingType.Neutral, vm.MediumEnding);
            await UpsertEnding(vm.StoryId, EndingType.Bad, vm.LowEnding);

            return RedirectToAction("Index", "Home");
        }

        private async Task UpsertEnding(int storyId, EndingType type, string text)
        {
            var e = await _db.EndingScenes.FirstOrDefaultAsync(x => x.StoryId == storyId && x.EndingType == type);
            if (e == null)
            {
                _db.EndingScenes.Add(new EndingScene { StoryId = storyId, EndingType = type, EndingText = text });
            }
            else
            {
                e.EndingText = text;
            }
            await _db.SaveChangesAsync();
        }
    }
}
