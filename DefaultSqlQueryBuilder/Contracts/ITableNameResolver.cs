using System;

namespace DefaultSqlQueryBuilder.Contracts
{
	public interface ITableNameResolver
	{
		string Resolve(Type type);
	}
}
