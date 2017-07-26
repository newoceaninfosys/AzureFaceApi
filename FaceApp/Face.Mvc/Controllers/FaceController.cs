using System.Linq;
using System.Threading.Tasks;
using Face.Service.FaceService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Face.Mvc.Models.Face;
using System.Collections.Generic;
using Face.Mvc.Helpers;
using Microsoft.AspNetCore.Http;
using System.Drawing;

namespace Face.Mvc.Controllers
{
    public class FaceController : Controller
    {
        #region variables

        private readonly IFaceService _faceService;
        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region constructors

        public FaceController(IFaceService faceService, IHostingEnvironment hostingEnvironment)
        {
            _faceService = faceService;
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region utilities

        private async Task<(List<FaceGroupModel> data, string errorMessage)> DetectMultipleImages(IFormFileCollection images)
        {
            var groups = new List<FaceGroupModel>();

            foreach (var image in images)
            {
                byte[] imageByteArray;
                using (var ms = new MemoryStream())
                {
                    image.CopyTo(ms);
                    imageByteArray = ms.ToArray();
                }
                var detectResult = await _faceService.DetectFace(imageByteArray);
                if (!detectResult.success)
                    return (null, detectResult.errorMessage + " at image [" + image.Name + "].");

                groups.Add(new FaceGroupModel
                {
                    ImageName = image.Name.Split('.').First(),
                    FaceIds = detectResult.data.Select(face => face.FaceId).ToList()
                });
            }

            return (groups, null);
        }

        private async Task<List<ImageSize>> GetImagesSize(IFormFileCollection images)
        {
            var imagesSize = new List<ImageSize>();
            foreach (var file in images)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var image = Image.FromStream(ms);
                    imagesSize.Add(new ImageSize
                    {
                        Height = image.Height,
                        Width = image.Width,
                        ImageFullName = file.Name
                    });
                }
            }
            return imagesSize;
        }

        private async Task<(List<DetectResultModel> data, string errorMessage)> DetectMultipleImagesV2(IFormFileCollection images)
        {
            var groups = new List<DetectResultModel>();

            foreach (var image in images)
            {
                byte[] imageByteArray;
                using (var ms = new MemoryStream())
                {
                    image.CopyTo(ms);
                    imageByteArray = ms.ToArray();
                }
                var detectResult = await _faceService.DetectFace(imageByteArray);
                if (!detectResult.success)
                    return (null, detectResult.errorMessage + " at image [" + image.Name + "].");

                groups.Add(new DetectResultModel
                {
                    ImageName = image.Name.Split('.').First(),
                    ImageFullName = image.Name,
                    Faces = detectResult.data.Select(face => new FacePostion
                    {
                        FaceId = face.FaceId,
                        Position = face.FaceRectangle
                    }).ToList()
                });
            }

            return (groups, null);
        }

        #endregion

        #region methods

        #region person faces

