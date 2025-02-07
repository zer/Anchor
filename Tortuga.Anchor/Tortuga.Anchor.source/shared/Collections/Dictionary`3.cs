﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Tortuga.Anchor.Collections
{
    /// <summary>
    /// A dictionary that uses a compound key.
    /// </summary>
    /// <typeparam name="TKey1">The type of the first key.</typeparam>
    /// <typeparam name="TKey2">The type of the second key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public partial class Dictionary<TKey1, TKey2, TValue> : IDictionary<ValueTuple<TKey1, TKey2>, TValue>, IReadOnlyDictionary<ValueTuple<TKey1, TKey2>, TValue>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2>
    {
        readonly Dictionary<ValueTuple<TKey1, TKey2>, TValue> m_Base;
        readonly IDictionary<ValueTuple<TKey1, TKey2>, TValue> m_Interface;

        /// <summary>
        /// Initializes a new instance of the Dictionary class.
        /// </summary>
        public Dictionary()
        {
            m_Base = new Dictionary<ValueTuple<TKey1, TKey2>, TValue>();
            m_Interface = m_Base;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dictionary{TKey1, TKey2, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">The dictionary to copy.</param>
        /// <exception cref="ArgumentNullException"></exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary(IDictionary<ValueTuple<TKey1, TKey2>, TValue> dictionary)
            : this()
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary), $"{nameof(dictionary)} is null or empty.");

            foreach (var item in dictionary)
                m_Interface.Add(item);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dictionary{TKey1, TKey2, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">The dictionary to copy.</param>
        /// <exception cref="ArgumentNullException"></exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary(Dictionary<ValueTuple<TKey1, TKey2>, TValue> dictionary)
            : this()
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary), $"{nameof(dictionary)} is null or empty.");

            foreach (var item in dictionary)
                m_Interface.Add(item);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dictionary{TKey1, TKey2, TValue}" /> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <exception cref="ArgumentNullException"></exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary(IEqualityComparer<ValueTuple<TKey1, TKey2>> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer), $"{nameof(comparer)} is null.");

            m_Base = new Dictionary<ValueTuple<TKey1, TKey2>, TValue>(comparer);
            m_Interface = m_Base;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dictionary{TKey1, TKey2, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Dictionary(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, $"{nameof(capacity)} cannot be less than zero");

            m_Base = new Dictionary<ValueTuple<TKey1, TKey2>, TValue>(capacity);
            m_Interface = m_Base;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dictionary{TKey1, TKey2, TValue}" /> class.
        /// </summary>
        /// <param name="dictionary">The dictionary to copy.</param>
        /// <param name="comparer">The comparer.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary(IDictionary<ValueTuple<TKey1, TKey2>, TValue> dictionary, IEqualityComparer<ValueTuple<TKey1, TKey2>> comparer)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary), $"{nameof(dictionary)} is null.");
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer), $"{nameof(comparer)} is null.");

            m_Base = new Dictionary<ValueTuple<TKey1, TKey2>, TValue>(dictionary.Count, comparer);
            m_Interface = m_Base;

            foreach (var item in dictionary)
                m_Interface.Add(item);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dictionary{TKey1, TKey2, TValue}" /> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="comparer">The comparer.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary(int capacity, IEqualityComparer<ValueTuple<TKey1, TKey2>> comparer)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, $"{nameof(capacity)} cannot be less than zero");
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer), $"{nameof(comparer)} is null.");

            m_Base = new Dictionary<ValueTuple<TKey1, TKey2>, TValue>(capacity, comparer);
            m_Interface = m_Base;
        }

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEqualityComparer<ValueTuple<TKey1, TKey2>> Comparer
        {
            get { return m_Base.Comparer; }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </returns>
        public int Count
        {
            get { return m_Base.Count; }
        }

        bool ICollection<KeyValuePair<ValueTuple<TKey1, TKey2>, TValue>>.IsReadOnly
        {
#pragma warning disable CA1033 // Interface methods should be callable by child types
            get { return false; }
#pragma warning restore CA1033 // Interface methods should be callable by child types
        }

        IEnumerable<ValueTuple<TKey1, TKey2>> IReadOnlyDictionary<ValueTuple<TKey1, TKey2>, TValue>.Keys
        {
            get { return m_Base.Keys; }
        }

        /// <summary>
        /// Gets an <see cref="System.Collections.Generic.ICollection{T}" /> containing the keys of the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.Generic.ICollection{T}" /> containing the keys of the object that implements <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </returns>
        public ICollection<ValueTuple<TKey1, TKey2>> Keys
        {
            get { return m_Base.Keys; }
        }

        IEnumerable<TValue> IReadOnlyDictionary<ValueTuple<TKey1, TKey2>, TValue>.Values
        {
            get { return m_Base.Values; }
        }

        /// <summary>
        /// Gets an <see cref="System.Collections.Generic.ICollection{T}" /> containing the values in the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.Generic.ICollection{T}" /> containing the values in the object that implements <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </returns>
        public ICollection<TValue> Values
        {
            get { return m_Base.Values; }
        }

#pragma warning disable CA1043 // Use Integral Or String Argument For Indexers

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TValue this[ValueTuple<TKey1, TKey2> key]
#pragma warning restore CA1043 // Use Integral Or String Argument For Indexers
        {
            get { return m_Base[key]; }
            set { m_Base[key] = value; }
        }

        /// <summary>
        /// Gets or sets the value with the specified keys.
        /// </summary>
        /// <value>
        ///
        /// </value>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        public TValue this[TKey1 key1, TKey2 key2]
        {
            get { return m_Base[ValueTuple.Create(key1, key2)]; }
            set { m_Base[ValueTuple.Create(key1, key2)] = value; }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <param name="key">
        /// The object to use as the key of the element to add.
        /// </param>
        /// <param name="value">
        /// The object to use as the value of the element to add.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="key" /> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// An element with the same key already exists in the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </exception>
        public void Add(ValueTuple<TKey1, TKey2> key, TValue value)
        {
            m_Base.Add(key, value);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.</exception>
        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            m_Base.Add(ValueTuple.Create(key1, key2), value);
        }

        /// <summary>
        /// Adds an item to the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </param>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="System.Collections.Generic.ICollection{T}" /> is read-only.
        /// </exception>
        public void Add(KeyValuePair<ValueTuple<TKey1, TKey2>, TValue> item)
        {
            m_Interface.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="System.Collections.Generic.ICollection{T}" /> is read-only.
        /// </exception>
        public void Clear()
        {
            m_Base.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="System.Collections.Generic.ICollection{T}" /> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="System.Collections.Generic.ICollection{T}" />; otherwise, false.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </param>
        public bool Contains(KeyValuePair<ValueTuple<TKey1, TKey2>, TValue> item)
        {
            return m_Base.Contains(item);
        }

        /// <summary>
        /// Determines whether the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.</param>
        /// <returns>
        /// true if the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(ValueTuple<TKey1, TKey2> key)
        {
            return m_Base.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <returns>
        /// true if the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            return m_Base.ContainsKey(ValueTuple.Create(key1, key2));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<ValueTuple<TKey1, TKey2>, TValue>[] array, int arrayIndex)
        {
            m_Interface.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Collections.Generic.IEnumerator{T}" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<ValueTuple<TKey1, TKey2>, TValue>> GetEnumerator()
        {
            return m_Base.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Base.GetEnumerator();
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </returns>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="key" /> is null.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" /> is read-only.
        /// </exception>
        public bool Remove(ValueTuple<TKey1, TKey2> key)
        {
            return m_Base.Remove(key);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key1" />+<paramref name="key2" /> was not found in the original <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" />
        /// </returns>
        public bool Remove(TKey1 key1, TKey2 key2)
        {
            return m_Base.Remove(ValueTuple.Create(key1, key2));
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="System.Collections.Generic.ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </returns>
        /// <param name="item">
        /// The object to remove from the <see cref="System.Collections.Generic.ICollection{T}" />.
        /// </param>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="System.Collections.Generic.ICollection{T}" /> is read-only.
        /// </exception>
        public bool Remove(KeyValuePair<ValueTuple<TKey1, TKey2>, TValue> item)
        {
            return m_Interface.Remove(item);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(ValueTuple<TKey1, TKey2> key, out TValue value)
        {
            return m_Base.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns> true if the object that implements <see cref="System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
        {
            return m_Base.TryGetValue(ValueTuple.Create(key1, key2), out value);
        }
    }
}
