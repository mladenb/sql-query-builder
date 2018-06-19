using System.Linq;
using System.Text.RegularExpressions;

namespace DefaultSqlQueryBuilder.Clauses
{
	public class SqlClauseMerger
	{
		public SqlClauseMerger(string sql1, object[] parameters1, string sql2, object[] parameters2)
		{
			_sql1 = sql1;
			_parameters1 = parameters1;
			_sql2 = sql2;
			_parameters2 = parameters2;
		}

		private const string Pattern = @"\@\d+";

		private readonly string _sql1;
		private readonly object[] _parameters1;
		private readonly string _sql2;
		private readonly object[] _parameters2;

		public CustomSqlClause Merge(string mergeFormat)
		{
			var newSql2 = Regex.Replace(_sql2, Pattern, ShiftParams);
			var newParams = _parameters1.Concat(_parameters2).ToArray();

			return new CustomSqlClause(string.Format(mergeFormat, _sql1, newSql2), newParams);
		}

		private string ShiftParams(Match match)
		{
			return $"@{int.Parse(match.Value.Substring(1)) + _parameters1.Length}";
		}
	}
}
