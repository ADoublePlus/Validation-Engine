using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

using ValidationEngine.DataStructures;

namespace ValidationEngine.Engine
{
    public class Rule
    {
        public string Name { get; set; }
        public string FieldPath { get; set; }
        public string FieldName { get; set; }
        public int? ArrayIndex { get; set; }
        public bool? NotEmpty { get; set; }

        /// <summary>
        /// Validate using a regular expression that will be ran against the FieldPath.FieldName.
        /// </summary>
        public string ValidationExpression { get; set; }

        /// <summary>
        /// Validate comparing FieldPath.FieldName with one or more properties from one or more objects.
        /// </summary>
        public List<FieldComparison> FieldComparisonList { get; set; }

        /// <summary>
        /// Validate using a CustomValidator delegate.
        /// </summary>
        public Func<object[], bool> CustomValidator { get; set; }

        /// <summary>
        /// Received a delegate from the RuleSet (linking back to the caller) to be invoked when a rule validation fails.
        /// </summary>
        internal Action<ComparableProperty> OnInvalid { get; set; }

        internal bool Validate(params object[] entities)
        {
            bool succeed = false;

            // Using custom validation
            if (this.CustomValidator != null)
                return this.CustomValidator(entities);

            // If not using, continue
            List<ComparableProperty> sourceProperties = GetProperty(entities, string.Format("{0}.{1}", this.FieldPath, this.FieldName), this.ArrayIndex);

            if (sourceProperties != null && sourceProperties.Count > 0)
            {
                // Regex validation
                if (!string.IsNullOrEmpty(ValidationExpression))
                {
                    // Go to each prop to validate regex
                    foreach (var prop in sourceProperties)
                    {
                        string propValue = prop.PropertyValue;

                        // Regex validation
                        if (!string.IsNullOrEmpty(ValidationExpression))
                        {
                            Regex regex = new Regex(ValidationExpression);
                            succeed = regex.IsMatch(propValue);

                            // If any are false, validation fails
                            if (!succeed)
                            {
                                if (OnInvalid != null)
                                {
                                    OnInvalid(prop);
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Compare validation
                    foreach (var field in FieldComparisonList)
                    {
                        // Reset the succeed flag, so on each iteration we get the current succeed value
                        // If something goes wrong and we don't get past the if, validation must fail, since a property wasn't found
                        succeed = false;

                        List<ComparableProperty> propTo = GetProperty(entities, string.Format("{0}.{1}", field.FieldPath, field.FieldName), field.ArrayIndex);

                        if (propTo != null && propTo.Count > 0)
                        {
                            if (sourceProperties.Count == propTo.Count)
                            {
                                var propToOrdered = propTo.OrderBy(p => p.PropertyValue);
                                var propOrdered = sourceProperties.OrderBy(p => p.PropertyValue);

                                for (int i = 0; i < propOrdered.Count(); i++)
                                {
                                    var currentProp = propOrdered.ElementAt(i);
                                    var currentPropTo = propToOrdered.ElementAt(i);

                                    succeed = currentProp.PropertyValue == currentPropTo.PropertyValue;

                                    if (!succeed)
                                    {
                                        if (OnInvalid != null)
                                        {
                                            OnInvalid(currentProp);
                                            OnInvalid(currentPropTo);
                                        }

                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Didn't succeed
                                if (OnInvalid != null)
                                {
                                    foreach (var comparableProperty in sourceProperties)
                                    {
                                        OnInvalid(comparableProperty);
                                    }

                                    foreach (var comparableProperty in propTo)
                                    {
                                        OnInvalid(comparableProperty);
                                    }
                                }
                            }
                        }

                        // If something fails, abort remainder of the validation
                        if (!succeed)
                            break;
                    }
                }
            }

            return succeed;
        }

        private static List<ComparableProperty> GetProperty(object[] entities, string propertyNameToFind, int? arrayIndex)
        {
            foreach (var entity in entities)
            {
                var prop = FindProperty(entity, string.Empty, propertyNameToFind, arrayIndex: arrayIndex);

                if (prop != null && prop.Count > 0)
                    return prop;
            }

            return null;
        }

        private static List<ComparableProperty> FindProperty(object entity, string parentEntity, string propertyNameToFind, List<object> visitedObjects = null, int? arrayIndex = null)
        {
            var returnVal = new List<ComparableProperty>();
            visitedObjects = visitedObjects ?? new List<object>();

            string parentEntityLocal = string.IsNullOrEmpty(parentEntity) ? entity.GetType().Name : parentEntity;

            foreach (var property in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    // Property full path
                    var propFullPath = parentEntityLocal + "." + property.Name;

                    var propValue = property.GetValue(entity, null);

                    string recursiveProp = string.Format(".{0}", property.Name);

                    if (propertyNameToFind.Equals(propFullPath))
                    {
                        // Check if property is basic type array and has an index defined
                        if (property.PropertyType.IsArray && arrayIndex.HasValue)
                        {
                            // If it is, get the index value
                            returnVal.Add(new ComparableProperty(entity, property, arrayIndex));
                        }
                        else if (property.PropertyType.IsArray)
                        {
                            // If not, but still an array, we need to use all the indexes, so we create a ComparableProperty for each array position
                            Array arrProp = (Array)propValue;

                            for (int i = 0; i < arrProp.Length; i++)
                            {
                                returnVal.Add(new ComparableProperty(entity, property, i));
                            }
                        }
                        else
                        {
                            // Just this value
                            returnVal.Add(new ComparableProperty(entity, property, arrayIndex));
                        }

                        return returnVal;
                    }
                    else if (property.PropertyType.IsArray)
                    {
                        bool isNextLevel = arrayIndex.HasValue && propertyNameToFind.StartsWith(propFullPath) && propertyNameToFind.Split('.').Length - 1 == propFullPath.Split('.').Length;

                        if (isNextLevel)
                        {
                            returnVal.AddRange(FindProperty(((Array)propValue).GetValue(arrayIndex.Value), parentEntityLocal + recursiveProp, propertyNameToFind, visitedObjects, arrayIndex));
                        }
                        else
                        {
                            foreach (var propArrayItem in (Array)propValue)
                            {
                                returnVal.AddRange(FindProperty(propArrayItem, parentEntityLocal + recursiveProp, propertyNameToFind, visitedObjects, arrayIndex));
                            }
                        }
                    }
                    else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        returnVal.AddRange(FindProperty(propValue, parentEntityLocal + recursiveProp, propertyNameToFind, visitedObjects, arrayIndex));
                    }
                }

                catch { continue; }
            }

            return returnVal;
        }
    }
}