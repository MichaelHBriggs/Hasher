using HasherDataObjects.Models;
using Microsoft.AspNetCore.Mvc;

namespace HasherWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(HasherContext DBContext) : Controller
    {
        [HttpGet("GetFileCount")]
        public int GetFileCount()
        {
            return DBContext.Files.Count(f => !f.IsDeleted);
        }

        [HttpGet("GetFileByID/{id}")]
        public ActionResult<HashableFile?> GetFileByID(Guid id)
        {
            var file = DBContext.Files.FirstOrDefault(f => f.Id == id && !f.IsDeleted);
            if (file == null)
            {
                return NotFound();
            }
            return Ok(file);
        }

        [HttpGet("GetFilesByJob/{jobId}")]
        public ActionResult<List<HashableFile>> GetFilesByJob(Guid jobId)
        {

            var job = DBContext.Jobs.FirstOrDefault(j => j.Id == jobId && !j.IsDeleted);
            if (job == null)
            {
                return NotFound();
            }

            var files = DBContext.Files
                .Where(f => f.LastJob==job && !f.IsDeleted)
                .ToList();
            if (files.Count == 0)
            {
                return NotFound();
            }
            return Ok(files);
        }

        [HttpGet("GetFileCountByExtensionByJob/{jobId}")]
        public ActionResult<Dictionary<string,int>> GetFileCountByExtensionByJob(Guid jobId)
        {
            var job = DBContext.Jobs.FirstOrDefault(j => j.Id == jobId && !j.IsDeleted);
            if (job == null)
            {
                return NotFound();
            }
            var counts = DBContext.Files
                .Where(f => f.LastJob == job && !f.IsDeleted)
                .GroupBy(f => f.Extension)
                .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                .ToList();
            if (counts.Any())
            {
                Dictionary<string, int> result = [];
                counts.ForEach(kv => result.Add(kv.Key, kv.Value));
                return Ok(result);
            }
            return NotFound();
        }


    }
}
