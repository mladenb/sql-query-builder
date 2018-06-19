using System;

namespace DefaultSqlQueryBuilder
{
	public interface IColumnNameResolver
	{
		string Resolve(Type type, string memberName);
	}
}
