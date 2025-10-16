using Jam.Models.Enums; 

namespace Jam.ViewModels
{
    // Viser alle tilgjengelige spill (public stories) med tittel, beskrivelse og kode
    public class PlayNewPublicGameViewModel //blir brukt i BrowseController
    {
        public IEnumerable<StoryListViewModel> Stories { get; set; } = new List<StoryListViewModel>(); // liste over spill som vises på siden
        public string? SearchQuery { get; set; } // brukes for søkefeltet (tittel-søk)
    }

    // Representerer ett spillkort i listen
    public class StoryListViewModel //blir brukt i BrowseController
    {
        public int Id { get; set; }                  // unikt ID-nummer for spillet
        public string Title { get; set; } = string.Empty;      // tittel på spillet

        public string Introduction { get; set; } = string.Empty; // kort beskrivelse
        public int QuestionCount { get; set; }       // hvor mange spørsmål historien har
        public DifficultyLevel Difficulty { get; set; }   // NY
    }
}
