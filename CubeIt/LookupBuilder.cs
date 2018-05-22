using System.Collections.Generic;

namespace CubeIt
{
    /// <summary>
    /// Helps with the creation of new cubes.
    /// </summary>
    /// <typeparam name="UValue">The type the cube holds.</typeparam>
    internal sealed class LookupBuilder<UValue>
    {
        /// <summary>
        /// Initializes a new instance of a LookupBuilder.
        /// </summary>
        /// <param name="dimensions">The dimensions in the resulting cube.</param>
        public LookupBuilder(IEnumerable<Dimension> dimensions)
        {
            KeyLookup = new Dictionary<Dimension, Dictionary<KeyPart, List<Key>>>();
            foreach (Dimension dimension in dimensions)
            {
                KeyLookup.Add(dimension, new Dictionary<KeyPart, List<Key>>());
            }
            ValueLookup = new Dictionary<Key, UValue>();
        }

        /// <summary>
        /// Associates the given value with the given key.
        /// </summary>
        /// <param name="key">The multi-index key to associate the value with.</param>
        /// <param name="value">The value to add to the cube.</param>
        public void Add(Key key, UValue value)
        {
            ValueLookup.Add(key, value);
            addKeyLookup(key);
        }

        private void addKeyLookup(Key key)
        {
            foreach (KeyPart keyPart in key.GetKeyParts())
            {
                Dictionary<KeyPart, List<Key>> dimensionLookup = KeyLookup[keyPart.Dimension];
                List<Key> keys;
                if (!dimensionLookup.TryGetValue(keyPart, out keys))
                {
                    keys = new List<Key>();
                    dimensionLookup.Add(keyPart, keys);
                }
                keys.Add(key);
            }
        }

        /// <summary>
        /// Gets a key to value lookup.
        /// </summary>
        public Dictionary<Key, UValue> ValueLookup { get; private set; }

        /// <summary>
        /// Gets a look, for each dimension, from key parts to their keys.
        /// </summary>
        public Dictionary<Dimension, Dictionary<KeyPart, List<Key>>> KeyLookup { get; private set; }

        /// <summary>
        /// Determines whether a value has been associated with a key.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>True if the key has already been associated with a value; otherwise, false.</returns>
        public bool ContainsKey(Key key)
        {
            return ValueLookup.ContainsKey(key);
        }
    }
}
