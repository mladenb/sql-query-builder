using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlQueryBuilders.Contracts;
using SqlQueryBuilders.SqlSyntaxes;
using SqlQueryBuilders.Tests.Extensions;
using System;
using System.Linq;

namespace SqlQueryBuilders.Tests
{
	[TestClass]
	public class MsSqlQueryBuilderTests
	{
		private static SqlQueryBuilder CreateSqlQueryBuilder() => new(new MsSqlSyntax());

		[TestMethod]
		public void SelectWithoutWhereTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => "*")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM [User]"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SelectWithStringFormatTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => string.Format("TOP 10 *"))
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT TOP 10 *",
				"FROM [User]"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SelectWithCoalesceTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => $"TOP 10 *")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT TOP 10 *",
				"FROM [User]"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SelectWithTopWithoutWhereTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => $"TOP 10 {user.Age}")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT TOP 10 [User].[Age]",
				"FROM [User]"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SelectAllWithTopWithoutWhereTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => "TOP 10 *")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT TOP 10 *",
				"FROM [User]"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SelectWithWhereTest()
		{
			const string name = "John";

			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.Select(user => "*")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM [User]",
				"WHERE ([User].[Name] LIKE '%' + @0 + '%')"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(1, query.Parameters.Length);
			Assert.AreEqual(name, query.Parameters.First());
		}

		[TestMethod]
		public void SelectWithGroupByTest()
		{
			const string name = "John";

			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.GroupBy(user => $"{user.UserGroupId}")
				.Select(user => $"AVG({user.Age})")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT AVG([User].[Age])",
				"FROM [User]",
				"WHERE ([User].[Name] LIKE '%' + @0 + '%')",
				"GROUP BY [User].[UserGroupId]"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(1, query.Parameters.Length);
			Assert.AreEqual(name, query.Parameters.First());
		}

		[TestMethod]
		public void SelectWithJoinTest()
		{
			const string name = "John";
			var validUserGroupIds = new[] { 1, 2, 3 };

			var baseQuery = CreateSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.Select(user => "*");

			var joinQuery = baseQuery
				.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
				.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
				.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", validUserGroupIds)
				.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

			var query = joinQuery.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT [User].[Id], [User].[Name], [User].[Age]",
				"FROM [User]",
				"INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]",
				"INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]",
				"WHERE (([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] IN (@1)))"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(2, query.Parameters.Length);
			Assert.AreEqual(name, query.Parameters.First());
		}

		[TestMethod]
		public void SelectWithMultipleJoinsAndWheresTest()
		{
			const string name = "John";
			var validUserGroupIds = new[] { 1, 2, 3 };

			var baseQuery = CreateSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.Select(user => "*");

			var joinQuery = baseQuery
				.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
				.Where((user, address) => $"{user.UserGroupId} = 1")
				.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
				.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", validUserGroupIds)
				.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

			var query = joinQuery.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT [User].[Id], [User].[Name], [User].[Age]",
				"FROM [User]",
				"INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]",
				"INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]",
				"WHERE ((([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] = 1)) AND ([User].[UserGroupId] IN (@1)))"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(2, query.Parameters.Length);
			Assert.AreEqual(name, query.Parameters.First());
		}

		[TestMethod]
		public void InsertSingleTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var query = CreateSqlQueryBuilder()
				.Insert<User>(user => $"{user.Age}, {user.AddressId}, {user.Name}", age, addressId, name)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"INSERT INTO [User] ([User].[Age], [User].[AddressId], [User].[Name])",
				"VALUES (@0, @1, @2)"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(3, query.Parameters.Length);
			Assert.AreEqual(age, query.Parameters[0]);
			Assert.AreEqual(addressId, query.Parameters[1]);
			Assert.AreEqual(name, query.Parameters[2]);
		}

		[TestMethod]
		public void InsertMultipleTest()
		{
			var users = new[]
			{
				new User
				{
					Name = "John",
					Age = 10,
					AddressId = 1,
				},
				new User
				{
					Name = "Jane",
					Age = 20,
					AddressId = 2,
				},
				new User
				{
					Name = "Smith",
					Age = 30,
					AddressId = 3,
				},
			};

			var parameters = users.Select(u => new object[] { u.Age, u.AddressId, u.Name }).ToArray();

			var query = CreateSqlQueryBuilder()
				.InsertMultiple<User>(user => $"{user.Age}, {user.AddressId}, {user.Name}", parameters)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"INSERT INTO [User] ([User].[Age], [User].[AddressId], [User].[Name])",
				"VALUES (@0, @1, @2), (@3, @4, @5), (@6, @7, @8)"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(9, query.Parameters.Length);

			Assert.AreEqual(parameters[0][0], query.Parameters[0]);
			Assert.AreEqual(parameters[0][1], query.Parameters[1]);
			Assert.AreEqual(parameters[0][2], query.Parameters[2]);

			Assert.AreEqual(parameters[1][0], query.Parameters[3]);
			Assert.AreEqual(parameters[1][1], query.Parameters[4]);
			Assert.AreEqual(parameters[1][2], query.Parameters[5]);

			Assert.AreEqual(parameters[2][0], query.Parameters[6]);
			Assert.AreEqual(parameters[2][1], query.Parameters[7]);
			Assert.AreEqual(parameters[2][2], query.Parameters[8]);
		}

		[TestMethod]
		public void UpdateWithoutWhereTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var query = CreateSqlQueryBuilder()
				.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1, {user.Name} = @2", age, addressId, name)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"UPDATE [User]",
				"SET [User].[Age] = @0, [User].[AddressId] = @1, [User].[Name] = @2"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(3, query.Parameters.Length);
			Assert.AreEqual(age, query.Parameters[0]);
			Assert.AreEqual(addressId, query.Parameters[1]);
			Assert.AreEqual(name, query.Parameters[2]);
		}

		[TestMethod]
		public void UpdateWithWhereTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var query = CreateSqlQueryBuilder()
				.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1", age, addressId)
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"UPDATE [User]",
				"SET [User].[Age] = @0, [User].[AddressId] = @1",
				"WHERE ([User].[Name] LIKE '%' + @2 + '%')"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(3, query.Parameters.Length);
			Assert.AreEqual(age, query.Parameters[0]);
			Assert.AreEqual(addressId, query.Parameters[1]);
			Assert.AreEqual(name, query.Parameters[2]);
		}

		[TestMethod]
		public void DeleteWithoutWhereTest()
		{
			var query = CreateSqlQueryBuilder()
				.Delete<User>()
				.ToSqlQuery();

			var expectedResult = "DELETE FROM [User]";

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void DeleteWithWhereTest()
		{
			const string name = "John";

			var query = CreateSqlQueryBuilder()
				.Delete<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"DELETE FROM [User]",
				"WHERE ([User].[Name] LIKE '%' + @0 + '%')"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(1, query.Parameters.Length);
			Assert.AreEqual(name, query.Parameters[0]);
		}

		[TestMethod]
		public void CustomStatementFirstTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var customInsertQuery = CreateSqlQueryBuilder()
				.Custom<User>(u => $"INSERT INTO {u} ({u.Name}, {u.Age}, {u.AddressId}) OUTPUT INSERTED.Id VALUES (@0, @1, @2)",
					name, age, addressId)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"INSERT INTO [User] ([User].[Name], [User].[Age], [User].[AddressId])",
				"OUTPUT INSERTED.Id",
				"VALUES (@0, @1, @2)"
			);

			Assert.That.SqlsAreEqual(expectedResult, customInsertQuery.Sql);
			Assert.AreEqual(3, customInsertQuery.Parameters.Length);
			Assert.AreEqual(name, customInsertQuery.Parameters[0]);
			Assert.AreEqual(age, customInsertQuery.Parameters[1]);
			Assert.AreEqual(addressId, customInsertQuery.Parameters[2]);
		}

		[TestMethod]
		public void QueryWithTableNamesRegressionTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(u => $"{u.Name}, {u}.[Age], {u.AddressId}")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT [User].[Name], [User].[Age], [User].[AddressId]",
				"FROM [User]"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SelectWithWhereAndOrderByRegressionTest()
		{
			const string name = "John";
			const int age = 10;

			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.Where(user => $"{user.Age} = @0", age)
				.OrderBy(user => $"{user.Age}")
				.Select(user => "*")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM [User]",
				"WHERE (([User].[Name] LIKE '%' + @0 + '%') AND ([User].[Age] = @1))",
				"ORDER BY [User].[Age]"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(2, query.Parameters.Length);
			Assert.AreEqual(name, query.Parameters.First());
			Assert.AreEqual(age, query.Parameters.Last());
		}

		[TestMethod]
		public void SelectWithWhereAndOrderByDescendingTest()
		{
			const string name = "John";
			const int age = 10;

			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.Where(user => $"{user.Age} = @0", age)
				.OrderBy(user => $"{user.Age}", OrderingDirection.Descending)
				.Select(user => "*")
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM [User]",
				"WHERE (([User].[Name] LIKE '%' + @0 + '%') AND ([User].[Age] = @1))",
				"ORDER BY [User].[Age] DESC"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(2, query.Parameters.Length);
			Assert.AreEqual(name, query.Parameters.First());
			Assert.AreEqual(age, query.Parameters.Last());
		}

		[TestMethod]
		public void SkipTakeTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Select(user => "*")
				.Skip(2)
				.Take(3)
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"SELECT *",
				"FROM [User]",
				"OFFSET 2 ROWS",
				"FETCH NEXT 3 ROWS ONLY"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void CreateTablesIfNotExistTest()
		{
			var query = CreateSqlQueryBuilder()
				.CreateTableIfNotExists<User>()
				.ToSqlQuery();

			var expectedResult = string.Join("\n",
				"IF OBJECT_ID(N'User', N'U') IS NULL",
				"BEGIN",
				"CREATE TABLE [User]",
				"(",
				"Id INT NOT NULL,",
				"AddressId INT NOT NULL,",
				"UserGroupId INT NOT NULL,",
				"Name NVARCHAR NOT NULL,",
				"Age INT NOT NULL,",
				"NullableAge INT,",
				"Date1 DATETIME2 NOT NULL,",
				"Date2 DATETIME2",
				");",
				"END;"
			);

			Assert.That.SqlsAreEqual(expectedResult, query.Sql);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		internal class User
		{
			public int Id { get; set; }
			public int AddressId { get; set; }
			public int UserGroupId { get; set; }
			public string Name { get; set; } = "";
			public int Age { get; set; }
			public int? NullableAge { get; set; }
			public DateTime Date1 { get; set; }
			public DateTime? Date2 { get; set; }
		}

		internal class Address
		{
			public int Id { get; set; }
		}

		internal class UserGroup
		{
			public int Id { get; set; }
		}
	}
}
