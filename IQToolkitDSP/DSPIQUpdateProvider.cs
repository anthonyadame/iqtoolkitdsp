
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Linq;
using System.Reflection;

namespace IQToolkitDSP
{
    using IQToolkit;
    using IQToolkit.Data;

    public class DSPIQUpdateProvider : IDataServiceUpdateProvider
    {
        private DSPMetadata metadata;
        private DbEntityProvider provider;
        private IEntitySession session;
        private List<Action> pendingChanges;


        /// <summary>
        /// Initializes a new instance of the <see cref="DSPIQUpdateProvider"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="session">The session.</param>
        public DSPIQUpdateProvider(DbEntityProvider provider, DSPMetadata metadata, IEntitySession session)
        {
            if (provider == null)
                throw new ArgumentException("The specified provider is null.");
            if (metadata == null)
                throw new ArgumentException("The specified metadata is null.");
            if (session == null)
                throw new ArgumentException("The specified session is null.");


            this.metadata = metadata;
            this.provider = provider;
            this.session = session;
            this.pendingChanges = new List<Action>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DSPIQUpdateProvider"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="metadata">The metadata.</param>
        public DSPIQUpdateProvider(DbEntityProvider provider, DSPMetadata metadata) :
            this(provider, metadata, new EntitySession(provider))
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DSPIQUpdateProvider"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public DSPIQUpdateProvider(IContext context) : 
			this(context.DbEntityProvider,context.ContextMapping)
        {
            if (context == null)
                throw new ArgumentException("The specified context is null.");
         
        }


        #region IUpdatable Members

        /// <summary>
        /// Adds the specified value to the collection.
        /// </summary>
        /// <param name="targetResource">Target object that defines the property.</param>
        /// <param name="propertyName">The name of the collection property to which the resource should be added..</param>
        /// <param name="resourceToBeAdded">The opaque object representing the resource to be added.</param>
        public virtual void AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
        {
            IList list = this.GetValue(targetResource, propertyName) as IList;
            if (list == null)
            {
                throw new ArgumentException("The value of the property '" + propertyName + "' does not implement IList, which is a requirement for resource set reference property.");
            }

            this.pendingChanges.Add(() =>
            {
                list.Add(resourceToBeAdded);
            });
        }

        /// <summary>
        /// Cancels a change to the data.
        /// </summary>
        public void ClearChanges()
        {
            this.pendingChanges.Clear();
            ClearSessionChanges(); 
        }

        private void ClearSessionChanges() 
        {
            Type sessionType = this.session.GetType();

            if (sessionType.FullName == typeof(EntitySession).FullName)
            {
                this.session = new EntitySession(this.provider);
            }
            else
            {
                this.session = new EntitySessionEx(this.provider);
            }
        }

        /// <summary>
        /// Creates the resource of the specified type and that belongs to the specified container.
        /// </summary>
        /// <param name="containerName">The name of the entity set to which the resource belongs.</param>
        /// <param name="fullTypeName">The full namespace-qualified type name of the resource.</param>
        /// <returns>
        /// The object representing a resource of specified type and belonging to the specified container.
        /// </returns>
        public virtual object CreateResource(string containerName, string fullTypeName)
        {
            Type instanceType = Type.GetType(fullTypeName);

            if (instanceType == null) 
            {
                ResourceType resourceType;
                if (!this.metadata.TryResolveResourceType(fullTypeName, out resourceType))
                {
                    throw new ArgumentException("Unknown resource type '" + fullTypeName + "'.");
                }
                else 
                {
                    instanceType = resourceType.InstanceType;
                }
            }

            object newResource = Activator.CreateInstance(instanceType);

            this.session.GetTable(instanceType, this.GetTableId(instanceType)).SetSubmitAction(newResource, SubmitAction.Insert);
            return newResource;
        }

        /// <summary>
        /// Deletes the specified resource.
        /// </summary>
        /// <param name="targetResource">The resource to be deleted.</param>
        public virtual void DeleteResource(object targetResource)
        {
            object dspTargetResource = ValidateResource(targetResource);

            this.session
                    .GetTable(dspTargetResource.GetType(), this.GetTableId(dspTargetResource.GetType()))
                    .SetSubmitAction(dspTargetResource, SubmitAction.Delete);
        }

        /// <summary>
        /// Gets the resource of the specified type identified by a query and type name.
        /// </summary>
        /// <param name="query">Language integrated query (LINQ) pointing to a particular resource.</param>
        /// <param name="fullTypeName">The fully qualified type name of resource.</param>
        /// <returns>
        /// An opaque object representing a resource of the specified type, referenced by the specified query.
        /// </returns>
        public virtual object GetResource(System.Linq.IQueryable query, string fullTypeName)
        {

            if (query == null)
                throw new ArgumentException("The specified IQueryable is null.");
            
            object resource = null;
            foreach (object r in query)
            {
                if (resource != null)
                {
                    throw new ArgumentException(String.Format(System.Globalization.CultureInfo.CurrentCulture, "Invalid Uri specified. The query '{0}' must refer to a single resource", query.ToString()));
                }

                resource = r;
            }

            if (resource != null)
            {
                Type resourceType = resource.GetType();
                if (fullTypeName != null)
                {

                    if (resourceType.FullName != fullTypeName)
                    {
                        throw new ArgumentException("Unknown resource type '" + fullTypeName + "'.");
                    }
                }

                this.session.GetTable(resourceType, this.GetTableId(resourceType)).SetSubmitAction(resource, SubmitAction.PossibleUpdate);

                return resource;
            }

            return null;
        }

        /// <summary>
        /// Gets the value of the specified property on the target object.
        /// </summary>
        /// <param name="targetResource">An opaque object that represents a resource.</param>
        /// <param name="propertyName">The name of the property whose value needs to be retrieved.</param>
        /// <returns>
        /// The value of the object.
        /// </returns>
        public virtual object GetValue(object targetResource, string propertyName)
        {
            object dspTargetResource = ValidateResource(targetResource);

            PropertyInfo pi = TypeSystem.GetPropertyInfoForType(dspTargetResource.GetType(), propertyName, false);
            object value = null;
            
            try
            {
                value = pi.GetValue(dspTargetResource, null);
            }
            catch (Exception ex)
            {
                throw new DataServiceException(string.Format(System.Globalization.CultureInfo.CurrentCulture, "Failed getting property {0} value", propertyName), ex);
            }

            return value;
        }

        /// <summary>
        /// Removes the specified value from the collection.
        /// </summary>
        /// <param name="targetResource">The target object that defines the property.</param>
        /// <param name="propertyName">The name of the property whose value needs to be updated.</param>
        /// <param name="resourceToBeRemoved">The property value that needs to be removed.</param>
        public virtual void RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
        {
            IList list = this.GetValue(targetResource, propertyName) as IList;
            if (list == null)
            {
                throw new ArgumentException("The value of the property '" + propertyName + "' does not implement IList, which is a requirement for resource set reference property.");
            }

            this.pendingChanges.Add(() =>
            {
                list.Remove(resourceToBeRemoved);
            });
        }

        /// <summary>
        /// Resets the resource identified by the parameter <paramref name="resource "/>to its default value.
        /// </summary>
        /// <param name="resource">The resource to be updated.</param>
        /// <returns>
        /// The resource with its value reset to the default value.
        /// </returns>
        public virtual object ResetResource(object resource)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the instance of the resource represented by the specified resource object.
        /// </summary>
        /// <param name="resource">The object representing the resource whose instance needs to be retrieved.</param>
        /// <returns>
        /// Returns the instance of the resource represented by the specified resource object.
        /// </returns>
        public object ResolveResource(object resource)
        {
            return resource;
        }

        /// <summary>
        /// Saves all the changes that have been made by using the <see cref="T:System.Data.Services.IUpdatable"/> APIs.
        /// </summary>
        public void SaveChanges()
        {
            foreach (var pendingChange in this.pendingChanges)
            {
                pendingChange();
            }
            this.session.SubmitChanges();
        }

        /// <summary>
        /// Sets the value of the specified reference property on the target object.
        /// </summary>
        /// <param name="targetResource">The target object that defines the property.</param>
        /// <param name="propertyName">The name of the property whose value needs to be updated.</param>
        /// <param name="propertyValue">The property value to be updated.</param>
        public virtual void SetReference(object targetResource, string propertyName, object propertyValue)
        {
            this.SetValue(targetResource, propertyName, propertyValue);
        }

        /// <summary>
        /// Sets the value of the property with the specified name on the target resource to the specified property value.
        /// </summary>
        /// <param name="targetResource">The target object that defines the property.</param>
        /// <param name="propertyName">The name of the property whose value needs to be updated.</param>
        /// <param name="propertyValue">The property value for update.</param>
        public virtual void SetValue(object targetResource, string propertyName, object propertyValue)
        {
            object dspTargetResource = ValidateResource(targetResource);

            PropertyInfo pi = TypeSystem.GetPropertyInfoForType(dspTargetResource.GetType(), propertyName, true);

            this.pendingChanges.Add(() =>
            {
                pi.SetValue(dspTargetResource, propertyValue, null);
            });
        }

        /// <summary>
        /// Applies the resource default value.
        /// </summary>
        /// <param name="targetResource">The target resource.</param>
        protected virtual void ApplyResourceDefaultValue(object targetResource)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataServiceUpdateProvider Members

        /// <summary>
        /// Validates the resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        public static object ValidateResource(object resource) 
        {
            if (resource == null)
                throw new ArgumentException("The specified resource is not valid.");
            return resource;
        }

        /// <summary>
        /// Gets the table id.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private string GetTableId(Type type)
        {
            return this.provider.Mapping.GetTableId(type);
        }

        /// <summary>
        /// Supplies the eTag value for the given entity resource.
        /// </summary>
        /// <param name="resourceCookie">Cookie that represents the resource.</param>
        /// <param name="checkForEquality">A <see cref="T:System.Boolean"/> that is true when property values must be compared for equality; false when property values must be compared for inequality.</param>
        /// <param name="concurrencyValues">An <see cref="T:System.Collections.Generic.IEnumerable`1"/> list of the eTag property names and corresponding values.</param>
        public void SetConcurrencyValues(object resourceCookie, bool? checkForEquality, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>> concurrencyValues)
        {
            if (checkForEquality == null)
                throw new DataServiceException(400, "Missing If-Match header");
            if (concurrencyValues == null)
                throw new DataServiceException(400, "The specified concurrencyValues is not valid.");
            

            foreach (var concurrencyToken in concurrencyValues)
            {
                object value = this.GetValue(resourceCookie, concurrencyToken.Key);
                if (checkForEquality == true)
                {
                    if (!Object.Equals(value, concurrencyToken.Value))
                    {
                        throw new DataServiceException(412, String.Format(System.Globalization.CultureInfo.CurrentCulture, "Concurrency: precondition failed for property '{0}'", concurrencyToken.Key));
                    }
                }
                else
                {
                    if (Object.Equals(value, concurrencyToken.Value))
                    {
                        throw new DataServiceException(412, String.Format(System.Globalization.CultureInfo.CurrentCulture, "Concurrency: precondition failed for property '{0}'", concurrencyToken.Key));
                    }
                }

                this.SetValue(resourceCookie, concurrencyToken.Key, concurrencyToken.Value);
            }

            SetConcurrencyValuesEx(resourceCookie, concurrencyValues);
        }

        /// <summary>
        /// Sets the concurrency values for the extended entitysession.
        /// </summary>
        /// <param name="resourceCookie">The resource cookie.</param>
        /// <param name="concurrencyValues">The concurrency values.</param>
        private void SetConcurrencyValuesEx(object resourceCookie, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>> concurrencyValues) 
        {
            Type sessionType = this.session.GetType();

            if (sessionType.FullName == typeof(EntitySessionEx).FullName)
            {
                var sessionTable = this.session.GetTable(resourceCookie.GetType(),
                                            this.GetTableId(resourceCookie.GetType()));

                ((ISessionTableEx)sessionTable).ConcurrencyMembers = concurrencyValues;
            }
        
        }

        #endregion

    }
    
}
