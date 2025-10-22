using System.Collections.Generic;
using Jam.Models.Enums;

namespace Jam.ViewModels
{
    public class BrowseViewModel
    {
        // søkefelter
        public string? SearchString { get; set; }   // for public games
        public string? SearchPrivate { get; set; }  // for private game code

        // results
        public List<GameCardViewModel> PublicGames { get; set; } = new();
        public GameCardViewModel? PrivateGame { get; set; } 
    }
}
