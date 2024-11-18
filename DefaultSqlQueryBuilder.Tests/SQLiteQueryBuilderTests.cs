using DefaultSqlQueryBuilder.SqlSyntaxes;
using DefaultSqlQueryBuilder.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DefaultSqlQueryBuilder.Tests
{
	[TestClass]
	public class SQLiteQueryBuilderTests
	{
		private static SqlQueryBuilder CreateSqlQueryBuilder() => new(new SQLiteSyntax());

		[TestMethod]
		public void SkipBeforeTakeTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => "*")
				.Skip(2)
				.Take(3)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM \"User\"",
				"LIMIT 3",
				"OFFSET 2"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SkipAfterTakeTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => "*")
				.Take(3)
				.Skip(2)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM \"User\"",
				"LIMIT 3",
				"OFFSET 2"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SkipWithoutTakeTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => "*")
				.Skip(2)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM \"User\"",
				"LIMIT -1",
				"OFFSET 2"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void TakeWithoutSkipTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => "*")
				.Take(3)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM \"User\"",
				"LIMIT 3"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		private class User
		{
			public int Id { get; set; }
			public int AddressId { get; set; }
			public int UserGroupId { get; set; }
			public string Name { get; set; } = "";
			public int Age { get; set; }
		}
	}
}
