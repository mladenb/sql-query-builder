using System;

namespace SqlQueryBuilder
{
	public interface IColumnNameResolver
	{
		string Resolve(Type type, string memberName);
	}
}
