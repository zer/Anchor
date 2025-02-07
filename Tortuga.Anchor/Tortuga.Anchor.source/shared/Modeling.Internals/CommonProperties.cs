﻿using System.ComponentModel;

namespace Tortuga.Anchor.Modeling.Internals
{
    /// <summary>
    /// These are used for property changed notifications so that new objects don't need to be allocated.
    /// </summary>

    internal static class CommonProperties
    {
        /// <summary>
        /// Used to indicate that many or all properties have been changed.
        /// </summary>
        public readonly static PropertyChangedEventArgs Empty = new PropertyChangedEventArgs("");

        /// <summary>
        /// The "HasErrors" property
        /// </summary>
        public readonly static PropertyChangedEventArgs HasErrorsProperty = new PropertyChangedEventArgs("HasErrors");

        /// <summary>
        /// The "IsChanged" property
        /// </summary>
        public readonly static PropertyChangedEventArgs IsChangedProperty = new PropertyChangedEventArgs("IsChanged");
    }
}