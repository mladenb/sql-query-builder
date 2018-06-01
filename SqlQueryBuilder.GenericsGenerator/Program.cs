using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlQueryBuilder.GenericsGenerator
{
	internal class Program
	{
		private const string DefaultFileName = "SqlQueryBuilder.01.cs";

		private static void Main(string[] args)
		{
			if (!args.Any())
			{
				args = new[] { DefaultFileName };
			}

			foreach (var arg in args)
			{
				Console.Write($"How much generics to create for the file '{arg}': ");
				var howMuch = int.Parse(Console.ReadLine());

				var content = File.ReadAllText(arg);
				for (var i = 2; i <= howMuch; i++)
				{
					var genericsCsv = string.Join(", ", CreateSequentialString("T", i));
					var replacedContent = content
						.Replace("<T>", $"<{genericsCsv}>")
						.Replace("<T,", $"<{genericsCsv},");

					var newPath = CalculateNewPath(arg, i);
					File.WriteAllText(newPath, replacedContent);
				}

				Console.WriteLine("Done.");
			}
		}

		private static string CalculateNewPath(string s, int i)
		{
			var fileName = Path.GetFileNameWithoutExtension(s);
			var fileExtension = Path.GetExtension(s);
			var additionalExtension = Path.GetExtension(fileName);

			// .Substring(1) - to remove the leading dot
			if (int.TryParse(additionalExtension.Substring(1), out int _))
			{
				var fileNameWithoutAdditionalExtension = Path.GetFileNameWithoutExtension(fileName);
				return $"{fileNameWithoutAdditionalExtension}.{i:D2}{fileExtension}";
			}
			else
			{
				return $"{fileName}.{i:D2}{fileExtension}";
			}
		}

		private static IEnumerable<string> CreateSequentialString(string s, int count)
		{
			for (var i = 1; i <= count; i++)
			{
				yield return $"{s}{i}";
			}
		}
	}
}
