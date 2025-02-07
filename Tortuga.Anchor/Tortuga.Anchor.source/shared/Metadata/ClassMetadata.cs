using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Tortuga.Anchor.Modeling;

namespace Tortuga.Anchor.Metadata
{
    /// <summary>
    /// Cached Metadata about the indicated type.
    /// </summary>

    public partial class ClassMetadata
    {
        ImmutableArray<string> m_ColumnMap;
        string m_CSharpFullName;
        string m_FSharpFullName;
        string m_VisualBasicFullName;

        internal ClassMetadata(TypeInfo typeInfo)
        {
            TypeInfo = typeInfo;

            var table = (TableAttribute)typeInfo.GetCustomAttributes(typeof(TableAttribute), true).SingleOrDefault();
            if (table != null)
            {
                MappedTableName = table.Name;
                MappedSchemaName = table.Schema;
            }

            var shadowingProperties = (from p in typeInfo.GetProperties() where IsHidingMember(p) select p).ToList();
            var propertyList = typeInfo.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            bool IsHidden(PropertyInfo propertyInfo) => !shadowingProperties.Contains(propertyInfo) && shadowingProperties.Any(p => string.CompareOrdinal(p.Name, propertyInfo.Name) == 0);

            Properties = new PropertyMetadataCollection(propertyList.Where(p => !IsHidden(p)).Select(p => new PropertyMetadata(p)));

            //List the properties that are affected when the indicated property is modified.
            foreach (var property in Properties)
                foreach (CalculatedFieldAttribute fieldList in property.PropertyInfo.GetCustomAttributes(typeof(CalculatedFieldAttribute), true))
                    foreach (var field in fieldList.SourceProperties)
                    {
                        if (!Properties.Contains(field))
                            throw new InvalidOperationException($"Cannot find property {field} on type {typeInfo.FullName}. This is needed for the calculated property {property.Name}");

                        Properties[field].AddCalculatedField(property);
                    }

            foreach (var property in Properties)
                property.EndInit();

            Constructors = new ConstructorMetadataCollection(typeInfo.DeclaredConstructors);
        }

        /// <summary>
        /// Gets the database columns for this class.
        /// </summary>
        /// <remarks>This honors the Column, NotMapped, and Decompose attributes.</remarks>
        public ImmutableArray<string> ColumnsFor
        {
            get
            {
                if (m_ColumnMap == null)
                    m_ColumnMap = ImmutableArray.CreateRange(MetadataCache.GetColumnsFor(this, null));
                return m_ColumnMap;
            }
        }

        /// <summary>
        /// Properties on the indicated class
        /// </summary>
        public ConstructorMetadataCollection Constructors { get; }

        /// <summary>
        /// Gets the fully quantified name in C# format.
        /// </summary>
        public string CSharpFullName
        {
            get
            {
                if (m_CSharpFullName == null)
                {
                    var result = new StringBuilder(TypeInfo.ToString().Length);
                    BuildFullName(TypeInfo.AsType(), null, result, "<", ", ", ">");

                    m_CSharpFullName = result.ToString();
                }
                return m_CSharpFullName;
            }
        }

        /// <summary>
        /// Gets the fully quantified name in F# format.
        /// </summary>
        public string FSharpFullName
        {
            get
            {
                if (m_FSharpFullName == null)
                {
                    var result = new StringBuilder(TypeInfo.ToString().Length);
                    BuildFullName(TypeInfo.AsType(), null, result, "<'", ", '", ">");

                    m_FSharpFullName = result.ToString();
                }
                return m_FSharpFullName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is nullable.
        /// </summary>
        /// <value>
        ///   True is the type is a reference type, interface, or a nullable value type.
        /// </value>
        public bool IsNullable
        {
            get
            {
                return (!TypeInfo.IsValueType || (TypeInfo.IsGenericType && (TypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))));
            }
        }

        /// <summary>
        /// Schema referred to by TableAttribute.
        /// </summary>
        public string MappedSchemaName { get; }

        /// <summary>
        /// Table referred to by TableAttribute.
        /// </summary>
        public string MappedTableName { get; }

        /// <summary>
        /// Properties on the indicated class
        /// </summary>
        public PropertyMetadataCollection Properties { get; internal set; }

        /// <summary>
        /// Gets the underlying type.
        /// </summary>
        public TypeInfo TypeInfo { get; private set; }

        /// <summary>
        /// Gets the fully quantified name in VB format.
        /// </summary>
        public string VisualBasicFullName
        {
            get
            {
                if (m_VisualBasicFullName == null)
                {
                    var result = new StringBuilder(TypeInfo.ToString().Length);
                    BuildFullName(TypeInfo.AsType(), null, result, "(Of ", ", ", ")");

                    m_VisualBasicFullName = result.ToString();
                }
                return m_VisualBasicFullName;
            }
        }

        /// <summary>
        /// Returns the non-nullable representation of the underlying type.
        /// </summary>
        /// <returns>If a nullable value type, returns the non-nullable type. Otherwise echoes the type.</returns>
        public ClassMetadata MakeNonNullable()
        {
            if (TypeInfo.IsGenericType && (TypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>)))
                return MetadataCache.GetMetadata(TypeInfo.GetGenericArguments()[0]);

            return this;
        }

