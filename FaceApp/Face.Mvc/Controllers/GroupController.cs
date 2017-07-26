using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Face.Mvc.Models.Group;
using Face.Service.FaceService;

namespace Face.Mvc.Controllers
{
    public class GroupController : Controller
    {
        #region variables

        private readonly IFaceService _faceService;

        #endregion

        #region constructors

        public GroupController(IFaceService faceService)
        {
            _faceService = faceService;
        }

        #endregion
        public async Task<IActionResult> Index()
        {
            var model = new GroupListViewModel
            {
                Groups = new List<Group>()
            };

            //get list groups from service
            var groupsData = await _faceService.ListGroup();
            if (groupsData.success && groupsData.data.Any())
            {
                foreach (var group in groupsData.data)
                {
                    //get group training status from service
                    var status = await _faceService.TrainingStatus(group.personGroupId);
                    model.Groups.Add(new Group
                    {
                        Id = group.personGroupId,
                        Name = group.name,
                        UserData = group.userData,
                        Status = status.success ? status.data.status : "Not trained",
                        StatusMessage = status.success ? status.data.message : string.Empty
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TrainGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return Json(new { success = false, error = "please sellect a group to train" });

            var train = await _faceService.TrainGroup(groupId);
            if(train.success)
            {
                var status = await _faceService.TrainingStatus(groupId);
                if (status.success)
                    return Json(new { success = true, data = status.data });

                return Json(new { success = false, error = status.data });
            }
            
            return Json(new { success = false, error = train.data });
        }

        [HttpPost]
        public async Task<IActionResult> Add(GroupViewModel model)
        {
            var id = Guid.NewGuid().ToString();
            var addResult = await _faceService.CreateGroup(id, model.Name, model.UserData);
            if (addResult.success)
            {
                model.Id = id;
                return Json(new { success = true, Data = model });
            }

            return Json(new { success = false, error = addResult.data });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GroupViewModel model)
        {
            var addResult = await _faceService.UpdateGroup(model.Id, model.Name, model.UserData);
            if (addResult.success)
                return Json(new { success = true, Data = model });

            return Json(new { success = false, error = addResult.data });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(GroupViewModel model)
        {
            var deleteResult = await _faceService.DeleteGroup(model.Id);

            if (deleteResult.success)
                return Json(new { success = true });

            return Json(new { success = false, error = deleteResult.data });
        }
    }
}