using DefaultSqlQueryBuilder.Extensions;
using DefaultSqlQueryBuilder.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DefaultSqlQueryBuilder.Tests
{
	[TestClass]
	public class SqlQueryBuilderTests
	{
		private static SqlQueryBuilder CreateSqlQueryBuilder()
		{
			return new SqlQueryBuilder();
		}

		[TestMethod]
		public void SelectWithoutWhereTest()
		{
			var query = CreateSqlQueryBuilder()
				.From<User>()
				.SelectAll()
				.ToSqlQuery();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT *",
				"FROM [User]",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
			Assert.AreEqual(0, query.Parameters.Length);
		}

		[TestMethod]
		public void SelectWithWhereTest()
		{
			const string name = "John";

			var query = CreateSqlQueryBuilder()
				.From<User>()
				.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
				.SelectAll()
				.ToSqlQuery();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT *",
				"FROM [User]",
				"WHERE ([User].[Name] LIKE '%' + @0 + '%')",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
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
				.SelectAll();

			var joinQuery = baseQuery
				.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
				.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
				.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", validUserGroupIds)
				.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

			var query = joinQuery.ToSqlQuery();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT [User].[Id], [User].[Name], [User].[Age]",
				"FROM [User]",
				"INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]",
				"INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]",
				"WHERE (([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] IN (@1)))",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
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
				.SelectAll();

			var joinQuery = baseQuery
				.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
				.Where((user, address) => $"{user.UserGroupId} = 1")
				.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
				.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", validUserGroupIds)
				.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

			var query = joinQuery.ToSqlQuery();

			var expectedResult = string.Join("\n", new[]
			{
				"SELECT [User].[Id], [User].[Name], [User].[Age]",
				"FROM [User]",
				"INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]",
				"INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]",
				"WHERE ((([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] = 1)) AND ([User].[UserGroupId] IN (@1)))",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
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

			var expectedResult = string.Join("\n", new[]
			{
				"INSERT INTO [User] ([User].[Age], [User].[AddressId], [User].[Name])",
				"VALUES (@0, @1, @2)",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
			Assert.AreEqual(3, query.Parameters.Length);
			Assert.AreEqual(age, query.Parameters[0]);
			Assert.AreEqual(addressId, query.Parameters[1]);
			Assert.AreEqual(name, query.Parameters[2]);
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

			var expectedResult = string.Join("\n", new[]
			{
				"UPDATE [User]",
				"SET [User].[Age] = @0, [User].[AddressId] = @1, [User].[Name] = @2",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
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

			var expectedResult = string.Join("\n", new[]
			{
				"UPDATE [User]",
				"SET [User].[Age] = @0, [User].[AddressId] = @1",
				"WHERE ([User].[Name] LIKE '%' + @2 + '%')",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
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

			var expectedResult = string.Join("\n", new[]
			{
				"DELETE FROM [User]",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
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

			var expectedResult = string.Join("\n", new[]
			{
				"DELETE FROM [User]",
				"WHERE ([User].[Name] LIKE '%' + @0 + '%')",
			});

			Assert.That.SqlsAreEqual(expectedResult, query.Command);
			Assert.AreEqual(1, query.Parameters.Length);
			Assert.AreEqual(name, query.Parameters[0]);
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
