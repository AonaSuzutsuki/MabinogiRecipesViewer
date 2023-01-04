using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoonInformationViewer.Models.Db
{
    public class SqlCreator
    {
        public enum SqlOrder
        {
            Default,
            Select,
            Update,
            Insert,
            Delete,
            Join,
            Where,
            GroupBy,
            Having,
            OrderBy,
            Limit
        }

        public class SqlElement : IComparable
        {
            public string Sql { get; set; }
            public SqlOrder Order { get; set; }
            public int CompareTo(object? obj)
            {
                if (!(obj is SqlElement element))
                    return 1;

                return Order.CompareTo(element.Order);
            }
        }

        //private string _sql;
        private readonly List<SqlElement> _sqlList = new List<SqlElement>();

        public string TableName { get; private set; }

        private SqlCreator(string query)
        {
            _sqlList.Add(new SqlElement
            {
                Sql = query,
                Order = SqlOrder.Default
            });
        }

        private SqlCreator() { }

        public static SqlCreator Create(string query) => new SqlCreator(query);

        public static SqlCreator Select(string selector, string tableName)
        {
            var sql = $"select {selector} from {tableName}";
            var creator = new SqlCreator { TableName = tableName };
            creator._sqlList.Add(new SqlElement
            {
                Sql = sql,
                Order = SqlOrder.Select
            });
            return creator;
        }

        public static SqlCreator Update(string tableName, params string[] names)
        {
            var varNames = string.Join(", ", names.Select(x => $"{x} = @{x}"));
            var sql = $"update {tableName} set {varNames}";
            var creator = new SqlCreator { TableName = tableName };
            creator._sqlList.Add(new SqlElement
            {
                Sql = sql,
                Order = SqlOrder.Update
            });
            return creator;
        }

        public static SqlCreator Insert(string tableName, params string[] names)
        {
            var enumerable = names;
            var varNames = string.Join(", ", enumerable.Select(x => $"@{x}"));
            var sql = $"insert into {tableName}({string.Join(", ", enumerable)}) values({varNames})";
            var creator = new SqlCreator { TableName = tableName };
            creator._sqlList.Add(new SqlElement
            {
                Sql = sql,
                Order = SqlOrder.Insert
            });
            return creator;
        }

        public static SqlCreator Delete(string tableName)
        {
            var sql = $"delete from {tableName}";
            var creator = new SqlCreator { TableName = tableName };
            creator._sqlList.Add(new SqlElement
            {
                Sql = sql,
                Order = SqlOrder.Delete
            });
            return creator;
        }

        public SqlCreator Where(string whereCond)
        {
            _sqlList.Add(new SqlElement
            {
                Sql = $"where {whereCond}",
                Order = SqlOrder.Where
            });
            return this;
        }

        public SqlCreator GroupBy(params string[] names)
        {
            var name = string.Join(", ", names);
            _sqlList.Add(new SqlElement
            {
                Sql = $"group by {name}",
                Order = SqlOrder.GroupBy
            });
            return this;
        }

        public SqlCreator Having(string cond)
        {
            _sqlList.Add(new SqlElement
            {
                Sql = $"having {cond}",
                Order = SqlOrder.Having
            });
            return this;
        }

        public SqlCreator OrderBy(params string[] names)
        {
            var name = string.Join(", ", names);
            _sqlList.Add(new SqlElement
            {
                Sql = $"order by {name}",
                Order = SqlOrder.OrderBy
            });
            return this;
        }

        public SqlCreator InnerJoin(string tableName, string cond)
        {
            _sqlList.Add(new SqlElement
            {
                Sql = $"inner join {tableName} on {cond}",
                Order = SqlOrder.Join
            });
            return this;
        }

        public SqlCreator LeftJoin(string tableName, string cond)
        {
            _sqlList.Add(new SqlElement
            {
                Sql = $"left join {tableName} on {cond}",
                Order = SqlOrder.Join
            });
            return this;
        }

        public SqlCreator RightJoin(string tableName, string cond)
        {
            _sqlList.Add(new SqlElement
            {
                Sql = $"right join {tableName} on {cond}",
                Order = SqlOrder.Join
            });
            return this;
        }

        public SqlCreator Limit(int limit, int offset)
        {
            _sqlList.Add(new SqlElement
            {
                Sql = $"limit {limit} offset {offset}",
                Order = SqlOrder.Limit
            });
            return this;
        }

        public SqlCreator CreateView(string viewName)
        {
            var sql = ToString(false);

            var firstQuery = _sqlList.First().Sql.Split(' ').FirstOrDefault();
            if (firstQuery != "select")
                throw new ArgumentException("Only the SELECT clause can be created as a view.");

            sql = $"create view {viewName} as {sql}";
            return new SqlCreator(sql)
            {
                TableName = viewName
            };
        }

        public static SqlCreator CreateTable(TableInfo table)
        {
            var tableInfos = table.Columns.ToList();
            var primaryKeys = (from x in tableInfos where x.PrimaryKey select x).ToList();
            var autoIncrements = (from x in primaryKeys where x.AutoIncrement select x).ToList();
            var foreignKeys = (from x in tableInfos where x.ForeignKey != null select x).ToList();

            var sb = new StringBuilder();
            sb.Append($"create table {table.TableName}(\n    ");
            sb.Append(string.Join(",\n    ", tableInfos));

            if (autoIncrements.Count > 0)
            {
                var elem = autoIncrements[0];
                sb.Append($",\n    primary key ({elem.ColumnName} autoincrement)");
            }

            if (primaryKeys.Count > 0 && autoIncrements.Count <= 0)
            {
                sb.Append(",\n    primary key (");
                sb.Append(string.Join(", ", primaryKeys.Select(x => x.ColumnName)));
                sb.Append(")");
            }

            if (foreignKeys.Count > 0)
            {
                foreach (var foreignKey in foreignKeys)
                {
                    sb.Append(
                        $",\n    foreign key ({foreignKey.ColumnName}) references {foreignKey.ForeignKey.Table.TableName}({foreignKey.ForeignKey.ColumnName})");
                }
            }

            sb.Append("\n)");

            var sql = sb.ToString();
            return new SqlCreator(sql)
            {
                TableName = table.TableName
            };
        }

        public string ToString(bool containSemicolon)
        {
            _sqlList.Sort();
            var sql = string.Join(" ", _sqlList.Select(x => x.Sql));

            return containSemicolon ? $"{sql};" : sql;
        }

        public override string ToString()
        {
            return ToString(true);
        }
    }
}