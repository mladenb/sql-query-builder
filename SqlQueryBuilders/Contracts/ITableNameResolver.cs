using System;

namespace SqlQueryBuilders.Contracts
{
	public interface ITableNameResolver
	{
		string Resolve(Type type);
	}
}
