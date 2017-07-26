using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Face.Service.FaceService;
using Face.Mvc.Models.Group;
using Face.Mvc.Models.Face;
using System.IO;

namespace Face.Mvc.Controllers
{
    public class IdentifyController : Controller
    {
        #region variables

        private readonly IFaceService _faceService;

        #endregion

        #region constructors

        public IdentifyController(IFaceService faceService)
        {
            _faceService = faceService;
        }

        #endregion

        #region utilities

        private async Task<List<GroupViewModel>> GetAllGroup()
        {
            var allGroups = new List<GroupViewModel>();
            var groupData = await _faceService.ListGroup();
            if (groupData.success && groupData.data.Any())
            {
                allGroups.AddRange(groupData.data.Select(x => new GroupViewModel()
                {
                    Id = x.personGroupId,
                    Name = x.name,
                    UserData = x.userData
                }));
            }
            return allGroups;
        }

        #endregion

        #region methods

        public async Task<IActionResult> Index()
        {
            //groups
            var allGroups = await GetAllGroup();

            //json
            var model = new IdentifyModel()
            {
                Groups = allGroups,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Identify(string personGroupId)
        {
            //check file
            if (Request.Form.Files.Count == 0)
                return Json(new { success = false, error = "No file choosen" });

            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
                return Json(new { success = false, error = "file empty" });

            //convert stream to byte array
            byte[] arr = null;
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                arr = ms.ToArray();
            }

            //call detect
            var detectResult = await _faceService.DetectFace(arr);

            //success
            if (detectResult.success)
            {
                var faceIds = detectResult.data.Select(x => x.FaceId);
                if (!faceIds.Any())
                    return Json(new { success = false, error = "Cannot detect face" });

                //call identify
                var identifyResult = await _faceService.Identify(personGroupId, detectResult.data.Select(x => x.FaceId).ToArray());
                if (identifyResult.success)
                {
                    //get list persons id
                    var personIds = new List<string>();
                    foreach (var item in identifyResult.data)
                        personIds.AddRange(item.Candidates.Select(x => x.PersonId));
                    personIds.Distinct();

                    //get persons data from group
                    var persons = new List<(string personId, string name, string userData, List<string> persistedFaceIds)>();
                    foreach (var item in personIds)
                    {
                        var personResult = await _faceService.GetPerson(personGroupId, item);
                        if (personResult.success)
                            persons.Add(personResult.data);
                    }

                    var result = new List<IdentifyResultModel>();
                    foreach (var item in identifyResult.data)
                    {
                        var itemResult = new IdentifyResultModel {
                            FaceId = item.FaceId,
                            Persons = new List<PersonIdentifyModel>() };
                        foreach (var candidate in item.Candidates)
                        {
                            var person = persons.FirstOrDefault(x => x.personId == candidate.PersonId);
                            itemResult.Persons.Add(new PersonIdentifyModel
                            {
                                Name = person.name,
                                //Url = person.persistedFaceIds.FirstOrDefault(),
                                Url = $"/Facing/{person.persistedFaceIds.FirstOrDefault()}.jpg",
                                Confidence = candidate.Confidence
                            });
                        }
                        result.Add(itemResult);
                    }

                    return Json(new { success = true, data = result });
                }

                return Json(new { success = false, error = identifyResult.errorMessage });
            }

            //error
            return Json(new { success = false, error = detectResult.errorMessage });
        }

        #endregion

    }
}