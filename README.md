# sql-query-builder
A C# library which helps create customized and extensible LINQ-like sql queries more easily.

It goes like this. First, we create an SqlQueryBuilder:
```csharp
var builder = new SqlQueryBuilder();
```
Then, we use it in various scenarios. For example, let's select everything from a table `User`:
```csharp
var sql = builder
	.From<User>()
	.SelectAll()
	.ToSql();
```
This will produce an SQL string in the form of:
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
	.ToSql();
```
That will result with an SQL string like this:
```sql
SELECT *
FROM [User]
WHERE ([User].[Name] LIKE '%' + @0 + '%')
```
with SQL query parameters set to:
```
@0 = "John"
```
We made use of the [String.Format()](https://msdn.microsoft.com/en-us/library/system.string.format(v=vs.110).aspx) method in order to leverage the help of IntelliSense, to help use write queries more conveniently. In these examples, we used the "[string interpolation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated)" feature of the C# language, to make things even easier. The usage of generic methods, like `Where()`, also helps us expand the possible choices, every time we do an additional join, by providing us with the appropriate lambda parameters, according to the tables, used in those joins.

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

var baseSql = baseQuery.ToSql();
var joinSql = joinQuery.ToSql();
```
we would end up with 2 SQL strings. The first one being `baseSql`, which would look like this:
```sql
SELECT *
FROM [User]
WHERE ([User].[Name] LIKE '%' + @0 + '%')
```
with SQL query parameters set to:
```
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
```
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

var baseSql = baseQuery.ToSql();
var joinSql = joinQuery.ToSql();
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
	.ToSql();
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
	.ToSql();
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
	.ToSql();
```
and the result would be as expected:
```sql
UPDATE [User]
SET [User].[Age] = @0, [User].[AddressId] = @1
WHERE ([User].[Name] LIKE '%' + @2 + '%')
```
