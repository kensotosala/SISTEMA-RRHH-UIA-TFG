using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly IRolesManager _managerRoles;

        public RolController(IRolesManager managerRoles)
        {
            _managerRoles = managerRoles;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _managerRoles.GetAllAsync();
            return Ok(roles);
        }
    }
}