using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.Data.Sqlite;
using CookInformationViewer.Models.Extensions;

namespace CookInformationViewer.Models.Db.Raw
{
    public class SqlExecutor : IDisposable
    {
        private readonly SqliteConnection _connection;

        private readonly Subject<string> _sqlExecuted = new Subject<string>();
        public IObservable<string> SqlExecutedObservable => _sqlExecuted;

        public SqlExecutor(string source, bool register = false)
        {
            if (register)
            {
                File.Delete(source);
            }

            var fileInfo = new FileInfo(source);
            if (!fileInfo.Exists)
            {
                var parent = Path.GetDirectoryName(fileInfo.FullName);
                if (string.IsNullOrEmpty(parent))
                    throw new DirectoryNotFoundException(fileInfo.FullName);

                var di = new DirectoryInfo(parent);
                if (!di.Exists)
                    di.Create();
            }

            _connection = new SqliteConnection($"Data Source={source}");
            _connection.Open();
        }

        public SqlTransactionExecutor BeginTransaction()
        {
            var tran = new SqlTransactionExecutor(this, _connection.BeginTransaction(), _sqlExecuted.OnNext);
            _sqlExecuted.OnNext("begin transaction;");
            return tran;
        }

        private SqliteCommand PrepareQuery(SqlCreator creator, SqlTransactionExecutor? transaction, params SelectParameter[] values)
        {
            var command = new SqliteCommand
            {
                Connection = _connection,
                CommandText = creator.ToString()
            };

            if (transaction != null)
                command.Transaction = transaction.GetTransaction();

            foreach (var selectParameter in values)
            {
                object item = DBNull.Value;
                if (selectParameter.Value != null)
                    item = selectParameter.Value;
                command.Parameters.AddWithValue($"@{selectParameter.ColumnName}", item);
            }

            return command;
        }

        public void CreateTable(TableInfo tableInfo)
        {
            var creator = SqlCreator.CreateTable(tableInfo);
            ExecuteNonQuery(creator);
        }

        public bool ExistsTable(string tableName) => ExistsObject(tableName, "table");

        public bool ExistsView(string viewName) => ExistsObject(viewName, "view");

        public bool ExistsObject(string name, string type)
        {
            var creator = SqlCreator.Create($"select name from sqlite_master where type = '{type}' and name = '{name}'");
            using var command = PrepareQuery(creator, null);

            using var reader = command.ExecuteReader();

            _sqlExecuted.OnNext(creator.ToString());

            return reader.HasRows;
        }

        public IEnumerable<string> ExistsObjects(string type, params string[] names)
        {
            var nameText = $"'{string.Join("', '", names)}'";
            var creator = SqlCreator.Create($"select name from sqlite_master where type = '{type}' and name in ({nameText})");

            var notExistsObjects = new List<string>(names);
            var hashSet = new HashSet<string>(names);
            var result = Execute(creator);
            foreach (var dict in result)
            {
                const string key = "name";
                var name = dict.Get(key, new SelectValue("", ColumnType.Undefined)).GetValue<string>();
                if (hashSet.Contains(name))
                    notExistsObjects.Remove(name);
            }

            return notExistsObjects;
        }

        public IEnumerable<ColumnInfo> GetTableColumns(string tableName)
        {
            var creator = SqlCreator.Create($"pragma table_info({tableName});");

            var result = Execute(creator).ToList();
            var names = result.Select(x =>
                x.Where(y => y.Key == "name")
                    .Select(y => y.Value).First()).ToList();
            var types = result.Select(x =>
                x.Where(y => y.Key == "type")
                    .Select(y => y.Value).First()).ToList();
            var merge = names.Zip(types, (x, y) => new ColumnInfo
            {
                ColumnName = x.ToString(),
                Type = ColumnConverter.ToColumnType(y.ToString())
            });

            return merge;
        }

        public int ExecuteNonQuery(SqlCreator creator, SqlTransactionExecutor? transaction, params SelectParameter[] values)
        {
            using var command = PrepareQuery(creator, transaction, values);

            var line = command.ExecuteNonQuery();

            _sqlExecuted.OnNext($"{creator}\n[{string.Join(",", values.Select(x => x.Value))}]");

            return line;
        }

        public int ExecuteNonQuery(SqlCreator creator, params SelectParameter[] values)
        {
            return ExecuteNonQuery(creator, null, values);
        }

#if NET48
        private static IEnumerable<(string Type, string ColumnName)> GetColumnSchemaCompatibility(DbDataReader reader)
        {
            var columnSchema = new List<(string, string)>();
            var schemaTable = reader.GetSchemaTable();
            if (schemaTable != null)
            {
                foreach (DataRow row in schemaTable.Rows)
                {
                    columnSchema.Add((row["DataTypeName"].ToString(), row["ColumnName"].ToString()));
                }
            }
            return new ReadOnlyCollection<(string, string)>(columnSchema);
        }
#endif

        public IEnumerable<Dictionary<string, SelectValue>> Execute(SqlCreator creator, SqlTransactionExecutor? transaction,
            params SelectParameter[] values)
        {
            using var command = PrepareQuery(creator, transaction, values);

            using var reader = command.ExecuteReader();
#if NET48
            var schemes = GetColumnSchemaCompatibility(reader)
                .Select(x => new { Name = x.ColumnName, Type = ColumnConverter.ToColumnType(x.Type) }).ToList();
#else
            var schemes = reader.GetColumnSchema()
                .Select(x => new { Name = x.ColumnName, Type = ColumnConverter.ToColumnType(x.DataTypeName) }).ToList();
#endif

            _sqlExecuted.OnNext($"{creator}\n[{string.Join(",", values.Select(x => x.Value))}]");

            var list = new List<Dictionary<string, SelectValue>>();
            while (reader.Read())
            {
                var dict = new Dictionary<string, SelectValue>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    dict.Add(schemes[i].Name, new SelectValue(reader[i], schemes[i].Type));
                }
                list.Add(dict);
            }

            return list;
        }

        public IEnumerable<Dictionary<string, SelectValue>> Execute(SqlCreator creator, params SelectParameter[] values)
        {
            return Execute(creator, null, values);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}