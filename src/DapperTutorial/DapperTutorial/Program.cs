using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
namespace DapperTutorial
{
    class Program
    {
        private static readonly string _connection =
            "Data Source=127.0.0.1;User ID=sa;Password=Password@12345;Database=Dapper;";


        static void Main(string[] args)
        {

        }

        public static void UnitOfWork()
        {
            var conn = GetConnection();

            using var context = new DapperDbContext(conn);
            var uow = new UnitOfWorkDapper(context, new BookRepository(context), new AuthorRepository(context));

            uow.Begin();
            var authors = uow.Authors.GetAll().ToList();
            uow.Complete();

            uow.Begin();
            var list = new List<Book>();
            list.Add(new Book{Price = 10, Title = "a wild rarment", AuthorId = authors.First().Id});
            list.Add(new Book{Price = 10, Title = "a wild rarment part 2", AuthorId = authors.First().Id});
            uow.Books.Add(list.First());
            
            var books = uow.Books.GetAll();
            
            PrintAuthors(authors);
            PrintBooks(books);
        }
        
        public static SqlConnection GetConnection()
        {
            var sqlConn = new SqlConnection(_connection);
            return sqlConn;
        }

        public static void ResetDb()
        {
            var conn = new SqlConnectionStringBuilder(_connection);
            conn.InitialCatalog = "master";
            using var sqlConn = new SqlConnection(conn.ConnectionString);
            var fileInfo = new FileInfo("dapper.sql");
            var script = fileInfo.OpenText().ReadToEnd();

            var server = new Server(new ServerConnection(sqlConn));
            server.ConnectionContext.ExecuteNonQuery(script);
        }
        
        public static void GetAuthors()
        {
            using var sqlConn = new SqlConnection(_connection);
            var authors = sqlConn.Query<Author>("SELECT * FROM Author").ToList();
            PrintAuthors(authors);
        }
       
        public static int GetAuthorId()
        {
            var sql = "SELECT TOP(1) id FROM Author";

            using var sqlConn = new SqlConnection(_connection);
            var id = sqlConn.QueryFirst<int>(sql);
            return id;
        }

        public static void InsertAuthor()
        {
            var sql = @"INSERT INTO Author values (@name)";
            using var sqlConn = new SqlConnection(_connection);
            sqlConn.Execute(sql, new {name = "New Author"});
        }

        public static void UpdateAuthor()
        {
            var id = GetAuthorId();

            var updateSql = "UPDATE Author SET Name = @name WHERE Id = @id";

            using var sqlConn = new SqlConnection(_connection);
            var rowsAffected = sqlConn.Execute(updateSql, new {name = "Sally", id});

            Console.WriteLine($"Rows affected : {rowsAffected}");
        }

        public static void GetBooks()
        {
            var sql = "SELECT * FROM Book";

            using var sqlConn = new SqlConnection(_connection);
            var books = sqlConn.Query<Book>(sql);

            PrintBooks(books);
        }

        public static void GetBooksWithAuthors()
        {
            using var sqlConn = new SqlConnection(_connection);
            
            var sql = @"SELECT * 
                        FROM Book b
                        JOIN Author a on a.id = b.authorId";

            var books = sqlConn.Query<Book, Author, Book>(sql, (book, author) =>
            {
                book.Author = author;
                return book;
            });

            PrintBooksWithAuthor(books);
        }

        public static void GetAuthorWithBooks()
        {
            using var sqlConn = new SqlConnection(_connection);
            
            var sql = @"SELECT * 
                        FROM Author a
                        JOIN Book b on a.id = b.authorId";

            var results = new Dictionary<int, Author>();
            var authors = sqlConn.Query<Author, Book, Author>(sql, (author, book) =>
            {
                Author authorEntry;
                if (!results.TryGetValue(author.Id, out authorEntry))
                {
                    results.Add(author.Id, author);
                    authorEntry = author;
                }

                authorEntry.Books.Add(book);
                return authorEntry;
            });
            foreach (var author in authors)
            {
                PrintAuthorAndBooks(author);
            }
        }

        public static int GetBookId()
        {
            var sql = "SELECT TOP(1) FROM Book";

            using var sqlConn = new SqlConnection(_connection);
            var id = sqlConn.QuerySingle<int>(sql);

            return id;
        }

        public static void InsertBook()
        {
            var authorId = GetAuthorId();
            var sql = "INSERT INTO Book values(@title, @price, @authorId)";

            using var sqlConn = new SqlConnection(_connection);
            sqlConn.Execute(sql, new {title = "New Book 5000", price = 19.99, id = authorId});
        }

