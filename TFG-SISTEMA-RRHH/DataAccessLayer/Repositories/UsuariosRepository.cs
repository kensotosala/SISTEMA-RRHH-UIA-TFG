using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// Implementación del repositorio de Usuarios usando Entity Framework
    /// </summary>
    public class UsuariosRepository : IUsuariosRepository
    {
        private readonly SistemaRhContext _context;

        public UsuariosRepository(SistemaRhContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public async Task<Usuarios?> GetByIdAsync(int id)
        {
            return await _context.Set<Usuarios>()
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);
        }

        /// <summary>
        /// Obtiene un usuario por nombre de usuario (sin detalles)
        /// </summary>
        public async Task<Usuarios?> GetByUsernameAsync(string username)
        {
            return await _context.Set<Usuarios>()
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);
        }

        /// <summary>
        /// Obtiene un usuario por nombre de usuario con todos sus detalles
        /// Incluye: Empleado, Roles y relaciones necesarias
        /// </summary>
        public async Task<Usuarios?> GetByUsernameWithDetailsAsync(string username)
        {
            return await _context.Set<Usuarios>()
                .Include(u => u.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(u => u.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.Rol)
                .Include(u => u.AuditoriaCambios)
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public async Task<List<Usuarios>> GetAllAsync()
        {
            return await _context.Set<Usuarios>()
                .Include(u => u.Empleado)
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.Rol)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene solo los usuarios activos
        /// </summary>
        public async Task<List<Usuarios>> GetActiveUsersAsync()
        {
            return await _context.Set<Usuarios>()
                .Include(u => u.Empleado)
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.Rol)
                .Where(u => u.Estado == "ACTIVO")
                .ToListAsync();
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        public async Task<Usuarios> CreateAsync(Usuarios usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            usuario.FechaCreacion = DateTime.UtcNow;
            usuario.Estado = usuario.Estado ?? "ACTIVO";

            _context.Set<Usuarios>().Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        public async Task<bool> UpdateAsync(Usuarios usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            usuario.FechaModificacion = DateTime.UtcNow;

            _context.Set<Usuarios>().Update(usuario);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        /// <summary>
        /// Elimina un usuario (eliminación lógica)
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await GetByIdAsync(id);
            if (usuario == null)
                return false;

            usuario.Estado = "INACTIVO";
            usuario.FechaModificacion = DateTime.UtcNow;

            return await UpdateAsync(usuario);
        }

        /// <summary>
        /// Verifica si existe un nombre de usuario
        /// </summary>
        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Set<Usuarios>()
                .AnyAsync(u => u.NombreUsuario == username);
        }

        /// <summary>
        /// Verifica si existe un nombre de usuario, excluyendo un ID específico
        /// Útil para actualizaciones
        /// </summary>
        public async Task<bool> ExistsByUsernameExcludingIdAsync(string username, int excludeId)
        {
            return await _context.Set<Usuarios>()
                .AnyAsync(u => u.NombreUsuario == username && u.IdUsuario != excludeId);
        }

        /// <summary>
        /// Actualiza la fecha de último acceso del usuario
        /// </summary>
        public async Task<bool> UpdateLastAccessAsync(int userId)
        {
            var usuario = await GetByIdAsync(userId);
            if (usuario == null)
                return false;

            usuario.UltimoAcceso = DateTime.UtcNow;
            return await UpdateAsync(usuario);
        }

        /// <summary>
        /// Cambia el estado de un usuario
        /// </summary>
        public async Task<bool> ChangeStatusAsync(int userId, string newStatus)
        {
            var estadosValidos = new[] { "ACTIVO", "INACTIVO", "BLOQUEADO" };
            if (!estadosValidos.Contains(newStatus.ToUpper()))
                throw new ArgumentException("Estado no válido", nameof(newStatus));

            var usuario = await GetByIdAsync(userId);
            if (usuario == null)
                return false;

            usuario.Estado = newStatus.ToUpper();
            usuario.FechaModificacion = DateTime.UtcNow;

            return await UpdateAsync(usuario);
        }
    }
}