        public async Task<IActionResult> List(string groupId, string personId)
        {
            var person = await _faceService.GetPerson(groupId, personId);

            var dir = Path.Combine(_hostingEnvironment.WebRootPath, "Facing");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var model = new FaceListViewModel()
            {
                PersonGroupId = groupId,
                PersonId = personId,
                Name = person.data.name,
                UserData = person.data.userData,
                FaceUrls = person.data.persistedFaceIds.Select(faceId => new FaceImage
                {
                    Id = faceId,
                    Url = $"/Facing/{faceId}.jpg",
                    UserData = _faceService.GetPersistedFace(groupId, personId, faceId).GetAwaiter().GetResult().data
                }).ToList()
            };

            //return
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add()
        {
            var image = Request.Form.Files.FirstOrDefault();
            if (image == null || image.Length == 0)
                return Json(new { success = false, error = "Please upload face image" });

            byte[] imageByteArray;
            using (var ms = new MemoryStream())
            {
                image.CopyTo(ms);
                imageByteArray = ms.ToArray();
            }


            var model = new FaceViewModel
            {
                PersonGroupId = Request.Form["personGroupId"].ToString(),
                PersonId = Request.Form["personId"].ToString(),
                TargetFace = Request.Form["targetFace"].ToString(),
                UserData = Request.Form["userData"].ToString()
            };

            var addResult = await _faceService.AddPersonFace(model.PersonGroupId, model.PersonId, imageByteArray, model.UserData, model.TargetFace);
            if (!addResult.success)
                return Json(new { success = false, error = addResult.data });

            // store image
            var dir = Path.Combine(_hostingEnvironment.WebRootPath, "Facing");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filename = Path.Combine(dir, $"{addResult.data}.jpg");
            using (FileStream fs = System.IO.File.Create(filename))
            {
                image.CopyTo(fs);
                fs.Flush();
            }

            // update model
            model.Id = addResult.data;
            model.Url = $"/Facing/{addResult.data}.jpg";

            return Json(new { success = true, Data = model });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FaceViewModel model)
        {
            var updateResult = await _faceService.UpdatePersonFace(model.PersonGroupId, model.PersonId, model.Id, model.UserData);
            if (!updateResult.success)
                return Json(new { success = false, error = updateResult.data });

            return Json(new { success = true, Data = model });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(FaceViewModel model)
        {
            var deleteResult = await _faceService.DeletePersonFace(model.PersonGroupId, model.PersonId, model.Id);
            if (!deleteResult.success)
                return Json(new { success = false, error = deleteResult.data });

            // remove face
            var dir = Path.Combine(_hostingEnvironment.WebRootPath, "Facing");
            var fileName = Path.Combine(dir, $"{model.Id}.jpg");
            if (System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);

            return Json(new { success = true });
        }

        #endregion

        #region group

        public IActionResult GroupView()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Group()
        {
            var images = Request.Form.Files;
            var imagesSize = await GetImagesSize(images);

            var detectResult = await DetectMultipleImagesV2(images);
            if (detectResult.errorMessage != null)
                return Json(new { success = false, error = detectResult.errorMessage });

            var groups = detectResult.data;

            var faceIds = groups.Select(x => x.Faces.Select(y => y.FaceId)).SelectMany(x => x).ToList();
            if (faceIds.Count < 2)
                return Json(new { success = false, error = " Must have at least 2 detected faces in all of images" });
            var groupResult = await _faceService.GroupFaces(faceIds);
            if (!groupResult.success)
                return Json(new { success = false, error = groupResult.errorMessage });

            var result = new List<List<FindSimilarViewModel>>();
            foreach(var group in groupResult.data.Groups)
            {
                result.Add(group.Select(faceId =>
                {
                    var face = detectResult.data.First(y => y.Faces.FirstOrDefault(z => z.FaceId == faceId) != null);
                    var image = imagesSize.First(i => i.ImageFullName == face.ImageFullName);
                    return new FindSimilarViewModel
                    {
                        FaceId = faceId,
                        ImageName = face.ImageName,
                        ImageFullName = face.ImageFullName,
                        Height = image.Height,
                        Width = image.Width,
                        Position = face.Faces.First(p => p.FaceId == faceId).Position
                    };
                }).ToList());

            }
            result.Add(groupResult.data.MessyGroup.Select(faceId =>
            {
                var face = detectResult.data.First(y => y.Faces.FirstOrDefault(z => z.FaceId == faceId) != null);
                var image = imagesSize.First(i => i.ImageFullName == face.ImageFullName);
                return new FindSimilarViewModel
                {
                    FaceId = faceId,
                    ImageName = face.ImageName,
                    ImageFullName = face.ImageFullName,
                    Height = image.Height,
                    Width = image.Width,
                    Position = face.Faces.First(p => p.FaceId == faceId).Position
                };
            }).ToList());

            return Json(new { success = true, Data = result });
        }
        #endregion

        #region verify

        public IActionResult VerifyView()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyFaceToFace()
        {
            var images = Request.Form.Files;
            if (images.Count != 2)
                return Json(new { success = false, error = "Must upload exactly 2 images" });

            var faceIds = new List<string>();

            foreach (var image in images)
            {
                byte[] imageByteArray;
                using (var ms = new MemoryStream())
                {
                    image.CopyTo(ms);
                    imageByteArray = ms.ToArray();
                }
                var detectResult = await _faceService.DetectFace(imageByteArray);
                if (!detectResult.success)
                    return Json(new { success = false, error = detectResult.errorMessage + " at image [" + image.Name + "]." });
                if (detectResult.data.Count != 1)
                    return Json(new { success = false, error = "Each image must contains only one face" });
                faceIds.Add(detectResult.data.First().FaceId);
            }

            var verifyResult = await _faceService.VerifyFaceToFace(faceIds);
            if (!verifyResult.success)
                return Json(new { success = false, error = verifyResult.errorMessage });
            var result = new FaceVerifyViewModel
            {
                IsIdentical = verifyResult.result.IsIdentical,
                Confidence = verifyResult.result.Confidence
            };

            return Json(new { success = true, Data = result });
        }

        public async Task<IActionResult> VerifyPersonView(string groupId, string personId)
        {
            var person = await _faceService.GetPerson(groupId, personId);

            var model = new FaceVerifyPersonViewModel()
            {
                PersonGroupId = groupId,
                PersonId = personId,
                Name = person.data.name,
                UserData = person.data.userData,
                FaceUrls = person.data.persistedFaceIds.Select(x => UrlHelper.GetFaceUrl(x)).ToList()
            };

            //return
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyFaceToPerson()
        {
            var images = Request.Form.Files;
            if (images.Count != 1)
                return Json(new { success = false, error = "Must upload exactly 1 image" });

            byte[] imageByteArray;
            using (var ms = new MemoryStream())
            {
                images[0].CopyTo(ms);
                imageByteArray = ms.ToArray();
            }
            var detectResult = await _faceService.DetectFace(imageByteArray);
            if (!detectResult.success)
                return Json(new { success = false, error = detectResult.errorMessage });
            if (detectResult.data.Count != 1)
                return Json(new { success = false, error = "Image must contains only one face" });

            var personGroupId = Request.Form["personGroupId"].ToString();
            var personId = Request.Form["personId"].ToString();

            var verifyResult = await _faceService.VerifyFaceToPerson(personGroupId, personId, detectResult.data.First().FaceId);
            if (!verifyResult.success)
                return Json(new { success = false, error = verifyResult.errorMessage });
            var result = new FaceVerifyViewModel
            {
                IsIdentical = verifyResult.result.IsIdentical,
                Confidence = verifyResult.result.Confidence
            };

            return Json(new { success = true, Data = result });
        }

        #endregion

        #region find similar
        public IActionResult FindSimilar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FindSimilar(string mode, string faceId)
        {
            try
            {
                var images = Request.Form.Files;
                var imagesSize = await GetImagesSize(images);

                var detectResult = await DetectMultipleImagesV2(images);
                if (detectResult.errorMessage != null)
                    return Json(new { success = false, error = detectResult.errorMessage });

                var faceIds = detectResult.data.Select(x => x.Faces.Select(y => y.FaceId)).SelectMany(x => x).ToList();
                var findResult = await _faceService.FindSimilar(mode, faceId, faceIds);
                if (!findResult.success)
                    return Json(new { success = false, error = findResult.errorMessage });
                return Json(new
                {
                    success = true,
                    Data = findResult.data.Select(x =>
                    {
                        var face = detectResult.data.First(y => y.Faces.FirstOrDefault(z => z.FaceId == x.FaceId) != null);
                        var imageSize = imagesSize.First(image => image.ImageFullName == face.ImageFullName);
                        return new FindSimilarViewModel
                        {
                            FaceId = x.FaceId,
                            Confidence = x.Confidence,
                            ImageName = face.ImageName,
                            ImageFullName = face.ImageFullName,
                            Height = imageSize.Height,
                            Width = imageSize.Width,
                            Position = face.Faces.First(p => p.FaceId == x.FaceId).Position
                        };
                    }).ToList()
                });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #endregion
    }
}