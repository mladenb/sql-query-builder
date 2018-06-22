# sql-query-builder
A C# library which helps create customized and extensible LINQ-like sql queries more easily.

## Introduction

Each time we need to create a data layer for an ASP.NET Core application, which is backed up by an SQL database,
we're faced with a question how to design our SQL queries, to make them easy to use, maintain and extend.
Of course, it's fairly easy to just [implement SQL queries in ASP.NET](https://msdn.microsoft.com/en-us/library/fksx3b4f.aspx).
But, once implemented, the process of extending and maintaining those queries can quickly become difficult, especially if we
implement those queries as simple string constants, which opens the door to [SQL injections](https://en.wikipedia.org/wiki/SQL_injection).

What we would like to implement, ideally, is something that is:
- easy to maintain (which means we can easily add/delete/update/rename tables/columns)
- easy to extend (which means we can quickly create new SQL queries on top of existing ones)
- immune to security threats (like SQL injections)

Many of us usually decide to use an [object-relational mapping (ORM) solution](https://en.wikipedia.org/wiki/List_of_object-relational_mapping_software),
like [EntityFramework](https://en.wikipedia.org/wiki/Entity_Framework),
[NPoco](https://github.com/schotime/NPoco), [LINQ to SQL](https://en.wikipedia.org/wiki/LINQ_to_SQL)
or similar, to deal with the data layer as a whole.
This approach is pretty much a standard these days and this article does not
offer an alternative to those solutions, but rather tries to make things even easier.

Note that SQL query builder, described in this article, is a standalone solution,
without any dependency, and it is NOT a requirement to have an ORM solution
in order to use it.

## What is SQL query builder

Working on a web API project recently, I had an idea to create a small library,
which could help us create SQL queries that are maintainable, reusable and easy to use.
Part of the inspiration for this library was the [.NET's Language Integrated Query facility (LINQ)](https://msdn.microsoft.com/en-us/library/bb308959.aspx)
and the elegance with which LINQ queries were extended.

The idea was to create reusable SQL queries and build any new queries on top of the existing ones.
Ideally, we should be able to create a basic query, like `SELECT * FROM [Table]`
and build any new queries on top of that basic query, just by extending it, just like
in a LINQ query, where we start from an enumeration and keep extending the query
until we shape it into what we need. For example:

```csharp
IEnumerable<User> allUsers = GetAll<User>();

var activeGroups = allUsers
                      .Where(user => user.IsActive)
                      .Select(user => user.Id)
                      .Distinct()
                      .ToArray();
```

We start with an enumeration of all users, using `GetAll<User>()`, which we can
reuse as many times as we need. Then we extend our query by adding a filter
(`Where()`), mapping the result to the list of user ids (`Select()`), reducing
the result further to the distinct enumeration of user ids. After we crafted our
query, we materialize it with `ToArray()`.

The SQL query builder implements the similar behavior, helping us to create our
queries in a similar fashion as LINQ queries, materializing them in the end as
simple objects which consist of a string (the actual SQL command) and an object
array (the SQL parameters).
That approach helps us create [parameterized SQL queries](https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.parameters(v=vs.110).aspx)
to avoid being a victim of an SQL injection attack.

## Usage

Let's take a look at some examples, which would explain it better, hopefully. First, we create an SqlQueryBuilder:

```csharp
var builder = new SqlQueryBuilder();
```
which we'll use in all the following examples. For example, let's select everything from a table `User`:
```csharp
var sql = builder
	.From<User>()
	.SelectAll()
	.ToSqlQuery();
```
This will produce an SQL command in the form of:
```sql
SELECT *
FROM [User]
```
Now, let's filter our result set with a `WHERE` clause:
```csharp
var name = "John";

var sql = builder
	.From<User>()
	.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
	.SelectAll()
	.ToSqlQuery();
```
That will result with an SQL command like this:
```sql
SELECT *
FROM [User]
WHERE ([User].[Name] LIKE '%' + @0 + '%')
```
with the trailing SQL parameters set to:
```sql
@0 = "John"
```
We made use of the [String.Format()](https://msdn.microsoft.com/en-us/library/system.string.format(v=vs.110).aspx) method in order to leverage the help of IntelliSense, to help us write queries more conveniently. In these examples, we used the "[string interpolation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated)" feature of the C# language, to make things even easier. The usage of generic methods, like `Where()`, also helps us expand the possible choices, every time we do an additional join, by providing us with the appropriate lambda parameters, according to the tables, used in those joins.

## Reusing queries

If we want to create a simple SQL query (in the example below: `baseQuery`), and later reuse it, to construct more complex queries (`joinQuery`), we could write something like this:
```csharp
var name = "John";
var userGroupIds = new[] { 1, 2, 3 };

var baseQuery = builder
	.From<User>()
	.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
	.SelectAll();

var joinQuery = baseQuery
	.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
	.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
	.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", userGroupIds)
	.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

var baseSql = baseQuery.ToSqlQuery();
var joinSql = joinQuery.ToSqlQuery();
```
we would end up with 2 SQL strings. The first one being `baseSql`, which would look like this:
```sql
SELECT *
FROM [User]
WHERE ([User].[Name] LIKE '%' + @0 + '%')
```
with SQL query parameters set to:
```sql
@0 = "John"
```
and the second SQL string, `joinSql`, which would look like:
```sql
SELECT [User].[Id], [User].[Name], [User].[Age]
FROM [User]
INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]
INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]
WHERE (([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] IN (@1,@2,@3)))
```
with SQL query parameters set to:
```sql
@0 = "John"
@1 = 1
@2 = 2
@3 = 3
```
Note that, in the joinSql, the first `SELECT *` got replaced with the later one `SELECT [User].[Id]...`.

## A couple of more complex queries

We can create even more complex queries, expanding the list of joined tables with multiple `WHERE` statements, later combined into one:
```csharp
var name = "John";
var userGroupIds = new[] { 1, 2, 3 };

var baseQuery = builder
	.From<User>()
	.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
	.SelectAll();

var joinQuery = baseQuery
	.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
	.Where((user, address) => $"{user.UserGroupId} = 1")
	.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
	.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", userGroupIds)
	.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

var baseSql = baseQuery.ToSqlQuery();
var joinSql = joinQuery.ToSqlQuery();
```
which would result in 2 SQL strings. The first one, `baseSql`:
```sql
SELECT *
FROM [User]
WHERE ([User].[Name] LIKE '%' + @0 + '%')
```
and the second, `joinSql`:
```sql
SELECT [User].[Id], [User].[Name], [User].[Age]
FROM [User]
INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]
INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]
WHERE ((([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] = 1)) AND ([User].[UserGroupId] IN (@1,@2,@3)))
```

## INSERT / UPDATE made easy

In order to create an `INSERT` SQL statement, it is just enough to write something like this:
```csharp
var age = 10;
var addressId = 1;
var name = "John";

var sql = builder
	.Insert<User>(user => $"{user.Age}, {user.AddressId}, {user.Name}", age, addressId, name)
	.ToSqlQuery();
```
which would produce this as a result:
```sql
INSERT INTO [User] ([User].[Age], [User].[AddressId], [User].[Name])
VALUES (@0, @1, @2)
```
*(TODO Add INSERT multiple values)*
For the `UPDATE` statement, it's quite similar:
```csharp
var age = 10;
var addressId = 1;
var name = "John";

var sql = builder
	.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1, {user.Name} = @2", age, addressId, name)
	.ToSqlQuery();
```
which will produce a result like:
```sql
UPDATE [User]
SET [User].[Age] = @0, [User].[AddressId] = @1, [User].[Name] = @2
```
Adding a `WHERE` statement is also a trivial thing to do:
```csharp
var age = 10;
var addressId = 1;
var name = "John";

var sql = builder
	.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1", age, addressId)
	.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
	.ToSqlQuery();
```
and the result would be as expected:
```sql
UPDATE [User]
SET [User].[Age] = @0, [User].[AddressId] = @1
WHERE ([User].[Name] LIKE '%' + @2 + '%')
```

## Mapping table/column names

If we have a scenario where our table/column names are not exactly "one-to-one" mapped to our classes/properties, we can specify our custom table/column mappers, when creating a new instance of an SqlQueryBuilder.

For example, if we create our `ITableNameResolver` like this:
```csharp
public class NPocoTableNameResolver : ITableNameResolver
{
	private readonly IDatabase _database;

	public NPocoTableNameResolver(IDatabase database)
	{
		_database = database ?? throw new ArgumentNullException(nameof(database));
	}

	public string Resolve(Type type)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));

		var tableName = _database
			.PocoDataFactory
			.ForType(type)
			.TableInfo
			.TableName;

		return $"[{tableName}]";
	}
}
```
and our `IColumnNameResolver` like this:
```csharp
public class NPocoColumnNameResolver : IColumnNameResolver
{
	private readonly IDatabase _database;

	public NPocoColumnNameResolver(IDatabase database)
	{
		_database = database ?? throw new ArgumentNullException(nameof(database));
	}

	public string Resolve(Type type, string memberName)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		if (memberName == null) throw new ArgumentNullException(nameof(memberName));

		var data = _database.PocoDataFactory.ForType(type);
		var tableName = data.TableInfo.TableName;
		var columnName = data
			.Members
			.First(x => x.Name == memberName)
			.PocoColumn
			.ColumnName;

		return $"[{tableName}].[{columnName}]";
	}
}
```
then we can make use of the [NPoco's mapping feature](https://github.com/schotime/NPoco/wiki/Mapping) and have even more customized SQL strings. We just need to create an instance of an SqlQueryBuilder like this:
```csharp
var db = new NPoco.Database("connStringName");
var tableNameResolver = new NPocoTableNameResolver(db);
var columnNameResolver = new NPocoColumnNameResolver(db);

var builder = new SqlQueryBuilder(tableNameResolver, columnNameResolver);
```
and we can reuse all the examples given here, in this document, the same way.
