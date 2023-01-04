using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace CoonInformationViewer.Models.Db
{
    public class SqlTransactionExecutor : IDisposable
    {
        private readonly SqlExecutor _executor;
        private readonly SqliteTransaction _transaction;

        private readonly Action<string> _sqlExecuted;

        public SqlTransactionExecutor(SqlExecutor executor, SqliteTransaction transaction, Action<string> sqlExecuted = null)
        {
            _executor = executor;
            _transaction = transaction;
            _sqlExecuted = sqlExecuted;
        }

        public IEnumerable<Dictionary<string, SelectValue>> Execute(SqlCreator creator, params SelectParameter[] values)
        {
            return _executor.Execute(creator, this, values);
        }

        public int ExecuteNonQuery(SqlCreator creator, params SelectParameter[] values)
        {
            return _executor.ExecuteNonQuery(creator, this, values);
        }

        public void Commit()
        {
            _transaction.Commit();
            _sqlExecuted("commit;");
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _sqlExecuted("rollback;");
        }

        internal SqliteTransaction GetTransaction()
        {
            return _transaction;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}