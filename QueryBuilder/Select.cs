using System;
using System.Linq.Expressions;

namespace QueryBuilder
{
    public class Select<T>
    {
        private string clause = "SELECT ";

        public override string ToString() => clause;

        public Select() { }

        public Select(Expression<Func<T, object>> select)
        {
            if(select.Body is MemberExpression body)
            {
                clause += body.Member.Name;
            }
            else if(select.Body is UnaryExpression unary)
            {
                if(unary.Operand is MemberExpression member)
                {
                    clause += member.Member.Name;
                }
            }
        }         

        public From<T> From(string table)
        {
            return new From<T>(this, table);
        }

        public From<T> From(Type classType = null, string prefix = null)
        {
            prefix = prefix ?? GetSourcePrefix(classType);
            classType = classType ?? typeof(T);
            if (typeof(IDataSource).IsAssignableFrom(classType))
            {
                return From($"{prefix}{Query.GetNameOfDataSourceType(classType)}");
            }
            else
            {
                return From($"{prefix}{classType.Name}");
            }
        }

        private static string GetSourcePrefix(Type sourceType)
        {
            var prefix = sourceType.GetProperty("SourcePrefix")?.GetValue(null, null);
            return prefix?.ToString() ?? "";
        }

        public Where<T> Where(Expression<Func<T, object>> where)
        {
            return From(typeof(T)).Where<T>(where);
        }
    }
}
