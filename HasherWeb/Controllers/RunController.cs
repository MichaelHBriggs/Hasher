using HasherDataObjects.Models;
using Microsoft.AspNetCore.Mvc;

namespace HasherWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunController(HasherContext DBContext) : Controller
    {
        [HttpGet("GetActiveRun")]
        public ActionResult<RunResults> GetActiveRun()
        {
            var query = DBContext.RunResults.Where(rr => rr.IsActive && rr.DeletedAt == null);
            if (query.Any())
            {
                return Ok(query.OrderByDescending(rr => rr.CreatedAt)
                    .FirstOrDefault());
            }
            return NotFound();
        }

        [HttpGet("IsAnyRunActive")]
        public bool IsAnyRunActive()
        {
            return DBContext.RunResults.Any(rr => rr.IsActive && rr.DeletedAt == null);
        }

        [HttpGet("GetAllRuns")]
        public ActionResult<IEnumerable<RunResults>> GetAllRuns()
        {
            List<RunResults> runs = DBContext.RunResults
                .Where(rr => rr.DeletedAt == null)
                .OrderByDescending(rr => rr.CreatedAt)
                .ToList();
            if (runs.Any())
            {
                return Ok(runs);
            }
            return NotFound();
        }

        [HttpGet("GetSpecificRun/{runId}")]
        public ActionResult<RunResults> GetSpecificRun(Guid runId)
        {
            RunResults? run = DBContext.RunResults
                                .Where(r => r.Id == runId && !r.IsDeleted)
                                .FirstOrDefault();
            if (run == null)
            {
                return NotFound();
            }
            return Ok(run);
        }
    }
}
