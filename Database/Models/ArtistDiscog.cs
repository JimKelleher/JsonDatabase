using System.Collections.Generic;

namespace Database.Models
{
    public class Album
    {
        public string albumId { get; set; }
        public int year { get; set; }
    }

    public class Section
    {
        public string sectionId { get; set; }
        public int style { get; set; }
        public List<Album> album { get; set; }
    }

    public class ArtistDiscog
    {
        public string id { get; set; }
        public string wikipedia { get; set; }
        public List<Section> section { get; set; }
    }

}