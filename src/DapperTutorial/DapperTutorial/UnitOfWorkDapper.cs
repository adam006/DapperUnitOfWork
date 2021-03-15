using System;
using System.Data;

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
        public IBookRepository Books { get; }
        public IAuthorRepository Authors { get; }

        public UnitOfWorkDapper(IDapperDbContext context, IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _context = context;
            Books = bookRepository;
            Authors = authorRepository;
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