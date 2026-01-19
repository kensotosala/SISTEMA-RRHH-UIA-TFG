using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class IncapacidadesRepository : IIncapacidadesRepository
    {
        private readonly SistemaRhContext _context;

        public IncapacidadesRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarIncapacidadAsync(Incapacidades incapacidad)
        {
            _context.Incapacidades.Update(incapacidad);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarIncapacidadAsync(int idIncapacidad)
        {
            var eliminarIncapacidad = await _context.Incapacidades.FindAsync(idIncapacidad);

            if (eliminarIncapacidad == null)
                return false;

            _context.Incapacidades.Remove(eliminarIncapacidad);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Incapacidades>> ListarIncapacidadesAsync()
        {
            return await _context.Incapacidades.ToListAsync();
        }

        public async Task<Incapacidades?> ListarIncapacidadPorId(int idIncapacidad)
        {
            return await _context.Incapacidades.FindAsync(idIncapacidad);
        }

        public async Task<Incapacidades?> RegistarIncapacidadesAsync(Incapacidades incapacidad)
        {
            _context.Incapacidades.Add(incapacidad);
            await _context.SaveChangesAsync();
            return incapacidad;
        }
    }
}