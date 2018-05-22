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
        public Key Key { get; private set; }

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
}
