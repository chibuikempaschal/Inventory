using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Api.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly IUser _userService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService, IUser userService)
        {
            _logger = logger;
            _authService = authService;
            _userService = userService;
        }


        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            if (result is null)
                return BadRequest("Registration could not be completed.");

            return Ok(result);
        }


        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponse?>> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var create = await _authService.LoginAsync(request, cancellationToken);
            return Ok(create);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthProfile?>> Profile(
    CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
            {
                throw new ForbiddenException("User identifier claim is missing or invalid.");
            }

            var profile = await _userService.GetProfileAsync(
                userId, cancellationToken) ?? throw new NotFoundException("User not found!");

            return Ok(profile);
        }

        [HttpPost("logout")]
        [Authorize] //  User must be logged in to log out
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            // Call the service we just created
            // Extract UserId from the Access Token automatically
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var success = await _authService.LogoutAsync(userId, cancellationToken);

            if (!success)
                return BadRequest(new { message = "Already logged out or no active sessions found." });

            return Ok(new { message = "Logged out successfully from all devices." });
        }

        [HttpDelete("delete_user")]
        [Authorize(Roles = "Owner,SystemAdmin,Manager")] //  User must be logged in 
        public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
                return Unauthorized();

            //permission check
            if (!User.IsInRole("Admin"))
                throw new ForbiddenException("You cannot access another user's profile.");

            var success = await _authService.DeleteUserAsync(userId, cancellationToken);

            if (!success)
                return BadRequest(new { message = "Already deleted or no active sessions found." });

            return Ok(new { message = "Deleted successfully from the database." });
        }


    }
}