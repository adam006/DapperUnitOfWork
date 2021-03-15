using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace DapperTutorial
{
    public interface IBookRepository :  IRepository<Book>
    {
        
    }
    public class BookRepository :IBookRepository
    {
        private readonly IDapperDbContext _context;

        public BookRepository(IDapperDbContext context)
        {
            _context = context;
        }
        public Book Get(int id)
        {
            var sql = "select * from book where id = @id";
            return _context.Connection.QueryFirst<Book>(sql, new {id}, _context.Transaction);
        }

        public IEnumerable<Book> GetAll()
        {
            var sql = "select * from book";
            return _context.Connection.Query<Book>(sql, transaction: _context.Transaction);
        }

        public void Add(Book entity)
        {
            var sql = "insert into Book (title, price, authorid) values(@Title, @Price, @AuthorId); select scope_identity();";
            var id = _context.Connection.QueryFirst<int>(sql,
                new {entity.Title, entity.Price, entity.AuthorId}, _context.Transaction);
            entity.Id = id;
        }

        public void AddRange(IEnumerable<Book> entities)
        {
            // var list = entities.ToList();
            // var sql = "insert into Book (title, price, authorid) values(@Title, @Price, @AuthorId); select scope_identity();";
            // var result = _context.Connection.QueryMultiple(sql, list, _context.Transaction);
            // foreach (var book in list)
            // {
            //     book.Id = result.ReadFirst<int>();
            // }
        }

        public void Remove(Book entity)
        {
            var sql = "delete from book where id = @Id";
            _context.Connection.Execute(sql, new {entity.Id}, _context.Transaction);
        }

        public void RemoveRange(IEnumerable<Book> entities)
        {
            var ids = entities.Select(s => s.Id);
            var sql = "delete from book where id in @ids";
            _context.Connection.Execute(sql, new {ids}, _context.Transaction);
        }
    }
}