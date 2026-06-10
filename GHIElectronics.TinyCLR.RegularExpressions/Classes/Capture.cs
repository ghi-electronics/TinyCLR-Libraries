using System;

namespace System.Text.RegularExpressions
{

    /// <summary>
    /// Represents the results from a single subexpression capture. 
    /// System.Text.RegularExpressions. 
    /// Capture respresents one substring to a single successful catpture
    /// </summary>
    [Serializable]
    public class Capture
    {
        #region Fields

        internal int _index;
        internal int _length;
        internal string _text;

        #endregion

        #region Methods

        internal Capture(string text, int i, int l)
        {
            this._text = text;
            this._index = i;
            this._length = l;
        }

        internal string GetLeftSubstring()
        {
            return this._text.Substring(0, this._index);
        }

        internal string GetOriginalString()
        {
            return this._text;
        }

        internal string GetRightSubstring()
        {
            return this._text.Substring(this._index + this._length, (this._text.Length - this._index) - this._length);
        }

        /// <summary>Returns the captured substring.</summary>
        public override string ToString()
        {
            return this.Value;
        }

        #endregion

        #region Properties

        /// <summary>The position in the original string where the captured substring begins.</summary>
        public int Index
        {
            get
            {
                return this._index;
            }
        }

        /// <summary>The length of the captured substring.</summary>
        public int Length
        {
            get
            {
                return this._length;
            }
        }

        /// <summary>The captured substring from the input string.</summary>
        public string Value
        {
            get
            {
                return this._text.Substring(this._index, this._length);
            }
        }

        #endregion

    }


}
