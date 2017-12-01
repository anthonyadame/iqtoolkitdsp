// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IQToolkit.Data
{
    using Common;

    public class EntitySessionEx : IEntitySession
    {
        EntityProvider provider;
        SessionProvider sessionProvider;
        Dictionary<MappingEntity, ISessionTable> tables;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySessionEx"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public EntitySessionEx(EntityProvider provider)
        {
            this.provider = provider;
            this.sessionProvider = new SessionProvider(this, provider);
            this.tables = new Dictionary<MappingEntity, ISessionTable>();
        }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        public IEntityProvider Provider
        {
            get { return this.sessionProvider; }
        }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        IEntityProvider IEntitySession.Provider
        {
            get { return this.Provider; }
        }

        /// <summary>
        /// Gets the tables.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<ISessionTable> GetTables()
        {
            return this.tables.Values;
        }

        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <param name="elementType">Type of the element.</param>
        /// <param name="tableId">The table id.</param>
        /// <returns></returns>
        public ISessionTable GetTable(Type elementType, string tableId)
        {
            return this.GetTable(this.sessionProvider.Provider.Mapping.GetEntity(elementType, tableId));
        }

        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableId">The table id.</param>
        /// <returns></returns>
        public ISessionTable<T> GetTable<T>(string tableId)
        {
            return (ISessionTable<T>)this.GetTable(typeof(T), tableId);
        }

        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected ISessionTable GetTable(MappingEntity entity)
        {
            ISessionTable table;
            if (!this.tables.TryGetValue(entity, out table))
            {
                table = this.CreateTable(entity);
                this.tables.Add(entity, table);
            }
            return table;
        }

        /// <summary>
        /// Called when [entity materialized].
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        private object OnEntityMaterialized(MappingEntity entity, object instance)
        {
            IEntitySessionTable table = (IEntitySessionTable)this.GetTable(entity);
            return table.OnEntityMaterialized(instance);
        }

        /// <summary>
        /// 
        /// </summary>
        interface IEntitySessionTable : ISessionTable
        {
            object OnEntityMaterialized(object instance);
            MappingEntity Entity { get; }
            IEnumerable<KeyValuePair<string, object>> ConcurrencyMembers { get; set; }
            
        }

        abstract class SessionTable<T> : Query<T>, ISessionTableEx<T>, ISessionTableEx, ISessionTable<T>, ISessionTable, IEntitySessionTable
        {
            EntitySessionEx session;
            MappingEntity entity;
            IEntityTable<T> underlyingTable;
            IEnumerable<KeyValuePair<string, object>> concurrencyValues;

            /// <summary>
            /// Initializes a new instance of the <see cref="SessionTable&lt;T&gt;"/> class.
            /// </summary>
            /// <param name="session">The session.</param>
            /// <param name="entity">The entity.</param>
            public SessionTable(EntitySessionEx session, MappingEntity entity)
                : base(session.sessionProvider, typeof(ISessionTable<T>))
            {
                this.session = session;
                this.entity = entity;
                this.underlyingTable = this.session.Provider.GetTable<T>(entity.TableId);
            }

            /// <summary>
            /// Gets the session.
            /// </summary>
            public IEntitySession Session
            {
                get { return this.session; }
            }

            /// <summary>
            /// Gets the entity.
            /// </summary>
            public MappingEntity Entity
            {
                get { return this.entity; }
            }

            /// <summary>
            /// Gets the provider table.
            /// </summary>
            public IEntityTable<T> ProviderTable
            {
                get { return this.underlyingTable; }
            }

            /// <summary>
            /// Gets the provider table.
            /// </summary>
            IEntityTable ISessionTable.ProviderTable
            {
                get { return this.underlyingTable; }
            }

            /// <summary>
            /// Gets or sets the concurrency members.
            /// </summary>
            /// <value>
            /// The concurrency members.
            /// </value>
            public IEnumerable<KeyValuePair<string, object>> ConcurrencyMembers
            {
                get { return this.concurrencyValues; }
                set { this.concurrencyValues = value; }
            }

            /// <summary>
            /// Gets the by id.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <returns></returns>
            public T GetById(object id)
            {
                return this.underlyingTable.GetById(id);
            }

            /// <summary>
            /// Gets the by id.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <returns></returns>
            object ISessionTable.GetById(object id)
            {
                return this.GetById(id);
            }

            /// <summary>
            /// Called when [entity materialized].
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns></returns>
            public virtual object OnEntityMaterialized(object instance)
            {
                return instance;
            }

            /// <summary>
            /// Sets the submit action.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <param name="action">The action.</param>
            public virtual void SetSubmitAction(T instance, SubmitAction action)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Sets the submit action.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <param name="action">The action.</param>
            void ISessionTable.SetSubmitAction(object instance, SubmitAction action)
            {
                this.SetSubmitAction((T)instance, action);
            }

            /// <summary>
            /// Gets the submit action.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns></returns>
            public virtual SubmitAction GetSubmitAction(T instance)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets the submit action.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns></returns>
            SubmitAction ISessionTable.GetSubmitAction(object instance)
            {
                return this.GetSubmitAction((T)instance);
            }
        }

        class SessionProvider : QueryProvider, IEntityProvider, ICreateExecutor
        {
            EntitySessionEx session;
            EntityProvider provider;

            /// <summary>
            /// Initializes a new instance of the <see cref="SessionProvider"/> class.
            /// </summary>
            /// <param name="session">The session.</param>
            /// <param name="provider">The provider.</param>
            public SessionProvider(EntitySessionEx session, EntityProvider provider)
            {
                this.session = session;
                this.provider = provider;
            }

            /// <summary>
            /// Gets the provider.
            /// </summary>
            public EntityProvider Provider
            {
                get { return this.provider; }
            }

            /// <summary>
            /// Executes the specified expression.
            /// </summary>
            /// <param name="expression">The expression.</param>
            /// <returns></returns>
            public override object Execute(Expression expression)
            {
                return this.provider.Execute(expression);
            }

            /// <summary>
            /// Gets the query text.
            /// </summary>
            /// <param name="expression">The expression.</param>
            /// <returns></returns>
            public override string GetQueryText(Expression expression)
            {
                return this.provider.GetQueryText(expression);
            }

            /// <summary>
            /// Gets the table.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="tableId">The table id.</param>
            /// <returns></returns>
            public IEntityTable<T> GetTable<T>(string tableId)
            {
                return this.provider.GetTable<T>(tableId);
            }

            /// <summary>
            /// Gets the table.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="tableId">The table id.</param>
            /// <returns></returns>
            public IEntityTable GetTable(Type type, string tableId)
            {
                return this.provider.GetTable(type, tableId);
            }

            /// <summary>
            /// Determines whether this instance [can be evaluated locally] the specified expression.
            /// </summary>
            /// <param name="expression">The expression.</param>
            /// <returns>
            ///   <c>true</c> if this instance [can be evaluated locally] the specified expression; otherwise, <c>false</c>.
            /// </returns>
            public bool CanBeEvaluatedLocally(Expression expression)
            {
                return this.provider.Mapping.CanBeEvaluatedLocally(expression);
            }

            /// <summary>
            /// Determines whether this instance [can be parameter] the specified expression.
            /// </summary>
            /// <param name="expression">The expression.</param>
            /// <returns>
            ///   <c>true</c> if this instance [can be parameter] the specified expression; otherwise, <c>false</c>.
            /// </returns>
            public bool CanBeParameter(Expression expression)
            {
                return this.provider.CanBeParameter(expression);
            }

            /// <summary>
            /// Creates the executor.
            /// </summary>
            /// <returns></returns>
            QueryExecutor ICreateExecutor.CreateExecutor()
            {
                return new SessionExecutor(this.session, ((ICreateExecutor)this.provider).CreateExecutor());
            }
        }

        class SessionExecutor : QueryExecutor
        {
            EntitySessionEx session;
            QueryExecutor executor;

            /// <summary>
            /// Initializes a new instance of the <see cref="SessionExecutor"/> class.
            /// </summary>
            /// <param name="session">The session.</param>
            /// <param name="executor">The executor.</param>
            public SessionExecutor(EntitySessionEx session, QueryExecutor executor)
            {
                this.session = session;
                this.executor = executor;
            }

            /// <summary>
            /// Gets the rows affected.
            /// </summary>
            public override int RowsAffected
            {
                get { return this.executor.RowsAffected; }
            }

            /// <summary>
            /// Converts the specified value.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="type">The type.</param>
            /// <returns></returns>
            public override object Convert(object value, Type type)
            {
                return this.executor.Convert(value, type);
            }

            /// <summary>
            /// Executes the specified command.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="command">The command.</param>
            /// <param name="fnProjector">The fn projector.</param>
            /// <param name="entity">The entity.</param>
            /// <param name="paramValues">The param values.</param>
            /// <returns></returns>
            public override IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues)
            {
                return this.executor.Execute<T>(command, Wrap(fnProjector, entity), entity, paramValues);
            }

            /// <summary>
            /// Executes the batch.
            /// </summary>
            /// <param name="query">The query.</param>
            /// <param name="paramSets">The param sets.</param>
            /// <param name="batchSize">Size of the batch.</param>
            /// <param name="stream">if set to <c>true</c> [stream].</param>
            /// <returns></returns>
            public override IEnumerable<int> ExecuteBatch(QueryCommand query, IEnumerable<object[]> paramSets, int batchSize, bool stream)
            {
                return this.executor.ExecuteBatch(query, paramSets, batchSize, stream);
            }

            /// <summary>
            /// Executes the batch.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="query">The query.</param>
            /// <param name="paramSets">The param sets.</param>
            /// <param name="fnProjector">The fn projector.</param>
            /// <param name="entity">The entity.</param>
            /// <param name="batchSize">Size of the batch.</param>
            /// <param name="stream">if set to <c>true</c> [stream].</param>
            /// <returns></returns>
            public override IEnumerable<T> ExecuteBatch<T>(QueryCommand query, IEnumerable<object[]> paramSets, Func<FieldReader, T> fnProjector, MappingEntity entity, int batchSize, bool stream)
            {
                return this.executor.ExecuteBatch<T>(query, paramSets, Wrap(fnProjector, entity), entity, batchSize, stream);
            }

            /// <summary>
            /// Executes the deferred.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="query">The query.</param>
            /// <param name="fnProjector">The fn projector.</param>
            /// <param name="entity">The entity.</param>
            /// <param name="paramValues">The param values.</param>
            /// <returns></returns>
            public override IEnumerable<T> ExecuteDeferred<T>(QueryCommand query, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues)
            {
                return this.executor.ExecuteDeferred<T>(query, Wrap(fnProjector, entity), entity, paramValues);
            }

            /// <summary>
            /// Executes the command.
            /// </summary>
            /// <param name="query">The query.</param>
            /// <param name="paramValues">The param values.</param>
            /// <returns></returns>
            public override int ExecuteCommand(QueryCommand query, object[] paramValues)
            {
                return this.executor.ExecuteCommand(query, paramValues);
            }

            /// <summary>
            /// Wraps the specified fn projector.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="fnProjector">The fn projector.</param>
            /// <param name="entity">The entity.</param>
            /// <returns></returns>
            private Func<FieldReader, T> Wrap<T>(Func<FieldReader, T> fnProjector, MappingEntity entity)
            {
                Func<FieldReader, T> fnWrapped = (fr) => (T)this.session.OnEntityMaterialized(entity, fnProjector(fr));
                return fnWrapped;
            }
        }

        /// <summary>
        /// Submits the changes.
        /// </summary>
        public virtual void SubmitChanges()
        {
            this.provider.DoTransacted(
                delegate
                {
                    var submitted = new List<TrackedItem>();

                    // do all submit actions
                    foreach (var item in this.GetOrderedItems())
                    {
                        if (item.Table.SubmitChanges(item))
                        {
                            submitted.Add(item);
                        }
                    }

                    // on completion, accept changes
                    foreach (var item in submitted)
                    {
                        item.Table.AcceptChanges(item);
                    }
                }
            );
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected virtual ISessionTable CreateTable(MappingEntity entity)
        {
            return (ISessionTable)Activator.CreateInstance(typeof(TrackedTable<>).MakeGenericType(entity.ElementType), new object[] { this, entity });
        }
        
        interface ITrackedTable : IEntitySessionTable
        {
            object GetFromCacheById(object key);
            IEnumerable<TrackedItem> TrackedItems { get; }
            TrackedItem GetTrackedItem(object instance);
            bool SubmitChanges(TrackedItem item);
            void AcceptChanges(TrackedItem item);
        }


        class TrackedTable<T> : SessionTable<T>, ITrackedTable
        {
            Dictionary<T, TrackedItem> tracked;
            Dictionary<object, T> identityCache;

            /// <summary>
            /// Initializes a new instance of the <see cref="TrackedTable&lt;T&gt;"/> class.
            /// </summary>
            /// <param name="session">The session.</param>
            /// <param name="entity">The entity.</param>
            public TrackedTable(EntitySessionEx session, MappingEntity entity)
                : base(session, entity)
            {
                this.tracked = new Dictionary<T, TrackedItem>();
                this.identityCache = new Dictionary<object, T>();
            }

            /// <summary>
            /// Gets the tracked items.
            /// </summary>
            IEnumerable<TrackedItem> ITrackedTable.TrackedItems
            {
                get { return this.tracked.Values; }
            }

            /// <summary>
            /// Gets the tracked item.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns></returns>
            TrackedItem ITrackedTable.GetTrackedItem(object instance)
            {
                TrackedItem ti;
                if (this.tracked.TryGetValue((T)instance, out ti))
                    return ti;
                return null;
            }

            /// <summary>
            /// Gets from cache by id.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns></returns>
            object ITrackedTable.GetFromCacheById(object key)
            {
                T cached;
                this.identityCache.TryGetValue(key, out cached);
                return cached;
            }

            /// <summary>
            /// Submits the changes.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <returns></returns>
            private bool SubmitChanges(TrackedItem item)
            {
                switch (item.State)
                {
                    case SubmitAction.Delete:
                        this.ProviderTable.Delete(item.Instance);
                        return true;
                    case SubmitAction.Insert:
                        this.ProviderTable.Insert(item.Instance);
                        return true;
                    case SubmitAction.InsertOrUpdate:
                        this.ProviderTable.InsertOrUpdate(item.Instance);
                        return true;
                    case SubmitAction.PossibleUpdate:
                        if (item.Original != null &&
                            this.Mapping.IsModified(item.Entity, item.Instance, item.Original))
                        {
                            var updatecheck = this.GetUpdateExpression(item, this.ConcurrencyMembers);

                            this.ProviderTable.Update((T)item.Instance, updatecheck);

                            return true;
                        }
                        break;
                    case SubmitAction.Update:
                        this.ProviderTable.Update(item.Instance);
                        return true;
                    default:
                        break; // do nothing
                }
                return false;
            }

            /// <summary>
            /// Gets the update expression.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="concurrencyValues">The concurrency values.</param>
            /// <returns></returns>
            private Expression<Func<T, bool>> GetUpdateExpression(TrackedItem item, IEnumerable<KeyValuePair<string, object>> concurrencyValues)
            {
                if (concurrencyValues == null) return null;
                
                ParameterExpression p = Expression.Parameter(typeof(T), "p");
                Expression pred = null;

                List<string> concurrencyMembers = concurrencyValues.Select(m => m.Key).ToList();

                foreach (var mi in this.Mapping.GetMappedMembers(item.Entity))
                {                    
                    if (concurrencyMembers.Contains(mi.Name)) 
                    { 
                        Expression eq = Expression.MakeMemberAccess(p, mi).Equal(Expression.Constant(mi.GetValue(item.Original)));
                        pred = (pred == null) ? eq : pred.And(eq);  
                    }
                }

                if (pred == null) return null;
                
                return Expression.Lambda<Func<T, bool>>(pred, p);
            }

            /// <summary>
            /// Submits the changes.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <returns></returns>
            bool ITrackedTable.SubmitChanges(TrackedItem item)
            {
                return this.SubmitChanges(item);
            }

            /// <summary>
            /// Accepts the changes.
            /// </summary>
            /// <param name="item">The item.</param>
            private void AcceptChanges(TrackedItem item)
            {
                switch (item.State)
                {
                    case SubmitAction.Delete:
                        this.RemoveFromCache((T)item.Instance);
                        this.AssignAction((T)item.Instance, SubmitAction.None);
                        break;
                    case SubmitAction.Insert:
                        this.AddToCache((T)item.Instance);
                        this.AssignAction((T)item.Instance, SubmitAction.PossibleUpdate);
                        break;
                    case SubmitAction.InsertOrUpdate:
                        this.AddToCache((T)item.Instance);
                        this.AssignAction((T)item.Instance, SubmitAction.PossibleUpdate);
                        break;
                    case SubmitAction.PossibleUpdate:
                    case SubmitAction.Update:
                        this.AssignAction((T)item.Instance, SubmitAction.PossibleUpdate);
                        break;
                    default:
                        break; // do nothing
                }
            }

            /// <summary>
            /// Accepts the changes.
            /// </summary>
            /// <param name="item">The item.</param>
            void ITrackedTable.AcceptChanges(TrackedItem item)
            {
                this.AcceptChanges(item);
            }

            /// <summary>
            /// Called when [entity materialized].
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns></returns>
            public override object OnEntityMaterialized(object instance)
            {
                T typedInstance = (T)instance;
                var cached = this.AddToCache(typedInstance);
                if ((object)cached == (object)typedInstance)
                {
                    this.AssignAction(typedInstance, SubmitAction.PossibleUpdate);
                }
                return cached;
            }

            /// <summary>
            /// Gets the submit action.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns></returns>
            public override SubmitAction GetSubmitAction(T instance)
            {
                TrackedItem ti;
                if (this.tracked.TryGetValue(instance, out ti))
                {
                    if (ti.State == SubmitAction.PossibleUpdate)
                    {
                        if (this.Mapping.IsModified(ti.Entity, ti.Instance, ti.Original))
                        {
                            return SubmitAction.Update;
                        }
                        else
                        {
                            return SubmitAction.None;
                        }
                    }
                    return ti.State;
                }
                return SubmitAction.None;
            }

            /// <summary>
            /// Sets the submit action.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <param name="action">The action.</param>
            public override void SetSubmitAction(T instance, SubmitAction action)
            {
                switch (action)
                {
                    case SubmitAction.None:
                    case SubmitAction.PossibleUpdate:
                    case SubmitAction.Update:
                    case SubmitAction.Delete:
                        var cached = this.AddToCache(instance);
                        if ((object)cached != (object)instance)
                        {
                            throw new InvalidOperationException("An different instance with the same key is already in the cache.");
                        }
                        break;
                }
                this.AssignAction(instance, action);
            }

            /// <summary>
            /// Gets the mapping.
            /// </summary>
            private QueryMapping Mapping
            {
                get { return ((EntitySessionEx)this.Session).provider.Mapping; }
            }

            /// <summary>
            /// Adds to cache.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns></returns>
            private T AddToCache(T instance)
            {
                object key = this.Mapping.GetPrimaryKey(this.Entity, instance);
                T cached;
                if (!this.identityCache.TryGetValue(key, out cached))
                {
                    cached = instance;
                    this.identityCache.Add(key, cached);
                }
                return cached;
            }

            /// <summary>
            /// Removes from cache.
            /// </summary>
            /// <param name="instance">The instance.</param>
            private void RemoveFromCache(T instance)
            {
                object key = this.Mapping.GetPrimaryKey(this.Entity, instance);
                this.identityCache.Remove(key);
            }

            /// <summary>
            /// Assigns the action.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <param name="action">The action.</param>
            private void AssignAction(T instance, SubmitAction action)
            {
                TrackedItem ti;
                this.tracked.TryGetValue(instance, out ti);

                switch (action)
                {
                    case SubmitAction.Insert:
                    case SubmitAction.InsertOrUpdate:
                    case SubmitAction.Update:
                    case SubmitAction.Delete:
                    case SubmitAction.None:
                        this.tracked[instance] = new TrackedItem(this, instance, ti != null ? ti.Original : null, action, ti != null ? ti.HookedEvent : false);
                        break;
                    case SubmitAction.PossibleUpdate:
                        INotifyPropertyChanging notify = instance as INotifyPropertyChanging;
                        if (notify != null)
                        {
                            if (!ti.HookedEvent)
                            {
                                notify.PropertyChanging += new PropertyChangingEventHandler(this.OnPropertyChanging);
                            }
                            this.tracked[instance] = new TrackedItem(this, instance, null, SubmitAction.PossibleUpdate, true);
                        }
                        else
                        {
                            var original = this.Mapping.CloneEntity(this.Entity, instance);
                            this.tracked[instance] = new TrackedItem(this, instance, original, SubmitAction.PossibleUpdate, false);
                        }
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unknown SubmitAction: {0}", action));
                }
            }

            /// <summary>
            /// Called when [property changing].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="args">The <see cref="System.ComponentModel.PropertyChangingEventArgs"/> instance containing the event data.</param>
            protected virtual void OnPropertyChanging(object sender, PropertyChangingEventArgs args)
            {
                TrackedItem ti;
                if (this.tracked.TryGetValue((T)sender, out ti) && ti.State == SubmitAction.PossibleUpdate)
                {
                    object clone = this.Mapping.CloneEntity(ti.Entity, ti.Instance);
                    this.tracked[(T)sender] = new TrackedItem(this, ti.Instance, clone, SubmitAction.Update, true);
                }
            }
        }

        class TrackedItem
        {
            ITrackedTable table;
            object instance;
            object original;
            SubmitAction state;
            bool hookedEvent;

            /// <summary>
            /// Initializes a new instance of the <see cref="TrackedItem"/> class.
            /// </summary>
            /// <param name="table">The table.</param>
            /// <param name="instance">The instance.</param>
            /// <param name="original">The original.</param>
            /// <param name="state">The state.</param>
            /// <param name="hookedEvent">if set to <c>true</c> [hooked event].</param>
            internal TrackedItem(ITrackedTable table, object instance, object original, SubmitAction state, bool hookedEvent)
            {
                this.table = table;
                this.instance = instance;
                this.original = original;
                this.state = state;
                this.hookedEvent = hookedEvent;
            }

            /// <summary>
            /// Gets the table.
            /// </summary>
            public ITrackedTable Table
            {
                get { return this.table; }
            }

            /// <summary>
            /// Gets the entity.
            /// </summary>
            public MappingEntity Entity
            {
                get { return this.table.Entity; }
            }

            /// <summary>
            /// Gets the instance.
            /// </summary>
            public object Instance
            {
                get { return this.instance; }
            }

            /// <summary>
            /// Gets the original.
            /// </summary>
            public object Original
            {
                get { return this.original; }
            }

            /// <summary>
            /// Gets the state.
            /// </summary>
            public SubmitAction State
            {
                get { return this.state; }
            }

            /// <summary>
            /// Gets a value indicating whether [hooked event].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [hooked event]; otherwise, <c>false</c>.
            /// </value>
            public bool HookedEvent
            {
                get { return this.hookedEvent; }
            }

            public static readonly IEnumerable<TrackedItem> EmptyList = new TrackedItem[] { };
        }

        private IEnumerable<TrackedItem> GetOrderedItems()
        {
            var items = (from tab in this.GetTables()
                         from ti in ((ITrackedTable)tab).TrackedItems
                         where ti.State != SubmitAction.None
                         select ti).ToList();

            // build edge maps to represent all references between entities
            var edges = this.GetEdges(items).Distinct().ToList();
            var stLookup = edges.ToLookup(e => e.Source, e => e.Target);
            var tsLookup = edges.ToLookup(e => e.Target, e => e.Source);

            return TopologicalSorter.Sort(items, item =>
            {
                switch (item.State)
                {
                    case SubmitAction.Insert:
                    case SubmitAction.InsertOrUpdate:
                        // all things this instance depends on must come first
                        var beforeMe = stLookup[item];

                        // if another object exists with same key that is being deleted, then the delete must come before the insert
                        object cached = item.Table.GetFromCacheById(this.provider.Mapping.GetPrimaryKey(item.Entity, item.Instance));
                        if (cached != null && cached != item.Instance)
                        {
                            var ti = item.Table.GetTrackedItem(cached);
                            if (ti != null && ti.State == SubmitAction.Delete)
                            {
                                beforeMe = beforeMe.Concat(new[] { ti });
                            }
                        }
                        return beforeMe;

                    case SubmitAction.Delete:
                        // all things that depend on this instance must come first
                        return tsLookup[item];
                    default:
                        return TrackedItem.EmptyList;
                }
            });
        }

        /// <summary>
        /// Gets the tracked item.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        private TrackedItem GetTrackedItem(EntityInfo entity)
        {
            ITrackedTable table = (ITrackedTable)this.GetTable(entity.Mapping);
            return table.GetTrackedItem(entity.Instance);
        }

        /// <summary>
        /// Gets the edges.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        private IEnumerable<Edge> GetEdges(IEnumerable<TrackedItem> items)
        {
            foreach (var c in items)
            {
                foreach (var d in this.provider.Mapping.GetDependingEntities(c.Entity, c.Instance))
                {
                    var dc = this.GetTrackedItem(d);
                    if (dc != null)
                    {
                        yield return new Edge(dc, c);
                    }
                }
                foreach (var d in this.provider.Mapping.GetDependentEntities(c.Entity, c.Instance))
                {
                    var dc = this.GetTrackedItem(d);
                    if (dc != null)
                    {
                        yield return new Edge(c, dc);
                    }
                }
            }
        }

        class Edge : IEquatable<Edge>
        {
            internal TrackedItem Source { get; private set; }
            internal TrackedItem Target { get; private set; }
            int hash;

            /// <summary>
            /// Initializes a new instance of the <see cref="Edge"/> class.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <param name="target">The target.</param>
            internal Edge(TrackedItem source, TrackedItem target)
            {
                this.Source = source;
                this.Target = target;
                this.hash = this.Source.GetHashCode() + this.Target.GetHashCode();
            }

            /// <summary>
            /// Equalses the specified edge.
            /// </summary>
            /// <param name="edge">The edge.</param>
            /// <returns></returns>
            public bool Equals(Edge edge)
            {
                return edge != null && this.Source == edge.Source && this.Target == edge.Target;
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                return this.Equals(obj as Edge);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return this.hash;
            }
        }
    
    
    }


    public interface ISessionTableEx : ISessionTable
    {
        IEnumerable<KeyValuePair<string, object>> ConcurrencyMembers { get; set; }
    }

    public interface ISessionTableEx<T> : ISessionTable
    {
        IEnumerable<KeyValuePair<string, object>> ConcurrencyMembers { get; set; }
    }
}