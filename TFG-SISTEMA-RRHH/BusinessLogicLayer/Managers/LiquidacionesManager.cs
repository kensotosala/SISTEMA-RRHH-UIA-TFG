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
        public LiquidacionesManager(ILiquidacionesRepository repo, IEmpleadosRepository repoEmpleados, NotificacionesManager notificacionesManager)
        {
            _repo = repo;
            _repoEmpleados = repoEmpleados;
            _notificacionesManager = notificacionesManager;
        }

        public async Task<ResultDTO<bool>> AnularLiquidacion(int idLiquidacion)
        {
            if (idLiquidacion <= 0)
                return ResultDTO<bool>.Failure("La liquidación no es válida.");

            var liquidacionExistente = await _repo.ObtenerLiquidacionPorId(idLiquidacion);
            if (liquidacionExistente == null)
                return ResultDTO<bool>.Failure("La liquidación no existe.");

            liquidacionExistente.Estado = "ANULADA";

            var detalles = $@"
                <p><strong>Fecha de Liquidación:</strong> {liquidacionExistente.FechaLiquidacion:dd/MM/yyyy}</p>
                <p><strong>La liquidación cuyo Id es {liquidacionExistente.IdLiquidacion} fue <strong>ANULADA</strong> correctamente</p>
            ";

            await _notificacionesManager.NotificarSolicitudAprobadaAsync(
                liquidacionExistente.EmpleadoId,
                "Liquidación",
                detalles
            );

            return await _repo.ModificarLiquidacion(liquidacionExistente)
                ? ResultDTO<bool>.Success(true, "Liquidación anulada exitosamente.")
                : ResultDTO<bool>.Failure("No se pudo anular la liquidación.");
        }

        public async Task<ResultadoAguinaldoProporcional> CalcularAguinaldoProporcional(int idEmpleado, DateOnly fechaSalida)
        {
            var nominas = await _repo.ObtenerNominasUltimos12Meses(idEmpleado);

            var aguinaldoProporcional = nominas.Any() ? nominas.Average(n => n.SalarioBase) / 12m : 0;
            return new ResultadoAguinaldoProporcional
            {
                MontoAguinaldoProporcional = aguinaldoProporcional
            };
        }

        public async Task<ResultadoAuxilioCesantia> CalcularAuxilioCesantia(int idEmpleado, DateOnly fechaSalida)
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
            var anosLaborados = (int)(tiempoLaborado.TotalDays / 365);

            var salarioPromedio = await CalcularSalarioPromedio(idEmpleado);

            decimal montoAuxilioCesantia = 0;

            if (mesesLaborados <= 6 && mesesLaborados >= 3)
            {
                montoAuxilioCesantia = salarioPromedio * 0.5m;
            }
            else if (mesesLaborados <= 12)
            {
                montoAuxilioCesantia = salarioPromedio * 1m;
            }
            else
            {
                montoAuxilioCesantia = salarioPromedio * anosLaborados;
            }

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

        public async Task<Liquidaciones> CrearLiquidacion(int idEmpleado, DateOnly fechaSalida, string motivo, bool preavisoEntregado = true)
        {
            var empleado = await _repo.ObtenerEmpleadoPorId(idEmpleado);

            if (empleado is null)
                throw new ArgumentException("Empleado no encontrado.", nameof(idEmpleado));

            // 1️. Calcular conceptos generales
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
                    {
                        deduccionPreaviso = preaviso.MontoPreaviso;
                    }
                    else
                    {
                        indemnizacion = 0m;
                    }
                    break;

                case "RENUNCIA_RESPONSABILIDAD_PATRONAL":
                    indemnizacion = 0m;
                    if (preavisoEntregado) indemnizacion += preaviso.MontoPreaviso;
                    break;

                case "DESPIDO_RESPONSABILIDAD_PATRONAL":
                    indemnizacion = cesantia.MontoAuxilioCesantia + preaviso.MontoPreaviso;
                    break;

                case "DESPIDO_SIN_RESPONSABILIDAD":
                    indemnizacion = preaviso.MontoPreaviso;
                    break;

                case "JUBILACION":
                    indemnizacion = 0m;
                    break;
            }

            // 2️. Crear entidad Liquidaciones
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

            // 3. Inactivar empleado si es despido
            if (motivo.StartsWith("DESPIDO"))
            {
                empleado.Estado = "Inactivo";
                await _repoEmpleados.UpdateAsync(empleado);
            }

            return liquidacion;
        }

        public async Task<ResultDTO<IEnumerable<LiquidacionDTO>>> ListarLiquidaciones()
        {
            try
            {
                var listaLiquidaciones = await _repo.ListarLiquidaciones();

                if (listaLiquidaciones == null || !listaLiquidaciones.Any())
                    return ResultDTO<IEnumerable<LiquidacionDTO>>.Failure("No se encontraron liquidaciones.");

                var listaDTO = new List<LiquidacionDTO>();

                foreach (var l in listaLiquidaciones)
                {
                    var fechaSalida = DateOnly.FromDateTime(l.FechaLiquidacion);

                    var preaviso = await CalcularPreaviso(l.EmpleadoId, fechaSalida);
                    var vacaciones = await CalcularVacacionesProporcionales(l.EmpleadoId, fechaSalida);
                    var aguinaldo = await CalcularAguinaldoProporcional(l.EmpleadoId, fechaSalida);
                    var cesantia = await CalcularAuxilioCesantia(l.EmpleadoId, fechaSalida);

                    listaDTO.Add(new LiquidacionDTO
                    {
                        IdLiquidacion = l.IdLiquidacion,
                        IdEmpleado = l.EmpleadoId,
                        MontoPreaviso = preaviso.MontoPreaviso,
                        MontoVacaciones = vacaciones.MontoVacacionesProporcionales,
                        MontoAguinaldo = aguinaldo.MontoAguinaldoProporcional,
                        MontoCesantia = cesantia.MontoAuxilioCesantia
                    });
                }

                return ResultDTO<IEnumerable<LiquidacionDTO>>.Success(listaDTO, "Liquidaciones obtenidas exitosamente.");
            }
            catch (Exception ex)
            {
                return ResultDTO<IEnumerable<LiquidacionDTO>>.Failure($"Ocurrió un error al obtener las liquidaciones: {ex.Message}");
            }
        }

        public async Task<ResultDTO<bool>> ModificarLiquidacion(Liquidaciones liquidacion)
        {
            if (liquidacion is null) return ResultDTO<bool>.Failure("La liquidación no es válida.");

            if (liquidacion.Estado != "CAlCULADA")
            {
                return ResultDTO<bool>.Failure(
                    "Solo se pueden modificar liquidaciones CALCULADAS"
                );
            }

            var liquidacionExistente = await ObtenerLiquidacionPorId(liquidacion.IdLiquidacion);

            if (liquidacionExistente is null) return ResultDTO<bool>.Failure("Liquidación no encontrada.");

            liquidacionExistente.FechaLiquidacion = liquidacion.FechaLiquidacion;
            liquidacionExistente.MotivoLiquidacion = liquidacion.MotivoLiquidacion;
            liquidacionExistente.SalarioBase = liquidacion.SalarioBase;
            liquidacionExistente.MotivoLiquidacion = liquidacion.MotivoLiquidacion;
            liquidacionExistente.VacacionesPendientes = liquidacion.VacacionesPendientes;
            liquidacionExistente.AguinaldoProporcional = liquidacion.AguinaldoProporcional;
            liquidacionExistente.Indemnizacion = liquidacion.Indemnizacion;
            liquidacionExistente.OtrosConceptos = liquidacion.OtrosConceptos;
            liquidacionExistente.TotalLiquidacion = liquidacion.TotalLiquidacion;
            liquidacionExistente.Estado = liquidacion.Estado;
            liquidacionExistente.FechaModificacion = DateTime.Now;

            var resultado = await _repo.ModificarLiquidacion(liquidacionExistente);

            var detalles = $@"
                <p><strong>Fecha de Liquidación:</strong> {liquidacionExistente.FechaLiquidacion:dd/MM/yyyy}</p>
                <p><strong>Motivo:</strong> {liquidacionExistente.MotivoLiquidacion}</p>
                <p><strong>Indemnizacion:</strong> {liquidacionExistente.Indemnizacion} día(s)</p>
                <p><strong>OtrosConceptos:</strong> {liquidacionExistente.OtrosConceptos}</p>
                <p><strong>TotalLiquidacion:</strong> {liquidacionExistente.TotalLiquidacion}</p>
            ";

            await _notificacionesManager.NotificarSolicitudAprobadaAsync(
                liquidacionExistente.EmpleadoId,
                "Liquidación",
                detalles
            );

            return resultado
                ? ResultDTO<bool>.Success(true, "Liquidacion actualizada exitosamente")
                : ResultDTO<bool>.Failure("No se pudo actualizar");
        }

        public async Task<Liquidaciones?> ObtenerLiquidacionPorId(int idLiquidacion)
        {
            if (idLiquidacion == 0) return null;

            return await _repo.ObtenerLiquidacionPorId(idLiquidacion);
        }
    }
}