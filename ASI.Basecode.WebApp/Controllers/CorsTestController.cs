using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Test controller to verify CORS configuration
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowVite")] // Explicitly enable CORS for this controller
    public class CorsTestController : ControllerBase
    {
        /// <summary>
        /// Simple test endpoint to verify CORS is working
        /// </summary>
        /// <returns>Test message</returns>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                success = true,
                message = "CORS is working!",
                timestamp = System.DateTime.UtcNow,
                origin = Request.Headers["Origin"].ToString(),
                userAgent = Request.Headers["User-Agent"].ToString()
            });
        }

        /// <summary>
        /// Test OPTIONS preflight request
        /// </summary>
        /// <returns>OK</returns>
        [HttpOptions("test")]
        public IActionResult TestOptions()
        {
            return Ok();
        }

        /// <summary>
        /// Test POST endpoint
        /// </summary>
        /// <returns>Test message</returns>
        [HttpPost("test")]
        public IActionResult TestPost([FromBody] object data)
        {
            return Ok(new
            {
                success = true,
                message = "CORS POST is working!",
                receivedData = data
            });
        }
    }
}
