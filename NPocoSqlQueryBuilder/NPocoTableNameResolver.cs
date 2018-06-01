using NPoco;
using SqlQueryBuilder;
using System;

namespace NPocoSqlQueryBuilder
{
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
}
