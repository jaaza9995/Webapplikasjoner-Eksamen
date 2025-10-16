namespace Jam.ViewModels
{
    // Viser alle tilgjengelige spill (public stories) med tittel, beskrivelse og kode
    public class PlayNewPublicGameViewModel
    {
        public IEnumerable<StoryListViewModel> Stories { get; set; } = new List<StoryListViewModel>(); // liste over spill som vises på siden
        public string? SearchQuery { get; set; } // brukes for søkefeltet (tittel-søk)
    }

    // Representerer ett spillkort i listen
    public class StoryListViewModel
    {
        public int Id { get; set; }                  // unikt ID-nummer for spillet
        public string Title { get; set; } =string.Empty;      // tittel på spillet
        public string Description { get; set; } = string.Empty; // kort beskrivelse
        public int QuestionCount { get; set; }       // hvor mange spørsmål historien har
        public string Code { get; set; } = string.Empty;       // spillkoden (vises nederst på kortet)
    }
}
