# SqlQueryBuilder.NET
A C# library which helps create extensible LINQ-like sql queries ([Nuget package](https://www.nuget.org/packages/SqlQueryBuilder.NET)).

## Quick Start

```csharp
var builder = new SqlQueryBuilder(new MsSqlSyntax());
// you can implement a custom ISqlSyntax or use an already implemented MsSqlSyntax

var query = builder
	.From<Booking>()
	// .SelectAll() // also take a look at the bottom of this composition
	.InnerJoin<Client>((booking, client) => $"{booking.ClientId} = {client.Id}")
	.InnerJoin<Room>((booking, client, room) => $"{booking.RoomId} = {room.Id}")
	.Where((booking, client, room) => $"{booking.Date} >= @0 AND {booking.Date} <= @1", filter.DateFrom, filter.DateTo)
	.Where((booking, client, room) => $"{booking.Price} >= @0 AND {booking.Price} <= @1", filter.PriceFrom, filter.PriceTo)
	.Where((booking, client, room) => $"{booking.ClientId} == @0", filter.ClientId)
	.Where((booking, client, room) => $"{booking.RoomId} == @0", filter.RoomId)
	.Select((booking, client, room) => $"{booking.Id}, {room.Name}, {client.Name}, {booking.Date}, {booking.Price}")
	// or .SelectAll()
	.ToSqlQuery();

public class Booking
{
	public int Id { get; set; }
	public int ClientId { get; set; }
	public int RoomId { get; set; }
	public DateTime Date { get; set; }
	public double Price { get; set; }
}
public class Client
{
	public int Id { get; set; }
	public string Name { get; set; }
}
public class Room
{
	public int Id { get; set; }
	public string Name { get; set; }
}
```

`query.Sql` will be a string with the following value:
```sql
SELECT [Bookings].[Id], [Rooms].[Name], [Clients].[Name], [Bookings].[Date], [Bookings].[Price]
FROM [Bookings]
INNER JOIN [Clients] ON [Bookings].[ClientId] = [Clients].[Id]
INNER JOIN [Rooms] ON [Bookings].[RoomId] = [Rooms].[Id]
WHERE (((([Bookings].[Date] >= @0 AND [Bookings].[Date] <= @1) AND ([Bookings].[Price] >= @2 AND [Bookings].[Price] <= @3)) AND ([Bookings].[ClientId] == @4)) AND ([Bookings].[RoomId] == @5))
```

`query.Parameters` will be an array of objects:
```
[0]: filter.DateFrom,
[1]: filter.DateTo,
[2]: filter.PriceFrom,
[3]: filter.PriceTo,
[4]: filter.ClientId,
[5]: filter.RoomId,
```

`query.NamedParameters` will be a dictionary:
```
{ "@0", filter.DateFrom },
{ "@1", filter.DateTo },
{ "@2", filter.PriceFrom },
{ "@3", filter.PriceTo },
{ "@4", filter.ClientId },
{ "@5", filter.RoomId },
```

