using ApplicationLayer.Interfaces;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Services
{
    public class PuestoService : IPuestoService
    {
        private readonly IPuestosManager _puestoManager;
        private readonly ILogger<PuestoService> _logger;

        // SIN AutoMapper - ya que los DTOs se comparten
        public PuestoService(IPuestosManager puestoManager, ILogger<PuestoService> logger)
        {
            _puestoManager = puestoManager;
            _logger = logger;
        }

        public async Task<IEnumerable<PuestoDTO>> GetAllPuestosAsync()
        {
            try
            {
                // PuestoManager ya devuelve PuestoDTO
                var result = await _puestoManager.GetAllPuestosAsync();
                return result?.ToList() ?? new List<PuestoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllPuestosAsync");
                throw;
            }
        }

        public async Task<PuestoDTO?> GetPuestoByIdAsync(int id)
        {
            try
            {
                return await _puestoManager.GetPuestoByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en GetPuestoByIdAsync con ID={id}");
                throw;
            }
        }

        public async Task<PuestoDTO> CreatePuestoAsync(CrearPuestoDTO puestoDto)
        {
            try
            {
                // Convierte CrearPuestoDTO a PuestoDTO
                var puesto = new PuestoDTO
                {
                    NombrePuesto = puestoDto.NombrePuesto,
                    Descripcion = puestoDto.Descripcion,
                    NivelJerarquico = puestoDto.NivelJerarquico,
                    SalarioMinimo = puestoDto.SalarioMinimo,
                    SalarioMaximo = puestoDto.SalarioMaximo,
                    Estado = puestoDto.Estado,
                    FechaCreacion = DateTime.UtcNow
                };

                return await _puestoManager.CreatePuestoAsync(puesto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreatePuestoAsync");
                throw;
            }
        }

        public async Task<bool> UpdatePuestoAsync(int id, ActualizarPuestoDTO puestoDto)
        {
            if (id != puestoDto.IdPuesto)
                throw new ArgumentException("ID no coincide");

            try
            {
                // Convierte ActualizarPuestoDTO a PuestoDTO
                var puesto = new PuestoDTO
                {
                    IdPuesto = puestoDto.IdPuesto,
                    NombrePuesto = puestoDto.NombrePuesto,
                    Descripcion = puestoDto.Descripcion,
                    NivelJerarquico = puestoDto.NivelJerarquico,
                    SalarioMinimo = puestoDto.SalarioMinimo,
                    SalarioMaximo = puestoDto.SalarioMaximo,
                    Estado = puestoDto.Estado,
                    FechaModificacion = DateTime.UtcNow
                };

                await _puestoManager.UpdatePuestoAsync(puesto);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en UpdatePuestoAsync con ID={id}");
                return false;
            }
        }

        public async Task<bool> DeletePuestoAsync(int id)
        {
            try
            {
                await _puestoManager.DeletePuestoAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en DeletePuestoAsync con ID={id}");
                return false;
            }
        }
    }
}