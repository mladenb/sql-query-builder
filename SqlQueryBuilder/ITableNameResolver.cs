using System;

namespace SqlQueryBuilder
{
	public interface ITableNameResolver
	{
		string Resolve(Type type);
	}
}
