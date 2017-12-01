using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.IO;

namespace AtomPayloadAnalyzer
{
    /// <summary>
    /// This class serves as a utility class with functions calculating expected type of result and parsing edmx data
    /// </summary>
    public class UriAnalyzer
    {
        /// <summary>
        /// This function returns the XElement with the specified name and attribute value
        /// </summary>
        /// <param name="root">desired element's parent</param>
        /// <param name="name">name of element</param>
        /// <param name="attribute">name of attribute</param>
        /// <param name="value">value of attribute</param>
        /// <returns></returns>
        public static XElement getElementByAttribute(XElement root, string name, string attribute, string value)
        {
            foreach (XElement element in root.Elements())
            {
                if (element.Name.LocalName == name && (string)element.Attribute(attribute) == value)
                    return element;
            }
            return null;
        }

        /// <summary>
        /// This function returns the default entity container
        /// </summary>
        /// <param name="edmx">root element of edmx</param>
        /// <returns></returns>
        public static XElement getDefaultEntityContainer(XElement edmx)
        {
            XNamespace edmxNamespace = edmx.GetNamespaceOfPrefix("edmx");
            XElement dataServicesElement = edmx.Element(edmxNamespace + "DataServices");
			return dataServicesElement.Descendants().Where(c => c.Name.LocalName == "EntityContainer").FirstOrDefault();
        }

        /// <summary>
        /// This function returns schema with attribute "Namespace" equal to a specified one, usually the name of entity container
        /// </summary>
        /// <param name="edmx">root element of edmx</param>
        /// <param name="strNameSpace">specified value of namespace</param>
        /// <returns></returns>
        public static XElement getSchemaByNamespace(XElement edmx, string strNameSpace)
        {
            XNamespace edmxNamespace = edmx.GetNamespaceOfPrefix("edmx");
			XElement dataServicesElement = edmx.Element(edmxNamespace + "DataServices");
			return dataServicesElement.Elements().FirstOrDefault(c => c.Name.LocalName == "Schema" && (string)c.Attribute("Namespace") == strNameSpace);
        }

        /// <summary>
        /// This function returns the metedata definition of an entity type
        /// </summary>
        /// <param name="edmx">root element of edmx</param>
        /// <param name="name">value of typename, usually the name got from entity container</param>
        /// <returns></returns>
        public static XElement getEntityTypeElement(XElement edmx, string name)
        {
            XElement entityContainer = getDefaultEntityContainer(edmx);
            if (entityContainer == null)
                return null;

            XElement entitySet = getElementByAttribute(entityContainer, "EntitySet", "Name", name);
            if (entitySet == null)
                return null;
            string entityType = entitySet.Attribute("EntityType").Value;
            int index = entityType.LastIndexOf('.');
            if (index < 0)
                return null;
            XElement schema = getSchemaByNamespace(edmx, entityType.Substring(0, index));
            if (schema == null)
                return null;
			return schema.Elements().FirstOrDefault(c => c.Name.LocalName == "EntityType" && (string)c.Attribute("Name") == entityType.Substring(index + 1));
        }

        /// <summary>
        /// This function returns the association set 
        /// </summary>
        /// <param name="edmx">root element of edmx</param>
        /// <param name="name">name of association</param>
        /// <returns></returns>
        public static XElement getAssociationElement(XElement edmx, string name)
        {
            int index = name.LastIndexOf('.');
            string ns = name.Substring(0, index);
            XElement schema = getSchemaByNamespace(edmx, ns);
            if (schema == null)
                return null;
            return getElementByAttribute(schema, "Association", "Name", name.Substring(index + 1));
        }

        /// <summary>
        /// This function returns the end role type based on association and role
        /// </summary>
        /// <param name="edmx">root element of edmx</param>
        /// <param name="associationName">name of association</param>
        /// <param name="roleName">name of role</param>
        /// <returns></returns>
        public static string getEndRoleType(XElement edmx, string associationName, string roleName)
        {
            XElement entityContainer = getDefaultEntityContainer(edmx);
            XElement associationSet = getElementByAttribute(entityContainer, "AssociationSet", "Association", associationName);
            XElement role = getElementByAttribute(associationSet, "End", "Role", roleName);
            string entitySet = role.Attribute("EntitySet").Value;
            return entitySet;
        }
	}
}
