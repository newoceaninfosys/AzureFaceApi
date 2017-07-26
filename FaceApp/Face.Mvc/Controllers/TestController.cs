using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Face.Service.FaceService;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Face.Mvc.Controllers
{
    [Route("api/face")]
    public class TestController : Controller
    {
        private readonly IFaceService _faceService;

        public TestController(IFaceService faceService)
        {
            _faceService = faceService;
        }

        // POST api/values
        [HttpPost("detect")]
        public async Task<IActionResult> Post()
        {
            //check file
            if (Request.Form.Files.Count == 0)
                return BadRequest();
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
                return BadRequest();

            byte[] arr = null;

            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                arr = ms.ToArray();
            }

            var test = await _faceService.DetectFace(arr);
            return Ok();
        }

        // POST api/values
        [HttpPost("identify")]
        public async Task<IActionResult> Identify()
        {
            //check file
            if (Request.Form.Files.Count == 0)
                return BadRequest();
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
                return BadRequest();

            byte[] arr = null;

            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                arr = ms.ToArray();
            }

            var train = await _faceService.TrainGroup("uit");

            var detect = await _faceService.DetectFace(arr);

            if (detect.success)
            {
                string[] faceIds = detect.data.Select(x => x.FaceId).ToArray();

                var identify = await _faceService.Identify("uit", faceIds, 2, 0.8);

                var person = await _faceService.GetPerson("uit", "2928d24b-88ac-4332-8e26-675256a34212");
            }
            return Ok();
        }
    }
}
