using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlQueryBuilder.Extensions;
using SqlQueryBuilder.Tests.Extensions;
using System.Linq;

namespace SqlQueryBuilder.Tests
{
	[TestClass]
	public class SqlQueryBuilderTests
	{
		private SqlQueryBuilder CreateSqlQueryBuilder()
		{
			return new SqlQueryBuilder();
		}

		[TestMethod]
		public void SelectWithoutWhereTest()
		{
			var sql = CreateSqlQueryBuilder()
				.From<User>()
				.SelectAll()
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT *",
				"FROM [User]",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.Sql);
			Assert.AreEqual(sql.Parameters.Length, 0);
		}

		[TestMethod]
		public void SelectWithWhereTest()
		{
			const string name = "John";

			var sql = CreateSqlQueryBuilder()
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

			Assert.That.SqlsAreEqual(expectedResult, sql.Sql);
			Assert.AreEqual(sql.Parameters.Length, 1);
			Assert.AreEqual(sql.Parameters.First(), name);
		}

		[TestMethod]
		public void SelectWithJoinTest()
		{
			const string name = "John";
			var validUserGroupIds = new[] { 1, 2, 3 };

			var baseQuery = CreateSqlQueryBuilder()
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
				"WHERE (([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] IN (@1)))",
			});

			Assert.That.SqlsAreEqual(expectedResult, joinSql.Sql);
			Assert.AreEqual(joinSql.Parameters.Length, 2);
			Assert.AreEqual(joinSql.Parameters.First(), name);
		}

		[TestMethod]
		public void SelectWithMultipleJoinsAndWheresTest()
		{
			const string name = "John";
			var validUserGroupIds = new[] { 1, 2, 3 };

			var baseQuery = CreateSqlQueryBuilder()
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
				"WHERE ((([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] = 1)) AND ([User].[UserGroupId] IN (@1)))",
			});

			Assert.That.SqlsAreEqual(expectedResult, joinSql.Sql);
			Assert.AreEqual(joinSql.Parameters.Length, 2);
			Assert.AreEqual(joinSql.Parameters.First(), name);
		}

		[TestMethod]
		public void InsertSingleTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var sql = CreateSqlQueryBuilder()
				.Insert<User>(user => $"{user.Age}, {user.AddressId}, {user.Name}", age, addressId, name)
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"INSERT INTO [User] ([User].[Age], [User].[AddressId], [User].[Name])",
				"VALUES (@0, @1, @2)",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.Sql);
			Assert.AreEqual(sql.Parameters.Length, 3);
			Assert.AreEqual(sql.Parameters[0], age);
			Assert.AreEqual(sql.Parameters[1], addressId);
			Assert.AreEqual(sql.Parameters[2], name);
		}

		[TestMethod]
		public void UpdateWithoutWhereTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var sql = CreateSqlQueryBuilder()
				.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1, {user.Name} = @2", age, addressId, name)
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"UPDATE [User]",
				"SET [User].[Age] = @0, [User].[AddressId] = @1, [User].[Name] = @2",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.Sql);
			Assert.AreEqual(sql.Parameters.Length, 3);
			Assert.AreEqual(sql.Parameters[0], age);
			Assert.AreEqual(sql.Parameters[1], addressId);
			Assert.AreEqual(sql.Parameters[2], name);
		}

		[TestMethod]
		public void UpdateWithWhereTest()
		{
			const int age = 10;
			const int addressId = 1;
			const string name = "John";

			var sql = CreateSqlQueryBuilder()
				.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1", age, addressId)
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.ToSql();

			var expectedResult = string.Join("\n", new[]
			{
				"UPDATE [User]",
				"SET [User].[Age] = @0, [User].[AddressId] = @1",
				"WHERE ([User].[Name] LIKE '%' + @2 + '%')",
			});

			Assert.That.SqlsAreEqual(expectedResult, sql.Sql);
			Assert.AreEqual(sql.Parameters.Length, 3);
			Assert.AreEqual(sql.Parameters[0], age);
			Assert.AreEqual(sql.Parameters[1], addressId);
			Assert.AreEqual(sql.Parameters[2], name);
		}
	}

	internal class User
	{
		public int Id { get; set; }
		public int AddressId { get; set; }
		public int UserGroupId { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
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
