using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Http;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly IAuditoriaRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(IAuditoriaRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<AuditoriaCambios>> ListarAsync()
        {
            return await _repo.ListarLogsAsync();
        }

        public async Task RegistrarAsync(string tablaAfectada, string descripcion)
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId is null)
                return;

            var auditoria = new AuditoriaCambios
            {
                TablaAfectada = tablaAfectada,
                Descripcion = descripcion,
                UsuarioId = usuarioId.Value,
                FechaCreacion = DateTime.Now
            };

            await _repo.CrearAsync(auditoria);
        }

        private int? ObtenerUsuarioId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("UserId");

            if (claim is null || !int.TryParse(claim.Value, out var id))
                return null;

            return id;
        }
    }
}