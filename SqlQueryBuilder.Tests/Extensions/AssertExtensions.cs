using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlQueryBuilder.Tests.Extensions
{
	public static class AssertExtensions
	{
		public static void SqlsAreEqual(this Assert source, string sql1, string sql2)
		{
			Assert.AreEqual(NormalizeSqlString(sql1), NormalizeSqlString(sql2));
		}

		private static string NormalizeSqlString(string sql)
		{
			return new Regex(@"\s+")
				.Replace(sql, " ");
		}
	}
}