        public static void InsertMultipleBooks()
        {
            var authorId = GetAuthorId();
            var sql = "INSERT INTO Book values(@title, @price, @authorId)";

            using var sqlConn = new SqlConnection(_connection);
            sqlConn.Execute(sql,
                new[]
                {
                    new {title = "New Book 5000", price = 19.99, authorId},
                    new {title = "Another New Book", price = 5.99, authorId}
                });
        }

        public static void UpdateBook()
        {
            using var sqlConn = new SqlConnection(_connection);
            
            
            var id = 1;
            var sql = @"UPDATE Book
                        SET title = @bookTitel 
                        WHERE id = @id";

            var title = "Awesome New Title";

            var parms = new DynamicParameters();
            
            parms.Add("id", 1, DbType.Int32);
            parms.Add("bookTitle", title, DbType.String);
            sqlConn.Execute(sql, parms);
        }

        public static void DeleteBook()
        {
            using var sqlConn = new SqlConnection(_connection);
            
            
            var id = 1;
            var sql = @"DELETE FROM Book 
                        WHERE id = @id";

            var title = "Awesome New Title";

            sqlConn.Execute(sql, new {id, title});
        }

        public static void CallStoredProc()
        {
            var sp = "uspGetBookByTitle";
            var sqlParams = new DynamicParameters();
            sqlParams.Add("title", "It isn't much, but it's honest work");

            using var sqlConn = new SqlConnection(_connection);
            var book = sqlConn.QueryFirst<Book>(sp, sqlParams, commandType: CommandType.StoredProcedure);

            Console.WriteLine($"Title: {book.Title} - Price: {book.Price} - AuthorId: {book.AuthorId}");
        }

        public static void InStatement()
        {
            var ids = new List<int> {1, 2, 3, 4};
            var sql = "SELECT * FROM Author WHERE id IN @ids";
            using var sqlConn = new SqlConnection(_connection);
            var authors = sqlConn.Query<Author>(sql, new {ids});
            PrintAuthors(authors);
        }

        public static void MultiInsert()
        {
            var authors = new List<Author>();
            var author = new Author
            {
                Name = "new name"
            };
            var author2 = new Author
            {
                Name = "another new name"
            };

            authors.Add(author);
            authors.Add(author2);

            using var sqlConn = new SqlConnection(_connection);
            var sql = @"INSERT INTO Author([name]) values(@Name)";
            var result = sqlConn.Execute(sql, authors);
            Console.WriteLine(result);

        }

        public static void QueryMultiple()
        {
            var sql = @"select *
                        from Author;
                        select *
                        from Book;";
            
            using var sqlConn = new SqlConnection(_connection);
            var result = sqlConn.QueryMultiple(sql);
            var authors = result.Read<Author>();
            var books = result.Read<Book>();
            PrintAuthors(authors);
            PrintBooks(books);
        }

        public static void Transactions()
        {
            using var sqlConn = new SqlConnection(_connection);
            sqlConn.Open();
            var transaction = sqlConn.BeginTransaction();
            var id = 1;
            var sql = @"DELETE FROM Book 
                        WHERE id = @id";

            var title = "Awesome New Title";

            sqlConn.Execute(sql, new {id, title}, transaction: transaction);
            transaction.Commit();
        }
        
        private static void PrintBooks(IEnumerable<Book> books)
        {
            foreach (var book in books)
            {
                Console.WriteLine($"book title: {book.Title}");
            }
        }
        
        private static void PrintBooksWithAuthor(IEnumerable<Book> books)
        {
            foreach (var book in books)
            {
                Console.WriteLine(
                    $"Id: {book.Id} - Title: {book.Title} - {book.Price} - AuthorId: {book.AuthorId} - Author Name: {book.Author.Name}");
            }
        }

        private static void PrintAuthors(IEnumerable<Author> authors)
        {
            foreach (var author in authors)
            {
                Console.WriteLine($"Id: {author.Id} | Name: {author.Name}");
            }
        }

        private static void PrintAuthorAndBooks(Author author)
        {
            Console.WriteLine($"Id: {author.Id} | Name: {author.Name}");
            foreach (var authorBook in author.Books)
            {
                Console.WriteLine($"********{authorBook.Title}");
            }
        }

        public static void SetupDapperMap<T>()
        {
            Dapper.SqlMapper.SetTypeMap(
                typeof(T),
                new Dapper.CustomPropertyTypeMap(
                    typeof(T),
                    (type, columnName) =>
                        type.GetProperties().FirstOrDefault(prop =>
                            prop.GetCustomAttributes(false)
                                .OfType<ColumnAttribute>()
                                .Any(attr => attr.Name == columnName) || prop.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))));
        }
    }
}
