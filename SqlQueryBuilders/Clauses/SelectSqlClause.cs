using SqlQueryBuilders.Contracts;

namespace SqlQueryBuilders.Clauses
{
	public class SelectSqlClause : ISqlClause
	{
		public string Columns { get; }

		public SelectSqlClause(string columns)
		{
			Columns = columns;
		}
	}
}
