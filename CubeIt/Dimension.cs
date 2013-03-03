using System;
using System.Collections;

namespace CubeIt
{
    /// <summary>
    /// Represents a dimension in a cube.
    /// </summary>
    public class Dimension
    {
        private readonly IEqualityComparer comparer;

        /// <summary>
        /// Initializes a new instance of a Dimension, that uses the given comparer
        /// to compare values.
        /// </summary>
        /// <param name="comparer">The equality comparer to use.</param>
        public Dimension(IEqualityComparer comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.comparer = comparer;
        }

        /// <summary>
        /// Gets the comparer used to compare key parts.
        /// </summary>
        public IEqualityComparer Comparer
        {
            get { return comparer; }
        }

        internal new bool Equals(object x, object y)
        {
            return comparer.Equals(x, y);
        }

        internal int GetHashCode(object x)
        {
            return comparer.GetHashCode(x);
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
