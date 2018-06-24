using System;

namespace DefaultSqlQueryBuilder
{
	public class SqlQuery
	{
		public string Command { get; }
		public object[] Parameters { get; }

		public SqlQuery(string command, object[] parameters = null)
		{
			Command = command ?? throw new ArgumentNullException(nameof(command));
			Parameters = parameters ?? new object[0];
		}
	}
}
