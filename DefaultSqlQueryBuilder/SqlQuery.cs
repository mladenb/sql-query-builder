using System.Linq;
using System.Text.RegularExpressions;

namespace DefaultSqlQueryBuilder
{
	public class SqlQuery
	{
		public string Sql { get; }
		public object[] Parameters { get; }

		public SqlQuery(string sql, params object[] parameters)
		{
			Sql = sql;
			Parameters = parameters;
		}

		public SqlQuery Append(string sql, object[] parameters, string mergeFormat = "{0}\n{1}")
		{
			var newSql = Regex.Replace(sql, @"\@\d+", ShiftParams);
			var newParams = Parameters.Concat(parameters).ToArray();

			return new SqlQuery(string.Format(mergeFormat, Sql, newSql), newParams);
		}

		private string ShiftParams(Match match)
		{
			return $"@{int.Parse(match.Value.Substring(1)) + Parameters.Length}";
		}
	}
}
