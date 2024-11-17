namespace DefaultSqlQueryBuilder
{
	public class SqlQuery
	{
		public string Command { get; }
		public object[] Parameters { get; }

		public SqlQuery(string command, params object[] parameters)
		{
			Command = command;
			Parameters = parameters;
		}
	}
}
