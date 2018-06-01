using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPocoSqlQueryBuilder.Extensions;
using NPocoSqlQueryBuilder.Tests.Extensions;
using System.Linq;

namespace NPocoSqlQueryBuilder.Tests
{
	[TestClass]
	public class NPocoSqlQueryBuilderTests : IMockDatabase
	{
		private SqlQueryBuilder.SqlQueryBuilder CreateNPocoSqlQueryBuilder()
		{
			var dbMock = this.CreateDatabaseMock();
			var tableNameResolver = new NPocoTableNameResolver(dbMock.Object);
			var columnNameResolver = new NPocoColumnNameResolver(dbMock.Object);

			return new SqlQueryBuilder.SqlQueryBuilder(tableNameResolver, columnNameResolver);
		}

		[TestMethod]
		public void SelectWithoutWhereTest()
		{
			var sql = CreateNPocoSqlQueryBuilder()
				.From<User>()
				.SelectAll()
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT *",
				"FROM [User]",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.SQL);
			Assert.AreEqual(sql.Arguments.Length, 0);
		}

		[TestMethod]
		public void SelectWithWhereTest()
		{
			const string name = "John";

			var sql = CreateNPocoSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.SelectAll()
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT *",
				"FROM [User]",
				"WHERE ([User].[Name] LIKE '%' + @0 + '%')",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.SQL);
			Assert.AreEqual(sql.Arguments.Length, 1);
			Assert.AreEqual(sql.Arguments.First(), name);
		}

		[TestMethod]
		public void SelectWithJoinTest()
		{
			const string name = "John";
			var validUserGroupIds = new[] { 1, 2, 3 };

			var baseQuery = CreateNPocoSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.SelectAll();

			var joinQuery = baseQuery
				.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
				.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
				.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", validUserGroupIds)
				.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

			var joinSql = joinQuery.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT [User].[Id], [User].[Name], [User].[Age]",
				"FROM [User]",
				"INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]",
				"INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]",
				"WHERE (([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] IN (@1,@2,@3)))",
			});

			Assert.That.SqlsAreEqual(expectedResult, joinSql.SQL);
			Assert.AreEqual(joinSql.Arguments.Length, 4);
			Assert.AreEqual(joinSql.Arguments.First(), name);
		}

		[TestMethod]
		public void SelectWithMultipleJoinsAndWheresTest()
		{
			const string name = "John";
			var validUserGroupIds = new[] { 1, 2, 3 };

			var baseQuery = CreateNPocoSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.SelectAll();

			var joinQuery = baseQuery
				.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
				.Where((user, address) => $"{user.UserGroupId} = 1")
				.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
				.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", validUserGroupIds)
				.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

			var joinSql = joinQuery.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT [User].[Id], [User].[Name], [User].[Age]",
				"FROM [User]",
				"INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]",
				"INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]",
				"WHERE ((([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] = 1)) AND ([User].[UserGroupId] IN (@1,@2,@3)))",
			});

			Assert.That.SqlsAreEqual(expectedResult, joinSql.SQL);
			Assert.AreEqual(joinSql.Arguments.Length, 4);
			Assert.AreEqual(joinSql.Arguments.First(), name);
		}

		[TestMethod]
		public void InsertSingleTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var sql = CreateNPocoSqlQueryBuilder()
				.Insert<User>(user => $"{user.Age}, {user.AddressId}, {user.Name}", age, addressId, name)
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"INSERT INTO [User] ([User].[Age], [User].[AddressId], [User].[Name])",
				"VALUES (@0, @1, @2)",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.SQL);
			Assert.AreEqual(sql.Arguments.Length, 3);
			Assert.AreEqual(sql.Arguments[0], age);
			Assert.AreEqual(sql.Arguments[1], addressId);
			Assert.AreEqual(sql.Arguments[2], name);
		}

		[TestMethod]
		public void UpdateWithoutWhereTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var sql = CreateNPocoSqlQueryBuilder()
				.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1, {user.Name} = @2", age, addressId, name)
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"UPDATE [User]",
				"SET [User].[Age] = @0, [User].[AddressId] = @1, [User].[Name] = @2",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.SQL);
			Assert.AreEqual(sql.Arguments.Length, 3);
			Assert.AreEqual(sql.Arguments[0], age);
			Assert.AreEqual(sql.Arguments[1], addressId);
			Assert.AreEqual(sql.Arguments[2], name);
		}

		[TestMethod]
		public void UpdateWithWhereTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var sql = CreateNPocoSqlQueryBuilder()
				.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1", age, addressId)
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"UPDATE [User]",
				"SET [User].[Age] = @0, [User].[AddressId] = @1",
				"WHERE ([User].[Name] LIKE '%' + @2 + '%')",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.SQL);
			Assert.AreEqual(sql.Arguments.Length, 3);
			Assert.AreEqual(sql.Arguments[0], age);
			Assert.AreEqual(sql.Arguments[1], addressId);
			Assert.AreEqual(sql.Arguments[2], name);
		}
	}

	public class User
	{
		public int Id { get; set; }
		public int AddressId { get; set; }
		public int UserGroupId { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
	}

	public class Address
	{
		public int Id { get; set; }
	}

	public class UserGroup
	{
		public int Id { get; set; }
	}
}