You can now use the `query` object for example to create an `IDbCommand`:
```csharp
public static IDbCommand CreateCommandFrom(this IDbConnection connection, SqlQuery sqlQuery)
{
	var cmd = connection.CreateCommand();
	cmd.CommandText = sqlQuery.Sql;
	foreach (var (name, value) in sqlQuery.NamedParameters)
		cmd.Parameters[name] = value;

	return cmd;
}
```
and call the [`ExecuteReader()`](https://learn.microsoft.com/en-us/dotnet/api/system.data.idbcommand.executereader?view=net-9.0#system-data-idbcommand-executereader) method from `System.Data.IDbCommand`.


## Introduction

This library makes designing a data layer of an ASP.NET application, backed by an SQL database, easier in cases we need to write things from scratch instead of using an ORM solution.

As an example, take a look at [manually executing SQL queries using ADO.NET and C#](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/ado-net-code-examples).

But, the process of maintaining and extending those SQL queries can quickly become tedious due to the nature of the hard coded SQL statements as strings.

What we need, ideally, is something that is:
- easy to maintain (we can easily add/delete/update/rename tables/columns)
- easy to extend (we can create new SQL queries on top of existing ones quickly)
- immune to SQL injections (I hope, in the 21st century, this should really go without saying)

## IMPORTANT NOTE

Many of us will probably find it easier to use an [object-relational mapping (ORM) solution](https://en.wikipedia.org/wiki/List_of_object-relational_mapping_software), like [EntityFramework](https://en.wikipedia.org/wiki/Entity_Framework), [NPoco](https://github.com/schotime/NPoco), [LINQ to SQL](https://en.wikipedia.org/wiki/LINQ_to_SQL) or similar, to deal with the data layer as a whole. This approach is a recommended one and pretty much the standard these days, and this library does not offer an alternative to these solutions, but rather deals with the cases when we're forced to do things from the ground up, for some reason.

## What is SQL query builder

SQL query builder is a lightweight library, without any dependencies besides the framework itself, and there's no requirement to have an ORM solution in order to use it.

Part of the inspiration for this library was the [.NET's Language Integrated Query facility (LINQ)](https://msdn.microsoft.com/en-us/library/bb308959.aspx) and the elegance with which LINQ queries were used. Similarly, the idea for this library was to design reusable SQL queries that are easy to extend and build new queries on top of the existing ones. Just like the `IEnumerable<T>` interface in LINQ, there is an `SqlQueryBuilder` class which behaves in a similar way. It allows us to build our SQL queries in the similar fashion like LINQ compositions in order to finally materialize the IEnumerable/SqlQueryBuilder into something we can use.

Ideally, we should be able to create a basic query, like `SELECT * FROM [Table]` and build any new queries on top of that basic query, just by extending it, just like in a LINQ query, where we start from an enumeration and keep extending it until we finally materialize it into something concrete. For example:

```csharp
IEnumerable<User> allUsers = GetAll<User>();

var activeUserIds = allUsers
	.Where(user => user.IsActive)
	.Select(user => user.Id)
	.Distinct()
	.ToArray();

var inactiveUserNames = allUsers
	.Where(user => !user.IsActive)
	.DistinctBy(user => user.Id)
	.Select(user => user.Name)
	.ToArray();
```

We start with an enumeration of all users, using `GetAll<User>()`, which we can reuse, to compose more complex queries. We, then, extend our query by adding a filter (`Where()`), mapping the result to the list of user ids (`Select()`), reducing the result further to distinct user ids. After we crafted our query, we materialize it with `ToArray()`. We also reuse the initial enumeration to create another collection of inactive user names in a similar fashion.

The SQL query builder implements the similar behavior, helping us to create reusable queries in a similar fashion as LINQ queries, materializing them in the end as simple `SqlQuery` objects which consist of a string (the actual SQL command) and an array of query parameters:

```csharp
public class SqlQuery
{
	public string Sql { get; }
	public object[] Parameters { get; }
	public IReadOnlyDictionary<string, object> NamedParameters { get; }
}
```

That approach helps us create [parameterized SQL queries](https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.parameters(v=vs.110).aspx) to avoid being a victim of an SQL injection attack, obviously.

## Usage

Let's take a look at some examples, which would explain it better, hopefully. First, we create an `SqlQueryBuilder` instace providing a specific `ISqlSyntax` to be used when composing our queries:

```csharp
var builder = new SqlQueryBuilder(new MsSqlSyntax());
```

which we'll use in all the following examples. For example, let's select everything from a table `User`:

```csharp
var allUsersQueryBuilder = builder
	.From<User>()
	.SelectAll();

var sqlQuery = allUsersQueryBuilder.ToSqlQuery();
```

In our `sqlQuery` variable, we'll have an `SqlQuery` instance, with the `Sql` property set to:

```sql
SELECT *
FROM [User]
```

with empty `Parameters` array. And if we extend the `allUsersQueryBuilder`, by adding a `WHERE` filter:

```csharp
var name = "John";

var newQueryBuilder = allUsersQueryBuilder
	.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name);

var newSqlQuery = newQuery.ToSqlQuery();
```

we'll get the `newSqlQuery` instance, with the `Sql` property set to:

```sql
SELECT *
FROM [User]
WHERE ([User].[Name] LIKE '%' + @0 + '%')
```

and `Parameters` property, which is an array of objects containing:

```sql
[0]: "John"
```

Note that we've made use of the [String.Format()](https://msdn.microsoft.com/en-us/library/system.string.format%28v=vs.110%29.aspx) method in order to leverage the help of [IntelliSense](https://docs.microsoft.com/en-us/visualstudio/ide/using-intellisense), to help us write queries more conveniently. We've also used the "[string interpolation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated)" feature of the C# language, to make things even easier.

The string `$"{user.Name} LIKE '%' + @0 + '%'"` is the same as `string.Format("{0} LIKE '%' + @0 + '%'", user.Name)`.

The `SqlQueryBuilder` will parse this construct and enumerate all the classes and properties used and will map them to the appropriate tables/columns of the underlying SQL database. The default convention is to use the same naming for the C# classes and SQL tables, as well as the same naming for the C# properties on those classes and SQL columns of those tables. We can, of course, customize this mapping by providing our own mapper implementations for `ITableNameResolver` and `IColumnNameResolver` (see section "Custom table/column names mapping").

Once we have our query built, we can use it, for example, directly using `Microsoft.Data.SqlClient.SqlConnection`, like this:

```csharp
using (var connection = new SqlConnection(ConnectionString))
{
	IDbCommand cmd = connection.CreateCommand();
	cmd.CommandText = query.Command;

	var param = cmd.CreateParameter();
	param.ParameterName = "@0";
	param.Value = query.Parameters[0];

	cmd.Parameters.Add(param);

	// or using SqlQuery.NamedParameters
	foreach (var (name, value) in sqlQuery.NamedParameters)
		cmd.Parameters[name] = value;

	connection.Open();
	...
}
```

This is just a basic example how to use the `SqlQuery` that `SqlQueryBuilder` has generated for us. But we can also use our favorite ORM solution instead.

## Reusing queries

If we want to create a simple SQL query and reuse it to construct more complex new queries, we could do it easily, writing something like this:

```csharp
var name = "John";
var userGroupIds = new[] { 1, 2, 3 };

var allUsersQuery = builder
	.From<User>()
	.SelectAll();

var usersByNameQuery = allUsersQuery
	.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name);

var usersByNameExtendedQuery = usersByNameQuery
	.InnerJoin<Address>((user, address) => $"{user.AddressId} = {address.Id}")
	.InnerJoin<UserGroup>((user, address, userGroup) => $"{user.UserGroupId} = {userGroup.Id}")
	.Where((user, address, userGroup) => $"{user.UserGroupId} IN (@0)", userGroupIds)
	.Select((user, address, userGroup) => $"{user.Id}, {user.Name}, {user.Age}");

var usersByNameSqlQuery = usersByNameQuery.ToSqlQuery();
var usersByNameExtendedSqlQuery = usersByNameExtendedQuery.ToSqlQuery();
```

we would end up with 2 `SqlQuery` objects. The first one, `usersByNameSqlQuery`, would have a `Sql` property of:

```sql
SELECT *
FROM [User]
WHERE ([User].[Name] LIKE '%' + @0 + '%')
```

and its `Parameters` set to:

```sql
[0]: "John"
```

The second one, `usersByNameExtendedSqlQuery`, would have `Sql/Parameters` properties set to:

```sql
SELECT [User].[Id], [User].[Name], [User].[Age]
FROM [User]
INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]
INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]
WHERE (([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] IN (@1,@2,@3)))
```
```sql
[0]: "John"
[1]: 1
[2]: 2
[3]: 3
```

Note that, in the `usersByNameExtendedSqlQuery`, the first "`SELECT *`" got replaced with the second
"`SELECT [User].[Id]...`", and all the `WHERE` clauses got merged together automatically.

## A couple of more complex queries

We can create some more complex queries, expanding the list of joined tables with multiple `WHERE` statements, later combined into one:

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

var baseSqlQuery = baseQuery.ToSqlQuery();
var joinSqlQuery = joinQuery.ToSqlQuery();
```

which would result in 2 `SqlQuery` objects. The first one, `baseSqlQuery` would have the `Sql/Parameters` properties like this:

```sql
SELECT *
FROM [User]
WHERE ([User].[Name] LIKE '%' + @0 + '%')
```
```sql
[0]: "John"
```

and the second one, `joinSqlQuery`, would look like this:

```sql
SELECT [User].[Id], [User].[Name], [User].[Age]
FROM [User]
INNER JOIN [Address] ON [User].[AddressId] = [Address].[Id]
INNER JOIN [UserGroup] ON [User].[UserGroupId] = [UserGroup].[Id]
WHERE ((([User].[Name] LIKE '%' + @0 + '%') AND ([User].[UserGroupId] = 1)) AND ([User].[UserGroupId] IN (@1,@2,@3)))
```
```sql
[0]: "John"
[1]: 1
[2]: 2
[3]: 3
```

## INSERT / UPDATE

In order to create an `INSERT` SQL statement, it is enough to write something like this:

```csharp
var age = 10;
var addressId = 1;
var name = "John";

var insertSqlQuery = builder
	.Insert<User>(user => $"{user.Age}, {user.AddressId}, {user.Name}", age, addressId, name)
	.ToSqlQuery();
```

which would produce this `SqlQuery` as a result:

```sql
INSERT INTO [User] ([User].[Age], [User].[AddressId], [User].[Name])
VALUES (@0, @1, @2)
```
```sql
[0]: 10
[1]: 1
[2]: "John"
```

In order to create an `INSERT` SQL statement with multiple rows of data at once, we could write:

```csharp
var users = new[]
{
	new User(Name: "John", Age: 10, AddressId: 1),
	new User(Name: "Jane", Age: 20, AddressId: 2),
	new User(Name: "Smith", Age: 30, AddressId: 3),
};

var parameters = users
	.Select(u => new object[] { u.Age, u.AddressId, u.Name })
	.ToArray();

var insertMultipleSqlQuery = builder
	.InsertMultiple<User>(user => $"{user.Age}, {user.AddressId}, {user.Name}", parameters)
	.ToSqlQuery();
```

which would create an `SqlQuery` instance like this one:

```sql
INSERT INTO [User] ([User].[Age], [User].[AddressId], [User].[Name])
VALUES (@0, @1, @2), (@3, @4, @5), (@6, @7, @8)
```
```sql
[0]: 10
[1]: 1
[2]: "John"
[3]: 20
[4]: 2
[5]: "Jane"
[6]: 30
[7]: 3
[8]: "Smith"
```

For the `UPDATE` statement, it's quite similar:

```csharp
var age = 10;
var addressId = 1;
var name = "John";

var updateByNameSqlQuery = builder
	.Update<User>(user => $"{user.Age} = @0, {user.AddressId} = @1", age, addressId)
	.Where(user => $"{user.Name} LIKE '%' + @0 + '%'", name)
	.ToSqlQuery();
```

and the result would be:

```sql
UPDATE [User]
SET [User].[Age] = @0, [User].[AddressId] = @1
WHERE ([User].[Name] LIKE '%' + @2 + '%')
```
```sql
[0]: 10
[1]: 1
[2]: "John"
```

Note that we don't have to keep track of the last parameter index used in a previous statement/clause, because each new statement/clause will start enumerating its parameters from a zero-based index. That's why, in the previous query in the `Where()` method, we didn't use the index "`@2`" for the user's `Name`, but rather used the parameter with index "`@0`".

## Custom table/column names mapping

If we have a scenario where our table/column names are not exactly "one-to-one" mapped to our classes/properties, we can specify our custom table/column mappers, when creating a new instance of an `SqlQueryBuilder`.

For example, a simple custom `ITableNameResolver` could map the class name written in singular to table names defined in plural, like this:

```csharp
public class PluralTableNameResolver : ITableNameResolver
{
	public string Resolve(Type type)
	{
		return type switch
		{
			_ when type == typeof(Booking) => "Bookings",
			_ when type == typeof(Client) => "Clients",
			_ when type == typeof(Room) => "Rooms",
			_ => throw new NotImplementedException()
		};
	}
}
```

For those of us, who like [NPoco](https://github.com/schotime/NPoco), we can create our `ITableNameResolver` like this:
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

		return _database
			.PocoDataFactory
			.ForType(type)
			.TableInfo
			.TableName;
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

		return data
			.Members
			.First(x => x.Name == memberName)
			.PocoColumn
			.ColumnName;
	}
}
```
then we can make use of the [NPoco's mapping feature](https://github.com/schotime/NPoco/wiki/Mapping). We could create an instance of an `SqlQueryBuilder` like this:
```csharp
var db = new NPoco.Database("connectionString");
var tableNameResolver = new NPocoTableNameResolver(db);
var columnNameResolver = new NPocoColumnNameResolver(db);
var sqlSyntax = new MsSqlSyntax();

var builder = new SqlQueryBuilder(sqlSyntax, tableNameResolver, columnNameResolver);
```
and we could reuse all the examples given above.

