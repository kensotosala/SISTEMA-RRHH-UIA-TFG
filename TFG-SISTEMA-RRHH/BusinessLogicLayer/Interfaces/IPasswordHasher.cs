namespace BusinessLogicLayer.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
    }
}