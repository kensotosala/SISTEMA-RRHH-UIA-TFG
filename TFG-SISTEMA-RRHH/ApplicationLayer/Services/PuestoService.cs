namespace ApplicationLayer.Services
{
    using ApplicationLayer.Interfaces;
    using AutoMapper;
    using BusinessLogicLayer.DTOs;
    using BusinessLogicLayer.Interfaces;
    using Microsoft.Extensions.Logging;

    public class PuestoService : IPuestoService
    {
        private readonly IPuestosManager _puestoManager;

        private readonly IMapper _mapper;

        private readonly ILogger<PuestoService> _logger;

        public PuestoService(IPuestosManager puestoManager, IMapper mapper, ILogger<PuestoService> logger)
        {
            _puestoManager = puestoManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PuestoDto>> GetAllPuestosAsync()
        {
            try
            {
                var result = await _puestoManager.GetAllPuestosAsync();

                return result?.ToList() ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllPuestosAsync");
                throw;
            }
        }

        public async Task<PuestoDto> GetPuestoByIdAsync(int id)
        {
            return await _puestoManager.GetPuestoByIdAsync(id);
        }

        public async Task<PuestoDto> CreatePuestoAsync(PuestoDto puestoDto)
        {
            return await _puestoManager.CreatePuestoAsync(puestoDto);
        }

        public async Task UpdatePuestoAsync(int id, PuestoDto puestoDto)
        {
            if (id != puestoDto.IdPuesto)
                throw new ArgumentException("ID no coincide");

            await _puestoManager.UpdatePuestoAsync(puestoDto);
        }

        public async Task DeletePuestoAsync(int id)
        {
            await _puestoManager.DeletePuestoAsync(id);
        }
    }
}
