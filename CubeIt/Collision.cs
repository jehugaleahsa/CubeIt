using System.Collections.Generic;

namespace CubeIt
{
    /// <summary>
    /// Represents a group of values that would appear in the same cell in a merged table.
    /// </summary>
    /// <typeparam name="TValue">The type of the values in the table.</typeparam>
    public sealed class Collision<TValue>
    {
        private List<TValue> values;

        internal Collision(Key key)
        {
            Key = key;
            values = new List<TValue>();
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
        /// Gets the values that are associated with the same key in the new table.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get { return values; }
        }

        internal void Add(TValue value)
        {
            values.Add(value);
        }
    }
}
