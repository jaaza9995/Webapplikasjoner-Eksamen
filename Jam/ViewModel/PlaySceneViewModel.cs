using Jam.Models.Enums; 

namespace Jam.ViewModels 
{
    public class PlaySceneViewModel
    {
        public int SessionId { get; set; } // ID-en til spilløkten (brukes for å vite hvilken spillerunde det gjelder)
        public int SceneId { get; set; } // ID for scenen som vises akkurat nå
        public string Question { get; set; } = string.Empty; // Spørsmålet som stilles 
        public string SceneText { get; set; } = string.Empty; // Teksten som vises i scenen (dialog, spørsmål osv.)
        public string SceneType { get; set; } = string.Empty; // Typen scene (f.eks. "Intro", "Question", "Ending")
        public IEnumerable<AnswerOptionViewModel> Answers { get; set; } = Enumerable.Empty<AnswerOptionViewModel>(); // Liste med svaralternativer (brukes hvis scenen er et spørsmål)
        public int? NextSceneId { get; set; } // Neste scene som skal vises (bare brukt hvis det ikke er et spørsmål)

        //public string Feedback { get; set; } = string.Empty; // Tilbakemelding etter å ha svart på et spørsmål (f.eks. "Riktig!" eller "Feil svar.")
    }

    // Viser statusen i spillet (score, liv, tittel, vanskelighetsgrad)
    public class SessionStatusViewModel
    {
        public Guid SessionId { get; set; } // Unik ID for spilløkten (brukes til å koble spillerens fremgang)
        public string StoryTitle { get; set; } = default!; // Tittelen på historien som spilles
        public int Score { get; set; } // Spillerens nåværende poengsum
        public int Hearts { get; set; } // Antall liv/hjerter spilleren har igjen
        public DifficultyLevel Level { get; set; } // Vanskelighetsgrad (enum-verdi: Easy, Medium, Hard)
    }

    // Representerer ett svaralternativ i en spørsmålsscene
    public class AnswerOptionViewModel
    {
        public int AnswerId { get; set; } // ID for svaret (brukes til å vite hvilket alternativ som ble valgt)
        public string Text { get; set; } = string.Empty; // Teksten som vises på svarknappen (f.eks. "Gå til høyre")
    }
}
