using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class EmpleadosRepository : IEmpleadosRepository
    {
        private readonly SistemaRhContext _context;

        public EmpleadosRepository(SistemaRhContext contex)
        {
            _context = contex;
        }

        public async Task<int> ContarSubordinadosAsync(int id)
        {
            return await _context.Empleados.CountAsync(e => e.JefeInmediatoId == id);
        }

        public async Task<Empleados> CreateAsync(Empleados empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var empleado = await _context.Empleados.Include(e => e.Usuarios).FirstOrDefaultAsync(e => e.IdEmpleado == id);

            if (empleado == null)
            {
                return false;
            }

            // Verificar si tiene relaciones que impiden eliminación
            var tieneAsistencias = await _context.Asistencias.AnyAsync(a => a.EmpleadoId == id);
            var tieneNominas = await _context.Nominas.AnyAsync(n => n.EmpleadoId == id);
            var tieneVacaciones = await _context.Vacaciones.AnyAsync(v => v.EmpleadoId == id);

            if (tieneAsistencias || tieneNominas || tieneVacaciones)
            {
                // En lugar de eliminar físicamente, cambiar el estado a INACTIVO
                empleado.Estado = "INACTIVO";
                await _context.SaveChangesAsync();
                return true;
            }

            // Si no tiene registros históricos, eliminar físicamente
            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EmaillRegistrado(string email)
        {
            return await _context.Empleados.AnyAsync(e => e.Email == email);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Empleados.AnyAsync(e => e.IdEmpleado == id);
        }

        public async Task<bool> ExistsByCodigoAsync(string codigo)
        {
            return await _context.Empleados.AnyAsync(e => e.CodigoEmpleado == codigo);
        }

        public async Task<IEnumerable<Empleados>> GetAllAsync()
        {
            return await _context.Empleados
                .Include(e => e.Puesto)
                .Include(e => e.Departamento)
                .Include(e => e.Usuarios)
                    .ThenInclude(u => u.UsuariosRoles)
                        .ThenInclude(ur => ur.Rol)
                .Where(e => e.Estado == "ACTIVO")
                .ToListAsync();
        }

        public async Task<IEnumerable<Empleados>> GetAllIncludingInactiveAsync()
        {
            return await _context.Empleados
                .Include(e => e.Puesto)
                .Include(e => e.Departamento)
                .Include(e => e.Usuarios)
                    .ThenInclude(u => u.UsuariosRoles)
                        .ThenInclude(ur => ur.Rol)
                .ToListAsync();
        }

        public async Task<IEnumerable<Empleados>> GetAllWithUsersAndRolesAsync()
        {
            return await _context.Empleados
                .Include(e => e.Usuarios)
                    .ThenInclude(u => u!.UsuariosRoles)
                        .ThenInclude(ur => ur.Rol)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Empleados?> GetByIdAsync(int id)
        {
            return await _context.Empleados.Include(e => e.Usuarios).FirstOrDefaultAsync(e => e.IdEmpleado == id);
            ;
        }

        public async Task<bool> TieneSubordinadosAsync(int jefeId)
        {
            return await _context.Empleados
                .AnyAsync(e => e.JefeInmediatoId == jefeId && e.Estado == "ACTIVO");
        }

        public async Task<bool> UpdateAsync(Empleados empleado)
        {
            _context.Empleados.Update(empleado);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}