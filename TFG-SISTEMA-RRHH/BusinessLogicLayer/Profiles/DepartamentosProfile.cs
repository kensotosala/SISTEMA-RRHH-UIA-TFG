using AutoMapper;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Profiles
{
    public class DepartamentosProfile : Profile
    {
        public DepartamentosProfile()
        {
            // Entity -> DTO (LECTURA)
            CreateMap<Departamentos, DepartamentoDTO>();

            // DTO -> Entity (CREAR)
            CreateMap<CrearDepartamentoDTO, Departamentos>()
            .ForMember(dest => dest.IdDepartamento, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(_ => "Activo"))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore());

            // DTO -> Entity (ACTUALIZAR)
            CreateMap<ActualizarDepartamentoDTO, Departamentos>()
            .ForMember(dest => dest.IdDepartamento, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}