/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Services.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TSQL;
using TSQL.Tokens;
using TSQL.Statements;

namespace Dev2.Sql.Tests
{
	[TestClass]
	public class SQLiteServerTests
	{
		#region SqliteServer_Connect
		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_Connect")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_Connect_ConnectionStringIsNull_ThrowsArgumentNullException()
		{
			//------------Setup for test--------------------------
			var sqliteServer = new SqliteServer();
			try
			{
				//------------Execute Test---------------------------
				Assert.ThrowsException<ArgumentNullException>(() => sqliteServer.Connect(null, CommandType.Text, null));
				//------------Assert Results-------------------------
			}
			finally
			{
				sqliteServer.Dispose();
			}
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_Connect")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_Connect_ConnectionStringIsInvalid_ThrowsArgumentException()
		{
			//------------Setup for test--------------------------
			var sqliteServer = new SqliteServer();
			try
			{
				//------------Execute Test---------------------------
				Assert.ThrowsException<ArgumentNullException>(() => sqliteServer.Connect("xxx", CommandType.Text, null));
				//------------Assert Results-------------------------
			}
			finally
			{
				sqliteServer.Dispose();
			}
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_Connect")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_Connect_CommandTextIsNull_ThrowsArgumentNullException()
		{
			//------------Setup for test--------------------------
			var sqliteServer = new SqliteServer();
			try
			{
				//------------Execute Test---------------------------
				Assert.ThrowsException<ArgumentNullException>(() => sqliteServer.Connect(":memory:", CommandType.Text, null));
				//------------Assert Results-------------------------
			}
			finally
			{
				sqliteServer.Dispose();
			}
		}
		#endregion

		#region Create Command
		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_CreateCommand")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_CreateCommand_ConnectionNotInitialized_ThrowsConnectFirstException()
		{
			//------------Setup for test--------------------------
			var sqliteServer = new SqliteServer();
			try
			{
				//------------Execute Test---------------------------
				Assert.ThrowsException<Exception>(() => sqliteServer.CreateCommand());
				//------------Assert Results-------------------------
			}
			finally
			{
				sqliteServer.Dispose();
			}
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_CreateCommand")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_CreateCommand_CommandIsNull_ThrowsArgumentNullException()
		{
			//------------Setup for test--------------------------
			var sqliteServer = new SqliteServer();
			try
			{
				//------------Execute Test---------------------------
				Assert.ThrowsException<ArgumentNullException>(() => sqliteServer.FetchDataTable((IDbCommand)null));
				//------------Assert Results-------------------------
			}
			finally
			{
				sqliteServer.Dispose();
			}
		}

		#endregion

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_SelectStatement_ReturnTableNames()
		{
			List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				@"select name,id,date from person; select address_id,line1,line2,postcode from Address",
				includeWhitespace: false);

			Assert.AreEqual(2, statements.Count);

			TSQLSelectStatement select1 = statements[0] as TSQLSelectStatement;

			foreach (TSQLToken token in select1.Select.Tokens)
			{
				Assert.AreEqual(TSQLStatementType.Select, statements[0].Type); Assert.AreEqual(TSQLStatementType.Select, statements[0].Type);
				if (token.Type.ToString() == "Identifier" && token.Text.ToLower() == "name")
				{
					Assert.AreEqual("name", token.Text.ToLower());
				}
				if (token.Type.ToString() == "Identifier" && token.Text.ToLower() == "id")
				{

					Assert.AreEqual("id", token.Text.ToLower());
				}
				if (token.Type.ToString() == "Identifier" && token.Text.ToLower() == "date")
				{

					Assert.AreEqual("date", token.Text.ToLower());
				}
			}
			foreach (TSQLToken token in select1.From.Tokens)
			{
				if (token.Type.ToString() == "Identifier")
				{
					Assert.AreEqual("person", token.Text.ToLower());
				}
			}

			TSQLSelectStatement select2 = statements[1] as TSQLSelectStatement;
			Assert.AreEqual(TSQLStatementType.Select, statements[1].Type);
			foreach (TSQLToken token in select2.Select.Tokens)
			{
				Assert.AreEqual(TSQLStatementType.Select, statements[1].Type); Assert.AreEqual(TSQLStatementType.Select, statements[0].Type);
				if (token.Type.ToString() == "Identifier" && token.Text.ToLower() == "address_id")
				{
					Assert.AreEqual("address_id", token.Text.ToLower());
				}
				if (token.Type.ToString() == "Identifier" && token.Text.ToLower() == "line1")
				{

					Assert.AreEqual("line1", token.Text.ToLower());
				}
				if (token.Type.ToString() == "Identifier" && token.Text.ToLower() == "line2")
				{
					Assert.AreEqual("line2", token.Text.ToLower());
				}
				if (token.Type.ToString() == "Identifier" && token.Text.ToLower() == "postcode")
				{
					Assert.AreEqual("postcode", token.Text.ToLower());
				}
			}
			foreach (TSQLToken token in select2.From.Tokens)
			{
				if (token.Type.ToString() == "Identifier")
				{
					Assert.AreEqual("address", token.Text.ToLower());
				}
			}
		}

		#region SqliteServer_ParseTSQL
		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_UnknownStatement_MissingFirstToken()
		{
			//------------Setup for test--------------------------

			try
			{
				//------------Execute Test---------------------------
				List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				@"DECLARE @Location VARCHAR(4)
				DECLARE @ReportDate DATETIME

				SET @Location = '1010'
				SET @ReportDate = '01/09/18'",
				includeWhitespace: false);

				Assert.IsTrue(statements[0].Tokens[0].IsKeyword(TSQLKeywords.DECLARE));
				//------------Assert Results-------------------------
			}
			finally
			{

			}
		}
		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_SelectStatement_SelectLiteral()
		{
			//------------Setup for test--------------------------

			try
			{
				//------------Execute Test---------------------------
				List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				"select 1;",
				includeWhitespace: true);
				TSQLSelectStatement select = statements[0] as TSQLSelectStatement;

				Assert.IsNotNull(statements);
				Assert.AreEqual(1, statements.Count);
				Assert.AreEqual(TSQLStatementType.Select, statements[0].Type);
				Assert.AreEqual(3, select.Tokens.Count);
				Assert.AreEqual(TSQLKeywords.SELECT, select.Tokens[0].AsKeyword.Keyword);
				Assert.AreEqual(" ", select.Tokens[1].AsWhitespace.Text);
				Assert.AreEqual("1", select.Tokens[2].AsNumericLiteral.Text);
				//------------Assert Results-------------------------
			}
			finally
			{

			}
		}
		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		public void SqliteServer_ParseTSQL_SelectStatement_SelectLiteralNoWhitespace()
		{
			List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				"select 1;",
				includeWhitespace: false);
			TSQLSelectStatement select = statements[0] as TSQLSelectStatement;

