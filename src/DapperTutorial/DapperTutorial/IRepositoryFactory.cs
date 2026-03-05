using System;

namespace DapperTutorial;

public interface IRepositoryFactory
{
    T CreateRepository<T>(IDapperDbContext context) where T : class;
}

public class RepositoryFactory : IRepositoryFactory
{
    public T CreateRepository<T>(IDapperDbContext context) where T : class
    {
        if (typeof(T) == typeof(IBookRepository))
        {
            return new BookRepository(context) as T;
        }

        if (typeof(T) == typeof(IAuthorRepository))
        {
            return new AuthorRepository(context) as T;
        }

        throw new ArgumentException($"No repository registered for type {typeof(T).Name}", nameof(T));
    }
}