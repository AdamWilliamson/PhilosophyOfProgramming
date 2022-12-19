using System.Collections.Generic;

namespace Validations_Tests.Demonstration.Moderate
{
    public struct Artist
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Song
    {
        public int TrackNumber { get; set; }
        public string TrackName { get; set; }
        public double Duration { get; set; }
        public string Description { get; set; }
    }

    public class Album
    {
        public List<Song> SongList { get; set; } = new()
        {
            new Song()
            {
                TrackNumber = 1,
                TrackName = "The First Song",
                Duration = 1.2,
            }
        };

        public Artist Artist { get; set; } = new()
        {
            FirstName = "John",
            LastName = "Williamson"
        };
    }
}
