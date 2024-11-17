using DefaultSqlQueryBuilder.Contracts;

namespace DefaultSqlQueryBuilder.Clauses
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
