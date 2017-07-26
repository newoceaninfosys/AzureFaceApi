using System.Collections.Generic;

namespace Face.Mvc.Models.Face
{
    public class FaceGroupModel
    {
        public string ImageName { get; set; }
        public List<string> FaceIds { get; set; }
    }

    public class FaceGroupViewModel
    {
        public FaceGroupViewModel()
        {
            Groups = new List<SimilarGroup>();
        }
        public List<SimilarGroup> Groups { get; set; }
        public SimilarGroup MessyGroup { get; set; }
    }

    public class SimilarGroup
    {
        public List<string> Images { get; set; }
        public List<string> FaceIds { get; set; }
    }
}
