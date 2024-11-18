using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class SkipSqlClause : ISqlClause
	{
		public int RowCount { get; }

		public SkipSqlClause(int rowCount)
		{
			RowCount = rowCount;
		}
	}
}
