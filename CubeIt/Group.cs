using System;
using System.Collections.Generic;

namespace CubeIt
{
    /// <summary>
    /// Represents a group of values that would appear in the same cell in a collapsed table.
    /// </summary>
    /// <typeparam name="TValue">The type of the values in the table.</typeparam>
    public sealed class Group<TValue>
    {
        private readonly List<GroupPair<TValue>> pairs;

        internal Group(Key key)
        {
            Key = key;
            this.pairs = new List<GroupPair<TValue>>();
        }

        /// <summary>
        /// Gets the key into the new table where the collision is occurring.
        /// </summary>
        public Key Key
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the key part/value pairs being collapsed into the new key.
        /// </summary>
        public IEnumerable<GroupPair<TValue>> Pairs
        {
            get { return pairs; }
        }

        internal void AddPair(object keyPartValue, TValue value)
        {
            pairs.Add(new GroupPair<TValue>(keyPartValue, value));
        }
    }

    /// <summary>
    /// Holds the key part and value from the old table that is being collapsed.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public sealed class GroupPair<TValue>
    {
        private readonly object keyPartValue;
        private readonly TValue value;

        internal GroupPair(object keyPartValue, TValue value)
        {
            this.keyPartValue = keyPartValue;
            this.value = value;
        }

        /// <summary>
        /// Gets the key part value from the source table.
        /// </summary>
        public object KeyPartValue 
        {
            get { return keyPartValue; }
        }

        /// <summary>
        /// Gets the value associated with the key part.
        /// </summary>
        public TValue Value 
        {
            get { return value; }
        }
    }
}
