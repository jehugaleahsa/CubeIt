using System;

namespace CubeIt
{
    /// <summary>
    /// Associates a dimension with a value.
    /// </summary>
    public sealed class KeyPart : IEquatable<KeyPart>
    {
        private readonly Dimension dimension;
        private readonly object value;

        /// <summary>
        /// Initializes a new instance of a KeyPart.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <param name="value">The dimension value.</param>
        public KeyPart(Dimension dimension, object value)
        {
            if (dimension == null)
            {
                throw new ArgumentNullException("dimension");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.dimension = dimension;
            this.value = value;
        }

        /// <summary>
        /// Gets the dimension.
        /// </summary>
        public Dimension Dimension
        {
            get
            {
                return dimension;
            }
        }

        /// <summary>
        /// Gets the dimension value.
        /// </summary>
        public object Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Gets whether two key parts are the same.
        /// </summary>
        /// <param name="obj">The other key part.</param>
        /// <returns>True if obj is a key part and it represent the same key part; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as KeyPart);
        }

        /// <summary>
        /// Gets whether two key parts are the same.
        /// </summary>
        /// <param name="other">The other key part.</param>
        /// <returns>True if obj is a key part and it represent the same key part; otherwise, false.</returns>
        public bool Equals(KeyPart other)
        {
            return other != null && dimension == other.dimension && dimension.Equals(value, other.value);
        }

        /// <summary>
        /// Gets the hash code for the key part.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return dimension.GetHashCode() ^ dimension.GetHashCode(value);
        }
    }
}
