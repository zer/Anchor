using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Tortuga.Anchor.Modeling;

namespace Tortuga.Anchor.Metadata
{
    /// <summary>
    /// This is a cache of metadata about a specific property.
    /// </summary>
    public partial class PropertyMetadata
    {
        readonly List<PropertyMetadata> m_CalculatedFieldsBuilder = new List<PropertyMetadata>();
        readonly MethodInfo m_GetMethod;
        readonly MethodInfo m_SetMethod;

        internal PropertyMetadata(PropertyInfo info)
        {
            PropertyInfo = info;

            Validators = ImmutableArray.CreateRange(info.GetCustomAttributes(typeof(ValidationAttribute), true).OfType<ValidationAttribute>());

            IsIndexed = info.GetIndexParameters().Length > 0;

            m_GetMethod = PropertyInfo.GetMethod;
            m_SetMethod = PropertyInfo.SetMethod;

            PropertyType = info.PropertyType;

            var name = info.ToString();
            Name = name.Substring(name.IndexOf(" ", StringComparison.Ordinal) + 1);

            if (IsIndexed)
                PropertyChangedEventArgs = new PropertyChangedEventArgs(info.Name + "[]");
            else
                PropertyChangedEventArgs = new PropertyChangedEventArgs(info.Name);

            if (IsIndexed)
                PropertyChangingEventArgs = new PropertyChangingEventArgs(info.Name + "[]");
            else
                PropertyChangingEventArgs = new PropertyChangingEventArgs(info.Name);

            IsKey = info.GetCustomAttributes(typeof(KeyAttribute), true).Any();

            var doNotMap = info.GetCustomAttributes(typeof(NotMappedAttribute), true).Any();
            if (!doNotMap)
            {
                var column = (ColumnAttribute)info.GetCustomAttributes(typeof(ColumnAttribute), true).SingleOrDefault();
                MappedColumnName = column != null ? column.Name : Name;
            }
            var decomposeAttribute = (DecomposeAttribute)(info.GetCustomAttributes(typeof(DecomposeAttribute), true).FirstOrDefault());
            if (decomposeAttribute != null)
            {
                Decompose = true;
                DecompositionPrefix = decomposeAttribute.Prefix;
            }
            IgnoreOnInsert = info.GetCustomAttributes(typeof(IgnoreOnInsertAttribute), true).Any();
            IgnoreOnUpdate = info.GetCustomAttributes(typeof(IgnoreOnUpdateAttribute), true).Any();
        }

        /// <summary>
        /// Returns true of this property needs to trigger updates to calculated fields
        /// </summary>
        public bool AffectsCalculatedFields
        {
            get { return m_CalculatedFieldsBuilder.Count > 0; }
        }

        /// <summary>
        /// This returns a list of calculated fields that need to be updated when this property is changed.
        /// </summary>
        public ImmutableArray<PropertyMetadata> CalculatedFields { get; private set; }

        /// <summary>
        /// Returns true if there is a public getter
        /// </summary>
        public bool CanRead
        {
            get { return m_GetMethod != null && m_GetMethod.IsPublic && !IsIndexed; }
        }

        /// <summary>
        /// Returns true is there is a public setter
        /// </summary>
        public bool CanWrite
        {
            get { return m_SetMethod != null && m_SetMethod.IsPublic && !IsIndexed; }
        }

        /// <summary>
        /// Gets a value indicating whether to map this object's columns to the child object's properties.
        /// </summary>
        public bool Decompose { get; }

        /// <summary>
        /// Gets the decomposition prefix.
        /// </summary>
        /// <value>The decomposition prefix.</value>
        public string DecompositionPrefix { get; }

        /// <summary>
        /// Gets a value indicating whether to ignore this property during insert operations.
        /// </summary>
        public bool IgnoreOnInsert { get; }

        /// <summary>
        /// Gets a value indicating whether to ignore this property during update operations.
        /// </summary>
        public bool IgnoreOnUpdate { get; }

        /// <summary>
        /// Returns true if this represents an indexed property
        /// </summary>
        public bool IsIndexed { get; }

        /// <summary>
        /// Property implements the Key attribute.
        /// </summary>
        public bool IsKey { get; }

        /// <summary>
        /// Column that this attribute is mapped to. Defaults to the property's name, but may be overridden by ColumnAttribute.
        /// </summary>
        public string MappedColumnName { get; }

        /// <summary>
        /// Public name of the property
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a cached instance of PropertyChangedEventArgs
        /// </summary>
        /// <remarks>For indexed properties such as "Item [Int32]" the property name will be reduced to "Item[]" to match ObservableCollection.</remarks>
        public PropertyChangedEventArgs PropertyChangedEventArgs { get; }

        /// <summary>
        /// Gets a cached instance of PropertyChangingEventArgs
        /// </summary>
        /// <remarks>For indexed properties such as "Item [Int32]" the property name will be reduced to "Item[]" to match ObservableCollection.</remarks>
        public PropertyChangingEventArgs PropertyChangingEventArgs { get; }

        /// <summary>
        /// Gets the type of this property.
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// List of validators that apply to the property
        /// </summary>
        public ImmutableArray<ValidationAttribute> Validators { get; }

        /// <summary>
        /// Cached PropertyInfo for the property.
        /// </summary>
        internal PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Invokes this property's getter on the supplied object
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CanRead")]
        public object InvokeGet(object target)
        {
            if (CanRead)
                try
                {
                    return m_GetMethod.Invoke(target, null);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException("Error getting property " + Name, ex);
                }
            else
                throw new InvalidOperationException($"CanRead is false on property {Name}.");
        }

        /// <summary>
        /// Invokes this property's setter on the supplied object
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CanWrite")]
        public object InvokeSet(object target, object value)
        {
            if (CanWrite)
                try
                {
                    return m_SetMethod.Invoke(target, new object[] { value });
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException("Error setting property " + Name, ex);
                }
            else
                throw new InvalidOperationException($"CanWrite is false for property {Name}");
        }

        /// <summary>
        /// Adds a property to the list of calculated values watching this property.
        /// </summary>
        /// <param name="affectedProperty"></param>
        internal void AddCalculatedField(PropertyMetadata affectedProperty)
        {
            m_CalculatedFieldsBuilder.Add(affectedProperty);
        }

        internal void EndInit()
        {
            CalculatedFields = ImmutableArray.CreateRange(m_CalculatedFieldsBuilder);
        }
    }
}