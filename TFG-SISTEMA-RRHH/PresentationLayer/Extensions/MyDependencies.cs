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

            services.AddScoped<ApplicationLayer.Interfaces.IPuestoService,
                                       ApplicationLayer.Services.PuestoService>();

            // Departamentos

            services.AddScoped<DataAccessLayer.Interfaces.IDepartamentosRepository,
                           DataAccessLayer.Repositories.DepartamentosRepository>();

            services.AddScoped<BusinessLogicLayer.Interfaces.IDepartamentosManager,
                                       BusinessLogicLayer.Managers.DepartamentoManager>();

            services.AddScoped<ApplicationLayer.Interfaces.IDepartamentoService,
                                       ApplicationLayer.Services.DepartamentoService>();
        

            return services;
        }
    }
}