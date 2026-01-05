using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class EmpleadosManager : IEmpleadosManager
    {
        private readonly IEmpleadosRepository _repoEmpleados;
        private readonly IUsuarioRepository _repoUsuarios;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;

        public EmpleadosManager(
            IEmpleadosRepository repoEmpleados,
            IUsuarioRepository repoUsuarios,
            IMapper mapper,
            IPasswordHasher passwordHasher)
        {
            _repoEmpleados = repoEmpleados;
            _repoUsuarios = repoUsuarios;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<CrearEmpleadoUsuarioDto> CreateAsync(CrearEmpleadoUsuarioDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Empleado is null)
                throw new ArgumentException("La información del empleado es obligatoria.");

            if (dto.Usuario is null)
                throw new ArgumentException("La información del usuario es obligatoria.");

            // Validaciones de las Business Rules

            if (await _repoEmpleados.ExistsByCodigoAsync(dto.Empleado.CodigoEmpleado))
                throw new InvalidOperationException(
                    $"Ya existe un empleado con el código '{dto.Empleado.CodigoEmpleado}'.");

            if (await _repoUsuarios.ExistsByUsernameAsync(dto.Usuario.NombreUsuario))
                throw new InvalidOperationException(
                    $"El nombre de usuario '{dto.Usuario.NombreUsuario}' ya está en uso.");

            if (dto.Empleado.SalarioBase < 0)
                throw new ArgumentException("El salario base no puede ser negativo.");

            if (dto.Empleado.FechaContratacion > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("La fecha de contratación no puede ser futura.");

            // Creación del empleado

            var empleado = _mapper.Map<Empleados>(dto.Empleado);

            empleado.Estado = "ACTIVO";
            empleado.FechaCreacion = DateTime.UtcNow;
            empleado.FechaModificacion = DateTime.UtcNow;

            var empleadoCreado = await _repoEmpleados.CreateAsync(empleado);

            // Creación del usuario

            var usuario = _mapper.Map<Usuarios>(dto.Usuario);

            usuario.PasswordHash = _passwordHasher.Hash(dto.Usuario.Password);
            usuario.EmpleadoId = empleadoCreado.IdEmpleado;
            usuario.FechaCreacion = DateTime.UtcNow;
            usuario.FechaModificacion = DateTime.UtcNow;
            usuario.Estado = EstadoEmpleado.ACTIVO.ToString();

            var usuarioCreado = await _repoUsuarios.CreateAsync(usuario);

            // Relación y respuesta

            empleadoCreado.Usuarios = usuarioCreado;

            return _mapper.Map<CrearEmpleadoUsuarioDto>(empleadoCreado);
        }

        public async Task<ListarEmpleadoUsuarioDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0.", nameof(id));

            var empleado = await _repoEmpleados.GetByIdAsync(id);

            if (empleado == null)
                return null;

            return _mapper.Map<ListarEmpleadoUsuarioDto>(empleado);
        }

        public async Task<IEnumerable<ListarEmpleadoUsuarioDto>> ListAsync()
        {
            var empleados = await _repoEmpleados.GetAllAsync();

            return _mapper.Map<IEnumerable<ListarEmpleadoUsuarioDto>>(empleados);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor a 0.", nameof(id));
            }

            if (!await _repoEmpleados.ExistsAsync(id))
                throw new ArgumentException("El ID no se ha encontrado", nameof(id)); ;

            var tieneSubordinados = await _repoEmpleados.TieneSubordinadosAsync(id);
            if (tieneSubordinados)
            {
                var subordinadosCount = await _repoEmpleados.ContarSubordinadosAsync(id);
                throw new InvalidOperationException(
                    $"No se puede eliminar el empleado porque es jefe inmediato de {subordinadosCount} empleado(s). " +
                    "Primero debe reasignar sus subordinados a otro jefe.");
            }

            var usuarioExistente = await _repoUsuarios.GetByIdAsync(id);

            if (usuarioExistente != null)
            {
                await _repoUsuarios.DeleteAsync(usuarioExistente.IdUsuario);
            }

            await _repoEmpleados.DeleteAsync(id);
        }

        public async Task UpdateAsync(int id, ActualizarEmpleadoUsuarioDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var empleadoExistente = await _repoEmpleados.GetByIdAsync(id);
            if (empleadoExistente == null)
                throw new KeyNotFoundException($"Empleado con ID {id} no encontrado.");

            var usuarioExistente = await _repoUsuarios.GetByIdAsync(id);

            _mapper.Map(dto.Empleado, empleadoExistente);
            empleadoExistente.FechaModificacion = DateTime.UtcNow;

            await _repoEmpleados.UpdateAsync(empleadoExistente);

            if (usuarioExistente != null && dto.Usuario != null)
            {
                _mapper.Map(dto.Usuario, usuarioExistente);
                usuarioExistente.FechaModificacion = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(dto.Usuario.Password))
                {
                    usuarioExistente.PasswordHash = _passwordHasher.Hash(dto.Usuario.Password);
                }

                await _repoUsuarios.UpdateAsync(usuarioExistente);
            }
        }
    }
}