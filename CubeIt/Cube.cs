using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeIt
{
    /// <summary>
    /// Represents an N-dimensional lookup cube.
    /// </summary>
    /// <typeparam name="TValue">The type of the values in the cube.</typeparam>
    public sealed class Cube<TValue>
    {
        private static readonly Cube<TValue> nullCube = createNullCube();
        private readonly Dictionary<Dimension, Dictionary<KeyPart, List<Key>>> keyLookup;
        private readonly Dictionary<Key, TValue> valueLookup;

        private static Cube<TValue> createNullCube()
        {
            Dimension[] dimensions = new Dimension[0];
            return new Cube<TValue>(new LookupBuilder<TValue>(dimensions));
        }

        /// <summary>
        /// Initializes a new instance of a Cube.
        /// </summary>
        /// <param name="builder">The builder used to build the lookups.</param>
        private Cube(LookupBuilder<TValue> builder)
        {
            this.valueLookup = builder.ValueLookup;
            this.keyLookup = builder.KeyLookup;
        }

        /// <summary>
        /// Gets a cube with no dimensions or values.
        /// </summary>
        public static Cube<TValue> NullCube
        {
            get { return nullCube; }
        }

        /// <summary>
        /// Creates an empty cube with the given dimensions.
        /// </summary>
        /// <param name="dimensions">The dimensions in the cube.</param>
        /// <returns>A new empty cube with the given dimensions.</returns>
        public static Cube<TValue> Define(params Dimension[] dimensions)
        {
            return Define((IEnumerable<Dimension>)dimensions);
        }

        /// <summary>
        /// Creates an empty cube with the given dimensions.
        /// </summary>
        /// <param name="dimensions">The dimensions in the cube.</param>
        /// <returns>A new empty cube with the given dimensions.</returns>
        public static Cube<TValue> Define(IEnumerable<Dimension> dimensions)
        {
            if (dimensions == null)
            {
                throw new ArgumentNullException("The dimensions list must not be null.");
            }
            if (!dimensions.Any())
            {
                return nullCube;
            }
            if (dimensions.Any(dimension => dimension == null))
            {
                throw new ArgumentException("One or more of the dimensions were null.", "dimensions");
            }
            LookupBuilder<TValue> builder = new LookupBuilder<TValue>(dimensions);
            return new Cube<TValue>(builder);
        }

        /// <summary>
        /// Creates an empty cube with the same dimensions of the current cube.
        /// </summary>
        /// <returns>The new empty cube.</returns>
        public Cube<TValue> Empty()
        {
            LookupBuilder<TValue> builder = new LookupBuilder<TValue>(keyLookup.Keys);
            return new Cube<TValue>(builder);
        }

        /// <summary>
        /// Creates a cube with a single value associated with all of the key parts.
        /// </summary>
        /// <param name="key">The key of the singleton.</param>
        /// <param name="value">The singleton value.</param>
        /// <returns>A new cube with a single key/value pair.</returns>
        public static Cube<TValue> Singleton(Key key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("The key must not be null.");
            }
            if (key.DimensionCount == 0)
            {
                return nullCube;
            }
            IEnumerable<Dimension> dimensions = key.GetKeyParts().Select(part => part.Dimension);
            LookupBuilder<TValue> builder = new LookupBuilder<TValue>(dimensions);
            builder.Add(key, value);
            return new Cube<TValue>(builder);
        }

        /// <summary>
        /// Gets whether the cube is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return valueLookup.Count == 0; }
        }

        /// <summary>
        /// Gets whether a value is associated with the given key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>True if a value is associated with the key; otherwise, false.</returns>
        public bool ContainsKey(Key key)
        {
            return valueLookup.ContainsKey(key);
        }

        /// <summary>
        /// Gets all of the keys.
        /// </summary>
        /// <returns>All of the keys.</returns>
        public IEnumerable<Key> GetKeys()
        {
            return valueLookup.Keys;
        }

        /// <summary>
        /// Gets the unique keys in the given dimension.
        /// </summary>
        /// <param name="dimension">The dimension to get the unique keys for.</param>
        /// <returns>The unique keys in the given dimension.</returns>
        public IEnumerable<KeyPart> GetUniqueKeyParts(Dimension dimension)
        {
            if (!keyLookup.ContainsKey(dimension))
            {
                throw new ArgumentException("The given dimension is not in the cube.", "dimension");
            }
            Dictionary<KeyPart, List<Key>> dimensionLookup = keyLookup[dimension];
            return dimensionLookup.Keys;
        }

        /// <summary>
        /// Gets or sets the value associated with the given key.
        /// </summary>
        /// <param name="key">The key associated with the value to get or set.</param>
        /// <returns>The value associated with the given key.</returns>
        public TValue this[Key key]
        {
            get { return valueLookup[key]; }
            set { valueLookup[key] = value; }
        }

        /// <summary>
        /// Attempts to get the value associated with the given key.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">The value to store the results in.</param>
        /// <returns>True if the value was found; otherwise, false.</returns>
        public bool TryGetValue(Key key, out TValue value)
        {
            return valueLookup.TryGetValue(key, out value);
        }

        /// <summary>
        /// Creates a cube with a new dimension where all the values from the current cube are associated with the given key part value.
        /// </summary>
        /// <param name="keyPart">The dimension/key value pair to append to the cube.</param>
        /// <returns>A new cube with the added dimension and values.</returns>
        public Cube<TValue> AddDimension(KeyPart keyPart)
        {
            if (keyPart == null)
            {
                throw new ArgumentNullException("keyPart");
            }
            if (keyLookup.ContainsKey(keyPart.Dimension))
            {
                throw new ArgumentException("The given dimension already exists.", "dimension");
            }
            List<Dimension> dimensions = keyLookup.Keys.ToList();
            dimensions.Add(keyPart.Dimension);
            LookupBuilder<TValue> builder = new LookupBuilder<TValue>(dimensions);
            foreach (KeyValuePair<Key, TValue> pair in valueLookup)
            {
                List<KeyPart> keyParts = new List<KeyPart>(pair.Key.GetKeyParts());
                keyParts.Add(keyPart);
                Key newKey = new Key(keyParts, true);
                builder.Add(newKey, pair.Value);
            }
            return new Cube<TValue>(builder);
        }

        /// <summary>
        /// Creates a new cube where the key part value in the specified dimension equals the given key part's value.
        /// </summary>
        /// <param name="keyPart">The value the keys in the given dimension must equal.</param>
        /// <returns>A new cube where the keys in the specified dimension equal the given value.</returns>
        public Cube<TValue> Splice(KeyPart keyPart)
        {
            if (keyPart == null)
            {
                throw new ArgumentNullException("keyPart");
            }
            if (!keyLookup.ContainsKey(keyPart.Dimension))
            {
                throw new ArgumentException("Cannot splice a dimension that does not exist in the cube", "dimension");
            }
            Dictionary<KeyPart, List<Key>> dimensionLookup = keyLookup[keyPart.Dimension];
            LookupBuilder<TValue> builder = new LookupBuilder<TValue>(keyLookup.Keys);
            List<Key> keys;
            if (dimensionLookup.TryGetValue(keyPart, out keys))
            {
                foreach (Key key in keys)
                {
                    TValue value = valueLookup[key];
                    builder.Add(key, value);
                }
            }
            return new Cube<TValue>(builder);
        }

        /// <summary>
        /// Creates a copy of the cube, excluding values whose keys have a matching key part value.
        /// </summary>
        /// <param name="keyPart">The dimension and key value of items to be removed.</param>
        /// <returns>A new cube where the values matching the key part have been removed.</returns>
        public Cube<TValue> Exclude(KeyPart keyPart)
        {
            if (keyPart == null)
            {
                throw new ArgumentNullException("keyPart");
            }
            if (!keyLookup.ContainsKey(keyPart.Dimension))
            {
                throw new ArgumentException("Cannot filter by a dimension that is not in the cube.", "dimension");
            }
            LookupBuilder<TValue> builder = new LookupBuilder<TValue>(keyLookup.Keys);
            foreach (KeyValuePair<Key, TValue> pair in valueLookup)
            {
                if (!keyPart.Equals(pair.Key.GetKeyPart(keyPart.Dimension)))
                {
                    builder.Add(pair.Key, pair.Value);
                }
            }
            return new Cube<TValue>(builder);
        }

        /// <summary>
        /// Merges the cube with the other cubes.
        /// </summary>
        /// <param name="others">The cubes to merge with the current cube.</param>
        /// <returns>A new cube containing the keys and values from both cubes.</returns>
        public Cube<TValue> Merge(params Cube<TValue>[] others)
        {
            return Merge((IEnumerable<Cube<TValue>>)others);
        }

        /// <summary>
        /// Merges the cube with other cubes, combining any items at the same key.
        /// </summary>
        /// <param name="others">The other cubes to merge with the current cube.</param>
        /// <returns>A new cube containing the keys and values from both cubes.</returns>
        public Cube<TValue> Merge(IEnumerable<Cube<TValue>> others)
        {
            if (others == null)
            {
                throw new ArgumentNullException("others");
            }
            List<Cube<TValue>> cubes = new List<Cube<TValue>>(others);
            if (cubes.Count == 0)
            {
                return this;
            }
            validateMerge(cubes);
            cubes.Add(this);
            return merge(keyLookup.Keys, cubes);
        }

        private void validateMerge(List<Cube<TValue>> others)
        {
            if (others.Contains(null))
            {
                throw new ArgumentException("One or more of the cubes is null.");
            }
            Func<Cube<TValue>, bool> areDifferentSizes = cube => cube.keyLookup.Count != keyLookup.Count;
            Func<Cube<TValue>, bool> haveDifferentKeys = cube => cube.keyLookup.Any(pair => !keyLookup.ContainsKey(pair.Key));
            if (others.Any(cube => areDifferentSizes(cube) || haveDifferentKeys(cube)))
            {
                throw new ArgumentException("You cannot merge cubes with different dimensions.", "others");
            }
        }

        private static Cube<TValue> merge(IEnumerable<Dimension> dimensions, IEnumerable<Cube<TValue>> cubes)
        {
            LookupBuilder<TValue> lookup = new LookupBuilder<TValue>(dimensions);
            foreach (Cube<TValue> cube in cubes)
            {
                foreach (KeyValuePair<Key, TValue> pair in cube.valueLookup)
                {
                    if (lookup.ContainsKey(pair.Key))
                    {
                        throw new InvalidOperationException("Two or more cubes had the same key.");
                    }
                        lookup.Add(pair.Key, pair.Value);
                }
            }
            return new Cube<TValue>(lookup);
        }

        /// <summary>
        /// Merges the cube with the other cubes, combining any items with the same key.
        /// </summary>
        /// <param name="others">The other cubes to merge with the current cube.</param>
        /// <param name="combiner">The means of combining items that exist at the same key.</param>
        /// <returns>A new cube containing the keys and values from both cubes.</returns>
        public Cube<UValue> ResolvingMerge<UValue>(Func<Collision<TValue>, UValue> combiner, params Cube<TValue>[] others)
        {
            return ResolvingMerge(combiner, (IEnumerable<Cube<TValue>>)others);
        }

        /// <summary>
        /// Merges the cube with the other cubes, combining any items with the same key.
        /// </summary>
        /// <param name="others">The other cubes to merge with the current cube.</param>
        /// <param name="combiner">The means of combining items that exist at the same key.</param>
        /// <returns>A new cube containing the keys and values from both cubes.</returns>
        public Cube<UValue> ResolvingMerge<UValue>(Func<Collision<TValue>, UValue> combiner, IEnumerable<Cube<TValue>> others)
        {
            if (combiner == null)
            {
                throw new ArgumentNullException("combiner");
            }
            if (others == null)
            {
                throw new ArgumentNullException("others");
            }
            List<Cube<TValue>> cubes = new List<Cube<TValue>>(others);
            validateMerge(cubes);
            cubes.Add(this);
            return resolvingMerge(keyLookup.Keys, cubes, combiner);
        }

        private static Cube<UValue> resolvingMerge<UValue>(IEnumerable<Dimension> dimensions, IEnumerable<Cube<TValue>> cubes, Func<Collision<TValue>, UValue> combiner)
        {
            Dictionary<Key, Collision<TValue>> lookup = new Dictionary<Key, Collision<TValue>>();
            foreach (Cube<TValue> cube in cubes)
            {
                foreach (KeyValuePair<Key, TValue> pair in cube.valueLookup)
                {
                    Collision<TValue> collisions;
                    if (lookup.ContainsKey(pair.Key))
                    {
                        collisions = lookup[pair.Key];
                    }
                    else
                    {
                        collisions = new Collision<TValue>(pair.Key);
                        lookup.Add(pair.Key, collisions);
                    }
                    collisions.Add(pair.Value);
                }
            }
            LookupBuilder<UValue> builder = new LookupBuilder<UValue>(dimensions);
            foreach (Collision<TValue> collision in lookup.Values)
            {
                UValue value = combiner(collision);
                builder.Add(collision.Key, value);
            }
            return new Cube<UValue>(builder);
        }

        /// <summary>
        /// Creates a new cube by collapsing the values in the cube.
        /// </summary>
        /// <typeparam name="UValue">The type of the value to collapse the items to.</typeparam>
        /// <param name="dimension">The dimension to collapse.</param>
        /// <param name="aggregator">The operation to perform to collapse the values.</param>
        /// <returns>A new cube with the collapsed values.</returns>
        public Cube<UValue> Collapse<UValue>(Dimension dimension, Func<Group<TValue>, UValue> aggregator)
        {
            if (dimension == null)
            {
                throw new ArgumentNullException("The dimension cannot be null.");
            }
            if (!keyLookup.ContainsKey(dimension))
            {
                throw new ArgumentException("Cannot collapse a dimension that is not in the cube.", "dimension");
            }
            if (aggregator == null)
            {
                throw new ArgumentNullException("aggregator");
            }
            return collapse<UValue>(dimension, aggregator);
        }

        private Cube<UValue> collapse<UValue>(Dimension dimension, Func<Group<TValue>, UValue> aggregator)
        {
            IEnumerable<Dimension> dimensions = keyLookup.Keys.Where(otherDimension => dimension != otherDimension).ToArray();
            Dictionary<Key, Group<TValue>> lookup = new Dictionary<Key, Group<TValue>>();
            foreach (KeyValuePair<Key, TValue> pair in valueLookup)
            {
                KeyPart oldKeyPart = pair.Key.GetKeyPart(dimension);
                IEnumerable<KeyPart> newKeyParts = dimensions.Select(item => pair.Key.GetKeyPart(item));
                Key newKey = new Key(newKeyParts, true);
                Group<TValue> group;
                if (lookup.ContainsKey(newKey))
                {
                    group = lookup[newKey];
                }
                else
                {
                    group = new Group<TValue>(newKey);
                    lookup.Add(newKey, group);
                }
                group.AddPair(oldKeyPart, pair.Value);
            }
            LookupBuilder<UValue> builder = new LookupBuilder<UValue>(dimensions);
            foreach (KeyValuePair<Key, Group<TValue>> pair in lookup)
            {
                UValue value = aggregator(pair.Value);
                builder.Add(pair.Key, value);
            }
            return new Cube<UValue>(builder);
        }

        /// <summary>
        /// Creates a new cube by applying an operation to every value in the cube.
        /// </summary>
        /// <typeparam name="UValue">The type of the values after the application.</typeparam>
        /// <param name="converter">The function for creating a UValue from a TValue.</param>
        /// <returns>A new cube holding the results of the application to every value in the cube.</returns>
        public Cube<UValue> Convert<UValue>(Func<TValue, UValue> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("applicator");
            }
            LookupBuilder<UValue> builder = new LookupBuilder<UValue>(keyLookup.Keys);
            foreach (KeyValuePair<Key, TValue> pair in this.valueLookup)
            {
                UValue value = converter(pair.Value);
                builder.Add(pair.Key, value);
            }
            return new Cube<UValue>(builder);
        }
    }
}
