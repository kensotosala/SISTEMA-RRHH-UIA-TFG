using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class VacacionesRepository : IVacacionesRepository
    {
        private readonly SistemaRhContext _context;

        public VacacionesRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarAsync(Vacaciones vacacion)
        {
            if (vacacion == null) throw new ArgumentNullException(nameof(vacacion));

            // Verificar que la vacación existe
            var vacacionExistente = await _context.Vacaciones.FindAsync(vacacion.IdVacacion);

            if (vacacionExistente == null) return false;

            // Guardar estado anteior para saber si se aprobó
            var estadoAnterior = vacacionExistente.EstadoSolicitud;

            // Actualizar campos
            vacacionExistente.EmpleadoId = vacacion.EmpleadoId;
            vacacionExistente.FechaInicio = vacacion.FechaInicio;
            vacacionExistente.FechaFin = vacacion.FechaFin;
            vacacionExistente.EstadoSolicitud = vacacion.EstadoSolicitud;
            vacacionExistente.JefeApruebaId = vacacion.JefeApruebaId;
            vacacionExistente.FechaAprobacion = vacacion.FechaAprobacion;
            vacacionExistente.ComentariosRechazo = vacacion.ComentariosRechazo;
            vacacionExistente.FechaModificacion = DateTime.Now;

            // Marcar como modificado
            _context.Vacaciones.Update(vacacionExistente);

            // Si se aprobó la vacación, descontar días del saldo
            if (estadoAnterior == "PENDIENTE" && vacacion.EstadoSolicitud == "APROBADA")
            {
                var diasVacaciones = (vacacion.FechaFin - vacacion.FechaInicio).Days + 1;
                var anio = vacacion.FechaInicio.Year;

                await DescontarDiasVacacionesAsync(vacacion.EmpleadoId, anio, diasVacaciones);
            }

            // Guardar cambios
            var resultado = await _context.SaveChangesAsync();

            return resultado > 0;
        }

        public async Task<SaldoVacaciones> ActualizarSaldoVacacionesAsync(SaldoVacaciones saldo)
        {
            if (saldo == null)
                throw new ArgumentNullException(nameof(saldo));

            var saldoExistente = await _context.SaldoVacaciones
                .FirstOrDefaultAsync(s =>
                    s.EmpleadoId == saldo.EmpleadoId &&
                    s.Anio == saldo.Anio
                );

            if (saldoExistente != null)
            {
                // Actualizar saldo existente
                saldoExistente.DiasAcumulados = saldo.DiasAcumulados;
                saldoExistente.DiasDisfrutados = saldo.DiasDisfrutados;
                saldoExistente.FechaActualizacion = DateTime.Now;

                _context.SaldoVacaciones.Update(saldoExistente);
            }
            else
            {
                // Crear nuevo saldo
                saldo.FechaCreacion = DateTime.Now;
                saldo.FechaActualizacion = DateTime.Now;

                await _context.SaldoVacaciones.AddAsync(saldo);
            }

            await _context.SaveChangesAsync();

            return saldoExistente ?? saldo;
        }

        public async Task<SaldoVacaciones> CalcularYGuardarSaldoAsync(int empleadoId, int anio)
        {
            // Obtener el empleado
            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(e => e.IdEmpleado == empleadoId);

            if (empleado == null)
                throw new InvalidOperationException($"No se encontró el empleado con ID {empleadoId}");

            // Fecha de contratación
            var fechaContratacion = empleado.FechaContratacion.ToDateTime(TimeOnly.MinValue);

            // Fecha de corte para el cálculo (31 de diciembre del año consultado)
            var fechaCorte = new DateTime(anio, 12, 31);

            // Si el empleado fue contratado después del año consultado
            if (fechaContratacion.Year > anio)
            {
                return new SaldoVacaciones
                {
                    EmpleadoId = empleadoId,
                    Anio = anio,
                    DiasAcumulados = 0,
                    DiasDisfrutados = 0
                };
            }

            // Calcular días según la ley de Costa Rica
            var tiempoTrabajado = fechaCorte - fechaContratacion;
            var semanasTrabajadas = tiempoTrabajado.TotalDays / 7;

            // ⭐ FÓRMULA LEGAL: (semanas / 50) * 14 días
            var diasGenerados = (int)Math.Floor((semanasTrabajadas / 50.0) * 14);

            // Obtener saldo del año anterior (si existe) para acumulación
            var saldoAnterior = await _context.SaldoVacaciones
                .FirstOrDefaultAsync(s =>
                    s.EmpleadoId == empleadoId &&
                    s.Anio == anio - 1
                );

            // Días no disfrutados del año anterior (máximo 2 períodos = 28 días)
            int diasArrastre = 0;
            if (saldoAnterior != null)
            {
                var diasNoUsadosAnterior = saldoAnterior.DiasAcumulados - (saldoAnterior.DiasDisfrutados ?? 0);

                // Máximo 14 días de arrastre (1 período)
                diasArrastre = Math.Min(diasNoUsadosAnterior, 14);
            }

            // Total acumulado (generados este año + arrastre del anterior)
            var diasAcumulados = diasGenerados + diasArrastre;

            // Límite máximo: 28 días (2 períodos)
            const int MAXIMO_DIAS_ACUMULABLES = 28;
            diasAcumulados = Math.Min(diasAcumulados, MAXIMO_DIAS_ACUMULABLES);

            // Contar días ya disfrutados en este año
            var diasDisfrutados = await ContarDiasVacacionesUsadosAsync(empleadoId, anio);

            // Crear o actualizar el saldo
            var saldo = new SaldoVacaciones
            {
                EmpleadoId = empleadoId,
                Anio = anio,
                DiasAcumulados = diasAcumulados,
                DiasDisfrutados = diasDisfrutados
            };

            return await ActualizarSaldoVacacionesAsync(saldo);
        }

        public async Task<int> ContarDiasVacacionesUsadosAsync(int empleadoId, int anio)
        {
            var vacacionesAprobadas = await _context.Vacaciones
                .Where(v =>
                    v.EmpleadoId == empleadoId &&
                    v.EstadoSolicitud == "APROBADA" &&
                    (v.FechaInicio.Year == anio || v.FechaFin.Year == anio)
                )
                .ToListAsync();

            int totalDias = 0;

            foreach (var vacacion in vacacionesAprobadas)
            {
                var dias = (vacacion.FechaFin - vacacion.FechaInicio).Days + 1;
                totalDias += dias;
            }

            return totalDias;
        }

        public async Task<Vacaciones> CrearAsync(Vacaciones vacacion)
        {
            // Validaciones básicas
            if (vacacion == null) throw new ArgumentNullException(nameof(vacacion));

            // Establecer valores por defecto
            vacacion.FechaSolicitud = DateTime.Now;
            vacacion.EstadoSolicitud = "PENDIENTE";
            vacacion.FechaCreacion = DateTime.Now;

            // Agregar al contexto
            await _context.Vacaciones.AddAsync(vacacion);

            // Guardar cambios
            await _context.SaveChangesAsync();

            return vacacion;
        }

        public async Task<bool> DescontarDiasVacacionesAsync(int empleadoId, int anio, int dias)
        {
            var saldo = await ObtenerSaldoVacacionesAsync(empleadoId, anio);

            if (saldo == null)
            {
                // Si no existe el saldo, calcularlo primero
                saldo = await CalcularYGuardarSaldoAsync(empleadoId, anio);
            }

            // Actualizar días disfrutados
            saldo.DiasDisfrutados = (saldo.DiasDisfrutados ?? 0) + dias;
            saldo.FechaActualizacion = DateTime.Now;

            _context.SaldoVacaciones.Update(saldo);

            var resultado = await _context.SaveChangesAsync();

            return resultado > 0;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            // Validación básica
            var vacacion = await _context.Vacaciones.FindAsync(id);

            if (vacacion == null) return false;

            // Si está aprobada, devolver días al saldo

            if (vacacion.EstadoSolicitud == "APROBADA")
            {
                var diasVacaciones = (vacacion.FechaFin - vacacion.FechaInicio).Days + 1;
                var anio = vacacion.FechaInicio.Year;

                // Devolver días (descuento negativo = suma)
                await DescontarDiasVacacionesAsync(vacacion.EmpleadoId, anio, -diasVacaciones);
            }

            // Lógica de eliminación
            vacacion.EstadoSolicitud = "CANCELADA";
            vacacion.FechaModificacion = DateTime.Now;

            var resultado = await _context.SaveChangesAsync();
            return resultado > 0;
        }

        public async Task<IEnumerable<SaldoVacaciones>> ObtenerHistorialSaldosAsync(int empleadoId)
        {
            return await _context.SaldoVacaciones
                .Where(s => s.EmpleadoId == empleadoId)
                .OrderByDescending(s => s.Anio)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vacaciones>> ObtenerPorEmpleadoIdAsync(int empleadoId)
        {
            if (empleadoId <= 0)
                throw new ArgumentException("El id debe ser mayor a cero", nameof(empleadoId));

            var vacaciones = await _context.Vacaciones
                .Include(v => v.JefeAprueba)
                .Where(v => v.EmpleadoId == empleadoId)
                .OrderByDescending(v => v.FechaSolicitud)
                .ToListAsync();

            return vacaciones;
        }

        public async Task<IEnumerable<Vacaciones>> ObtenerPorEstadoAsync(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("El estado no puede estar vacío", nameof(estado));

            return await _context.Vacaciones
                .Include(v => v.Empleado)
                .Include(v => v.JefeAprueba)
                .Where(v => v.EstadoSolicitud == estado.ToUpper())
                .OrderByDescending(v => v.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<Vacaciones> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El id debe ser mayor a cero", nameof(id));

            var vacacion = await _context.Vacaciones
                .Include(v => v.Empleado)
                .Include(v => v.JefeAprueba)
                .FirstOrDefaultAsync(v => v.IdVacacion == id);

            return vacacion
                ?? throw new KeyNotFoundException("Vacación no encontrada");
        }

        public async Task<IEnumerable<Vacaciones>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Vacaciones
                .Include(v => v.Empleado)
                .Include(v => v.JefeAprueba)
                .Where(v =>
                    (v.FechaInicio >= fechaInicio && v.FechaInicio <= fechaFin) ||
                    (v.FechaFin >= fechaInicio && v.FechaFin <= fechaFin) ||
                    (v.FechaInicio <= fechaInicio && v.FechaFin >= fechaFin)
                )
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();
        }

        public async Task<SaldoVacaciones?> ObtenerSaldoVacacionesAsync(int empleadoId, int anio)
        {
            return await _context.SaldoVacaciones
                .Include(s => s.Empleado)
                .FirstOrDefaultAsync(s =>
                    s.EmpleadoId == empleadoId &&
                    s.Anio == anio
                );
        }

        public async Task<IEnumerable<Vacaciones>> ObtenerTodosAsync()
        {
            return await _context.Vacaciones
                .Include(v => v.Empleado)
                .Include(v => v.JefeAprueba)
                .OrderByDescending(v => v.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<bool> TieneVacacionesEnRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Vacaciones
                .AnyAsync(v =>
                    v.EmpleadoId == empleadoId &&
                    v.EstadoSolicitud == "APROBADA" &&
                    (
                        (fechaInicio >= v.FechaInicio && fechaInicio <= v.FechaFin) ||
                        (fechaFin >= v.FechaInicio && fechaFin <= v.FechaFin) ||
                        (fechaInicio <= v.FechaInicio && fechaFin >= v.FechaFin)
                    )
                );
        }
    }
}