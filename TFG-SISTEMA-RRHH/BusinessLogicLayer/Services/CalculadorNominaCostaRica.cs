using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Services
{

    public class CalculadorNominaCostaRica
    {
        // CONSTANTES CCSS 2026
        private const decimal CCSS_TOTAL_EMPLEADO = 0.1667m;    // 16.67%

        private const decimal CCSS_SEM_EMPLEADO = 0.1050m;
        private const decimal CCSS_IVM_EMPLEADO = 0.0417m;
        private const decimal CCSS_BANCO_POPULAR = 0.0100m;
        private const decimal CCSS_ANP_EMPLEADO = 0.0100m;

        private const decimal RECARGO_HORA_EXTRA_NORMAL = 1.5m;
        private const decimal RECARGO_HORA_EXTRA_FERIADO = 2.0m;
        private const decimal HORAS_MENSUALES = 240m;

        // TABLA IMPUESTO SOBRE LA RENTA 2026
        private readonly List<TramoImpuestoRenta> _tramosImpuesto = new()
        {
            new TramoImpuestoRenta { Desde = 0, Hasta = 941000, Tarifa = 0.00m, ImpuestoBase = 0 },
            new TramoImpuestoRenta { Desde = 941001, Hasta = 1381000, Tarifa = 0.10m, ImpuestoBase = 0 },
            new TramoImpuestoRenta { Desde = 1381001, Hasta = 2423000, Tarifa = 0.15m, ImpuestoBase = 44000 },
            new TramoImpuestoRenta { Desde = 2423001, Hasta = 4845000, Tarifa = 0.20m, ImpuestoBase = 200300 },
            new TramoImpuestoRenta { Desde = 4845001, Hasta = decimal.MaxValue, Tarifa = 0.25m, ImpuestoBase = 684700 }
        };

        public DetalleNominaDTO CalcularNominaQuincenal(
            Empleados empleado,
            int quincena,
            int mes,
            int anio,
            List<HorasExtras>? horasExtras = null,
            List<Incapacidades>? incapacidades = null,
            List<Permisos>? permisos = null)
        {
            var detalle = new DetalleNominaDTO
            {
                EmpleadoId = empleado.IdEmpleado,
                CodigoEmpleado = empleado.CodigoEmpleado,
                NombreCompleto = $"{empleado.Nombre} {empleado.PrimerApellido} {empleado.SegundoApellido}".Trim(),
                Puesto = empleado.Puesto?.NombrePuesto ?? "",
                Departamento = empleado.Departamento?.NombreDepartamento ?? ""
            };

            // 1. SALARIO BASE QUINCENAL
            detalle.SalarioBaseQuincenal = empleado.SalarioBase / 2;

            // 2. HORAS EXTRA
            var (totalHorasExtra, diurnas, nocturnas, feriados) = CalcularHorasExtra(
                empleado.SalarioBase, horasExtras, quincena, mes, anio);

            detalle.HorasExtraDiurnas = diurnas;
            detalle.HorasExtraNocturnas = nocturnas;
            detalle.HorasExtraFeriados = feriados;
            detalle.TotalHorasExtra = totalHorasExtra;

            // 3. BONIFICACIONES
            detalle.Bonificaciones = 0;

            // 4. TOTAL BRUTO
            detalle.TotalBruto = detalle.SalarioBaseQuincenal + detalle.TotalHorasExtra + detalle.Bonificaciones;

            // 5. AJUSTES POR AUSENCIAS
            detalle.AjustesAusencias = CalcularAjustesAusencias(
                empleado.SalarioBase, quincena, mes, anio, incapacidades, permisos);

            // Ajustar total bruto
            detalle.TotalBruto -= detalle.AjustesAusencias.TotalAjustes;

            // 6. DEDUCCIONES CCSS
            detalle.DeduccionesCCSS = CalcularDeduccionesCCSS(detalle.TotalBruto);
            detalle.TotalCCSS = detalle.DeduccionesCCSS.Total;

            // 7. IMPUESTO SOBRE LA RENTA
            var baseImponible = detalle.TotalBruto - detalle.TotalCCSS;
            detalle.ImpuestoRenta = CalcularImpuestoRenta(baseImponible);

            // 8. OTRAS DEDUCCIONES
            detalle.PensionAlimenticia = 0;
            detalle.Prestamos = 0;
            detalle.Embargos = 0;
            detalle.OtrasDeducciones = 0;

            // 9. TOTAL DEDUCCIONES
            detalle.TotalDeducciones = detalle.TotalCCSS +
                                      detalle.ImpuestoRenta.ImpuestoQuincenal +
                                      detalle.PensionAlimenticia +
                                      detalle.Prestamos +
                                      detalle.Embargos +
                                      detalle.OtrasDeducciones;

            // 10. TOTAL NETO
            detalle.TotalNeto = detalle.TotalBruto - detalle.TotalDeducciones;

            return detalle;
        }

        private DeduccionesCCSSDTO CalcularDeduccionesCCSS(decimal salarioBruto)
        {
            return new DeduccionesCCSSDTO
            {
                SEM = Math.Round(salarioBruto * CCSS_SEM_EMPLEADO, 2),
                IVM = Math.Round(salarioBruto * CCSS_IVM_EMPLEADO, 2),
                BancoPopular = Math.Round(salarioBruto * CCSS_BANCO_POPULAR, 2),
                ANP = Math.Round(salarioBruto * CCSS_ANP_EMPLEADO, 2),
                Total = Math.Round(salarioBruto * CCSS_TOTAL_EMPLEADO, 2)
            };
        }

        private ImpuestoRentaDTO CalcularImpuestoRenta(decimal baseImponibleQuincenal)
        {
            var proyeccionMensual = baseImponibleQuincenal * 2;
            var tramo = _tramosImpuesto.FirstOrDefault(t =>
                proyeccionMensual >= t.Desde && proyeccionMensual <= t.Hasta
            ) ?? _tramosImpuesto.Last();

            decimal impuestoMensual = 0;
            if (tramo.Tarifa > 0)
            {
                var excedente = proyeccionMensual - tramo.Desde + 1;
                impuestoMensual = tramo.ImpuestoBase + (excedente * tramo.Tarifa);
            }

            var impuestoQuincenal = Math.Round(impuestoMensual / 2, 2);

            return new ImpuestoRentaDTO
            {
                BaseImponible = baseImponibleQuincenal,
                ProyeccionMensual = proyeccionMensual,
                ImpuestoMensual = Math.Round(impuestoMensual, 2),
                ImpuestoQuincenal = impuestoQuincenal,
                TramoAplicado = $"₡{tramo.Desde:N0} - ₡{tramo.Hasta:N0} ({tramo.Tarifa * 100}%)"
            };
        }

        private (decimal total, decimal diurnas, decimal nocturnas, decimal feriados) CalcularHorasExtra(
            decimal salarioMensual, List<HorasExtras>? horasExtras, int quincena, int mes, int anio)
        {
            if (horasExtras == null || !horasExtras.Any())
                return (0, 0, 0, 0);

            var salarioPorHora = salarioMensual / HORAS_MENSUALES;
            var fechaInicio = new DateTime(anio, mes, quincena == 1 ? 1 : 16);
            var fechaFin = quincena == 1
                ? new DateTime(anio, mes, 15)
                : new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            var horasQuincena = horasExtras.Where(he =>
                he.EstadoSolicitud == "APROBADA" &&
                he.FechaInicio.Date >= fechaInicio &&
                he.FechaFin.Date <= fechaFin
            ).ToList();

            decimal montoDiurnas = 0, montoNocturnas = 0, montoFeriados = 0;

            foreach (var he in horasQuincena)
            {
                var horas = (decimal)(he.FechaFin - he.FechaInicio).TotalHours;
                switch (he.TipoHoraExtra?.ToUpper())
                {
                    case "DIURNA":
                        montoDiurnas += horas * salarioPorHora * RECARGO_HORA_EXTRA_NORMAL;
                        break;

                    case "NOCTURNA":
                        montoNocturnas += horas * salarioPorHora * RECARGO_HORA_EXTRA_NORMAL;
                        break;

                    case "FERIADO":
                    case "DOMINGO":
                        montoFeriados += horas * salarioPorHora * RECARGO_HORA_EXTRA_FERIADO;
                        break;

                    default:
                        montoDiurnas += horas * salarioPorHora * RECARGO_HORA_EXTRA_NORMAL;
                        break;
                }
            }

            return (
                Math.Round(montoDiurnas + montoNocturnas + montoFeriados, 2),
                Math.Round(montoDiurnas, 2),
                Math.Round(montoNocturnas, 2),
                Math.Round(montoFeriados, 2)
            );
        }

        private AjustesAusenciasDTO CalcularAjustesAusencias(
            decimal salarioMensual, int quincena, int mes, int anio,
            List<Incapacidades>? incapacidades, List<Permisos>? permisos)
        {
            var ajustes = new AjustesAusenciasDTO();
            var salarioDiario = salarioMensual / 30;

            var fechaInicio = new DateTime(anio, mes, quincena == 1 ? 1 : 16);
            var fechaFin = quincena == 1
                ? new DateTime(anio, mes, 15)
                : new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            // INCAPACIDADES
            if (incapacidades != null)
            {
                foreach (var inc in incapacidades.Where(i => i.Estado == "ACTIVA"))
                {
                    var inicio = inc.FechaInicio > fechaInicio ? inc.FechaInicio : fechaInicio;
                    var fin = inc.FechaFin < fechaFin ? inc.FechaFin : fechaFin;

                    if (inicio <= fechaFin && fin >= fechaInicio)
                    {
                        var dias = (fin - inicio).Days + 1;
                        ajustes.DiasIncapacidad += dias;
                        // Días 1-3: 50%, día 4+: 60% (paga CCSS)
                        ajustes.MontoIncapacidad += dias * salarioDiario * 0.5m;
                    }
                }
            }

            // PERMISOS SIN GOCE
            if (permisos != null)
            {
                foreach (var per in permisos.Where(p =>
                    p.EstadoSolicitud == "APROBADA" && p.ConGoceSalario == false))
                {
                    if (per.FechaPermiso >= fechaInicio && per.FechaPermiso <= fechaFin)
                    {
                        ajustes.DiasPermisoSinGoce++;
                        ajustes.MontoPermisoSinGoce += salarioDiario;
                    }
                }
            }

            ajustes.TotalAjustes = ajustes.MontoIncapacidad + ajustes.MontoPermisoSinGoce;
            return ajustes;
        }
    }

    internal class TramoImpuestoRenta
    {
        public decimal Desde { get; set; }
        public decimal Hasta { get; set; }
        public decimal Tarifa { get; set; }
        public decimal ImpuestoBase { get; set; }
    }
}