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
        private readonly IPuestosRepository _repoPuestos;
        private readonly IDepartamentosRepository _repoDepartamentos;
        private readonly IRolesRepository _repoRoles;
        private readonly IPasswordHasher _passwordHasher;

        public EmpleadosManager(
            IEmpleadosRepository repoEmpleados,
            IUsuarioRepository repoUsuarios,
            IPuestosRepository repoPuestos,
            IDepartamentosRepository repoDepartamentos,
            IRolesRepository repoRoles,
            IUsuariosRolesRepository repoUsuariosRoles,
            IPasswordHasher passwordHasher)
        {
            _repoEmpleados = repoEmpleados;
            _repoUsuarios = repoUsuarios;
            _repoPuestos = repoPuestos;
            _repoDepartamentos = repoDepartamentos;
            _repoRoles = repoRoles;
            _passwordHasher = passwordHasher;
        }

        public async Task<DetalleEmpleadoDTO> CreateAsync(CrearEmpleadoYUsuarioDTO dto)
        {
            // ======================================================
            // VALIDACIONES DE NEGOCIO
            // ======================================================

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (await _repoEmpleados.ExistsByCodigoAsync(dto.CodigoEmpleado))
                throw new ArgumentException(
                    $"Ya existe un empleado con el código '{dto.CodigoEmpleado}'.");

            if (await _repoUsuarios.ExistsByUsernameAsync(dto.NombreUsuario))
                throw new BusinessException(
                    $"El nombre de usuario '{dto.NombreUsuario}' ya está en uso.",
                    code: "USERNAME_DUPLICADO");

            if (await _repoEmpleados.EmaillRegistrado(dto.Email))
                throw new BusinessException(
                    $"La dirección de correo '{dto.Email}' ya está en uso.",
                    code: "EMAIL_DUPLICADO");

            if (!await _repoPuestos.ExistsAsync(dto.PuestoId))
                throw new BusinessException(
                    $"El Puesto cuyo ID es '{dto.PuestoId}' no se encuentra",
                    code: "PUESTO_NO_EXISTE");

            if (!await _repoDepartamentos.ExistsAsync(dto.PuestoId))
                throw new BusinessException(
                    $"El Departamento cuyo ID es '{dto.PuestoId}' no se encuentra.",
                    code: "DEPARTAMENTO_NO_EXISTE");

            if (dto.SalarioBase < 0)
                throw new BusinessException(
                    "El salario base no puede ser negativo.",
                    "SALARIO_INVALIDO");

            if (dto.FechaContratacion > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BusinessException(
                   $"La fecha de contratación no puede ser futura.",
                   code: "FUTURE_DATE");

            if (dto.JefeInmediatoId.HasValue && !await _repoEmpleados.ExistsAsync(dto.JefeInmediatoId.Value))
            {
                throw new BusinessException(
                    $"El jefe inmediato con ID '{dto.JefeInmediatoId}' no existe.",
                    "JEFE_NO_EXISTE");
            }

            if (!await _repoRoles.ExistsAsync(dto.RolId))
                throw new BusinessException(
                    $"El Rol cuyo ID es '{dto.RolId}' no se encuentra.",
                    code: "ROL_NO_EXISTE");

            // ======================================================
            // CREAR EMPLEADO (DTO → ENTIDAD)
            // ======================================================

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
                TipoContrato = dto.TipoContrato.ToString(),

                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow
            };

            var empleadoCreado = await _repoEmpleados.CreateAsync(empleado);

            // ======================================================
            // CREAR USUARIO (DTO → ENTIDAD)
            // ======================================================

            var usuario = new Usuarios
            {
                NombreUsuario = dto.NombreUsuario,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                EmpleadoId = empleadoCreado.IdEmpleado,

                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow
            };

            var usuarioCreado = await _repoUsuarios.CreateAsync(usuario);

            // ======================================================
            // RELACIÓN
            // ======================================================

            empleadoCreado.Usuarios = usuarioCreado;

            // ======================================================
            // RESPUESTA (ENTIDAD → DTO)
            // ======================================================

            var rol = await _repoRoles.GetByIdAsync(dto.RolId);

            return new DetalleEmpleadoDTO
            {
                IdEmpleado = empleadoCreado.IdEmpleado,
                CodigoEmpleado = empleadoCreado.CodigoEmpleado,
                Nombre = empleadoCreado.Nombre,
                PrimerApellido = empleadoCreado.PrimerApellido,
                SegundoApellido = empleadoCreado.SegundoApellido,
                Email = empleadoCreado.Email,
                Telefono = empleadoCreado.Telefono,
                FechaContratacion = empleadoCreado.FechaContratacion,

                PuestoId = empleadoCreado.PuestoId,
                DepartamentoId = empleadoCreado.DepartamentoId,
                JefeInmediatoId = empleadoCreado.JefeInmediatoId,
                SalarioBase = empleadoCreado.SalarioBase,

                TipoContrato = Enum.Parse<TipoContrato>(empleadoCreado.TipoContrato),

                Estado = empleadoCreado.Estado,
                FechaCreacion = empleadoCreado.FechaCreacion,
                FechaModificacion = empleadoCreado.FechaModificacion,

                IdUsuario = usuarioCreado.IdUsuario,
                NombreUsuario = usuarioCreado.NombreUsuario,
                NombreRol = rol?.Nombre ?? string.Empty
            };
        }

        public async Task<DetalleEmpleadoDTO?> GetByIdAsync(int id)
        {
            var empleado = await _repoEmpleados.GetByIdAsync(id);
            if (empleado == null) return null;

            var usuario = await _repoUsuarios.GetByEmpleadoIdAsync(empleado.IdEmpleado);

            return new DetalleEmpleadoDTO
            {
                IdEmpleado = empleado.IdEmpleado,
                CodigoEmpleado = empleado.CodigoEmpleado,
                Nombre = empleado.Nombre,
                PrimerApellido = empleado.PrimerApellido,
                SegundoApellido = empleado.SegundoApellido,
                Email = empleado.Email,
                Telefono = empleado.Telefono,
                FechaContratacion = empleado.FechaContratacion,
                PuestoId = empleado.PuestoId,
                DepartamentoId = empleado.DepartamentoId,
                JefeInmediatoId = empleado.JefeInmediatoId,
                SalarioBase = empleado.SalarioBase,
                TipoContrato = Enum.Parse<TipoContrato>(empleado.TipoContrato),
                Estado = empleado.Estado,
                FechaCreacion = empleado.FechaCreacion,
                FechaModificacion = empleado.FechaModificacion,
                IdUsuario = usuario?.IdUsuario ?? 0,
                NombreUsuario = usuario?.NombreUsuario ?? string.Empty,
                NombreRol = usuario?.UsuariosRoles
                    .Select(ur => ur.Rol.Nombre)
                    .FirstOrDefault() ?? string.Empty
            };
        }

        public async Task<IEnumerable<DetalleEmpleadoDTO>> ListAsync()
        {
            // Cargar empleados con los usuarios y sus roles (Eager Loading)
            var empleados = await _repoEmpleados.GetAllWithUsersAndRolesAsync();

            // Extraer todos los IDs de roles únicos
            var rolIds = empleados
                .Select(e => e.Usuarios)
                .Where(u => u != null)
                .SelectMany(u => u!.UsuariosRoles)
                .Select(ur => ur.RolId)
                .Distinct()
                .ToList();

            // Obtener nombres de roles en batch
            var rolesLookup = await _repoRoles.GetNamesByIdsAsync(rolIds);

            // Mapear a DTO usando LINQ (más eficiente y legible)
            var resultado = empleados.Select(empleado =>
            {
                var usuario = empleado.Usuarios;
                var nombreRol = string.Empty;
                var roles = new List<string>();

                if (usuario?.UsuariosRoles != null && usuario.UsuariosRoles.Any())
                {
                    // Si un usuario puede tener múltiples roles, obtenemos todos
                    var rolesUsuario = usuario.UsuariosRoles
                        .Select(ur => ur.RolId)
                        .Where(rolId => rolesLookup.ContainsKey(rolId))
                        .Select(rolId => rolesLookup[rolId])
                        .ToList();

                    if (rolesUsuario.Any())
                    {
                        roles = rolesUsuario;
                        nombreRol = rolesUsuario.First(); // Para compatibilidad con propiedad existente
                    }
                }

                return new DetalleEmpleadoDTO
                {
                    IdEmpleado = empleado.IdEmpleado,
                    CodigoEmpleado = empleado.CodigoEmpleado,
                    Nombre = empleado.Nombre,
                    PrimerApellido = empleado.PrimerApellido,
                    SegundoApellido = empleado.SegundoApellido,
                    Email = empleado.Email,
                    Telefono = empleado.Telefono,
                    FechaContratacion = empleado.FechaContratacion,
                    PuestoId = empleado.PuestoId,
                    DepartamentoId = empleado.DepartamentoId,
                    JefeInmediatoId = empleado.JefeInmediatoId,
                    SalarioBase = empleado.SalarioBase,
                    TipoContrato = Enum.Parse<TipoContrato>(empleado.TipoContrato),
                    Estado = empleado.Estado,
                    FechaCreacion = empleado.FechaCreacion,
                    FechaModificacion = empleado.FechaModificacion,
                    IdUsuario = usuario?.IdUsuario ?? 0,
                    NombreUsuario = usuario?.NombreUsuario ?? string.Empty,
                    NombreRol = nombreRol,
                };
            }).ToList();

            return resultado;
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

        public async Task UpdateAsync(int id, ActualizarEmpleadoDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0.", nameof(id));

            // ===== OBTENER EMPLEADO =====
            var empleado = await _repoEmpleados.GetByIdAsync(id);
            if (empleado == null)
                throw new KeyNotFoundException($"Empleado con ID {id} no encontrado.");

            // ===== ACTUALIZAR EMPLEADO (SOLO CAMPOS INFORMADOS) =====

            if (dto.CodigoEmpleado != null)
                empleado.CodigoEmpleado = dto.CodigoEmpleado;

            if (dto.Nombre != null)
                empleado.Nombre = dto.Nombre;

            if (dto.PrimerApellido != null)
                empleado.PrimerApellido = dto.PrimerApellido;

            if (dto.SegundoApellido != null)
                empleado.SegundoApellido = dto.SegundoApellido;

            if (dto.Email != null)
                empleado.Email = dto.Email;

            if (dto.Telefono != null)
                empleado.Telefono = dto.Telefono;

            if (dto.FechaContratacion.HasValue)
                empleado.FechaContratacion = dto.FechaContratacion.Value;

            if (dto.PuestoId.HasValue)
                empleado.PuestoId = dto.PuestoId.Value;

            if (dto.DepartamentoId.HasValue)
                empleado.DepartamentoId = dto.DepartamentoId.Value;

            if (dto.JefeInmediatoId.HasValue)
                empleado.JefeInmediatoId = dto.JefeInmediatoId.Value;

            if (dto.SalarioBase.HasValue)
                empleado.SalarioBase = dto.SalarioBase.Value;

            if (dto.TipoContrato.HasValue)
                empleado.TipoContrato = dto.TipoContrato.Value.ToString();

            if (dto.Estado.HasValue)
                empleado.Estado = dto.Estado.Value.ToString();

            empleado.FechaModificacion = DateTime.UtcNow;

            await _repoEmpleados.UpdateAsync(empleado);

            // ===== ACTUALIZAR ROL DE USUARIO (SI APLICA) =====

            if (dto.RolId.HasValue && dto.RolId.Value > 0 && empleado.Usuarios != null)
            {
                var usuario = empleado.Usuarios;

                usuario.UsuariosRoles.Clear();
                usuario.UsuariosRoles.Add(new UsuariosRoles
                {
                    RolId = dto.RolId.Value
                });

                usuario.FechaModificacion = DateTime.UtcNow;

                await _repoUsuarios.UpdateAsync(usuario);
            }
        }
    }
}