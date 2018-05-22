using System;

namespace CubeIt
{
    /// <summary>
    /// Associates a dimension with a value.
    /// </summary>
    public sealed class KeyPart : IEquatable<KeyPart>
    {
        /// <summary>
        /// Initializes a new instance of a KeyPart.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <param name="value">The dimension value.</param>
        public KeyPart(Dimension dimension, object value)
        {
            Dimension = dimension ?? throw new ArgumentNullException("dimension");
            Value = value ?? throw new ArgumentNullException("value");
        }

        /// <summary>
        /// Gets the dimension.
        /// </summary>
        public Dimension Dimension { get; private set; }

        /// <summary>
        /// Gets the dimension value.
        /// </summary>
        public object Value { get; private set; }

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
            return other != null && Dimension == other.Dimension && Dimension.Equals(Value, other.Value);
        }

        /// <summary>
        /// Gets the hash code for the key part.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Dimension.GetHashCode() ^ Dimension.GetHashCode(Value);
        }
    }
}
