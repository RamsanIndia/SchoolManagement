using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.API.Extensions;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.Auth.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Shared.Utilities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Exceptions;
using System.Security.Claims;

namespace SchoolManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IpAddressHelper _ipAddressHelper;
        private readonly ILogger<AuthController> _logger;
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator, ICurrentUserService currentUserService,
        ILogger<AuthController> logger,IpAddressHelper ipAddressHelper )
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
            _ipAddressHelper = ipAddressHelper;
        }

        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            // Input validation (defense-in-depth)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                       
            // Map DTO → Command (use AutoMapper in production)
            var command = new RegisterCommand
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserType = (UserType)request.Role,  //  Enum casting
                PhoneNumber = request.PhoneNumber ?? null, // Optional
                ConfirmPassword = request.Password  // For validation
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Refreshes the access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        //[AllowAnonymous] // Add this attribute
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Logs out the user by revoking the refresh token
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            var command = new LogoutCommand
            {
                RefreshToken = request.RefreshToken,
                UserId = User.GetUserId()
            };

            await _mediator.Send(command);
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Revokes all refresh tokens for the current user
        /// </summary>
        [Authorize]
        [HttpPost("revoke-all")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> RevokeAllTokens()
        {
            var command = new RevokeAllTokensCommand
            {
                UserId = User.GetUserId(),
                RevokedByIp = _ipAddressHelper.GetIpAddress()

            };

            await _mediator.Send(command);
            return Ok(new { message = "All tokens revoked successfully" });
        }

        /// <summary>
        /// Gets the current user's profile
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var query = new GetUserProfileQuery(User.GetUserId());

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Validates the current token (useful for checking if user is still authenticated)
        /// </summary>
        [Authorize]
        [HttpGet("validate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public ActionResult ValidateToken()
        {
            return Ok(new
            {
                message = "Token is valid",
                userId = User.GetUserId(),
                email = User.GetUserEmail(),
                role = User.GetUserRole()
            });
        }

        [Authorize]
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            
            // Test all methods of getting username
            var identityName = User.Identity?.Name;
            var claimTypesName = User.FindFirst(ClaimTypes.Name)?.Value;
            var uniqueName = User.FindFirst("unique_name")?.Value;
            var usernameClaim = User.FindFirst("username")?.Value;
            var serviceUsername = _currentUserService.Username;

            var allClaims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                AuthenticationType = User.Identity?.AuthenticationType,

                // Different ways to get username
                IdentityName = identityName,
                ClaimTypesName = claimTypesName,
                UniqueName = uniqueName,
                UsernameClaim = usernameClaim,
                ServiceUsername = serviceUsername,

                // Other user info
                UserId = _currentUserService.UserId,
                Email = _currentUserService.Email,
                FullName = _currentUserService.FullName,
                UserType = _currentUserService.UserType,

                // All claims for debugging
                AllClaims = allClaims
            });
        }
    }
}
