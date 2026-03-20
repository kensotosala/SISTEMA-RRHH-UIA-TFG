using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class LiquidacionesManager : ILiquidacionesManager
    {
        private readonly NotificacionesManager _notificacionesManager;
        private readonly ILiquidacionesRepository _repo;
        private readonly IEmpleadosRepository _repoEmpleados;
        private readonly IAuditoriaService _auditoria;

        public LiquidacionesManager(ILiquidacionesRepository repo, IEmpleadosRepository repoEmpleados, NotificacionesManager notificacionesManager, IAuditoriaService auditoria)
        {
            _repo = repo;
            _repoEmpleados = repoEmpleados;
            _notificacionesManager = notificacionesManager;
            _auditoria = auditoria;
        }

        public async Task<ResultDTO<bool>> AnularLiquidacion(int idLiquidacion)
        {
            if (idLiquidacion <= 0)
                return ResultDTO<bool>.Failure("La liquidación no es válida.");

            var liquidacion = await _repo.ObtenerLiquidacionPorId(idLiquidacion);
            if (liquidacion == null)
                return ResultDTO<bool>.Failure("La liquidación no existe.");

            liquidacion.Estado = "ANULADA";

            var resultado = await _repo.ModificarLiquidacion(liquidacion);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "liquidaciones",
                    descripcion: $"Liquidación ID {idLiquidacion} anulada. " +
                                   $"Empleado ID {liquidacion.EmpleadoId}, " +
                                   $"fecha liquidación: {liquidacion.FechaLiquidacion:dd/MM/yyyy}."
                );

            await _notificacionesManager.NotificarSolicitudAprobadaAsync(
                liquidacion.EmpleadoId, "Liquidación",
                $@"<p><strong>Fecha de Liquidación:</strong> {liquidacion.FechaLiquidacion:dd/MM/yyyy}</p>
                   <p>La liquidación con Id {liquidacion.IdLiquidacion} fue <strong>ANULADA</strong> correctamente.</p>"
            );

            return resultado
                ? ResultDTO<bool>.Success(true, "Liquidación anulada exitosamente.")
                : ResultDTO<bool>.Failure("No se pudo anular la liquidación.");
        }

        public async Task<ResultadoAguinaldoProporcional> CalcularAguinaldoProporcional(
            int idEmpleado, DateOnly fechaSalida)
        {
            var nominas = await _repo.ObtenerNominasUltimos12Meses(idEmpleado);

            var aguinaldoProporcional = nominas.Any()
                ? nominas.Sum(n => n.SalarioBase) / 12m
                : 0m;

            return new ResultadoAguinaldoProporcional
            {
                MontoAguinaldoProporcional = aguinaldoProporcional
            };
        }

        public async Task<ResultadoAuxilioCesantia> CalcularAuxilioCesantia(
            int idEmpleado, DateOnly fechaSalida)
        {
            if (idEmpleado <= 0)
                throw new ArgumentException("El ID del empleado no puede ser cero o negativo.");

            if (fechaSalida > DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("La fecha de salida no puede ser futura.");

            var empleado = await _repo.ObtenerEmpleadoPorId(idEmpleado);
            if (empleado is null)
                throw new ArgumentException("Empleado no encontrado.", nameof(idEmpleado));

            var tiempoLaborado = fechaSalida.ToDateTime(TimeOnly.MinValue)
                               - empleado.FechaContratacion.ToDateTime(TimeOnly.MinValue);

            var mesesLaborados = (int)(tiempoLaborado.TotalDays / 30);
            var anosLaborados = tiempoLaborado.TotalDays / 365.25;

            var salarioPromedio = await CalcularSalarioPromedio(idEmpleado);
            var salarioDiario = salarioPromedio / 30m;

            decimal diasCesantia = 0m;

            // Tabla Art. 29 Código de Trabajo — Costa Rica
            if (mesesLaborados < 3)
            {
                diasCesantia = 0m;
            }
            else if (mesesLaborados < 6)   // 3 a < 6 meses
            {
                diasCesantia = 7m;
            }
            else if (mesesLaborados < 12)  // 6 a < 12 meses
            {
                diasCesantia = 14m;
            }
            else if (anosLaborados < 2)    // 1 a < 2 años
            {
                diasCesantia = 19.5m;
            }
            else if (anosLaborados < 3)
            {
                diasCesantia = 20m;
            }
            else if (anosLaborados < 4)
            {
                diasCesantia = 20.5m;
            }
            else if (anosLaborados < 5)
            {
                diasCesantia = 21m;
            }
            else if (anosLaborados < 6)
            {
                diasCesantia = 21.24m;
            }
            else if (anosLaborados < 7)
            {
                diasCesantia = 21.5m;
            }
            else if (anosLaborados < 8)
            {
                diasCesantia = 22m;
            }
            else if (anosLaborados < 9)
            {
                diasCesantia = 22.5m;
            }
            else if (anosLaborados < 10)
            {
                diasCesantia = 23m;
            }
            else
            {
                diasCesantia = Math.Min((decimal)anosLaborados, 8m) * 30m;
            }

            var montoAuxilioCesantia = salarioDiario * diasCesantia;

            return new ResultadoAuxilioCesantia
            {
                MesesLaborados = mesesLaborados,
                MontoAuxilioCesantia = montoAuxilioCesantia
            };
        }

        public async Task<LiquidacionDTO> CalcularLiquidacion(int idEmpleado, DateOnly fechaSalida)
        {
            if (idEmpleado <= 0)
                throw new ArgumentException("El ID del empleado no puede ser cero o negativo.");

            if (fechaSalida > DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("La fecha de salida no puede ser futura.");

            var empleado = await _repo.ObtenerEmpleadoPorId(idEmpleado);
            if (empleado is null)
                throw new ArgumentException("Empleado no encontrado.", nameof(idEmpleado));

            var preaviso = await CalcularPreaviso(idEmpleado, fechaSalida);
            var vacaciones = await CalcularVacacionesProporcionales(idEmpleado, fechaSalida);
            var aguinaldo = await CalcularAguinaldoProporcional(idEmpleado, fechaSalida);
            var cesantia = await CalcularAuxilioCesantia(idEmpleado, fechaSalida);

            var liquidacion = new LiquidacionDTO
            {
                IdEmpleado = idEmpleado,
                MontoPreaviso = preaviso.MontoPreaviso,
                MontoVacaciones = vacaciones.MontoVacacionesProporcionales,
                MontoAguinaldo = aguinaldo.MontoAguinaldoProporcional,
                MontoCesantia = cesantia.MontoAuxilioCesantia
            };

            return liquidacion;
        }

        public async Task<ResultadoPreaviso> CalcularPreaviso(int idEmpleado, DateOnly fechaSalida)
        {
            if (idEmpleado == 0)
            {
                throw new ArgumentException("El ID del empleado no puede ser cero.");
            }

            if (fechaSalida > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("La fecha de salida no puede ser futura.");
            }

            if (fechaSalida < DateOnly.FromDateTime(DateTime.Now).AddYears(-1))
            {
                throw new ArgumentException("La fecha de salida no puede ser anterior a un año.");
            }

            var empleado = await _repo.ObtenerEmpleadoPorId(idEmpleado);

            if (empleado is null)
            {
                throw new ArgumentException("Empleado no encontrado.", nameof(idEmpleado));
            }

            var tiempoLaborado = fechaSalida.ToDateTime(TimeOnly.MinValue) -
                                 empleado.FechaContratacion.ToDateTime(TimeOnly.MinValue);

            var mesesLaborados = (int)(tiempoLaborado.TotalDays / 30);

            int diasPreaviso = 0;

            if (mesesLaborados < 6)
            {
                diasPreaviso = 7;
            }
            else if (mesesLaborados < 12)
            {
                diasPreaviso = 14;
            }
            else
            {
                diasPreaviso = 30;
            }

            var salarioPromedio = await CalcularSalarioPromedio(idEmpleado);
            var montoPreaviso = (salarioPromedio / 30) * diasPreaviso;

            return new ResultadoPreaviso
            {
                DiasPreaviso = diasPreaviso,
                MontoPreaviso = montoPreaviso
            };
        }

        public async Task<decimal> CalcularSalarioPromedio(int idEmpleado)
        {
            var nominas = await _repo.ObtenerNominasUltimos6Meses(idEmpleado);

            return nominas.Any() ? nominas.Average(n => n.SalarioBase) : 0;
        }

        public async Task<ResultadoVacacionesProporcionales> CalcularVacacionesProporcionales(int idEmpleado, DateOnly fechaSalida)
        {
            var empleado = await _repo.ObtenerEmpleadoPorId(idEmpleado);
            if (empleado is null)
                throw new ArgumentException("Empleado no encontrado.", nameof(idEmpleado));

            var tiempoLaborado = fechaSalida.ToDateTime(TimeOnly.MinValue) - empleado.FechaContratacion.ToDateTime(TimeOnly.MinValue);
            var mesesLaborados = (int)Math.Floor(tiempoLaborado.TotalDays / 30.0);
            decimal diasVacacionesProporcionalesDecimal = (mesesLaborados / 12m) * 15;
            int diasVacacionesProporcionales = (int)Math.Round(diasVacacionesProporcionalesDecimal, MidpointRounding.AwayFromZero);

            var diasVacacionesAcumuladas = empleado.VacacionesEmpleado
                .Where(v => v.EstadoSolicitud == "Aprobada" && v.FechaFin < DateTime.Today)
                .Sum(v => (v.FechaFin - v.FechaInicio).Days + 1);

            int totalDiasVacaciones = diasVacacionesProporcionales + diasVacacionesAcumuladas;

            var salarioPromedio = await CalcularSalarioPromedio(idEmpleado);
            var salarioDiario = salarioPromedio / 30m;
            var montoVacaciones = salarioDiario * totalDiasVacaciones;

            return new ResultadoVacacionesProporcionales
            {
                DiasVacacionesProporcionales = totalDiasVacaciones,
                MontoVacacionesProporcionales = montoVacaciones
            };
        }

        public async Task<Liquidaciones> CrearLiquidacion(
            int idEmpleado, DateOnly fechaSalida, string motivo, bool preavisoEntregado = true)
        {
            var empleado = await _repo.ObtenerEmpleadoPorId(idEmpleado);
            if (empleado is null)
                throw new ArgumentException("Empleado no encontrado.", nameof(idEmpleado));

            var vacaciones = await CalcularVacacionesProporcionales(idEmpleado, fechaSalida);
            var aguinaldo = await CalcularAguinaldoProporcional(idEmpleado, fechaSalida);
            var preaviso = await CalcularPreaviso(idEmpleado, fechaSalida);
            var cesantia = await CalcularAuxilioCesantia(idEmpleado, fechaSalida);

            decimal indemnizacion = 0m;
            decimal deduccionPreaviso = 0m;

            switch (motivo)
            {
                case "RENUNCIA":
                    if (!preavisoEntregado)
                        deduccionPreaviso = preaviso.MontoPreaviso;
                    break;

                case "RENUNCIA_RESPONSABILIDAD_PATRONAL":
                case "DESPIDO_RESPONSABILIDAD_PATRONAL":
                    indemnizacion = cesantia.MontoAuxilioCesantia + preaviso.MontoPreaviso;
                    break;

                case "DESPIDO_SIN_RESPONSABILIDAD":
                case "JUBILACION":
                    indemnizacion = 0m;
                    break;

                default:
                    throw new ArgumentException($"Motivo de liquidación no reconocido: {motivo}");
            }

            var liquidacion = new Liquidaciones
            {
                EmpleadoId = idEmpleado,
                FechaLiquidacion = DateTime.Now,
                MotivoLiquidacion = motivo,
                SalarioBase = await CalcularSalarioPromedio(idEmpleado),
                VacacionesPendientes = vacaciones.MontoVacacionesProporcionales,
                AguinaldoProporcional = aguinaldo.MontoAguinaldoProporcional,
                Indemnizacion = cesantia.MontoAuxilioCesantia,
                OtrosConceptos = deduccionPreaviso > 0 ? -deduccionPreaviso : 0m,
                TotalLiquidacion = vacaciones.MontoVacacionesProporcionales
                                      + aguinaldo.MontoAguinaldoProporcional
                                      + indemnizacion
                                      - deduccionPreaviso,
                Estado = "CALCULADA",
                FechaCreacion = DateTime.Now
            };

            await _repo.CrearLiquidacion(liquidacion);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "liquidaciones",
                descripcion: $"Liquidación creada (ID {liquidacion.IdLiquidacion}) " +
                               $"para empleado ID {idEmpleado}, " +
                               $"motivo: {motivo}, " +
                               $"total: {liquidacion.TotalLiquidacion:N2}."
            );

            if (motivo.StartsWith("DESPIDO"))
            {
                empleado.Estado = "Inactivo";
                await _repoEmpleados.UpdateAsync(empleado);

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "empleados",
                    descripcion: $"Empleado ID {idEmpleado} inactivado por liquidación " +
                                   $"(ID {liquidacion.IdLiquidacion}), motivo: {motivo}."
                );
            }

            return liquidacion;
        }

        public async Task<ResultDTO<IEnumerable<LiquidacionDTO>>> ListarLiquidaciones()
        {
            try
            {
                var listaLiquidaciones = await _repo.ListarLiquidaciones();

                if (listaLiquidaciones == null || !listaLiquidaciones.Any())
                    return ResultDTO<IEnumerable<LiquidacionDTO>>
                        .Failure("No se encontraron liquidaciones.");

                var listaDTO = listaLiquidaciones.Select(l => new LiquidacionDTO
                {
                    IdLiquidacion = l.IdLiquidacion,
                    IdEmpleado = l.EmpleadoId,
                    MontoPreaviso = l.MontoPreaviso ?? 0m,
                    MontoVacaciones = l.VacacionesPendientes ?? 0m,
                    MontoAguinaldo = l.AguinaldoProporcional ?? 0m,
                    MontoCesantia = l.Indemnizacion ?? 0m,
                    FechaLiquidacion = l.FechaLiquidacion,
                    Estado = l.Estado
                }).ToList();

                return ResultDTO<IEnumerable<LiquidacionDTO>>
                    .Success(listaDTO, "Liquidaciones obtenidas exitosamente.");
            }
            catch (Exception ex)
            {
                return ResultDTO<IEnumerable<LiquidacionDTO>>
                    .Failure($"Error al obtener las liquidaciones: {ex.Message}");
            }
        }

        public async Task<ResultDTO<IEnumerable<LiquidacionDTO>>> ListarLiquidacionesPorEmpleado(int idEmpleado)
        {
            try
            {
                var listaLiquidaciones = await _repo.ListarLiquidacionesPorEmpleado(idEmpleado);

                if (listaLiquidaciones == null || !listaLiquidaciones.Any())
                    return ResultDTO<IEnumerable<LiquidacionDTO>>
                        .Failure("No se encontraron liquidaciones.");

                var listaDTO = listaLiquidaciones.Select(l => new LiquidacionDTO
                {
                    IdLiquidacion = l.IdLiquidacion,
                    IdEmpleado = l.EmpleadoId,
                    MontoPreaviso = l.MontoPreaviso ?? 0m,
                    MontoVacaciones = l.VacacionesPendientes ?? 0m,
                    MontoAguinaldo = l.AguinaldoProporcional ?? 0m,
                    MontoCesantia = l.Indemnizacion ?? 0m,
                    Estado = l.Estado,
                    FechaLiquidacion = l.FechaLiquidacion
                }).ToList();

                return ResultDTO<IEnumerable<LiquidacionDTO>>
                    .Success(listaDTO, "Liquidaciones obtenidas exitosamente.");
            }
            catch (Exception ex)
            {
                return ResultDTO<IEnumerable<LiquidacionDTO>>
                    .Failure($"Error al obtener las liquidaciones: {ex.Message}");
            }
        }

        public async Task<ResultDTO<bool>> ModificarLiquidacion(Liquidaciones liquidacion)
        {
            if (liquidacion is null)
                return ResultDTO<bool>.Failure("La liquidación no es válida.");

            if (liquidacion.Estado != "CALCULADA")
                return ResultDTO<bool>.Failure("Solo se pueden modificar liquidaciones CALCULADAS");

            liquidacion.FechaModificacion = DateTime.Now;

            var resultado = await _repo.ModificarLiquidacion(liquidacion);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "liquidaciones",
                    descripcion: $"Liquidación ID {liquidacion.IdLiquidacion} modificada. " +
                                   $"Empleado ID {liquidacion.EmpleadoId}, " +
                                   $"motivo: {liquidacion.MotivoLiquidacion}, " +
                                   $"total: {liquidacion.TotalLiquidacion:N2}."
                );

            await _notificacionesManager.NotificarSolicitudAprobadaAsync(
                liquidacion.EmpleadoId, "Liquidación",
                $@"<p><strong>Fecha de Liquidación:</strong> {liquidacion.FechaLiquidacion:dd/MM/yyyy}</p>
                   <p><strong>Motivo:</strong> {liquidacion.MotivoLiquidacion}</p>
                   <p><strong>Indemnización:</strong> {liquidacion.Indemnizacion}</p>
                   <p><strong>Otros Conceptos:</strong> {liquidacion.OtrosConceptos}</p>
                   <p><strong>Total Liquidación:</strong> {liquidacion.TotalLiquidacion}</p>"
            );

            return resultado
                ? ResultDTO<bool>.Success(true, "Liquidación actualizada exitosamente")
                : ResultDTO<bool>.Failure("No se pudo actualizar");
        }

        public async Task<Liquidaciones?> ObtenerLiquidacionPorId(int idLiquidacion)
        {
            if (idLiquidacion <= 0) return null;   // consistente con el resto
            return await _repo.ObtenerLiquidacionPorId(idLiquidacion);
        }
    }
}