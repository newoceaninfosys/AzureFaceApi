using System.Collections.Generic;

namespace Face.Mvc.Models.Face
{
    public class FaceVerifyViewModel
    {
        public bool IsIdentical { get; set; }
        public double Confidence { get; set; }
    }

    public class FaceVerifyPersonViewModel: FaceVerifyViewModel
    {
        public string PersonGroupId { get; set; }
        public string PersonId { get; set; }
        public string Name { get; set; }
        public string UserData { get; set; }
        public List<string> FaceUrls { get; set; }
    }
}