			Assert.IsNotNull(statements);
			Assert.AreEqual(1, statements.Count);
			Assert.AreEqual(TSQLStatementType.Select, statements[0].Type);
			Assert.AreEqual(2, select.Tokens.Count);
			Assert.AreEqual(TSQLKeywords.SELECT, select.Tokens[0].AsKeyword.Keyword);
			Assert.AreEqual("1", select.Tokens[1].AsNumericLiteral.Text);
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_SelectStatement_TwoLiteralSelects()
		{
			List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				"select 1;select 2;",
				includeWhitespace: true);
			TSQLSelectStatement select1 = statements[0] as TSQLSelectStatement;
			TSQLSelectStatement select2 = statements[1] as TSQLSelectStatement;

			Assert.IsNotNull(statements);
			Assert.AreEqual(2, statements.Count);

			Assert.AreEqual(TSQLStatementType.Select, select1.Type);
			Assert.AreEqual(3, select1.Tokens.Count);
			Assert.AreEqual(TSQLKeywords.SELECT, select1.Tokens[0].AsKeyword.Keyword);
			Assert.AreEqual(" ", select1.Tokens[1].AsWhitespace.Text);
			Assert.AreEqual("1", select1.Tokens[2].AsNumericLiteral.Text);

			Assert.AreEqual(TSQLStatementType.Select, select2.Type);
			Assert.AreEqual(3, select2.Tokens.Count);
			Assert.AreEqual(TSQLKeywords.SELECT, select2.Tokens[0].AsKeyword.Keyword);
			Assert.AreEqual(" ", select2.Tokens[1].AsWhitespace.Text);
			Assert.AreEqual("2", select2.Tokens[2].AsNumericLiteral.Text);
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_SelectStatement_CorrelatedSelect()
		{
			List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				"select (select 1);",
				includeWhitespace: true);
			TSQLSelectStatement select = statements[0] as TSQLSelectStatement;

