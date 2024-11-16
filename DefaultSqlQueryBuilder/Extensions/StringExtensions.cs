using System.Collections.Generic;

namespace DefaultSqlQueryBuilder.Extensions
{
	public static class StringExtensions
	{
		public static string ToCsv<T>(this IEnumerable<T> source, string delimiter = ",") => string.Join(delimiter, source);
	}
}
