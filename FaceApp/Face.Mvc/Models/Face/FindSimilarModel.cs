using Face.Service.Models;
namespace Face.Mvc.Models.Face
{
    public class FindSimilarViewModel : ImageSize
    {
        public string FaceId { get; set; }
        public double Confidence { get; set; }
    }

    public class ImageSize
    {
        public string ImageName { get; set; }
        public string ImageFullName { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public FaceRectangle Position { get; set; }
    }
}
