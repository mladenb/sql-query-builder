using SqlQueryBuilders.Contracts;

namespace SqlQueryBuilders.Clauses
{
	public class GroupBySqlClause : ISqlClause
	{
		public string Columns { get; }

		public GroupBySqlClause(string columns)
		{
			Columns = columns;
		}
	}
}
