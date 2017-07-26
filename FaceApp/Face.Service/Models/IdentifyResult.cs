using System.Collections.Generic;

namespace Face.Service.Models
{
    public class IdentifyResult
    {
        public string FaceId { get; set; }

        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        public string PersonId { get; set; }

        public double Confidence { get; set; }
    }
}
