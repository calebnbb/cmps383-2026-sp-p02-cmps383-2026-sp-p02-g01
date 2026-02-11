using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Domain.Identity;
using Selu383.SP26.Api.Features.Locations;

namespace Selu383.SP26.Api.Controllers;

[Route("api/locations")]
[ApiController]
public class LocationsController(DataContext dataContext) : ControllerBase
{
    // GET /api/locations
    // No auth required
    [HttpGet]
    public IQueryable<LocationDto> GetAll()
    {
        return dataContext.Set<Location>()
            .Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                TableCount = x.TableCount,
                ManagerId = x.ManagerId,
            });
    }

    // GET /api/locations/{id}
    // No auth required
    [HttpGet("{id}")]
    public ActionResult<LocationDto> GetById(int id)
    {
        var location = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (location is null)
        {
            return NotFound();
        }

        return Ok(new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            TableCount = location.TableCount,
            ManagerId = location.ManagerId,
        });
    }

    // POST /api/locations
    // Admin only:
    // - Not logged in => 401
    // - Logged in non-admin => 403
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public ActionResult<LocationDto> Create(LocationDto dto)
    {
        if (dto.TableCount < 1)
        {
            return BadRequest();
        }

        // ManagerId can be null, but if provided it must reference a real user
        if (dto.ManagerId is not null)
        {
            var managerExists = dataContext.Set<User>().Any(u => u.Id == dto.ManagerId.Value);
            if (!managerExists)
            {
                return BadRequest();
            }
        }

        var location = new Location
        {
            Name = dto.Name,
            Address = dto.Address,
            TableCount = dto.TableCount,
            ManagerId = dto.ManagerId
        };

        dataContext.Set<Location>().Add(location);
        dataContext.SaveChanges();

        var resultDto = new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            TableCount = location.TableCount,
            ManagerId = location.ManagerId
        };

        return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
    }

    // PUT /api/locations/{id}
    // - Not logged in => 401
    // - Admin can update anything (including ManagerId)
    // - Non-admin can update ONLY if they are the manager, and they cannot change ManagerId
    [Authorize]
    [HttpPut("{id}")]
    public ActionResult<LocationDto> Update(int id, LocationDto dto)
    {
        if (dto.TableCount < 1)
        {
            return BadRequest();
        }

        var location = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (location is null)
        {
            return NotFound();
        }

        var isAdmin = User.IsInRole("Admin");

        // ClaimTypes.NameIdentifier is the user's Id (int) for IdentityUser<int>
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdRaw) || !int.TryParse(userIdRaw, out var currentUserId))
        {
            // Shouldn't happen in normal flows, but keeps behavior deterministic
            return Unauthorized();
        }

        if (!isAdmin)
        {
            // Must be the manager to update
            if (location.ManagerId != currentUserId)
            {
                return Forbid();
            }

            // Non-admin cannot change ManagerId
            dto.ManagerId = location.ManagerId;
        }
        else
        {
            // Admin may set ManagerId, but if provided it must be valid
            if (dto.ManagerId is not null)
            {
                var managerExists = dataContext.Set<User>().Any(u => u.Id == dto.ManagerId.Value);
                if (!managerExists)
                {
                    return BadRequest();
                }
            }

            location.ManagerId = dto.ManagerId;
        }

        location.Name = dto.Name;
        location.Address = dto.Address;
        location.TableCount = dto.TableCount;

        dataContext.SaveChanges();

        return Ok(new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            TableCount = location.TableCount,
            ManagerId = location.ManagerId
        });
    }

    // DELETE /api/locations/{id}
    // - Not logged in => 401
    // - Admin can delete anything
    // - Non-admin can delete ONLY if they are the manager
    [Authorize]
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var location = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (location is null)
        {
            return NotFound();
        }

        var isAdmin = User.IsInRole("Admin");

        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdRaw) || !int.TryParse(userIdRaw, out var currentUserId))
        {
            return Unauthorized();
        }

        if (!isAdmin && location.ManagerId != currentUserId)
        {
            return Forbid();
        }

        dataContext.Set<Location>().Remove(location);
        dataContext.SaveChanges();

        return Ok();
    }
}
