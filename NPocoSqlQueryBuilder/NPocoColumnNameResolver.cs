using DefaultSqlQueryBuilder;
using NPoco;
using System;
using System.Linq;

namespace NPocoSqlQueryBuilder
{
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
}