        /// <summary>
        /// Returns the nullable representation of the underlying type.
        /// </summary>
        /// <returns>If a non-nullable value type, returns the nullable type. Otherwise echoes the type.</returns>
        public ClassMetadata MakeNullable()
        {
            if (TypeInfo.IsValueType && !IsNullable)
                return MetadataCache.GetMetadata(typeof(Nullable<>).MakeGenericType(new[] { TypeInfo }));

            return this;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return TypeInfo.ToString();
        }

        static void BuildFullName(Type typeInfo, List<Type> typeArgs, StringBuilder result, string genericOpen, string genericSeparator, string genericClose)
        {
            var localTypeParamCount = typeInfo.GetTypeInfo().GenericTypeParameters.Length;
            var localTypeArgCount = typeInfo.GetTypeInfo().GenericTypeArguments.Length;

            if (typeArgs == null)
                typeArgs = new List<Type>(typeInfo.GetTypeInfo().GenericTypeArguments);

            if (typeInfo.IsNested)
                BuildFullName(typeInfo.DeclaringType, typeArgs, result, genericOpen, genericSeparator, genericClose);
            else
                result.Append(typeInfo.Namespace);

            result.Append(".");
            foreach (var c in typeInfo.Name)
            {
                if (c == '`') //we found a generic
                    break;
                result.Append(c);
            }

            if (localTypeParamCount > 0)
            {
                result.Append(genericOpen);

                for (int i = 0; i < localTypeParamCount; i++)
                {
                    if (i > 0)
                        result.Append(genericSeparator);
                    BuildFullName(typeArgs[i], null, result, genericOpen, genericSeparator, genericClose); //note that we are "eating" the typeArgs that we passed to us from the nested type.
                }
                typeArgs.RemoveRange(0, localTypeParamCount); //remove the used args

                result.Append(genericClose);
            }
            else if (localTypeArgCount > 0 && typeArgs.Count > 0)
            {
                result.Append(genericOpen);

                for (int i = 0; i < Math.Min(localTypeArgCount, typeArgs.Count); i++)
                {
                    if (i > 0)
                        result.Append(genericSeparator);
                    BuildFullName(typeArgs[i], null, result, genericOpen, genericSeparator, genericClose);
                }
                result.Append(genericClose);
            }
        }

        static bool IsHidingMember(PropertyInfo propertyInfo)
        {
            var baseType = propertyInfo.DeclaringType.GetTypeInfo().BaseType;
            if (baseType == null)
                return false;

            var baseProperty = baseType.GetRuntimeProperty(propertyInfo.Name);

            if (baseProperty == null)
                return false;

            if (baseProperty.DeclaringType == propertyInfo.DeclaringType)
                return false;

            if (baseProperty.GetMethod == null || propertyInfo.GetMethod == null)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot support classes with write-only properties. Class: {0} PropertyName {1}", propertyInfo.DeclaringType.Name, propertyInfo.Name));

            var basePropertyGetGetMethod = baseProperty.GetMethod;
            var propertyInfoGetGetMethod = propertyInfo.GetMethod;

            var baseMethodDefinition = basePropertyGetGetMethod.GetRuntimeBaseDefinition();
            var thisMethodDefinition = propertyInfoGetGetMethod.GetRuntimeBaseDefinition();

            return baseMethodDefinition.DeclaringType != thisMethodDefinition.DeclaringType;
        }
    }
}
