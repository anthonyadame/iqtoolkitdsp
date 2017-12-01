// <summary>
//    DbEntityRepository
//     * original code found at: http://www.tpisolutions.com/blog/CategoryView,category,IQToolkit.aspx
//     * codeplex project found at:  http://iqtoolkitcontrib.codeplex.com/
// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Common;

namespace IQTContrib {
    
    public class DbEntityRepository : ARepository {
    
        public DbEntityProvider Provider { get; private set; }

        
        public DbEntityRepository(DbEntityProvider provider) {
            this.Provider = provider;
        }

        protected override string GetPrimaryKeyPropertyName<T>() {
            MappingEntity mappingEntity = this.Provider.Mapping.GetEntity(typeof(T));
            List<MemberInfo> memberInfoList = this.Provider.Mapping.GetPrimaryKeyMembers(mappingEntity).ToList();

            if (memberInfoList.Count != 1) {
                throw new ApplicationException(string.Format("Cannot determine the primary key for {0}", typeof(T)));
            }

            MemberInfo primaryKeyMemberInfo = memberInfoList[0];
            return primaryKeyMemberInfo.Name;
        }

        public override T Get<T>(object id) {
            
            List<T> list = this.List<T>().Where<T>(this.CreateGetExpression<T>(id)).ToList();

            if (list.Count == 0) {
                return default(T);
            }

            return list[0];
        }
        
        public override IQueryable<T> List<T>() {
            return this.GetEntityTable<T>();
        }

        public override void Insert<T>(T entity) {
            if (entity is IValidate) {
                ((IValidate)entity).Validate();
            }

            this.GetEntityTable<T>().Insert<T>(entity);
        }
        
        public override void Update<T>(T entity) {
            if (entity is IValidate) {
                ((IValidate)entity).Validate();
            }

            this.GetEntityTable<T>().Update<T>(entity);
        }

        public override void Delete<T>(T entity) {
            this.GetEntityTable<T>().Delete<T>(entity);
        }

        private IEntityTable<T> GetEntityTable<T>() {
            return this.Provider.GetTable<T>(this.Provider.Mapping.GetTableId(typeof(T)));
        }
    }
}
