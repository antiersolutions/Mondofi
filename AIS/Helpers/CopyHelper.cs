using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace AIS.Helpers
{
    public static class CopyHelper
    {

        #region Private Members

        // We are interested in non-static, public properties with getters and setters
        private const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty;

        #endregion

        /// <summary>
        /// Copies all public properties from one object to another.
        /// </summary>
        /// <param name="fromType">The type of the from object, preferably an interface. We could infer this using reflection, but this allows us to contrain the copy to an interface.</param>
        /// <param name="from">The object to copy from</param>
        /// <param name="toType">The type of the to object, preferably an interface. We could infer this using reflection, but this allows us to contrain the copy to an interface.</param>
        /// <param name="to">The object to copy to</param>
        public static void Copy(Type fromType, object from, Type toType, object to)
        {
            if (fromType == null)
                throw new ArgumentNullException("fromType", "The type that you are copying from cannot be null");

            if (from == null)
                throw new ArgumentNullException("from", "The object you are copying from cannot be null");

            if (toType == null)
                throw new ArgumentNullException("toType", "The type that you are copying to cannot be null");

            if (to == null)
                throw new ArgumentNullException("to", "The object you are copying to cannot be null");

            // Don't copy if they are the same object
            if (!ReferenceEquals(from, to))
            {
                // Get all of the public properties in the toType with getters and setters
                Dictionary<string, PropertyInfo> toProperties = new Dictionary<string, PropertyInfo>();
                PropertyInfo[] properties = toType.GetProperties(flags);
                foreach (PropertyInfo property in properties)
                {
                    toProperties.Add(property.Name, property);
                }

                // Now get all of the public properties in the fromType with getters and setters
                properties = fromType.GetProperties(flags);
                foreach (PropertyInfo fromProperty in properties)
                {
                    // If a property matches in name and type, copy across
                    if (toProperties.ContainsKey(fromProperty.Name))
                    {
                        PropertyInfo toProperty = toProperties[fromProperty.Name];
                        if (toProperty.PropertyType == fromProperty.PropertyType)
                        {
                            object value = fromProperty.GetValue(from, null);
                            toProperty.SetValue(to, value, null);
                        }
                    }
                }
            }
        }
    }
}