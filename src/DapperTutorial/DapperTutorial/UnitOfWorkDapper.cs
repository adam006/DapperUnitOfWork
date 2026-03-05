using System;

namespace DapperTutorial
{
    public interface IUnitOfWorkDapper
    {
        IBookRepository Books { get; }
        IAuthorRepository Authors { get; }
        void Begin();
        void Complete();
    }

    public class UnitOfWorkDapper : IUnitOfWorkDapper
    {
        private readonly IDapperDbContext _context;
        private readonly IRepositoryFactory _repositoryFactory;

        private readonly Lazy<IBookRepository> _books;
        public IBookRepository Books => _books.Value;

        private readonly Lazy<IAuthorRepository> _authors;
        public IAuthorRepository Authors => _authors.Value;

        public UnitOfWorkDapper(IDapperDbContext context, IRepositoryFactory repositoryFactory)
        {
            _context = context;
            _repositoryFactory = repositoryFactory;
            _books = new Lazy<IBookRepository>(() => _repositoryFactory.CreateRepository<IBookRepository>(_context));
            _authors = new Lazy<IAuthorRepository>(() =>
                _repositoryFactory.CreateRepository<IAuthorRepository>(_context));
        }

        public void Begin()
        {
            _context.BeginTransaction();
        }

        public void Complete()
        {
            try
            {
                _context.Commit();
            }
            catch
            {
                _context.Rollback();
                throw;
            }
        }
    }
}