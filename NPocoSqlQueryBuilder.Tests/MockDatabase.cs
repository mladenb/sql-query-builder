using Moq;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NPocoSqlQueryBuilder.Tests
{
	public interface IMockDatabase
	{
	}

	public static class MockDatabaseExtensions
	{
		public static Mock<IDatabase> CreateDatabaseMock(this IMockDatabase source)
		{
			var connectionMock = new Mock<IDatabase>();
			connectionMock
				.SetupGet(db => db.PocoDataFactory)
				.Returns(CreatePocoDataFactoryMock().Object);

			return connectionMock;
		}

		private static Mock<IPocoDataFactory> CreatePocoDataFactoryMock()
		{
			var pocoDataFactoryMock = new Mock<IPocoDataFactory>();
			pocoDataFactoryMock
				.Setup(f => f.ForType(It.IsAny<Type>()))
				.Returns((Type type) => CreateFakePocoData(type));

			return pocoDataFactoryMock;
		}

		private static FakePocoData CreateFakePocoData(Type type)
		{
			return new FakePocoData(type);
		}
	}

	public class FakePocoData : PocoData
	{
		public FakePocoData(Type type)
		{
			TableInfo = CreateFakeTableInfo($"{type.Name}");
			Members = CreatePocoMemberList(type);
		}

		private static TableInfo CreateFakeTableInfo(string tableName)
		{
			return new FakeTableInfo(tableName);
		}

		private static List<PocoMember> CreatePocoMemberList(Type type)
		{
			return GetPublicProperties(type)
				.Select(propertyInfo => new FakePocoMember(propertyInfo))
				.ToList<PocoMember>();
		}

		private static IEnumerable<PropertyInfo> GetPublicProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		}
	}

	internal class FakeTableInfo : TableInfo
	{
		public FakeTableInfo(string tableName)
		{
			TableName = tableName;
		}
	}

	internal class FakePocoMember : PocoMember
	{
		public FakePocoMember(MemberInfo propertyInfo)
		{
			MemberInfoData = new MemberInfoData(propertyInfo);
			PocoColumn = CreateFakePocoColumn(propertyInfo.Name);
		}

		private static PocoColumn CreateFakePocoColumn(string columnName)
		{
			return new PocoColumn
			{
				ColumnName = columnName
			};
		}
	}
}
