using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class TakeSqlClause : ISqlClause
	{
		public uint RowCount { get; }

		public TakeSqlClause(uint rowCount)
		{
			RowCount = rowCount;
		}
	}
}
