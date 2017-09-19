using System;
using System.Collections.Generic;
using System.Text;

namespace PasLib
{
    internal struct SubString
    {
        private readonly string _master;
        private readonly int _offset;
        private readonly int _length;

        public SubString(string master, int offset)
        {
            if (master == null)
            {
                throw new ArgumentNullException(nameof(master));
            }
            if (offset < 0 || offset > master.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            _master = master;
            _offset = offset;
            _length = _master.Length - _offset;
        }

        public SubString(string master, int offset, int length) : this(master, offset)
        {
            if (length < 0 || length > master.Length - offset)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            _length = length;
        }

        public static implicit operator SubString(string text)
        {
            return new SubString(text, 0);
        }

        public bool HasContent { get { return _master != null && _length != 0; } }

        public int Length { get { return _length; } }

        public char First
        {
            get
            {
                if (!HasContent)
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    return _master[_offset];
                }
            }
        }

        public bool IsNull { get { return _master == null; } }

        public SubString Take(int length)
        {
            if (length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Must be smaller than the sub list length");
            }

            return new SubString(_master, _offset, length);
        }

        public SubString Skip(int offset)
        {
            if (offset > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Must be smaller than the sub list length");
            }

            return new SubString(_master, _offset + offset, _length - offset);
        }

        public IEnumerable<char> Enumerate()
        {
            for (int i = _offset; i != _offset + _length; ++i)
            {
                yield return _master[i];
            }
        }

        #region object methods
        public override string ToString()
        {
            return _master == null
                ? "<null>"
                : _master.Substring(_offset, _length);
        }
        #endregion
    }
}