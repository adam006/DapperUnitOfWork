using System;
using System.Data;

namespace DapperTutorial
{
    public interface IDapperDbContext : IDisposable
    {
        IDbTransaction Transaction { get; }
        IDbConnection Connection { get; }
        void BeginTransaction();
        void Commit();
        void Rollback();
    }

    public class DapperDbContext : IDapperDbContext
    {
        public IDbTransaction Transaction { get; private set; }

        public IDbConnection Connection { get; }

        public DapperDbContext(IDbConnection connection)
        { 
            Connection = connection;
        }

        public void BeginTransaction()
        {
            if(Connection.State != ConnectionState.Open)
                Connection.Open();
            Transaction = Connection.BeginTransaction();
        }

        public void Commit()
        {
            Transaction?.Commit();
        }

        public void Rollback()
        {
            Transaction?.Rollback();
        }
        
        public void Dispose()
        {
            if(Connection?.State == ConnectionState.Open)
                Connection.Close();
            Transaction?.Dispose();
        }
    }
}