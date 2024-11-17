using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
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
