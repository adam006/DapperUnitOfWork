namespace DapperTutorial;

public interface IRepositoryFactory
{
    T CreateRepository<T>(IDapperDbContext context) where T : class;
}