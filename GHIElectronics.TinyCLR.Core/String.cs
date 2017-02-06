////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Runtime.CompilerServices;
    /**
     * <p>The <code>String</code> class represents a static string of characters.  Many of
     * the <code>String</code> methods perform some type of transformation on the current
     * instance and return the result as a new <code>String</code>. All comparison methods are
     * implemented as a part of <code>String</code>.</p>  As with arrays, character positions
     * (indices) are zero-based.
     *
     * <p>When passing a null string into a constructor in VJ and VC, the null should be
     * explicitly type cast to a <code>String</code>.</p>
     * <p>For Example:<br>
     * <pre>String s = new String((String)null);
     * Text.Out.WriteLine(s);</pre></p>
     *
     * @author Jay Roxe (jroxe)
     * @version
     */
    [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class String : IComparable
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public static readonly string Empty = "";
        public override bool Equals(object obj)
        {
            if (obj is string s) {
                return String.Equals(this, s);
            }

            return false;
        }

        public static string Format(string format, params object[] args) {
            var result = "";
            var last = 0;
            var next = 0;
            var end = 0;

            while ((next = format.IndexOf("{", last)) >= 0 && (end = format.IndexOf("}", next)) > 0) {
                result += format.Substring(last, next - last);

                var current = format.Substring(next + 1, end - next - 1);
                var parts = current.Split(':');

                int.TryParse(parts[0], out var index);

                if (parts.Length == 1) {
                    result += args[index].ToString();
                }
                else {
                    var member = args[index].GetType().GetMethod("ToString", new Type[] { typeof(string) });

                    result += member.Invoke(args[index], new object[] { parts[1].ToString() });
                }

                format = format.Substring(end + 1);
            }

            return result;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool Equals(string a, string b);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool operator ==(string a, string b);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool operator !=(string a, string b);

        [System.Runtime.CompilerServices.IndexerName("Chars")]
        public extern char this[int index]
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern char[] ToCharArray();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern char[] ToCharArray(int startIndex, int length);

        public extern int Length
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string[] Split(params char[] separator);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string[] Split(char[] separator, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string Substring(int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string Substring(int startIndex, int length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string Trim(params char[] trimChars);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string TrimStart(params char[] trimChars);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string TrimEnd(params char[] trimChars);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String(char[] value, int startIndex, int length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String(char[] value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern String(char c, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static int Compare(string strA, string strB);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int CompareTo(object value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int CompareTo(string strB);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(char value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(char value, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(char value, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOfAny(char[] anyOf);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOfAny(char[] anyOf, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOfAny(char[] anyOf, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(string value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(string value, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int IndexOf(string value, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(char value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(char value, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(char value, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOfAny(char[] anyOf);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOfAny(char[] anyOf, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOfAny(char[] anyOf, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(string value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(string value, int startIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int LastIndexOf(string value, int startIndex, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string ToLower();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string ToUpper();

        public override string ToString() => this;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern string Trim();
        ////// This method contains the same functionality as StringBuilder Replace. The only difference is that
        ////// a new String has to be allocated since Strings are immutable
        public static string Concat(object arg0)
        {
            if (arg0 == null)
            {
                return String.Empty;
            }

            return arg0.ToString();
        }

        public static string Concat(object arg0, object arg1)
        {
            if (arg0 == null)
            {
                arg0 = String.Empty;
            }

            if (arg1 == null)
            {
                arg1 = String.Empty;
            }

            return Concat(arg0.ToString(), arg1.ToString());
        }

        public static string Concat(object arg0, object arg1, object arg2)
        {
            if (arg0 == null)
            {
                arg0 = String.Empty;
            }

            if (arg1 == null)
            {
                arg1 = String.Empty;
            }

            if (arg2 == null)
            {
                arg2 = String.Empty;
            }

            return Concat(arg0.ToString(), arg1.ToString(), arg2.ToString());
        }

        public static string Concat(params object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            var length = args.Length;
            var sArgs = new string[length];

            for (var i = 0; i < length; i++)
            {
                sArgs[i] = ((args[i] == null) ? (String.Empty) : (args[i].ToString()));
            }

            return String.Concat(sArgs);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static string Concat(string str0, string str1);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static string Concat(string str0, string str1, string str2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static string Concat(string str0, string str1, string str2, string str3);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static string Concat(params string[] values);

        public static string Intern(string str) =>
            // We don't support "interning" of strings. So simply return the string.
            str;

        public static string IsInterned(string str) =>
            // We don't support "interning" of strings. So simply return the string.
            str;

    }
}


