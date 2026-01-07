namespace BusinessLogicLayer.Interfaces
{
    public interface IAsistenciaManager
    {
        Task MarcarAsistenciaAsync(int empleadoId);
    }
}