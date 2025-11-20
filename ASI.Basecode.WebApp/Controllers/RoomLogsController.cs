using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RoomLogsController : ASI.Basecode.WebApp.Mvc.ControllerBase<RoomLogsController>
    {
        private readonly WorkSyncDbContext _db;

        public RoomLogsController(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            AutoMapper.IMapper mapper,
            WorkSyncDbContext db)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            try
            {
                var items = await _db.RoomLogs.ToListAsync(cancellationToken);
                return Ok(items);
            }
            catch (Exception ex)
            {
                var list = new List<string>();
                var e = ex;
                while (e != null)
                {
                    list.Add(e.Message);
                    e = e.InnerException;
                }
                // write error to console for diagnostics
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { error = "Get room logs failed", details = list });
            }
        }

        // Quick test endpoint to insert a sample RoomLog and verify DB write
        [HttpPost("TestInsert")] 
        public async Task<IActionResult> TestInsert(CancellationToken cancellationToken)
        {
            try
            {
                var log = new RoomLog
                {
                    RoomIdString = "TEST",
                    RoomName = "Test Room",
                    AuthorId = null,
                    AuthorName = null,
                    ChangeType = "test",
                    Message = "test insert",
                    Timestamp = DateTime.UtcNow
                };

                _db.RoomLogs.Add(log);
                await _db.SaveChangesAsync(cancellationToken);

                return CreatedAtAction(nameof(Get), new { id = log.RoomLogId }, log);
            }
            catch (Exception ex)
            {
                // write error to console for diagnostics
                Console.WriteLine(ex.ToString());
                var list = new List<string>();
                var e = ex;
                while (e != null)
                {
                    list.Add(e.Message);
                    e = e.InnerException;
                }
                return StatusCode(500, new { error = "Test insert failed", details = list });
            }
        }
    }
}
