using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class AsistenciaManager : IAsistenciaManager
    {
        private readonly IAsistenciasRepository _asistenciasRepo;
        private readonly IEmpleadosRepository _empleadosRepo;

        public AsistenciaManager(
            IAsistenciasRepository asistenciasRepo,
            IEmpleadosRepository empleadosRepo)
        {
            _asistenciasRepo = asistenciasRepo;
            _empleadosRepo = empleadosRepo;
        }

        #region Métodos originales (Marcado de asistencia por empleado)

        public async Task<MarcarAsistenciaResponse> MarcarAsistenciaAsync(int empleadoId)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            var hoy = DateTime.Today;
            var ahora = DateTime.Now;
            var registro = await _asistenciasRepo.GetByEmpleadoYFechaAsync(empleadoId, hoy);

            if (registro == null)
            {
                var nuevoRegistro = new Asistencias
                {
                    EmpleadoId = empleadoId,
                    FechaRegistro = hoy,
                    HoraEntrada = ahora,
                    Estado = DeterminarEstado(ahora, null),
                    FechaCreacion = DateTime.UtcNow
                };

                await _asistenciasRepo.CreateAsync(nuevoRegistro);

                return new MarcarAsistenciaResponse
                {
                    Accion = "ENTRADA",
                    Hora = ahora,
                    HoraEntrada = ahora,
                    Estado = nuevoRegistro.Estado,
                    Mensaje = "Entrada registrada correctamente",
                    Exito = true
                };
            }
            else if (registro.HoraSalida == null)
            {
                registro.HoraSalida = ahora;
                registro.FechaModificacion = DateTime.UtcNow;
                await _asistenciasRepo.UpdateAsync(registro);

                return new MarcarAsistenciaResponse
                {
                    Accion = "SALIDA",
                    Hora = ahora,
                    HoraEntrada = registro.HoraEntrada,
                    HoraSalida = ahora,
                    Estado = registro.Estado,
                    Mensaje = "Salida registrada correctamente",
                    Exito = true
                };
            }
            else
            {
                return new MarcarAsistenciaResponse
                {
                    Accion = "NINGUNA",
                    Hora = ahora,
                    HoraEntrada = registro.HoraEntrada,
                    HoraSalida = registro.HoraSalida,
                    Estado = registro.Estado,
                    Mensaje = "Ya has registrado entrada y salida para hoy",
                    Exito = false
                };
            }
        }

        public async Task<EstadoAsistenciaDTO> ObtenerEstadoAsistenciaAsync(int empleadoId)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            var hoy = DateTime.Today;
            var registro = await _asistenciasRepo.GetByEmpleadoYFechaAsync(empleadoId, hoy);

            if (registro == null)
            {
                return new EstadoAsistenciaDTO
                {
                    TieneRegistro = false,
                    Estado = "SIN_REGISTRO",
                    PuedeMarcarEntrada = true,
                    PuedeMarcarSalida = false,
                    Mensaje = "No has registrado asistencia hoy"
                };
            }

            return new EstadoAsistenciaDTO
            {
                TieneRegistro = true,
                HoraEntrada = registro.HoraEntrada,
                HoraSalida = registro.HoraSalida,
                Estado = registro.Estado ?? "DESCONOCIDO",
                PuedeMarcarEntrada = false,
                PuedeMarcarSalida = registro.HoraSalida == null,
                Mensaje = registro.HoraSalida == null
                    ? "Puedes registrar tu salida"
                    : "Asistencia completa para hoy"
            };
        }

        #endregion Métodos originales (Marcado de asistencia por empleado)

        #region CRUD para Administrador

        public async Task<AsistenciaDTO> CreateAsync(CrearAsistenciaDTO dto)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(dto.EmpleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            var existe = await _asistenciasRepo.ExisteRegistroAsync(
                dto.EmpleadoId,
                dto.FechaRegistro.Date);

            if (existe)
            {
                throw new BusinessException(
                    "Ya existe un registro de asistencia para este empleado en esta fecha",
                    "REGISTRO_DUPLICADO");
            }

            var asistencia = new Asistencias
            {
                EmpleadoId = dto.EmpleadoId,
                FechaRegistro = dto.FechaRegistro.Date,
                HoraEntrada = dto.HoraEntrada,
                HoraSalida = dto.HoraSalida,
                Estado = dto.Estado,
                FechaCreacion = DateTime.UtcNow
            };

            await _asistenciasRepo.CreateAsync(asistencia);

            var registroCreado = await _asistenciasRepo.GetByIdAsync(asistencia.IdAsistencia);
            return MapToDTO(registroCreado!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existe = await _asistenciasRepo.ExistsAsync(id);
            if (!existe)
            {
                throw new BusinessException("Registro de asistencia no encontrado", "ASISTENCIA_NO_ENCONTRADA");
            }

            return await _asistenciasRepo.DeleteAsync(id);
        }

        public async Task<IEnumerable<AsistenciaDTO>> GetAllAsync()
        {
            var asistencias = await _asistenciasRepo.GetAllAsync();
            return asistencias.Select(MapToDTO);
        }

        public async Task<IEnumerable<AsistenciaDTO>> GetByFiltrosAsync(FiltrosAsistenciaDTO filtros)
        {
            var asistencias = await _asistenciasRepo.GetByFiltrosAsync(
                filtros.EmpleadoId,
                filtros.FechaInicio,
                filtros.FechaFin,
                filtros.Estado,
                filtros.DepartamentoId
            );

            return asistencias.Select(MapToDTO);
        }

        public async Task<AsistenciaDTO?> GetByIdAsync(int id)
        {
            var asistencia = await _asistenciasRepo.GetByIdAsync(id);
            return asistencia != null ? MapToDTO(asistencia) : null;
        }
        public async Task<ReporteAsistenciaDTO> GetReporteEmpleadoAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            var asistencias = await _asistenciasRepo.GetByFiltrosAsync(
                empleadoId, fechaInicio, fechaFin, null, null);

            var lista = asistencias.ToList();
            var totalDias = lista.Count;
            var presente = lista.Count(a => a.Estado == "PRESENTE");
            var ausente = lista.Count(a => a.Estado == "AUSENTE");
            var tardanza = lista.Count(a => a.Estado == "TARDANZA");
            var permiso = lista.Count(a => a.Estado == "PERMISO");

            return new ReporteAsistenciaDTO
            {
                EmpleadoId = empleadoId,
                NombreCompleto = $"{empleado.Nombre} {empleado.PrimerApellido}",
                Departamento = empleado.Departamento?.NombreDepartamento ?? "N/A",
                TotalDias = totalDias,
                DiasPresente = presente,
                DiasAusente = ausente,
                DiasTardanza = tardanza,
                DiasPermiso = permiso,
                PorcentajeAsistencia = totalDias > 0
                    ? Math.Round((decimal)presente / totalDias * 100, 2)
                    : 0
            };
        }

        public async Task<bool> UpdateAsync(int id, ActualizarAsistenciaDTO dto)
        {
            var asistencia = await _asistenciasRepo.GetByIdAsync(id);
            if (asistencia == null)
            {
                throw new BusinessException("Registro de asistencia no encontrado", "ASISTENCIA_NO_ENCONTRADA");
            }

            var empleado = await _empleadosRepo.GetByIdAsync(dto.EmpleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            if (asistencia.EmpleadoId != dto.EmpleadoId ||
                asistencia.FechaRegistro.Date != dto.FechaRegistro.Date)
            {
                var existe = await _asistenciasRepo.ExisteRegistroAsync(
                    dto.EmpleadoId,
                    dto.FechaRegistro.Date);

                if (existe)
                {
                    throw new BusinessException(
                        "Ya existe un registro de asistencia para este empleado en esta fecha",
                        "REGISTRO_DUPLICADO");
                }
            }

            asistencia.EmpleadoId = dto.EmpleadoId;
            asistencia.FechaRegistro = dto.FechaRegistro.Date;
            asistencia.HoraEntrada = dto.HoraEntrada;
            asistencia.HoraSalida = dto.HoraSalida;
            asistencia.Estado = dto.Estado;
            asistencia.FechaModificacion = DateTime.UtcNow;

            return await _asistenciasRepo.UpdateAsync(asistencia);
        }
        #endregion CRUD para Administrador

        #region Métodos privados

        private string DeterminarEstado(DateTime horaEntrada, DateTime? horaSalida)
        {
            var horaTrabajo = new TimeSpan(8, 0, 0);
            return horaEntrada.TimeOfDay > horaTrabajo
                ? EstadoAsistencia.TARDANZA.ToString()
                : EstadoAsistencia.PRESENTE.ToString();
        }

        private AsistenciaDTO MapToDTO(Asistencias asistencia)
        {
            if (asistencia.Empleado == null)
            {
                throw new BusinessException("Los datos del empleado no están cargados", "DATOS_INCOMPLETOS");
            }

            TimeSpan? horasTrabajadas = null;
            if (asistencia.HoraEntrada.HasValue && asistencia.HoraSalida.HasValue)
            {
                horasTrabajadas = asistencia.HoraSalida.Value - asistencia.HoraEntrada.Value;
            }

            return new AsistenciaDTO
            {
                IdAsistencia = asistencia.IdAsistencia,
                EmpleadoId = asistencia.EmpleadoId,
                NombreEmpleado = $"{asistencia.Empleado.Nombre} {asistencia.Empleado.PrimerApellido}",
                CodigoEmpleado = asistencia.Empleado.CodigoEmpleado,
                FechaRegistro = asistencia.FechaRegistro,
                HoraEntrada = asistencia.HoraEntrada,
                HoraSalida = asistencia.HoraSalida,
                Estado = asistencia.Estado ?? "DESCONOCIDO",
                HorasTrabajadas = horasTrabajadas
            };
        }

        #endregion
    }
}