using BusinessLogicLayer.Managers;

namespace PresentationLayer.Extensions
{
    public static class MyDependencies
    {
        public static IServiceCollection Dependencies(this IServiceCollection services)
        {
            // Puestos
            services.AddScoped<DataAccessLayer.Interfaces.IPuestosRepository,
                           DataAccessLayer.Repositories.PuestoRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IPuestosManager,
                                       BusinessLogicLayer.Managers.PuestoManager>();

            // Departamentos

            services.AddScoped<DataAccessLayer.Interfaces.IDepartamentosRepository,
                           DataAccessLayer.Repositories.DepartamentosRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IDepartamentosManager,
                                       BusinessLogicLayer.Managers.DepartamentoManager>();

            // Auth

            services.AddScoped<BusinessLogicLayer.Interfaces.IPasswordHasher,
                                       BusinessLogicLayer.Managers.PasswordHasher>();
            services.AddScoped<BusinessLogicLayer.Interfaces.IAuthManager,
                                       BusinessLogicLayer.Managers.AuthManager>();

            // Empleados

            services.AddScoped<DataAccessLayer.Interfaces.IEmpleadosRepository,
                           DataAccessLayer.Repositories.EmpleadosRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IEmpleadosManager,
                                       BusinessLogicLayer.Managers.EmpleadosManager>();

            // Usuarios

            services.AddScoped<DataAccessLayer.Interfaces.IUsuariosRepository,
                           DataAccessLayer.Repositories.UsuariosRepository>();
            services.AddScoped<BusinessLogicLayer.Interfaces.IUsuariosManager,
                                       BusinessLogicLayer.Managers.UsuariosManager>();

            // Roles

            services.AddScoped<DataAccessLayer.Interfaces.IRolesRepository,
                           DataAccessLayer.Repositories.RolesRepository>();
            services.AddScoped<DataAccessLayer.Interfaces.IUsuariosRolesRepository,
                           DataAccessLayer.Repositories.UsuariosRolesRepository>();

            // Asistencias

            services.AddScoped<DataAccessLayer.Interfaces.IAsistenciasRepository,
                           DataAccessLayer.Repositories.AsistenciasRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IAsistenciaManager,
                                       BusinessLogicLayer.Managers.AsistenciaManager>();

            // Horas Extra

            services.AddScoped<DataAccessLayer.Interfaces.IHorasExtrasRepository,
                           DataAccessLayer.Repositories.HorasExtrasRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IHorasExtrasManager,
                                       BusinessLogicLayer.Managers.HorasExtrasManager>();

            // Permisos

            services.AddScoped<DataAccessLayer.Interfaces.IPermisosRepository,
                           DataAccessLayer.Repositories.PermisosRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IPermisosManager,
                                       BusinessLogicLayer.Managers.PermisosManager>();

            // Incapacidades

            services.AddScoped<DataAccessLayer.Interfaces.IIncapacidadesRepository,
                           DataAccessLayer.Repositories.IncapacidadesRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IIncapacidadesManager,
                                       BusinessLogicLayer.Managers.IncapacidadesManager>();

            // Vacaciones

            services.AddScoped<DataAccessLayer.Interfaces.IVacacionesRepository,
                           DataAccessLayer.Repositories.VacacionesRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IVacacionesManager,
                                       BusinessLogicLayer.Managers.VacacionesManager>();

            // Email
            services.AddScoped<BusinessLogicLayer.Interfaces.IEmailService,
                                       BusinessLogicLayer.Services.EmailService>();

            services.AddScoped<NotificacionesManager>();

            // Nomina
            services.AddScoped<DataAccessLayer.Interfaces.INominaRepository,
                           DataAccessLayer.Repositories.NominaRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.INominaManager,
                                       BusinessLogicLayer.Managers.NominaManager>();

            // Aguinaldo
            services.AddScoped<DataAccessLayer.Interfaces.IAguinaldoRepository,
                           DataAccessLayer.Repositories.AguinaldoRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IAguinaldoManager,
                                       BusinessLogicLayer.Managers.AguinaldoManager>();

            // Roles
            services.AddScoped<BusinessLogicLayer.Interfaces.IRolesManager,
                                       BusinessLogicLayer.Managers.RolesManager>();

            // Liquidaciones
            services.AddScoped<DataAccessLayer.Interfaces.ILiquidacionesRepository,
                           DataAccessLayer.Repositories.LiquidacionesRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.ILiquidacionesManager,
                                       BusinessLogicLayer.Managers.LiquidacionesManager>();

            return services;
        }
    }
}