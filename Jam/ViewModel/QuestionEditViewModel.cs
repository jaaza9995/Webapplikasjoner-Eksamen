namespace Jam.ViewModels;

public class QuestionEditViewModel
    {
        public int SceneId { get; set; }        // hvilken scene spørsmålet tilhører
        public string QuestionText { get; set; } = string.Empty;  // selve spørsmålet

        // Liste over svaralternativer
        public List<AnswerOptionEditViewModel> Answers { get; set; } = new();
    }

    public class AnswerOptionEditViewModel
    {
        public int AnswerId { get; set; } // ID for svaret (brukes til å vite hvilket alternativ som ble valgt)
        public string Text { get; set; } = string.Empty; // teksten som vises på svarknappen
        public int? NextSceneId { get; set; } // hopper videre til neste scene
    }


