using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PasLib
{
    public struct SubString : IEnumerable<char>
    {
        private readonly ArraySegment<char> _segment;

        private SubString(ArraySegment<char> segment)
        {
            _segment = segment;
        }

        public static implicit operator SubString(string text)
        {
            return new SubString(new ArraySegment<char>(text.ToCharArray()));
        }

        public bool HasContent { get { return _segment.Any(); } }

        public int Length { get { return _segment.Count; } }

        public char First()
        {
            if (!HasContent)
            {
                throw new IndexOutOfRangeException();
            }
            else
            {
                return _segment[0];
            }
        }

        public SubString Take(int length)
        {
            if (length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Must be smaller than the sub-string length");
            }

            return new SubString(_segment.Slice(0, length));
        }

        public SubString Skip(int offset)
        {
            if (offset > Length || offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Must be smaller than the sub list length");
            }

            return offset == 0
                ? this
                : new SubString(_segment.Slice(offset));
        }

        #region object methods
        public override string ToString()
        {
            return new string(_segment.ToArray());
        }
        #endregion

        #region IEnumerable<char> methods
        public IEnumerator<char> GetEnumerator()
        {
            return ((IEnumerable<char>)_segment).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<char>)_segment).GetEnumerator();
        }
        #endregion
    }
}