using ApplicationLayer.Interfaces;
using BusinessLogicLayer.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var token = await _authService.LoginAsync(login);
            if (token == null) return Unauthorized("Invalid credentials");

            return Ok(new { token });
        }
    }
}