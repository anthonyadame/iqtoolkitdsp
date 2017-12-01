
using System.Data.Services.Providers;
using System.Reflection;

namespace IQToolkitDSP
{
    internal static class ResourcePropertyExtensions
    {
        //internal static ResourcePropertyAnnotation GetAnnotation(this ResourceProperty resourceProperty)
        //{
        //    return resourceProperty.CustomState as ResourcePropertyAnnotation;
        //}
    }

    internal class ResourcePropertyAnnotation
    {
        public PropertyInfo InstanceProperty { get; set; }

        public ResourceAssociationSet ResourceAssociationSet { get; set; }
    }
}
