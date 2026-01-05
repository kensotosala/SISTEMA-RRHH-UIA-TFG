using AutoMapper;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

public class EmpleadosProfile : Profile
{
    public EmpleadosProfile()
    {
        // =========================
        // EMPLEADO
        // =========================
        CreateMap<Empleados, CrearEmpleadoDTO>();
        CreateMap<Empleados, DetalleEmpleadoDTO>();

        CreateMap<CrearEmpleadoDTO, Empleados>()
         .ForMember(d => d.IdEmpleado, o => o.Ignore())
         .ForMember(d => d.Estado, o => o.Ignore())
         .ForMember(d => d.FechaCreacion, o => o.Ignore())
         .ForMember(d => d.FechaModificacion, o => o.Ignore());

        CreateMap<ActualizarEmpleadoDTO, Empleados>()
            .ForMember(d => d.IdEmpleado, o => o.Ignore())
            .ForMember(d => d.FechaCreacion, o => o.Ignore())
            .ForMember(d => d.FechaModificacion,
                o => o.MapFrom(_ => DateTime.UtcNow));

        // =========================
        // USUARIO
        // =========================
        CreateMap<Usuarios, CrearUsuarioDTO>();
        CreateMap<CrearUsuarioDTO, Usuarios>()
        .ForMember(d => d.IdUsuario, o => o.Ignore())
        .ForMember(d => d.EmpleadoId, o => o.Ignore())
        .ForMember(d => d.PasswordHash, o => o.Ignore())
        .ForMember(d => d.Estado, o => o.Ignore())
        .ForMember(d => d.FechaCreacion, o => o.Ignore())
        .ForMember(d => d.FechaModificacion, o => o.Ignore());

        CreateMap<ActualizarUsuarioDTO, Usuarios>()
            .ForMember(d => d.IdUsuario, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.EmpleadoId, o => o.Ignore())
            .ForMember(d => d.FechaCreacion, o => o.Ignore())
            .ForMember(d => d.FechaModificacion,
                o => o.MapFrom(_ => DateTime.UtcNow));

        // =========================
        // DTO COMPUESTO (LECTURA)
        // =========================
        CreateMap<Empleados, ListarEmpleadoUsuarioDto>()
            .ForMember(d => d.Empleado, o => o.MapFrom(src => src))
            .ForMember(d => d.Usuario, o => o.MapFrom(src => src.Usuarios));

        // =========================
        // DTO COMPUESTO (CREACIÓN)
        // =========================
        CreateMap<Empleados, CrearEmpleadoUsuarioDto>()
            .ForMember(d => d.Empleado, o => o.MapFrom(src => src))
            .ForMember(d => d.Usuario, o => o.MapFrom(src => src.Usuarios));
    }
}