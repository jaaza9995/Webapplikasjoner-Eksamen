namespace Jam.ViewModels
{
    public class PlaySceneViewModel
    {
        public int SessionId { get; set; }
        public int SceneId { get; set; }
        public bool IsQuestion { get; set; }
        public string SceneText { get; set; } = string.Empty;
        public string SceneType { get; set; } = string.Empty;
        public IEnumerable<AnswerOptionViewModel> Answers { get; set; } = Enumerable.Empty<AnswerOptionViewModel>();
        public int? NextSceneId { get; set; } // brukes når scenen ikke er et spørsmål
    }

    public class AnswerOptionViewModel
    {
        public int AnswerId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
