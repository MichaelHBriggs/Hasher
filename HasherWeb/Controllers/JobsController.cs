using HasherDataObjects.Models;
using Microsoft.AspNetCore.Mvc;

namespace HasherWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController(HasherContext DBContext) : Controller
    {
        [HttpGet("GetAllJobs")]
        public List<JobInfo> GetAllJobs()
        {
            return DBContext.Jobs.ToList();
        }

        [HttpGet("GetAllJobCount")]
        public int GetAllJobCount()
        {
            return DBContext.Jobs.Count(j => j.DeletedAt == null);
        }

        [HttpGet("GetAllJobsByPage/{pageNumber}/{pageSize}")]
        public List<JobInfo> GetAllJobsByPage(int pageNumber, int pageSize)
        {
            return DBContext.Jobs
                .Where(j => j.DeletedAt == null)
                .OrderByDescending(j => j.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        [HttpGet("GetJobsForRun/{runId}")]
        public List<JobInfo> GetJobsForRun(Guid runId)
        {
            RunResults? runResults = DBContext.RunResults
                .Where(rr => rr.Id == runId && rr.DeletedAt == null)
                .OrderByDescending(rr => rr.CreatedAt)
                .FirstOrDefault();
            try
            {
                return DBContext.Jobs.Where(j => j.MostRecentRun == runResults && j.DeletedAt == null)
                                     .OrderByDescending(j => j.CreatedAt)
                                     .ToList();
            }
            catch
            {
                Thread.Sleep(100);
                return GetJobsForRun(runId);
            }
        }
    }
}
