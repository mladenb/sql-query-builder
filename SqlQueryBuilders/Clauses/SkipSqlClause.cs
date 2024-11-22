using SqlQueryBuilders.Contracts;

namespace SqlQueryBuilders.Clauses
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
