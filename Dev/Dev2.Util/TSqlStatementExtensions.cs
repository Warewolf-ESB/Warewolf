/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Globalization;
using TSQL.Statements;
using TSQL.Tokens;

namespace Dev2.Util
{
    public static class TSqlStatementExtensions
    {
        public static List<TSqlTable> GetAllTables(this TSQLStatement statement)
        {
            var tables = new List<TSqlTable>();
            var tableNames = GetPreTableIndexes(statement);
            foreach (var tableName in tableNames)
            {
                if (!tables.Exists(e => e.TableName == tableName))
                {
                    var table = new TSqlTable
                    {
                        TableName = tableName
                    };
                    tables.Add(table);
                }
            }
            return tables;
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        private static List<string> GetPreTableIndexes(TSQLStatement statement)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var tokens = statement.Tokens;
            var countOfTokens = tokens.Count;
            var canProcess = false;
            var tableNames = new List<string>();
            for (int i = 0; i < countOfTokens; i++)
            {
                var tokenValue = tokens[i].Text.ToUpper(CultureInfo.InvariantCulture);
                switch (tokenValue)
                {
                    case "FROM":
                    case "JOIN":
                    case "INNER":
                    case "OUTER":
                    case "NATURAL":
					case "UPDATE":
					case "INSERT":
					case "CROSS":
					case "INTO":
						if (tokens[i].Type == TSQLTokenType.Keyword)
                        {
                            canProcess = true;
                            continue;
                        }
                        break;
					case "SET":
                    case "WHERE":
                    case "ON":
                    case "USING":
                    case "DELETE":
                    case "SELECT":
					case "ORDER":
                    case "GROUP":
                    case "HAVING":
                        if (tokens[i].Type == TSQLTokenType.Keyword)
                        {
                            canProcess = false;
                        }
                        break;
                    case ")":
                    case "OFFSET":                        
					case "(":
						canProcess = false;
                        break;
                    default:
                        break;
                }
                if (canProcess)
                {
                    if (SkipToken(tokens[i - 1]))
                    {
                        continue;
                    }
                    tableNames.Add(tokenValue.ToLower(CultureInfo.InvariantCulture));
                }
            }
            return tableNames;
        }

        private static bool SkipToken(TSQLToken token) 
            => token.Type == TSQLTokenType.Identifier || 
            (token.Type == TSQLTokenType.Keyword && token.Text.ToUpper(CultureInfo.InvariantCulture) == "AS");

    }

    public struct TSqlTable
    {
        public string TableName { get; set; }

        public TSqlTable(string tableName, string tableAlias)
        {
            TableName = tableName;
        }
    }

    public class TSqlField
    {
        private readonly List<string> _tokens;

        public TSqlField()
        {
            _tokens = new List<string>();
        }

        public string FieldName { get; set; }
        public string TableAlias { get; set; }
        public string FieldNameAlias { get; set; }

        internal void AddFieldToken(string text)
        {
            _tokens.Add(text);
        }

        internal void ProcessTokens()
        {
            if (_tokens.Count == 1)
            {
                FieldName = _tokens[0];
            }
            else
            {
                var tableAliasIndicator = _tokens.IndexOf(".");
                if (tableAliasIndicator != -1)
                {
                    TableAlias = _tokens[tableAliasIndicator - 1];
                    FieldName = _tokens[tableAliasIndicator + 1];
                }
                var columnAliasIndicator = _tokens.FindIndex(s => s.ToUpper(CultureInfo.InvariantCulture) == "AS");
                if (columnAliasIndicator != -1)
                {
                    FieldNameAlias = _tokens[columnAliasIndicator + 1];
                    if (!string.IsNullOrEmpty(FieldNameAlias))
                    {
                        FieldName = FieldNameAlias;
                    }
                }
            }
        }
    }
}

