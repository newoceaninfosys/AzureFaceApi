using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Face.Mvc.Models.Face
{
    public class FaceViewModel : FaceImage
    {
        /// <summary>
        /// person group id
        /// </summary>
        [Required]
        public string PersonGroupId { get; set; }

        /// <summary>
        /// person id
        /// </summary>
        [Required]
        public string PersonId { get; set; }

        /// <summary>
        /// left, top, width, height of face if image has more than one face
        /// </summary>
        public string TargetFace { get; set; }
    }

    public class FaceListViewModel
    {
        public string PersonGroupId { get; set; }
        public string PersonId { get; set; }
        public string Name { get; set; }
        public string UserData { get; set; }

        public List<FaceImage> FaceUrls { get; set; } 
    }

    public class FaceImage
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string UserData { get; set; }
    }
}
