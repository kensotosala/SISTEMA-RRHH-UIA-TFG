using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginDTO login)
        //{
        //    var token = await _authManager.(login);
        //    if (token == null) return Unauthorized("Invalid credentials");

        //    return Ok(new { token });
        //}
    }
}