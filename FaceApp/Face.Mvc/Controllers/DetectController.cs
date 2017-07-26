using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Face.Service.FaceService;
using System.IO;
using System.Drawing;
using Face.Mvc.Models.Face;
using System.Linq;

namespace Face.Mvc.Controllers
{
    public class DetectController : Controller
    {
        #region variables

        private readonly IFaceService _faceService;

        #endregion

        #region constructors

        public DetectController(IFaceService faceService)
        {
            _faceService = faceService;
        }

        #endregion

        #region methods

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            //check file
            if (Request.Form.Files.Count == 0)
                return Json(new { success = false, error = "No file choosen" });

            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
                return Json(new { success = false, error = "file empty" });

            //convert stream to byte array
            byte[] arr = null;
            var imageSize = new ImageSize();
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                var image = Image.FromStream(ms);
                imageSize.Height = image.Height;
                imageSize.Width = image.Width;
                imageSize.ImageName = file.Name.Split('.')[0];
                imageSize.ImageFullName = file.Name;
                arr = ms.ToArray();
            }

            //call detect
            var detectResult = await _faceService.DetectFace(arr);

            var result = detectResult.data.Select(x => new
            {
                FaceId = x.FaceId,
                ImageName = imageSize.ImageName,
                ImageFullName = imageSize.ImageFullName,
                Height = imageSize.Height,
                Width = imageSize.Width,
                Position = x.FaceRectangle,
                FaceAttributes = x.FaceAttributes
            }).ToList();

            //success
            if (detectResult.success)
                return Json(new { success = true, data = result, image = imageSize });

            //error
            return Json(new { success = false, error = detectResult.errorMessage });
        }

        #endregion
    }
}