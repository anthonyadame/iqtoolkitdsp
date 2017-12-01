// <summary>
//    ARepository - Abstract class implementing IRepository 
//     * original code found at: http://www.tpisolutions.com/blog/CategoryView,category,IQToolkit.aspx
//     * codeplex project found at:  http://iqtoolkitcontrib.codeplex.com/
// </summary>

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IQTContrib {
    
    public abstract class ARepository : IRepository {
    
        protected Expression<Func<T, bool>> CreateGetExpression<T>(object id) {
            ParameterExpression e = Expression.Parameter(typeof(T), "e");
            PropertyInfo pi = typeof(T).GetProperty(this.GetPrimaryKeyPropertyName<T>());
            MemberExpression m = Expression.MakeMemberAccess(e, pi);
            ConstantExpression c = Expression.Constant(id, id.GetType());
            BinaryExpression b = Expression.Equal(m, c);
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(b, e);
            return lambda;
        }

        protected abstract string GetPrimaryKeyPropertyName<T>();

        public abstract T Get<T>(object id) where T : class;

        public abstract IQueryable<T> List<T>() where T : class;

        public abstract void Insert<T>(T entity) where T : class;

        public abstract void Update<T>(T entity) where T : class;

        public abstract void Delete<T>(T entity) where T : class;
    }
}
