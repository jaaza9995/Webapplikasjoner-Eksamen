namespace Jam.ViewModels;

// Brukes til å vise detaljer om én bestemt historie (Story) i detaljvisningen
public class StoryDetailsViewModel
{
    public int StoryId { get; set; }                     // ID-en til historien
    public string Title { get; set; } = string.Empty;    // Tittel på historien
    public string Code { get; set; } = string.Empty;     // Kode som brukes for å starte spillet
    public string Description { get; set; } = string.Empty; // Kort beskrivelse av historien

    // Statistikkinformasjon som vises på detaljsiden
    public int SceneCount { get; set; }      // Hvor mange scener historien inneholder
    public int QuestionCount { get; set; }   // Hvor mange av scenene som er spørsmålsscener

}

// Brukes til å vise en enkel oversikt over hver scene inne i detaljvisningen
public class SceneSummaryViewModel
{
    public int SceneId { get; set; }                   // ID for scenen
    public string Title { get; set; } = string.Empty;  // Navn eller tittel på scenen
    public string SceneType { get; set; } = string.Empty; // Type scene (Intro, Question, Ending)
}
