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

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RoomsController : ASI.Basecode.WebApp.Mvc.ControllerBase<RoomsController>
    {
        private readonly IRoomService _roomService;
        private readonly IWebHostEnvironment _env;

        public RoomsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IRoomService roomService,
            IWebHostEnvironment env)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _roomService = roomService;
            _env = env;
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
                OperatingHours = request.OperatingHours == null ? null : JsonSerializer.Serialize(request.OperatingHours),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoomAmenities = new List<RoomAmenity>(),
            };

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
                await _roomService.CreateAsync(room, cancellationToken);
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
            return await UpdateRoomInternal(id, request, cancellationToken);
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutJson(string id, [FromBody] CreateRoomRequest request, CancellationToken cancellationToken)
        {
            return await UpdateRoomInternal(id, request, cancellationToken);
        }

        private async Task<IActionResult> UpdateRoomInternal(string id, CreateRoomRequest request, CancellationToken cancellationToken)
        {
            if (request == null) return BadRequest();

            var room = await _roomService.GetByIdAsync(id, cancellationToken);
            if (room == null) return NotFound();

            room.Name = request.Name ?? room.Name;
            room.Code = request.Code ?? room.Code;
            room.Seats = request.Seats ?? room.Seats;
            room.Location = request.Location ?? room.Location;
            room.Level = request.Level ?? room.Level;
            room.SizeLabel = request.SizeLabel ?? room.SizeLabel;
            room.Status = request.Status ?? room.Status;
            room.OperatingHours = request.OperatingHours == null ? room.OperatingHours : JsonSerializer.Serialize(request.OperatingHours);
            room.UpdatedAt = DateTime.UtcNow;

            await ProcessImageForRoom(request, room, cancellationToken);

            room.RoomAmenities = room.RoomAmenities ?? new List<RoomAmenity>();
            room.RoomAmenities.Clear();
            if (request.Amenities != null)
            {
                foreach (var a in request.Amenities.Distinct())
                {
                    room.RoomAmenities.Add(new RoomAmenity { RoomId = room.RoomId, Amenity = a });
                }
            }

            try
            {
                await _roomService.UpdateAsync(room, cancellationToken);
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
                return StatusCode(500, new { error = "Update room failed", details = list });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            try
            {
                await _roomService.DeleteAsync(id, cancellationToken);
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
        public OperatingHoursDto OperatingHours { get; set; }
        public List<string> Amenities { get; set; }
        public string ImageUrl { get; set; }
        // Optional uploaded image when content-type is multipart/form-data
        public IFormFile Image { get; set; }
    }

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
}
