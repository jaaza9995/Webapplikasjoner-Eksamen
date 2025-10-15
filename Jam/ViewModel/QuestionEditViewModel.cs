namespace Jam.ViewModels;

public class QuestionEditViewModel
{
    public int SceneId { get; set; }        // hvilken scene spørsmålet tilhører
    public string QuestionText { get; set; } = string.Empty;  // selve spørsmålet

    // Liste over svaralternativer
    public List<AnswerOptionEditVM> Answers { get; set; } = new();
}

public class AnswerOptionEditVM
{
    public int AnswerId { get; set; }
    public string Text { get; set; } = "";
    public int? NextSceneId { get; set; } // hopper videre til neste scene
}
