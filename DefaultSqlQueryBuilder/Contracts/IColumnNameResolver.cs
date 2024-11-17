using System;

namespace DefaultSqlQueryBuilder.Contracts
{
	public interface IColumnNameResolver
	{
		string Resolve(Type type, string memberName);
	}
}
