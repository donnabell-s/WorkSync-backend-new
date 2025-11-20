using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using ASI.Basecode.Data;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RoomsController : ASI.Basecode.WebApp.Mvc.ControllerBase<RoomsController>
    {
        private readonly IRoomService _roomService;
        private readonly IWebHostEnvironment _env;
        private readonly WorkSyncDbContext _db;

        public RoomsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IRoomService roomService,
            IWebHostEnvironment env,
            WorkSyncDbContext db)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _roomService = roomService;
            _env = env;
            _db = db;
        }

        // DTOs
        public class OperatingHoursDto
        {
            public DayHoursDto Weekdays { get; set; }
            public DayHoursDto Weekends { get; set; }
        }

        public class DayHoursDto
        {
            public string Open { get; set; }
            public string Close { get; set; }
        }

        // Test endpoint to verify authentication/token and claims without role authorization
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestToken()
        {
            var user = HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;

            List<object> claims;
            if (user == null)
            {
                claims = new List<object>();
            }
            else
            {
                claims = user.Claims.Select(c => (object)new { c.Type, c.Value }).ToList();
            }

            return Ok(new { isAuthenticated, claims });
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            try
            {
                var items = await _roomService.GetRoomsAsync(cancellationToken);

                var results = items.Select(r => new
                {
                    r.RoomId,
                    r.Name,
                    r.Code,
                    r.Seats,
                    r.Location,
                    r.Level,
                    r.SizeLabel,
                    r.Status,
                    OperatingHours = r.OperatingHours,
                    r.ImageUrl,
                    r.CreatedAt,
                    r.UpdatedAt,
                    Amenities = r.RoomAmenities?.Select(a => a.Amenity).ToList() ?? new List<string>()
                }).ToList();

                return Ok(results);
            }
            catch (System.Exception ex)
            {
                var list = new List<string>();
                var e = ex;
                while (e != null)
                {
                    list.Add(e.Message);
                    e = e.InnerException;
                }
                return StatusCode(500, new { error = "Get rooms failed", details = list });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            try
            {
                var item = await _roomService.GetByIdAsync(id, cancellationToken);
                if (item == null) return NotFound();

                var result = new
                {
                    item.RoomId,
                    item.Name,
                    item.Code,
                    item.Seats,
                    item.Location,
                    item.Level,
                    item.SizeLabel,
                    item.Status,
                    OperatingHours = item.OperatingHours,
                    item.ImageUrl,
                    item.CreatedAt,
                    item.UpdatedAt,
                    Amenities = item.RoomAmenities?.Select(a => a.Amenity).ToList() ?? new List<string>()
                };

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                var list = new List<string>();
                var e = ex;
                while (e != null)
                {
                    list.Add(e.Message);
                    e = e.InnerException;
                }
                return StatusCode(500, new { error = "Get room failed", details = list });
            }
        }

        // Admin / SuperAdmin only endpoint to create a room. Accepts a CreateRoomRequest from multipart/form-data with an optional file field named 'Image'.
        [HttpPost]
        [RequestSizeLimit(1024 * 1024 * 50)] // 50 MB
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Post([FromForm] CreateRoomRequest request, CancellationToken cancellationToken)
        {
            Console.WriteLine("[RoomsController] POST endpoint invoked.");
            return await CreateRoomInternal(request, cancellationToken);
        }

        // JSON variant
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> PostJson([FromBody] CreateRoomRequest request, CancellationToken cancellationToken)
        {
            return await CreateRoomInternal(request, cancellationToken);
        }

        private async Task<IActionResult> CreateRoomInternal(CreateRoomRequest request, CancellationToken cancellationToken)
        {
            if (request == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Name and Code are required.");

            var room = new Room
            {
                RoomId = Guid.NewGuid().ToString(),
                Name = request.Name,
                Code = request.Code,
                Seats = request.Seats,
                Location = request.Location,
                Level = request.Level,
                SizeLabel = request.SizeLabel,
                Status = request.Status,
                OperatingHours = null, // Will set below
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoomAmenities = new List<RoomAmenity>(),
            };

            // Deserialize OperatingHours
            if (!string.IsNullOrWhiteSpace(request.OperatingHours))
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var ops = System.Text.Json.JsonSerializer.Deserialize<OperatingHoursDto>(request.OperatingHours, options);
                    room.OperatingHours = System.Text.Json.JsonSerializer.Serialize(ops);
                }
                catch
                {
                    // Invalid JSON, set to null
                    room.OperatingHours = null;
                }
            }

            if (request.Amenities != null)
            {
                foreach (var a in request.Amenities.Distinct())
                {
                    room.RoomAmenities.Add(new RoomAmenity { RoomId = room.RoomId, Amenity = a });
                }
            }

            // Handle image (uploaded or remote/data)
            await ProcessImageForRoom(request, room, cancellationToken);

            try
            {
                // get actor id
                int? authorId = null;
                var actor = HttpContext.User;
                if (actor != null && actor.Identity.IsAuthenticated)
                {
                    var claim = actor.Claims.FirstOrDefault(c => c.Type.Equals("UserRefId", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals(System.Security.Claims.ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("id", StringComparison.OrdinalIgnoreCase));
                    if (claim != null && int.TryParse(claim.Value, out var val)) authorId = val;
                }

                await _roomService.CreateAsync(room, actorId: authorId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                var messages = new List<string>();
                var e = ex;
                while (e != null)
                {
                    messages.Add(e.Message);
                    e = e.InnerException;
                }
                return StatusCode(500, new { error = "Failed to create room", details = messages });
            }

            var response = new
            {
                room.RoomId,
                room.Name,
                room.Code,
                room.Seats,
                room.Location,
                room.Level,
                room.SizeLabel,
                room.Status,
                OperatingHours = room.OperatingHours,
                room.ImageUrl,
                room.CreatedAt,
                room.UpdatedAt,
                Amenities = room.RoomAmenities?.Select(a => a.Amenity).ToList() ?? new List<string>()
            };

            return CreatedAtAction(nameof(Get), new { id = room.RoomId }, response);
        }

        [HttpPut("{id}")]
        [RequestSizeLimit(1024 * 1024 * 50)] // 50 MB
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Put(string id, [FromForm] CreateRoomRequest request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[RoomsController] PUT endpoint invoked for RoomId={id}.");
            return await UpdateRoomInternal(id, request, cancellationToken);
        }

        private async Task<IActionResult> UpdateRoomInternal(string id, CreateRoomRequest request, CancellationToken cancellationToken)
        {
            if (request == null) return BadRequest(new { message = "Request payload missing" });

            var roomOriginal = await _roomService.GetByIdAsync(id, cancellationToken);
            if (roomOriginal == null) return NotFound(new { message = "Room not found" });

            var updated = new Room
            {
                RoomId = roomOriginal.RoomId,
                Name = string.IsNullOrWhiteSpace(request.Name) ? roomOriginal.Name : request.Name,
                Code = string.IsNullOrWhiteSpace(request.Code) ? roomOriginal.Code : request.Code,
                Seats = request.Seats ?? roomOriginal.Seats,
                Location = string.IsNullOrWhiteSpace(request.Location) ? roomOriginal.Location : request.Location,
                Level = string.IsNullOrWhiteSpace(request.Level) ? roomOriginal.Level : request.Level,
                SizeLabel = string.IsNullOrWhiteSpace(request.SizeLabel) ? roomOriginal.SizeLabel : request.SizeLabel,
                Status = string.IsNullOrWhiteSpace(request.Status) ? roomOriginal.Status : request.Status,
                OperatingHours = null, // Will set below
                ImageUrl = roomOriginal.ImageUrl,
                UpdatedAt = DateTime.UtcNow,
                RoomAmenities = new List<RoomAmenity>()
            };

            // Deserialize OperatingHours
            if (!string.IsNullOrWhiteSpace(request.OperatingHours))
            {
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var ops = System.Text.Json.JsonSerializer.Deserialize<OperatingHoursDto>(request.OperatingHours, options);
                    updated.OperatingHours = System.Text.Json.JsonSerializer.Serialize(ops);
                }
                catch
                {
                    // Keep original if invalid
                    updated.OperatingHours = roomOriginal.OperatingHours;
                }
            }
            else
            {
                updated.OperatingHours = roomOriginal.OperatingHours;
            }

            // Image processing into detached instance
            await ProcessImageForRoom(request, updated, cancellationToken);

            if (request.Amenities != null && request.Amenities.Count > 0)
            {
                foreach (var a in request.Amenities.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                {
                    updated.RoomAmenities.Add(new RoomAmenity { RoomId = updated.RoomId, Amenity = a.Trim() });
                }
            }

            try
            {
                int? authorId = null;
                var actor = HttpContext.User;
                if (actor?.Identity?.IsAuthenticated == true)
                {
                    var claim = actor.Claims.FirstOrDefault(c => c.Type.Equals("UserRefId", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals(System.Security.Claims.ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("id", StringComparison.OrdinalIgnoreCase));
                    if (claim != null && int.TryParse(claim.Value, out var val)) authorId = val;
                }

                await _roomService.UpdateAsync(updated, actorId: authorId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                var messages = new List<string>();
                for (var e = ex; e != null; e = e.InnerException) messages.Add(e.Message);
                return StatusCode(500, new { error = "Update room failed", details = messages });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[RoomsController] DELETE endpoint invoked for RoomId={id}.");
            var room = await _roomService.GetByIdAsync(id, cancellationToken);
            if (room == null) return NotFound();

            try
            {
                // actor id
                int? authorId = null;
                var actor = HttpContext.User;
                if (actor != null && actor.Identity.IsAuthenticated)
                {
                    var claim = actor.Claims.FirstOrDefault(c => c.Type.Equals("UserRefId", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals(System.Security.Claims.ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase)
                                                                || c.Type.Equals("id", StringComparison.OrdinalIgnoreCase));
                    if (claim != null && int.TryParse(claim.Value, out var val)) authorId = val;
                }

                Console.WriteLine($"[RoomsController] DELETE operation: AuthorId={authorId}.");
                await _roomService.DeleteAsync(id, actorId: authorId, cancellationToken: cancellationToken);

                return NoContent();
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
                Console.WriteLine($"[RoomsController] DELETE operation failed: {string.Join(", ", list)}");
                return StatusCode(500, new { error = "Delete room failed", details = list });
            }
        }

        // Handle uploaded or provided image (data URI or remote URL) and save local copy to wwwroot/room-images
        private async Task ProcessImageForRoom(CreateRoomRequest request, Room room, CancellationToken cancellationToken)
        {
            if (request.Image != null && request.Image.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(request.Image.FileName)?.ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext))
                {
                    // invalid extension - ignore image
                    return;
                }

                var imagesRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "room-images");
                if (!Directory.Exists(imagesRoot)) Directory.CreateDirectory(imagesRoot);
                var fileName = $"{room.RoomId}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(imagesRoot, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream, cancellationToken);
                }
                room.ImageUrl = $"{Request.Scheme}://{Request.Host.Value}/room-images/{fileName}";
                return;
            }

            if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                var imgVal = request.ImageUrl.Trim();
                try
                {
                    string localRelative = null;

                    if (imgVal.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var comma = imgVal.IndexOf(',');
                        if (comma > 0)
                        {
                            var meta = imgVal.Substring(5, comma - 5);
                            var isBase64 = meta.IndexOf("base64", StringComparison.OrdinalIgnoreCase) >= 0;
                            var mime = meta.Split(';')[0];
                            if (isBase64)
                            {
                                var b64 = imgVal.Substring(comma + 1);
                                var bytes = Convert.FromBase64String(b64);
                                var ext = mime switch
                                {
                                    "image/jpeg" => ".jpg",
                                    "image/jpg" => ".jpg",
                                    "image/png" => ".png",
                                    "image/gif" => ".gif",
                                    "image/webp" => ".webp",
                                    _ => ".bin"
                                };

                                var imagesRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "room-images");
                                if (!Directory.Exists(imagesRoot)) Directory.CreateDirectory(imagesRoot);
                                var fileName = $"{room.RoomId}_{Guid.NewGuid()}{ext}";
                                var filePath = Path.Combine(imagesRoot, fileName);
                                System.IO.File.WriteAllBytes(filePath, bytes);
                                localRelative = $"{Request.Scheme}://{Request.Host.Value}/room-images/{fileName}";
                            }
                        }
                    }
                    else if (imgVal.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || imgVal.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        using var http = new System.Net.Http.HttpClient();
                        var resp = await http.GetAsync(imgVal, cancellationToken);
                        if (resp.IsSuccessStatusCode)
                        {
                            var contentType = resp.Content.Headers.ContentType?.MediaType;
                            var ext =
                                contentType == "image/jpeg" || contentType == "image/jpg" ? ".jpg" :
                                contentType == "image/png" ? ".png" :
                                contentType == "image/gif" ? ".gif" :
                                contentType == "image/webp" ? ".webp" :
                                Path.GetExtension((new Uri(imgVal)).AbsolutePath);

                            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

                            var imagesRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "room-images");
                            if (!Directory.Exists(imagesRoot)) Directory.CreateDirectory(imagesRoot);
                            var fileName = $"{room.RoomId}_{Guid.NewGuid()}{ext}";
                            var filePath = Path.Combine(imagesRoot, fileName);

                            using (var stream = await resp.Content.ReadAsStreamAsync(cancellationToken))
                            using (var outFs = new FileStream(filePath, FileMode.Create))
                            {
                                await stream.CopyToAsync(outFs, cancellationToken);
                            }

                            localRelative = $"{Request.Scheme}://{Request.Host.Value}/room-images/{fileName}";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(localRelative))
                    {
                        room.ImageUrl = localRelative;
                    }
                    else
                    {
                        room.ImageUrl = request.ImageUrl;
                    }
                }
                catch
                {
                    room.ImageUrl = request.ImageUrl;
                }
            }
        }

        // DTO for creating a room from Admin / SuperAdmin UI
        public class CreateRoomRequest
        {
            public string Name { get; set; }
            public string Code { get; set; } // Room number
            public string Location { get; set; }
            public string Level { get; set; }
            public string SizeLabel { get; set; } // Small, Medium, Large
            public int? Seats { get; set; }
            public string Status { get; set; } // Available, Occupied, Under Maintenance
            public string OperatingHours { get; set; } // JSON string
            public List<string> Amenities { get; set; }
            public string ImageUrl { get; set; }
            // Optional uploaded image when content-type is multipart/form-data
            public IFormFile Image { get; set; }
        }
    }
}
