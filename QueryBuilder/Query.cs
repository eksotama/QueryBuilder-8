using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ArLehm.QueryBuilder
{
    public static class Query
    {
        public static From<T> Select<T>(Expression<Func<T, object>> select) where T : IDataSource
        {
            var tableName = GetNameOfDataSourceType(typeof(T));
            return new Select<T>(select).From(tableName);
        }

        internal static string GetNameOfDataSourceType(Type datasource)
        {
            var nameAttribute = datasource.GetTypeInfo().GetCustomAttributes(typeof(DataSourceNameAttribute), false).First();
            return (nameAttribute as DataSourceNameAttribute).Name;
        }
    }
}
