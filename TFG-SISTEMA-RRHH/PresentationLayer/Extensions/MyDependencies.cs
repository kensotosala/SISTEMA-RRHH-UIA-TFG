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

            // Empleados

            services.AddScoped<DataAccessLayer.Interfaces.IEmpleadosRepository,
                           DataAccessLayer.Repositories.EmpleadosRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IEmpleadosManager,
                                       BusinessLogicLayer.Managers.EmpleadosManager>();

            // Usuarios

            services.AddScoped<DataAccessLayer.Interfaces.IUsuarioRepository,
                           DataAccessLayer.Repositories.UsuarioRepository>();

            //services.AddScoped<BusinessLogicLayer.Interfaces.IUsuarioManager,
            //                           BusinessLogicLayer.Managers.UsuariosManager>();

            return services;
        }


    }
}