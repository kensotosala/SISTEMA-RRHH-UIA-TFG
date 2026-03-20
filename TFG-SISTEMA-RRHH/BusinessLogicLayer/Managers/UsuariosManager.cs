using BusinessLayer.DTOs;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    /// <summary>
    /// Manager de usuarios actualizado con BCrypt
    /// </summary>
    public class UsuariosManager : IUsuariosManager
    {
        private readonly IUsuariosRepository _repo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuditoriaService _auditoria;

        public UsuariosManager(IUsuariosRepository repo, IPasswordHasher passwordHasher, IAuditoriaService auditoria)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _auditoria = auditoria;
        }

        public async Task<ResultadoOperacion<UsuarioDTO>> ActualizarAsync(ActualizarUsuarioDTO actualizarDTO)
        {
            try
            {
                if (actualizarDTO.IdUsuario <= 0)
                    return ResultadoOperacion<UsuarioDTO>.Error("ID de usuario inválido");

                var usuarioExistente = await _repo.GetByIdAsync(actualizarDTO.IdUsuario);
                if (usuarioExistente == null)
                    return ResultadoOperacion<UsuarioDTO>.Error("Usuario no encontrado");

                var estadoAnterior = usuarioExistente.Estado;
                var nombreAnterior = usuarioExistente.NombreUsuario;
                var cambioPassword = false;

                if (!string.IsNullOrWhiteSpace(actualizarDTO.NombreUsuario))
                {
                    if (await _repo.ExistsByUsernameExcludingIdAsync(
                        actualizarDTO.NombreUsuario, actualizarDTO.IdUsuario))
                        return ResultadoOperacion<UsuarioDTO>.Error("El nombre de usuario ya existe");

                    usuarioExistente.NombreUsuario = actualizarDTO.NombreUsuario;
                }

                if (!string.IsNullOrWhiteSpace(actualizarDTO.Password))
                {
                    usuarioExistente.PasswordHash = _passwordHasher.Hash(actualizarDTO.Password);
                    cambioPassword = true;
                }

                if (!string.IsNullOrWhiteSpace(actualizarDTO.Estado))
                {
                    var estadosValidos = new[] { "ACTIVO", "INACTIVO", "BLOQUEADO" };
                    if (!estadosValidos.Contains(actualizarDTO.Estado))
                        return ResultadoOperacion<UsuarioDTO>.Error("Estado no válido");

                    usuarioExistente.Estado = actualizarDTO.Estado;
                }

                usuarioExistente.FechaModificacion = DateTime.UtcNow;

                await _repo.UpdateAsync(usuarioExistente);

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "usuarios",
                    descripcion: $"Usuario ID {actualizarDTO.IdUsuario} actualizado. " +
                                   $"Nombre anterior: '{nombreAnterior}', " +
                                   $"nombre nuevo: '{usuarioExistente.NombreUsuario}', " +
                                   $"estado anterior: '{estadoAnterior}', " +
                                   $"estado nuevo: '{usuarioExistente.Estado}'" +
                                   (cambioPassword ? ", contraseña actualizada." : ".")
                );

                return ResultadoOperacion<UsuarioDTO>.Exito(
                    MapearADTO(usuarioExistente), "Usuario actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<UsuarioDTO>.Error($"Error al actualizar usuario: {ex.Message}");
            }
        }

        public async Task<ResultadoOperacion<UsuarioDTO>> AutenticarAsync(LoginDTO loginDTO)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginDTO.Username))
                    return ResultadoOperacion<UsuarioDTO>.Error("Nombre de usuario requerido");

                if (string.IsNullOrWhiteSpace(loginDTO.Password))
                    return ResultadoOperacion<UsuarioDTO>.Error("Contraseña requerida");

                var usuario = await _repo.GetByUsernameAsync(loginDTO.Username);

                if (usuario == null)
                    return ResultadoOperacion<UsuarioDTO>.Error("Credenciales inválidas");

                if (usuario.Estado == "BLOQUEADO")
                    return ResultadoOperacion<UsuarioDTO>.Error("Usuario bloqueado");

                if (usuario.Estado == "INACTIVO")
                    return ResultadoOperacion<UsuarioDTO>.Error("Usuario inactivo");

                // USAR BCRYPT EN VEZ DE SHA256
                if (!_passwordHasher.Verify(loginDTO.Password, usuario.PasswordHash))
                    return ResultadoOperacion<UsuarioDTO>.Error("Credenciales inválidas");

                await _repo.UpdateLastAccessAsync(usuario.IdUsuario);

                return ResultadoOperacion<UsuarioDTO>.Exito(
                    MapearADTO(usuario),
                    "Autenticación exitosa"
                );
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<UsuarioDTO>.Error(
                    $"Error al autenticar usuario: {ex.Message}"
                );
            }
        }

        public async Task<ResultadoOperacion<bool>> CambiarEstadoAsync(int idUsuario, string nuevoEstado)
        {
            try
            {
                if (idUsuario <= 0)
                    return ResultadoOperacion<bool>.Error("ID de usuario inválido");

                var estadosValidos = new[] { "ACTIVO", "INACTIVO", "BLOQUEADO" };
                if (!estadosValidos.Contains(nuevoEstado))
                    return ResultadoOperacion<bool>.Error("Estado no válido");

                var resultado = await _repo.ChangeStatusAsync(idUsuario, nuevoEstado);

                if (!resultado)
                    return ResultadoOperacion<bool>.Error("Usuario no encontrado");

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "usuarios",
                    descripcion: $"Estado del usuario ID {idUsuario} cambiado a '{nuevoEstado}'."
                );

                return ResultadoOperacion<bool>.Exito(
                    true, $"Estado cambiado a {nuevoEstado} exitosamente");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<bool>.Error($"Error al cambiar estado: {ex.Message}");
            }
        }

        public async Task<ResultadoOperacion<UsuarioDTO>> CrearAsync(CrearUsuarioDTO crearUsuarioDTO)
        {
            try
            {
                var validacion = ValidarCrearUsuario(crearUsuarioDTO);
                if (!validacion.Exitoso)
                    return ResultadoOperacion<UsuarioDTO>.Error(validacion.Mensaje);

                if (await _repo.ExistsByUsernameAsync(crearUsuarioDTO.NombreUsuario))
                    return ResultadoOperacion<UsuarioDTO>.Error("El nombre de usuario ya existe");

                var usuario = new Usuarios
                {
                    EmpleadoId = crearUsuarioDTO.EmpleadoId,
                    NombreUsuario = crearUsuarioDTO.NombreUsuario,
                    PasswordHash = _passwordHasher.Hash(crearUsuarioDTO.Password),
                    Estado = "ACTIVO",
                    FechaCreacion = DateTime.Now,
                };

                var usuarioCreado = await _repo.CreateAsync(usuario);

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "usuarios",
                    descripcion: $"Usuario creado: '{usuarioCreado.NombreUsuario}' " +
                                   $"(ID {usuarioCreado.IdUsuario}), " +
                                   $"empleado ID {usuarioCreado.EmpleadoId}."
                );

                return ResultadoOperacion<UsuarioDTO>.Exito(
                    MapearADTO(usuarioCreado), "Usuario creado exitosamente");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<UsuarioDTO>.Error($"Error al crear usuario: {ex.Message}");
            }
        }

        public async Task<ResultadoOperacion<bool>> EliminarAsync(int idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                    return ResultadoOperacion<bool>.Error("ID de usuario inválido");

                var usuario = await _repo.GetByIdAsync(idUsuario);

                var resultado = await _repo.DeleteAsync(idUsuario);

                if (!resultado)
                    return ResultadoOperacion<bool>.Error("Usuario no encontrado");

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "usuarios",
                    descripcion: $"Usuario ID {idUsuario} eliminado" +
                                   (usuario != null
                                       ? $" ('{usuario.NombreUsuario}', empleado ID {usuario.EmpleadoId})."
                                       : ".")
                );

                return ResultadoOperacion<bool>.Exito(true, "Usuario eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<bool>.Error($"Error al eliminar usuario: {ex.Message}");
            }
        }

        public async Task<ResultadoOperacion<UsuarioDTO>> ObtenerPorIdAsync(int idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                    return ResultadoOperacion<UsuarioDTO>.Error("ID de usuario inválido");

                var usuario = await _repo.GetByIdAsync(idUsuario);

                if (usuario == null)
                    return ResultadoOperacion<UsuarioDTO>.Error("Usuario no encontrado");

                return ResultadoOperacion<UsuarioDTO>.Exito(
                    MapearADTO(usuario),
                    "Usuario encontrado"
                );
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<UsuarioDTO>.Error(
                    $"Error al obtener usuario: {ex.Message}"
                );
            }
        }

        public async Task<ResultadoOperacion<List<UsuarioDTO>>> ObtenerTodosAsync()
        {
            try
            {
                var usuarios = await _repo.GetAllAsync();
                var usuariosDTO = usuarios.Select(MapearADTO).ToList();

                return ResultadoOperacion<List<UsuarioDTO>>.Exito(
                    usuariosDTO,
                    $"Se obtuvieron {usuariosDTO.Count} usuarios"
                );
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<List<UsuarioDTO>>.Error(
                    $"Error al obtener usuarios: {ex.Message}"
                );
            }
        }

        #region Métodos Privados

        private UsuarioDTO MapearADTO(Usuarios usuario)
        {
            return new UsuarioDTO
            {
                IdUsuario = usuario.IdUsuario,
                EmpleadoId = usuario.EmpleadoId,
                NombreUsuario = usuario.NombreUsuario,
                Estado = usuario.Estado,
                UltimoAcceso = usuario.UltimoAcceso,
                FechaCreacion = usuario.FechaCreacion,
                NombreEmpleado = usuario.Empleado?.Nombre ?? "N/A"
            };
        }

        private ResultadoOperacion<bool> ValidarCrearUsuario(CrearUsuarioDTO dto)
        {
            if (dto.EmpleadoId <= 0)
                return ResultadoOperacion<bool>.Error("ID de empleado inválido");

            if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
                return ResultadoOperacion<bool>.Error("Nombre de usuario requerido");

            if (dto.NombreUsuario.Length > 50)
                return ResultadoOperacion<bool>.Error("Nombre de usuario muy largo (máximo 50 caracteres)");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return ResultadoOperacion<bool>.Error("Contraseña requerida");

            if (dto.Password.Length < 6)
                return ResultadoOperacion<bool>.Error("La contraseña debe tener al menos 6 caracteres");

            return ResultadoOperacion<bool>.Exito(true, "Validación exitosa");
        }

        #endregion Métodos Privados
    }
}