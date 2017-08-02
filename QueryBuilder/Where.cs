using System;
using System.Linq.Expressions;

namespace QueryBuilder
{
    public class Where<T>
    {
        private From<T> from;
        private string clause = "";

        public Where(From<T> from, Expression<Func<T, object>> where)
        {
            this.from = from;

            if(where.Body is MemberExpression body)
            {
                
            }
        }

        public override string ToString()
        {
            return $"{from} WHERE {clause}";
        }
    }
}