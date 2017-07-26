using System.Threading.Tasks;
using Face.Service.FaceService;
using Microsoft.AspNetCore.Mvc;

namespace Face.Web.Controllers
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
        
        [HttpGet]
        public async Task<IActionResult> List(string groupId)
        {
            return View();
        }
    }
}
