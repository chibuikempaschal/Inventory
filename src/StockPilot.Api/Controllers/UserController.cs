using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static StockPilot.Application.Common.Exceptions.ApplicationException;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUser _userService;

        public UserController(ILogger<UserController> logger, IUser userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("UserById")]
        [Authorize(Roles = "Owner,SystemAdmin,Manager")]
        public async Task<ActionResult<UserResponse>> GetUserById(Guid userId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
                return Unauthorized();

            // Ownership / permission check
            if (currentUserId != userId && !User.IsInRole("Admin"))
                throw new ForbiddenException("You cannot access another user's profile.");

            var user = await _userService.GetUserByIdAsync(userId, cancellationToken)
        ?? throw new NotFoundException($"User {userId} not found.");

            return Ok(user);
        }
        [HttpGet("UserByName")]
        [Authorize(Roles = "Owner,SystemAdmin,Manager")]
        public async Task<ActionResult<UserResponse>> GetUserByName(string name, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByNameAsync(name, cancellationToken)
           ?? throw new NotFoundException($"User {name} not found.");

            return Ok(user);
        }

        [HttpGet("Users")]
        [Authorize(Roles = "Owner,SystemAdmin,Manager")]
        public async Task<ActionResult<List<GetAllUserResponse>>> GetUsers(CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(
               User.FindFirstValue(ClaimTypes.NameIdentifier),
               out var userId))
            {
                throw new ForbiddenException("User identifier claim is missing or invalid.");
            }

            var user = await _userService.GetAllUsersAsync(cancellationToken)
                             ?? throw new NotFoundException("Users not found.");

            return Ok(user);
        }


        [HttpPut("Update-User")] // Or simply [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult> UpdateUser(UpdateUser updateUser, CancellationToken cancellationToken)
        {
            // 1. Extract the userId strictly from the JWT Token
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
                return Unauthorized();

            // 2. Pass the extracted currentUserId to the service
            var update = await _userService.UpdateUserAsync(currentUserId, updateUser, cancellationToken)
                         ?? throw new NotFoundException("No user found for update.");

            return Ok(update);
        }
    }
}