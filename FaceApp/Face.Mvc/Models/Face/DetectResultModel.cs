using Face.Service.Models;
using System.Collections.Generic;

namespace Face.Mvc.Models.Face
{
    public class DetectResultModel
    {
        public string ImageFullName { get; set; }
        public string ImageName { get; set; }
        public List<FacePostion> Faces { get; set; }
    }
    public class FacePostion
    {
        public string FaceId { get; set; }
        public FaceRectangle Position { get; set; }
    }
}
