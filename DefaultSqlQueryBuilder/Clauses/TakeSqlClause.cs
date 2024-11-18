using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class TakeSqlClause : ISqlClause
	{
		public int RowCount { get; }

		public TakeSqlClause(int rowCount)
		{
			RowCount = rowCount;
		}
	}
}
