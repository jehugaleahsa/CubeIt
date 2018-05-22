using System;

namespace CubeIt
{
    /// <summary>
    /// Holds the key part and value from the old table that is being collapsed.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public sealed class GroupPair<TValue>
    {
        internal GroupPair(object keyPartValue, TValue value)
        {
            KeyPartValue = keyPartValue;
            Value = value;
        }

        /// <summary>
        /// Gets the key part value from the source table.
        /// </summary>
        public object KeyPartValue { get; private set; }

        /// <summary>
        /// Gets the value associated with the key part.
        /// </summary>
        public TValue Value { get; private set; }
    }
}
