using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class AsistenciasRepository : IAsistenciasRepository
    {
        private readonly SistemaRhContext _context;

        public AsistenciasRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Asistencias asistencia)
        {
            _context.Asistencias.Add(asistencia);
            await _context.SaveChangesAsync();
        }

        public async Task<Asistencias?> GetByEmpleadoYFechaAsync(int empleadoId, DateTime fecha)
        {
            var inicio = fecha.Date;
            var fin = inicio.AddDays(1);

            return await _context.Asistencias
                .FirstOrDefaultAsync(a =>
                    a.EmpleadoId == empleadoId &&
                    a.FechaRegistro >= inicio &&
                    a.FechaRegistro < fin);
        }

        public async Task UpdateAsync(Asistencias asistencia)
        {
            _context.Asistencias.Update(asistencia);
            await _context.SaveChangesAsync();
        }
    }
}