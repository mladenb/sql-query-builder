using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class SkipSqlClause : ISqlClause
	{
		public uint RowCount { get; }

		public SkipSqlClause(uint rowCount)
		{
			RowCount = rowCount;
		}
	}
}