			Assert.IsNotNull(statements);
			Assert.AreEqual(1, statements.Count);
			Assert.AreEqual(TSQLStatementType.Select, statements[0].Type);
			Assert.AreEqual(7, statements[0].Tokens.Count);
			Assert.AreEqual(TSQLKeywords.SELECT, select.Tokens[0].AsKeyword.Keyword);
			Assert.AreEqual(" ", select.Tokens[1].AsWhitespace.Text);
			Assert.AreEqual("(", select.Tokens[2].AsCharacter.Text);
			Assert.AreEqual("select", select.Tokens[3].AsKeyword.Text);
			Assert.AreEqual(" ", select.Tokens[4].AsWhitespace.Text);
			Assert.AreEqual("1", select.Tokens[5].AsNumericLiteral.Text);
			Assert.AreEqual(")", select.Tokens[6].AsCharacter.Text);
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_SelectStatement_CommonSelect()
		{
			List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				@"select t.a, t.b, (select 1) as e
				into #tempt
				from
					[table] t
						inner join [table] t2 on
							t.id = t2.id
				where
					t.c = 5
				group by
					t.a,
					t.b
				having
					count(*) > 1
				order by
					t.a,
					t.b;",
				includeWhitespace: true);
			TSQLSelectStatement select = statements[0] as TSQLSelectStatement;

			Assert.IsNotNull(statements);
			Assert.AreEqual(1, statements.Count);
			Assert.AreEqual(TSQLStatementType.Select, statements[0].Type);
			Assert.AreEqual(98, select.Tokens.Count);
			Assert.AreEqual(TSQLKeywords.SELECT, select.Tokens[0].AsKeyword.Keyword);
			Assert.AreEqual(" ", select.Tokens[1].AsWhitespace.Text);
			Assert.AreEqual("t", select.Tokens[2].AsIdentifier.Name);
			Assert.AreEqual(TSQLCharacters.Period, select.Tokens[3].AsCharacter.Character);
			Assert.AreEqual(22, select.Select.Tokens.Count);
			Assert.AreEqual(4, select.Into.Tokens.Count);
			Assert.AreEqual(26, select.From.Tokens.Count);
			Assert.AreEqual(10, select.Where.Tokens.Count);
			Assert.AreEqual(13, select.GroupBy.Tokens.Count);
			Assert.AreEqual(11, select.Having.Tokens.Count);
			Assert.AreEqual(12, select.OrderBy.Tokens.Count);
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_SelectStatement_MultipleSelectsWithoutSemicolon()
		{
			List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				"select 1 select 1",
				includeWhitespace: true);
			TSQLSelectStatement select1 = statements[0] as TSQLSelectStatement;
			TSQLSelectStatement select2 = statements[1] as TSQLSelectStatement;

			Assert.IsNotNull(statements);
			Assert.AreEqual(2, statements.Count);

			Assert.AreEqual(TSQLStatementType.Select, select1.Type);

			Assert.AreEqual(TSQLStatementType.Select, select2.Type);
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_SelectStatement_Option()
		{
			List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				@"SELECT *
				FROM
					Sales.SalesOrderHeader oh
				OPTION (FAST 10)",
				includeWhitespace: false);

			Assert.AreEqual(1, statements.Count);
			Assert.AreEqual(TSQLStatementType.Select, statements[0].Type);

			TSQLSelectStatement select = statements[0] as TSQLSelectStatement;

			Assert.AreEqual(12, select.Tokens.Count);
			Assert.IsNotNull(select.Option);
			Assert.AreEqual(5, select.Option.Tokens.Count);
		}

		[TestMethod]
		[Owner("Candice Daniel")]
		[TestCategory("SqliteServer_ParseTSQL")]
		[DeploymentItem(@"x86\SQLite.Interop.dll")]
		public void SqliteServer_ParseTSQL_SelectStatement_DontEatFinalDescAsKeyword()
		{
			List<TSQLStatement> statements = TSQLStatementReader.ParseStatements(
				@"select 1 as blah order by 1 desc select 1",
				includeWhitespace: false);

			Assert.AreEqual(2, statements.Count);
			Assert.AreEqual(TSQLStatementType.Select, statements[0].Type);
			Assert.AreEqual(TSQLStatementType.Select, statements[1].Type);

			TSQLSelectStatement select1 = statements[0] as TSQLSelectStatement;
			TSQLSelectStatement select2 = statements[1] as TSQLSelectStatement;

			Assert.AreEqual(8, select1.Tokens.Count);

			Assert.AreEqual(2, select2.Tokens.Count);
		}
		#endregion
		
	}
}