using AutoMapper;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapea entre la entidad Puestos y el DTO PuestoDto
            CreateMap<Puestos, PuestoDTO>().ReverseMap();

            // Si tienes un DTO de creación o actualización:
            // CreateMap<PuestoCreateDto, Puestos>();
        }
    }
}
