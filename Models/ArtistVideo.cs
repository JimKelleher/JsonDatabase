using System.Collections.Generic;

namespace Database.Models
{
    public class Video
    {
        public string videoId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public int seconds { get; set; }
    }

    public class ArtistVideo
    {
        public string id { get; set; }
        public List<Video> video { get; set; }
    }

}