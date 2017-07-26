using Face.Mvc.Models.Group;
using System.Collections.Generic;

namespace Face.Mvc.Models.Face
{
    public class IdentifyModel
    {
        public List<GroupViewModel> Groups { get; set; }
    }

    public class IdentifyResultModel
    {
        public string FaceId { get; set; }
        public List<PersonIdentifyModel> Persons { get; set; }
    }

    public class PersonIdentifyModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public double Confidence { get; set; }
    }
}
