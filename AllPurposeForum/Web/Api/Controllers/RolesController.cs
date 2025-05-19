using AllPurposeForum.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllPurposeForum.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RolesController> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(ILogger<RolesController> logger, RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _logger = logger;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return Ok(roles);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName)) return BadRequest("Role name cannot be null or empty.");
        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (roleExists) return Conflict($"Role '{roleName}' already exists.");
        var role = new IdentityRole(roleName);
        var result = await _roleManager.CreateAsync(role);
        if (result.Succeeded) return Ok($"Role '{roleName}' created successfully.");
        return BadRequest(
            $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveRole(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return NotFound($"Role '{roleName}' not found.");
        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded) return Ok($"Role '{roleName}' removed successfully.");
        return BadRequest(
            $"Failed to remove role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}