using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CommonCoreLib.Bool;

namespace CoonInformationViewer.Models.Db
{
    public class TableInfo : ICloneable
    {
        private IEnumerable<ColumnInfo> _columns;

        public string TableName { get; set; }

        public IEnumerable<ColumnInfo> Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                foreach (var columnInfo in value)
                {
                    columnInfo.Table = this;
                }
            }
        }

        public TableInfo Clone()
        {
            var tableInfo = new TableInfo
            {
                TableName = TableName,
                Columns = new List<ColumnInfo>(Columns)
            };

            return tableInfo;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public static bool EqualsTableInfo(TableInfo left, TableInfo right)
        {
            if (left == right)
                return true;

            var collector = new BoolCollector();
            collector.ChangeBool(nameof(left.TableName), left.TableName == right.TableName);
            collector.ChangeBool("count", left.Columns.Count() == right.Columns.Count());
            
            var columnZip = left.Columns.Zip(right.Columns, (l, r) => new { Left = l, Right = r });
            foreach (var item in columnZip)
            {
                var subCollector = new BoolCollector();
                subCollector.ChangeBool(nameof(item.Left.AutoIncrement), item.Left.AutoIncrement == item.Right.AutoIncrement);
                subCollector.ChangeBool(nameof(item.Left.Type), item.Left.Type == item.Right.Type);
                subCollector.ChangeBool(nameof(item.Left.PrimaryKey), item.Left.PrimaryKey == item.Right.PrimaryKey);
                subCollector.ChangeBool(nameof(item.Left.NotNull), item.Left.NotNull == item.Right.NotNull);
                subCollector.ChangeBool(nameof(item.Left.Default), Equals(item.Left.Default, item.Right.Default));
                subCollector.ChangeBool(nameof(item.Left.Unique), item.Left.Unique == item.Right.Unique);
                subCollector.ChangeBool(nameof(item.Left.ForeignKey), new Func<bool>(() =>
                {
                    var leftForeignKey = item.Left.ForeignKey;
                    var rightForeignKey = item.Right.ForeignKey;

                    if (leftForeignKey == null && rightForeignKey == null)
                        return true;
                    if (leftForeignKey == null || rightForeignKey == null)
                        return false;

                    return leftForeignKey.ColumnName == rightForeignKey.ColumnName
                        && leftForeignKey.Table.TableName == rightForeignKey.Table.TableName;
                }).Invoke());

                collector.ChangeBool(item.Left.ColumnName, subCollector.Value);
            }

            return collector.Value;
        }

        public static TableInfo ConvertFromCreateSql(SqlExecutor executor, string tableName)
        {
            var tableInfo = new TableInfo
            {
                TableName = tableName
            };
            var columns = new Dictionary<string, ColumnInfo>();

            var pragmaCreator = SqlCreator.Create($"PRAGMA table_info({tableName})");
            var tableInfoList = executor.Execute(pragmaCreator);

            foreach (var dict in tableInfoList)
            {
                var columnName = dict["name"].GetValue<string>();
                var type = dict["type"].GetValue<string>().ToLower();
                var dbType = ColumnConverter.ToColumnType(type);
                var notNull = dict["notnull"].GetValue<long>();
                var pk = dict["pk"].GetValue<long>();
                var defaultValue = dict["dflt_value"].GetValue<object>();

                if (defaultValue == System.DBNull.Value)
                    defaultValue = null;
                if (defaultValue is string defaultValueText)
                {
                    if (dbType == ColumnType.Text)
                        defaultValue = defaultValueText.Trim('\'');
                    else if (dbType == ColumnType.Integer)
                        defaultValue = long.Parse(defaultValueText);
                    else if (dbType == ColumnType.Real)
                        defaultValue = double.Parse(defaultValueText);
                }

                columns.Add(columnName, new ColumnInfo
                {
                    ColumnName = columnName,
                    Type = ColumnConverter.ToColumnType(type),
                    NotNull = notNull == 1,
                    PrimaryKey = pk == 1,
                    Default = defaultValue
                });
            }

            tableInfo.Columns = new List<ColumnInfo>(columns.Values);

            var masterCreator = SqlCreator.Select("sql", "\"main\".sqlite_master").Where($"type = 'table' and tbl_name = '{tableName}'");
            var tableSqlResult = executor.Execute(masterCreator);
            var tableSqlItem = tableSqlResult.FirstOrDefault();
            if (tableSqlItem == null)
                return tableInfo;

            var tableSql = tableSqlItem["sql"].GetValue<string>();

            var foreignKeyRegexPattern = "^[\\t\\s]*((?i)FOREIGN)[\\t\\s]+((?i)KEY)[\\t\\s]*\\(\"?(?<columnName>[a-zA-Z0-9_\\.\\-]+)\"?\\)[\\t\\s]+((?i)REFERENCES)[\\t\\s]+\"?(?<fTableName>[a-zA-Z0-9_\\.\\-]+)\"?[\\t\\s]*\\(\"?(?<fColumnName>[a-zA-Z0-9_\\.\\-]+)\"?\\)";
            var primaryKeyRegexPattern = "^[\\t\\s]*((?i)PRIMARY)[\\t\\s]+((?i)KEY)[\\t\\s]*\\(\"?(?<columnName>[a-zA-Z0-9_\\.\\-]+)\"?([\\t\\s]+(?<attributes>((?i)AUTOINCREMENT)))*\\)";
            var uniqueRegexPattern = "^[\\t\\s]+(?<columnName>[\\S]+)[\\t\\s]+(.*)[\\t\\s]+(?<unique>(?i)unique)";
            var foreignKeyRegex = new Regex(foreignKeyRegexPattern, RegexOptions.Multiline);
            var primaryKeyRegex = new Regex(primaryKeyRegexPattern, RegexOptions.Multiline);
            var uniqueRegex = new Regex(uniqueRegexPattern, RegexOptions.Multiline);

            var matches = foreignKeyRegex.Matches(tableSql);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var columnName = match.Groups["columnName"].ToString();
                    var foreignTableName = match.Groups["fTableName"].ToString();
                    var foreignColumnName = match.Groups["fColumnName"].ToString();

                    var column = tableInfo.Columns.First(x => x.ColumnName == columnName);
                    column.ForeignKey = new ColumnInfo
                    {
                        ColumnName = foreignColumnName,
                        Table = new TableInfo
                        {
                            TableName = foreignTableName
                        }
                    };
                }
            }

            var primaryKeyMatches = primaryKeyRegex.Matches(tableSql);
            foreach (Match match in primaryKeyMatches)
            {
                if (match.Success)
                {
                    var columnName = match.Groups["columnName"].ToString();
                    var isAutoIncrement = match.Groups["attributes"].ToString().ToLower() == "autoincrement";

                    var column = tableInfo.Columns.First(x => x.ColumnName == columnName);
                    column.AutoIncrement = isAutoIncrement;
                }
            }

            var uniqueMatches = uniqueRegex.Matches(tableSql);
            foreach (Match match in uniqueMatches)
            {
                if (match.Success)
                {
                    var columnName = match.Groups["columnName"].ToString();
                    var isUnique = match.Groups["unique"].ToString().ToLower() == "unique";

                    var column = tableInfo.Columns.First(x => x.ColumnName == columnName);
                    column.Unique = isUnique;
                }
            }

            return tableInfo;
        }
    }
}