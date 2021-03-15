using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace DapperTutorial
{
    public interface IAuthorRepository :  IRepository<Author>
    {
    }

    public class AuthorRepository : IAuthorRepository
    {
        private readonly IDapperDbContext _context;

        public AuthorRepository(IDapperDbContext context)
        {
            _context = context;
        }
        public Author Get(int id)
        {
            var sql = "select * from author where id = @id";
            return _context.Connection.QueryFirst<Author>(sql, new {id}, _context.Transaction);
        }

        public IEnumerable<Author> GetAll()
        {
            var sql = "select * from author";
            return _context.Connection.Query<Author>(sql, transaction: _context.Transaction);
        }

        public void Add(Author entity)
        {
            var sql = "insert into author (name) values(@Name); select scope_identity();";
            var id = _context.Connection.QueryFirst<int>(sql,
                new {entity.Name}, _context.Transaction);
            entity.Id = id;
        }

        public void AddRange(IEnumerable<Author> entities)
        {
            // var list = entities.ToList();
            // var sql = "insert into author (name) values(@Name); select scope_identity();";
            // var result = _context.Connection.QueryMultiple(sql, list, _context.Transaction);
            // foreach (var author in list)
            // {
            //     author.Id = result.ReadFirst<int>();
            // }
        }

        public void Remove(Author entity)
        {
            var sql = "delete from author where id = @Id";
            _context.Connection.Execute(sql, new {entity.Id}, _context.Transaction);
        }

        public void RemoveRange(IEnumerable<Author> entities)
        {
            var ids = entities.Select(s => s.Id);
            var sql = "delete from author where id in @ids";
            _context.Connection.Execute(sql, new {ids}, _context.Transaction);
        }
    }
}