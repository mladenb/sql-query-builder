using SqlQueryBuilders.Contracts;

namespace SqlQueryBuilders.Clauses
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
