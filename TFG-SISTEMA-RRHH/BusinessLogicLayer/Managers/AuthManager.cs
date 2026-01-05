using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class AuthManager : IAuthManager
    {
        private readonly IUsuarioRepository _repo;

        public AuthManager(IUsuarioRepository repo)
        {
            _repo = repo;
        }



        public async Task<Usuarios?> RegistrarNuevoUsuario(Usuarios usuario)
        {
            var user = await _repo.GetByUsernameAsync(usuario.NombreUsuario);
            if (user == null) return null;

            return await _repo.CreateAsync(usuario);
        }

        public async Task<Usuarios?> ValidarCredencialesAsync(string username, string password)
        {
            var user = await _repo.GetByUsernameAsync(username);
            if (user == null) return null;

            return user.PasswordHash == password ? user: null;
        }
    }
}