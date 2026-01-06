using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthManager _authManager;

        public AuthController(IAuthManager authManager)
        {
            _authManager = authManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var result = await _authManager.LoginAsync(login);

            if (result == null)
                return Unauthorized("Credenciales inválidas");

            return Ok(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // JWT puro → logout es CLIENT SIDE
            return Ok("Logout exitoso");
        }
    }
}