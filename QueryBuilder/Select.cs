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
        }         

        public From<T> From(string table)
        {
            return new From<T>(this, table);
        }

        public From<T> From(Type classType)
        {
            if(typeof(IDataSource).IsAssignableFrom(classType))
            {
                return From(Query.GetNameOfDataSourceType(classType));
            }
            else
            {
                return From(classType.Name);
            }
        }

        public Where<T> Where(Expression<Func<T, object>> where)
        {
            return From(typeof(T)).Where<T>(where);
        }
    }
}
