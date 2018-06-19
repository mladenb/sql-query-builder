using System;

namespace DefaultSqlQueryBuilder
{
	public interface ITableNameResolver
	{
		string Resolve(Type type);
	}
}
