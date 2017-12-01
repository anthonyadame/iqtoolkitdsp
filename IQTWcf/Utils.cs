using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Data.Services;

namespace IQTWcf
{
    public class Utils
    {

        /// <summary>
        /// Gets the map path. Checks for relative paths to a file
        /// app and other.
        /// </summary>
        /// <param name="mapsetting">The mapsetting.</param>
        /// <returns></returns>
        public static string GetMapPath(string mapsetting)
        {
            string mapping = ConfigurationManager.AppSettings[mapsetting];

            if (File.Exists(mapping))
            {
                return mapping;
            }
            else
            {
                string mappedPath = string.Empty;
                if (mapping.StartsWith(@".\"))
                {
                    mappedPath = ResolvePhysicalPath(mapping);
                    if (File.Exists(mappedPath))
                    {
                        return mappedPath;
                    }
                }
                else
                {
                    mappedPath = AppDomain.CurrentDomain.RelativeSearchPath + mapping;
                    if (File.Exists(mappedPath))
                    {
                        return mappedPath;
                    }
                }                
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the log path.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static string GetLogPath(string filename)
        {
            string logpath = ConfigurationManager.AppSettings["logprovider"];
            if (logpath.Length > 0)
            {
                return Path.Combine(Utils.ResolvePhysicalPath(logpath), filename);
            }
            else { return string.Empty; }
            
        }

        /// <summary>
        /// Resolves the physical path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public static string ResolvePhysicalPath(string relativePath)
        {
            if (HttpContext.Current == null)
            {
                throw new DataServiceException(500, "Unable to resolve physical path.");
            }

            string appRoot = HttpContext.Current.Request.PhysicalApplicationPath;
            if (relativePath.StartsWith(@".\"))
            {
                return Path.Combine(appRoot, relativePath.Substring(2));

            }
            else
            {
                return Path.Combine(appRoot, relativePath);
            }
        }

        /// <summary>
        /// Resolves the name of the local.
        /// </summary>
        /// <returns></returns>
        public static string ResolveLocalName()
        {
            if (HttpContext.Current == null)
            {
                throw new DataServiceException(500, "Unable to resolve uri path.");
            }

            return HttpContext.Current.Request.Url.LocalPath.Substring(1);
            
        }

    }
}