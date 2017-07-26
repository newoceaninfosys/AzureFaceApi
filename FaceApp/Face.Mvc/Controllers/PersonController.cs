using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Face.Mvc.Models.Group;
using Face.Mvc.Models.Person;
using Face.Service.FaceService;
using Microsoft.AspNetCore.Mvc;

namespace Face.Mvc.Controllers
{
    public class PersonController : Controller
    {
        #region variables

        private readonly IFaceService _faceService;

        #endregion

        #region constructors

        public PersonController(IFaceService faceService)
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
            return RedirectToAction("List");
        }

        public async Task<IActionResult> List(string groupId)
        {
            //groups
            var allGroups = await GetAllGroup();

            //select groups
            var selectGroups = allGroups;
            if (!string.IsNullOrEmpty(groupId))
            {
                selectGroups = new List<GroupViewModel>();
                var selectGroup = allGroups.FirstOrDefault(x => x.Id == groupId)?? allGroups.FirstOrDefault();
                if (selectGroup != null)
                    selectGroups.Add(selectGroup);
            }

            //person
            var persons = new List<PersonViewModel>();
            foreach (var group in selectGroups)
            {
                var personsInGroupData = await _faceService.PersonsInGroup(group.Id);
                if (personsInGroupData.success && personsInGroupData.data.Any())
                {
                    persons.AddRange(personsInGroupData.data.Select(y => new PersonViewModel()
                    {
                        Id = y.personId,
                        Name = y.name,
                        UserData = y.userData,
                        PersistedFaceIds = y.persistedFaceIds,
                        PersistedFaceUrls = y.persistedFaceIds.Select(z => $"/Facing/{z}.jpg").ToList(),
                        Group = group
                    }));
                }
            }

            //json
            var model = new PersonListViewModel()
            {
                GroupId = groupId,
                Groups = allGroups,
                Persons = persons,
            };

            //return
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Add(PersonViewModel model)
        {
            var addResult = await _faceService.AddPersonToGroup(model.Group.Id, model.Name, model.UserData);
            if (!addResult.success)
                return Json(new { success = false, error = addResult.data });

            model.Id = addResult.data;
            var getGroupResult = await _faceService.GetGroup(model.Group.Id);
            if (getGroupResult.success)
                model.Group.Name = getGroupResult.data.name;

            return Json(new {success = true, Data = model});
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(PersonViewModel model)
        {
            var updateResult = await _faceService.UpdatePerson(model.Group.Id, model.Id, model.Name, model.UserData);
            if (!updateResult.success)
                return Json(new {success = false, error = updateResult.data});

            var getGroupResult = await _faceService.GetGroup(model.Group.Id);
            if (getGroupResult.success)
                model.Group.Name = getGroupResult.data.name;

            return Json(new {success = true, Data = model});
        }

        [HttpPost]
        public async Task<IActionResult> Delete(PersonViewModel model)
        {
            var deleteResult = await _faceService.DeletePerson(model.Group.Id, model.Id);
            if (!deleteResult.success)
                return Json(new { success = false, error = deleteResult.data });

            return Json(new { success = true });
        }

        #endregion
    }
}