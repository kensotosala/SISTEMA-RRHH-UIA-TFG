using AutoMapper;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Profiles
{
    public class PuestosProfile : Profile
    {
        public PuestosProfile()
        {
            // Entity -> DTO (LECTURA)
            CreateMap<Puestos, PuestoDTO>()
                .ForMember(dest => dest.NivelJerarquico, opt => opt.MapFrom(src => src.NivelJerarquico));

            // DTO -> Entity (CREAR)
            CreateMap<CrearPuestoDTO, Puestos>()
            .ForMember(dest => dest.IdPuesto, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore())
            .ForMember(dest => dest.NivelJerarquico,
                opt => opt.MapFrom(src => (sbyte?)src.NivelJerarquico))
            .ForMember(dest => dest.Empleados, opt => opt.Ignore());

            // DTO -> Entity (ACTUALIZAR)
            CreateMap<ActualizarPuestoDTO, Puestos>()
           .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
           .ForMember(dest => dest.FechaModificacion,
               opt => opt.MapFrom(_ => DateTime.UtcNow))
           .ForMember(dest => dest.NivelJerarquico,
               opt => opt.MapFrom(src => (sbyte?)src.NivelJerarquico))
           .ForMember(dest => dest.Empleados, opt => opt.Ignore());
        }
    }
}