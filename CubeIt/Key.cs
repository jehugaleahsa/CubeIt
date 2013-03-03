using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeIt
{
    /// <summary>
    /// Represents an multi-dimensional index into a cube.
    /// </summary>
    public sealed class Key : IEquatable<Key>
    {
        private int? hashCode;
        private readonly Dictionary<Dimension, KeyPart> keyParts;

        /// <summary>
        /// Initializes a new instance of a Key.
        /// </summary>
        /// <param name="parts">The key parts making up the key.</param>
        public Key(params KeyPart[] parts)
            : this((IEnumerable<KeyPart>)parts)
        {
        }

        /// <summary>
        /// Initializes a new instance of a Key.
        /// </summary>
        public Key(IEnumerable<KeyPart> parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException("parts");
            }
            if (parts.Contains(null))
            {
                throw new ArgumentException("One or more of the key parts were null.", "parts");
            }
            this.keyParts = parts.ToDictionary(part => part.Dimension);
        }

        internal Key(IEnumerable<KeyPart> keyParts, bool ignore)
        {
            this.keyParts = keyParts.ToDictionary(part => part.Dimension);
        }

        /// <summary>
        /// Gets the keys part for the given dimension.
        /// </summary>
        /// <param name="dimension">The dimension to get the key part for.</param>
        /// <returns>The key part.</returns>
        public KeyPart GetKeyPart(Dimension dimension)
        {
            return keyParts[dimension];
        }

        /// <summary>
        /// Gets the key parts associated with the key.
        /// </summary>
        /// <returns>The key parts.</returns>
        internal IEnumerable<KeyPart> GetKeyParts()
        {
            return keyParts.Values;
        }

        /// <summary>
        /// Gets the number of key parts in the key.
        /// </summary>
        internal int DimensionCount
        {
            get { return keyParts.Count; }
        }

        /// <summary>
        /// Gets whether the given key is equal.
        /// </summary>
        /// <param name="obj">The other key.</param>
        /// <returns>True if obj is a Key and it represents the same key; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Key);
        }

        /// <summary>
        /// Gets whether the given key is equal.
        /// </summary>
        /// <param name="other">The other key.</param>
        /// <returns>True if obj is a Key and it represents the same key; otherwise, false.</returns>
        public bool Equals(Key other)
        {
            if (other == null)
            {
                return false;
            }
            foreach (KeyValuePair<Dimension, KeyPart> pair in keyParts)
            {
                Dimension dimension = pair.Key;
                KeyPart keyPart = pair.Value;
                if (!other.keyParts.ContainsKey(dimension))
                {
                    return false;
                }
                KeyPart otherKeyPart = other.keyParts[dimension];
                if (!keyPart.Equals(otherKeyPart))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the hash code for the key.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return hashCode ?? getHashCode();
        }

        private int getHashCode()
        {
            int newHashCode = 0;
            foreach (KeyPart keyPart in keyParts.Values)
            {
                newHashCode ^= keyPart.GetHashCode();
            }
            // cache the hash code
            hashCode = newHashCode;
            return newHashCode;
        }
    }
}
