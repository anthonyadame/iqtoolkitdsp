
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace IQToolkitDSP
{
    public partial class DSPMetadata : IDataServiceMetadataProvider
    {
        private Dictionary<string, ResourceSet> resourceSets;
        private Dictionary<string, ResourceType> resourceTypes;
        private Queue<ResourceTypeMapping> resourceTypeMappingsToProcess;

        private string containerName;
        private string namespaceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DSPMetadata"/> class.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="namespaceName">Name of the namespace.</param>
        public DSPMetadata(string containerName, string namespaceName)
        {
            if (containerName == null)
                throw new ArgumentException("The specified containerName is null.");
            if (namespaceName == null)
                throw new ArgumentException("The specified namespaceName is null.");
            
            this.resourceSets = new Dictionary<string, ResourceSet>();
            this.resourceTypes = new Dictionary<string, ResourceType>();
            this.containerName = containerName;
            this.namespaceName = namespaceName;

            this.resourceTypeMappingsToProcess = new Queue<ResourceTypeMapping>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DSPMetadata"/> class.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="namespaceName">Name of the namespace.</param>
        /// <param name="resourceItems">The resource items.</param>
        public DSPMetadata(string containerName, string namespaceName, Dictionary<string, Type> resourceItems) 
            : this(containerName,namespaceName)
        {
            if (resourceItems == null)
                throw new ArgumentException("The specified resourceItems is null.");

            AddResourceTypes(resourceItems);
            InitializeMetadataMapping();
        }

        /// <summary>
        /// Adds the resource types.
        /// </summary>
        /// <param name="resourceItems">The resource items.</param>
        public void AddResourceTypes(Dictionary<string, Type> resourceItems) 
        {
            if (resourceItems == null)
                throw new ArgumentException("The specified resourceItems is null.");
            
            foreach (var resourceItem in resourceItems.Keys)
            {
                Type instanceType = null;
                if (resourceItems.TryGetValue(resourceItem, out instanceType)) 
                {
                    MapResourceSet(resourceItem, instanceType);
                }
            }
        }

        /// <summary>
        /// Adds the type of the entity.
        /// </summary>
        /// <param name="instanceType">Type of the instance.</param>
        /// <returns></returns>
        public ResourceType AddEntityType(Type instanceType)
        {
            if (instanceType == null)
                throw new ArgumentException("The specified instanceType is null.");

            return AddEntityType(instanceType, instanceType.Name);
        }

        public ResourceType AddEntityType(Type instanceType, string name)
        {
            if (this.resourceTypes.Values.Any(rt => rt.InstanceType == instanceType))
            {
                throw new NotSupportedException("Multiple resource types with the same instance type are not supported.");
            }

            ResourceType resourceType = new ResourceType(instanceType, ResourceTypeKind.EntityType, null, this.namespaceName, name, false);
            resourceType.CanReflectOnInstanceType = true;
            resourceType.CustomState = new ResourceTypeAnnotation();
            this.resourceTypes.Add(resourceType.FullName, resourceType);
            return resourceType;
        }

        public ResourceType AddComplexType(Type instanceType)
        {
            if (instanceType == null)
                throw new ArgumentException("The specified instanceType is null.");

            return AddComplexType(instanceType, instanceType.Name);
        }

        public ResourceType AddComplexType(Type instanceType, string name)
        {
            if (this.resourceTypes.Values.Any(rt => rt.InstanceType == instanceType))
            {
                throw new NotSupportedException("Multiple resource types with the same instance type are not supported.");
            }

            ResourceType resourceType = new ResourceType(instanceType, ResourceTypeKind.ComplexType, null, this.namespaceName, name, false);
            resourceType.CanReflectOnInstanceType = true;
            this.resourceTypes.Add(resourceType.FullName, resourceType);
            return resourceType;
        }

        public ResourceSet AddResourceSet(string name, ResourceType entityType)
        {
            if (entityType == null)
                throw new ArgumentException("The specified resource type is null.");
            
            if (entityType.ResourceTypeKind != ResourceTypeKind.EntityType)
            {
                throw new ArgumentException("The resource type specified as the base type of a resource set is not an entity type.");
            }

            ResourceSet resourceSet = new ResourceSet(name, entityType);
            entityType.GetAnnotation().ResourceSet = resourceSet;
            this.resourceSets.Add(name, resourceSet);
            return resourceSet;
        }

        internal void SetReadOnly()
        {
            foreach (var type in this.resourceTypes.Values)
            {
                type.SetReadOnly();
            }

            foreach (var set in this.resourceSets.Values)
            {
                set.SetReadOnly();
            }
        }

        private void MarkMetadataAsReadonly()
        {
            SetReadOnly();
        }

       
        #region IDataServiceMetadataProvider Members

        public string ContainerName
        {
            get { return this.containerName; }
        }

        public string ContainerNamespace
        {
            get { return this.namespaceName; }
        }

        public System.Collections.Generic.IEnumerable<ResourceType> GetDerivedTypes(ResourceType resourceType)
        {
            return new ResourceType[0];
        }

        public ResourceAssociationSet GetResourceAssociationSet(
            ResourceSet resourceSet,
            ResourceType resourceType,
            ResourceProperty resourceProperty)
        {
            if (resourceSet == null)
                throw new ArgumentException("The specified resource set is null.");
            if (resourceType == null)
                throw new ArgumentException("The specified resource type is null.");
            if (resourceProperty == null)
                throw new ArgumentException("The specified resource property is null.");
            
            
            ResourceType targetResourceType = resourceProperty.ResourceType;
            ResourceSet targetResourceSet = this.ResourceSets.
                Where(rs => rs.ResourceType.InstanceType.IsAssignableFrom(targetResourceType.InstanceType)).Single();
                        
            string associationName = resourceType.Name + '_' + resourceProperty.Name;
            ResourceAssociationSetEnd sourceEnd = new ResourceAssociationSetEnd(resourceSet, resourceType, resourceProperty);
            ResourceAssociationSetEnd targetEnd = new ResourceAssociationSetEnd(targetResourceSet, targetResourceType, null);
            
            return new ResourceAssociationSet(associationName, sourceEnd, targetEnd);
        }

        public bool HasDerivedTypes(ResourceType resourceType)
        {
            return false;
        }

        public System.Collections.Generic.IEnumerable<ResourceSet> ResourceSets
        {
            get { return this.resourceSets.Values; }
        }

        public System.Collections.Generic.IEnumerable<ServiceOperation> ServiceOperations
        {
            get { return new ServiceOperation[0]; }
        }

        public bool TryResolveResourceSet(string name, out ResourceSet resourceSet)
        {
            return this.resourceSets.TryGetValue(name, out resourceSet); 
        }

        public bool TryResolveResourceType(string name, out ResourceType resourceType)
        {
            return this.resourceTypes.TryGetValue(name, out resourceType);
        }

        public bool TryResolveServiceOperation(string name, out ServiceOperation serviceOperation)
        {
            serviceOperation = null;
            return false;
        }

        public System.Collections.Generic.IEnumerable<ResourceType> Types
        {
            get { return this.resourceTypes.Values; }
        }

        #endregion
    }


    public partial class DSPMetadata : IDataServiceMetadataProvider
    {

        public void MapResourceSet(string resourceSetName, Type baseClientClrType)
        {
            ResourceTypeMapping resourceTypeMapping = this.CreateEntityResourceTypeMapping(baseClientClrType);

            ResourceSetMapping resourceSetMapping = new ResourceSetMapping(resourceSetName, resourceTypeMapping);
            this.resourceSets.Add(resourceSetName, resourceSetMapping);
        }

        private ResourceTypeMapping CreateEntityResourceTypeMapping(Type clrType)
        {
            ResourceType baseResourceType = null;
            this.resourceTypes.TryGetValue(clrType.BaseType.FullName, out baseResourceType);
            ResourceTypeMapping resourceTypeMapping = new ResourceTypeMapping(
                clrType,
                ResourceTypeKind.EntityType,
                baseResourceType as ResourceTypeMapping,
                this.namespaceName,
                clrType.Name,
                clrType.IsAbstract);
            this.RegisterResourceTypeMapping(resourceTypeMapping);
            return resourceTypeMapping;
        }

        private ResourceTypeMapping CreateComplexResourceTypeMapping(Type clrType)
        {
            ResourceType baseResourceType;
            this.resourceTypes.TryGetValue(clrType.BaseType.FullName, out baseResourceType);
            ResourceTypeMapping resourceTypeMapping = new ResourceTypeMapping(
                clrType,
                ResourceTypeKind.ComplexType,
                baseResourceType as ResourceTypeMapping,
                this.namespaceName,
                clrType.Name,
                clrType.IsAbstract);
            this.RegisterResourceTypeMapping(resourceTypeMapping);
            return resourceTypeMapping;
        }

        private void RegisterResourceTypeMapping(ResourceTypeMapping resourceTypeMapping)
        {
            this.resourceTypes.Add(resourceTypeMapping.InstanceType.FullName, resourceTypeMapping);
            this.resourceTypeMappingsToProcess.Enqueue(resourceTypeMapping);
        }

        public void InitializeMetadataMapping()
        {
            ResourceType resourceType;
            this.PopulateEnqueuedResourceTypeProperties();

            HashSet<Assembly> assemblies = new HashSet<Assembly>(EqualityComparer<Assembly>.Default);
            List<ResourceType> knownTypes = this.resourceTypes.Values.Where(rt => rt.BaseType == null).ToList();
            List<Type> derivedTypes = new List<Type>();
            foreach (ResourceType baseType in knownTypes)
            {
                Assembly assembly = baseType.InstanceType.Assembly;
                if (assemblies.Contains(assembly))
                {
                    continue;
                }

                assemblies.Add(assembly);

                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.IsVisible)
                    {
                        continue;
                    }

                    if (knownTypes.Any(rt => rt.InstanceType == type))
                    {
                        continue;
                    }

                    if (knownTypes.Any(rt => rt.InstanceType.IsAssignableFrom(type)))
                    {
                        derivedTypes.Add(type);
                    }
                }
            }

            foreach (Type derivedType in derivedTypes)
            {
                Stack<Type> ancestorsAndSelf = new Stack<Type>();
                Type ancestorType = derivedType;
                ResourceType baseResourceType = null;
                while (ancestorType != null)
                {
                    if (this.resourceTypes.TryGetValue(ancestorType.FullName, out baseResourceType))
                    {
                        break;
                    }

                    ancestorsAndSelf.Push(ancestorType);
                    ancestorType = ancestorType.BaseType;
                }

                if (baseResourceType != null)
                {
                    while (ancestorsAndSelf.Count > 0 && (ancestorType = ancestorsAndSelf.Pop()) != null)
                    {
                        if (knownTypes.Any(rt => rt.InstanceType == ancestorType))
                        {
                            continue;
                        }

                        if (!this.resourceTypes.TryGetValue(ancestorType.FullName, out resourceType))
                        {
                            resourceType = this.CreateEntityResourceTypeMapping(ancestorType);
                        }

                        knownTypes.Add(resourceType);
                    }
                }
            }

            this.PopulateEnqueuedResourceTypeProperties();
            this.MarkMetadataAsReadonly();        
        }

        private void PopulateEnqueuedResourceTypeProperties()
        {
            while (this.resourceTypeMappingsToProcess.Count > 0)
            {
                this.PopulateResourceTypeProperties(this.resourceTypeMappingsToProcess.Dequeue());
            }
        }

        private void PopulateResourceTypeProperties(ResourceTypeMapping resourceTypeMapping)
        {
            Type clientClrType = resourceTypeMapping.InstanceType;

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (resourceTypeMapping.BaseType != null)
            {
                bindingFlags |= BindingFlags.DeclaredOnly;
            }

            foreach (PropertyInfo propertyInfo in clientClrType.GetProperties(bindingFlags))
            {
                ResourcePropertyKind propertyKind = 0;

                Type propertyType = propertyInfo.PropertyType;
                Type enumerableElementType = TypeSystem.GetIEnumerableElementType(propertyType);
                if (enumerableElementType != null)
                {
                    propertyType = enumerableElementType;
                }

                ResourceType propertyResourceType;
                if (!this.resourceTypes.TryGetValue(propertyType.FullName, out propertyResourceType))
                {
                    propertyResourceType = this.CreatePrimitiveOrComplexResourceTypeMapping(propertyInfo.PropertyType);
                    if (propertyResourceType.ResourceTypeKind == ResourceTypeKind.ComplexType)
                    {
                        propertyKind = ResourcePropertyKind.ComplexType;
                    }
                    else
                    {
                        propertyKind = ResourcePropertyKind.Primitive;
                    }
                }
                else
                {
                    if (enumerableElementType != null)
                    {
                        propertyKind = ResourcePropertyKind.ResourceSetReference;
                        if (propertyResourceType.ResourceTypeKind != ResourceTypeKind.EntityType)
                        {
                            throw new NotSupportedException("Collections are only supported for entity types.");
                        }
                    }
                    else
                    {
                        propertyKind = ResourcePropertyKind.ResourceReference;
                    }
                }

                if (IsPropertyKeyProperty(propertyInfo))
                {
                    propertyKind |= ResourcePropertyKind.Key;
                }

                if (IsPropertyETagProperty(propertyInfo))
                {
                    propertyKind |= ResourcePropertyKind.ETag;
                }


                ResourceProperty property = new ResourceProperty(propertyInfo.Name, propertyKind, propertyResourceType);
                property.CanReflectOnInstanceTypeProperty = false;
                resourceTypeMapping.AddProperty(property);
            }
        }

        private ResourceType CreatePrimitiveOrComplexResourceTypeMapping(Type clrType)
        {
            ResourceType result = null;
            if (!this.resourceTypes.TryGetValue(clrType.FullName, out result))
            {
                result = ResourceType.GetPrimitiveResourceType(clrType);
                if (result == null)
                {
                    result = this.CreateComplexResourceTypeMapping(clrType);
                }
            }

            return result;
        }

        private static bool IsPropertyETagProperty(PropertyInfo property)
        {
            ETagAttribute etagAttribute = property.ReflectedType.GetCustomAttributes(true).OfType<ETagAttribute>().FirstOrDefault();

            if (etagAttribute != null && etagAttribute.PropertyNames.Contains(property.Name))
            {
                return true;
            }

            return false;
        }

        private static bool IsPropertyKeyProperty(PropertyInfo property)
        {
            DataServiceKeyAttribute keyAttribute = property.ReflectedType.GetCustomAttributes(true).OfType<DataServiceKeyAttribute>().FirstOrDefault();
            if (keyAttribute != null && keyAttribute.KeyNames.Contains(property.Name))
            {
                return true;
            }
            else if (property.Name == property.DeclaringType.Name + "ID")
            {
                return true;
            }
            else if (property.Name == "ID")
            {
                return true;
            }

            return false;
        }
    }

    internal class ResourceTypeMapping : ResourceType
    {
        public ResourceTypeMapping(
            Type clientClrType,
            ResourceTypeKind resourceTypeKind,
            ResourceTypeMapping baseType,
            string namespaceName,
            string name,
            bool isAbstract)
            : base(clientClrType, resourceTypeKind, baseType, namespaceName, name, isAbstract)
        {
        }
    }

    internal class ResourceSetMapping : ResourceSet
    {
        public ResourceSetMapping(string name, ResourceTypeMapping elementType)
            : base(name, elementType)
        {
        }
    }
}