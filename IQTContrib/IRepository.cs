// <summary>
//    IRepository - Repository interface
//     * original code found at: http://www.tpisolutions.com/blog/CategoryView,category,IQToolkit.aspx
//     * codeplex project found at:  http://iqtoolkitcontrib.codeplex.com/
// </summary>

using System.Linq;

namespace IQTContrib {
    
    public interface IRepository {
    
        T Get<T>(object id) where T : class;
        
        IQueryable<T> List<T>() where T : class;
        void Insert<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
    }
}
