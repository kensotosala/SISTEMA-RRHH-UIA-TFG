using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class EmpleadosManager : IEmpleadosManager
    {
        private readonly IEmpleadosRepository _repo;

        public EmpleadosManager(IEmpleadosRepository repo)
        {
            _repo = repo;
        }

        public async Task<EmpleadosDTO> CreateAsync(EmpleadoCreateDTO dto)
        {
            var empleado = new Empleados
            {
                CodigoEmpleado = dto.CodigoEmpleado,
                Nombre = dto.Nombre,
                PrimerApellido = dto.PrimerApellido,
                SegundoApellido = dto.SegundoApellido,
                Email = dto.Email,
                Telefono = dto.Telefono,
                FechaContratacion = dto.FechaContratacion,
                PuestoId = dto.PuestoId,
                DepartamentoId = dto.DepartamentoId,
                JefeInmediatoId = dto.JefeInmediatoId,
                SalarioBase = dto.SalarioBase,
                Estado = "activo",
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
            };

            var registroCreado = await _repo.CreateAsync(empleado);

            var entityToReturn = new EmpleadosDTO
            {
                IdEmpleado = registroCreado.IdEmpleado,
                CodigoEmpleado = registroCreado.CodigoEmpleado,
                Nombre = registroCreado.Nombre,
                PrimerApellido = registroCreado.PrimerApellido,
                SegundoApellido = registroCreado.SegundoApellido,
                Email = registroCreado.Email,
                Telefono = registroCreado.Telefono,
                FechaContratacion = registroCreado.FechaContratacion,
                PuestoId = registroCreado.PuestoId,
                DepartamentoId = registroCreado.DepartamentoId,
                JefeInmediatoId = registroCreado.JefeInmediatoId,
                SalarioBase = registroCreado.SalarioBase,
                Estado = registroCreado.Estado,
                FechaCreacion = registroCreado.FechaCreacion,
                FechaModificacion = registroCreado.FechaModificacion,
            };

            return entityToReturn;
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<EmpleadosDTO?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<EmpleadosDTO>> ListAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(int id, EmpleadosDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}