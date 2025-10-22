using EasyChat.Api.Models;
using EasyChat.Api.Models.DTOs;
using EasyChat.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EasyChat.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<IActionResult> Register([FromBody] RegisterDto dto) 
        {
            // Check if model is valid (validations from RegisterDto)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            // Create new user
            var user = new IdentityUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors });
            }

            // Generate JWT token
            var token = _tokenService.GenerateToken(user);

            // Return success response
            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        public async Task<IActionResult> Login([FromBody] LoginDto dto) 
        {
            // Check if model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Generate JWT token
            var token = _tokenService.GenerateToken(user);

            // Return success response
            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }
    }
}
