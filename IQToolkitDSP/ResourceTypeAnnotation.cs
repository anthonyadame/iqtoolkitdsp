
using System.Data.Services.Providers;

namespace IQToolkitDSP
{
    internal static class ResourceTypeExtensions
    {
        /// <summary>
        /// Gets the annotation.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        internal static ResourceTypeAnnotation GetAnnotation(this ResourceType resourceType)
        {
            return resourceType.CustomState as ResourceTypeAnnotation;
        }
    }

    internal class ResourceTypeAnnotation
    {
        public ResourceSet ResourceSet { get; set; }
    }
}
