using DefaultSqlQueryBuilder.Contracts;
using System;

namespace DefaultSqlQueryBuilder.Resolvers
{
	public class DefaultColumnNameResolver : IColumnNameResolver
	{
		public string Resolve(Type type, string memberName) => $"[{type.Name}].[{memberName}]";
	}
}
