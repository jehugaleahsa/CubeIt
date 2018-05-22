using System;
using System.Collections;

namespace CubeIt
{
    /// <summary>
    /// Represents a dimension in a cube.
    /// </summary>
    public class Dimension
    {
        /// <summary>
        /// Initializes a new instance of a Dimension, that uses the given comparer
        /// to compare values.
        /// </summary>
        /// <param name="comparer">The equality comparer to use.</param>
        public Dimension(IEqualityComparer comparer)
        {
            Comparer = comparer ?? throw new ArgumentNullException("comparer");
        }

        /// <summary>
        /// Gets the comparer used to compare key parts.
        /// </summary>
        public IEqualityComparer Comparer { get; private set; }

        internal new bool Equals(object x, object y)
        {
            return Comparer.Equals(x, y);
        }

        internal int GetHashCode(object x)
        {
            return Comparer.GetHashCode(x);
        }

        private sealed class EqualityComparer : IEqualityComparer
        {
            public EqualityComparer()
            {
            }

            public new bool Equals(object x, object y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
