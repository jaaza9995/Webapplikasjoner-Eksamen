using Microsoft.AspNetCore.Mvc;
using Jam.DAL.StoryDAL;
using Jam.DAL.SceneDAL;
using Jam.Models;
using Jam.Models.Enums;
using Jam.ViewModels;

namespace Jam.Controllers
{
    // Forfatter-flyt: opprett Story + Intro, så videre til spørsmål
    public class CreateGameController : Controller
    {
        private readonly IStoryRepository _stories;
        private readonly ISceneRepository _scenes;

        public CreateGameController(IStoryRepository stories, ISceneRepository scenes)
        {
            _stories = stories;
            _scenes = scenes;
        }

        // GET: /StoryCreation/CreateStoryAndIntro
        [HttpGet]
        public IActionResult CreateStoryAndIntro()
        {
            return View(new CreateStoryAndIntroViewModel());
        }

        // POST: /StoryCreation/CreateStoryAndIntro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStoryAndIntro(CreateStoryAndIntroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Midlertidig "innlogget" bruker (Barry i DBInit)
            int userId = 1;

            // 1) Lagre selve historien
            var story = new Story
            {
                Title = model.Title,
                Description = model.Description,
                DifficultyLevel = model.DifficultyLevel,
                Accessible = Accessibility.Public, // kan endres senere i editing
                UserId = userId
            };
            await _stories.CreateStory(story); // setter StoryId

            // 2) Lagre intro-scene
            var intro = new Scene
            {
                SceneType = SceneType.Introduction,
                SceneText = model.IntroText,
                StoryId = story.StoryId
            };
            await _scenes.CreateScene(intro, previousSceneId: null); // setter SceneId

            // 3) Neste steg: gå til "CreateQuestionScene" (du har viewet)
            //    Sender med storyId + previousSceneId (intro) så vi kan lenke videre
            return RedirectToAction("CreateQuestionScene", "CreateGame",
                new { storyId = story.StoryId, previousSceneId = intro.SceneId });
            }

            // (Valgfritt) Stub for neste steg – så routing ikke feiler hvis du ikke har action enda
            [HttpGet]
            public IActionResult CreateQuestionScene(int storyId, int previousSceneId)
            {
                // Returner ditt eksisterende view (du har CreateQuestionScene.cshtml)
                // Her kan du senere fylle en VM med storyId/previousSceneId
                ViewBag.StoryId = storyId;
                ViewBag.Previous = previousSceneId;
                return View();
            }
        }
    } 

