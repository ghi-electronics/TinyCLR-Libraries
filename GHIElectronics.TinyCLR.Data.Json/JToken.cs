using System;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Data.Json
{
	/// <summary>
	/// Abstract base for every JSON value type: <see cref="JObject"/>, <see cref="JArray"/>,
	/// <see cref="JValue"/>, and <see cref="JProperty"/>. Use <see cref="JsonConverter"/>
	/// to parse JSON text into a token tree or serialize a tree back to text.
	/// </summary>
	public abstract class JToken
	{
		private bool _fOwnsContext;

		/// <summary>Begins a serialization scope, establishing the shared formatting context if none exists.</summary>
		protected void EnterSerialization(JsonSerializationOptions options = null)
		{
            if (options == null)
            {
                options = new JsonSerializationOptions();
            }

			lock (JsonConverter.SyncObj)
			{
				if (JsonConverter.SerializationContext == null)
				{
					JsonConverter.SerializationContext = new JsonConverter.SerializationCtx(options);
					JsonConverter.SerializationContext.IndentLevel = 0;
					Monitor.Enter(JsonConverter.SerializationContext);
					_fOwnsContext = true;
				}
			}
		}

		/// <summary>Ends the serialization scope, releasing the shared formatting context if this token owns it.</summary>
		protected void ExitSerialization()
		{
			lock (JsonConverter.SyncObj)
			{
				if (_fOwnsContext)
				{
					var monitorObj = JsonConverter.SerializationContext;
					JsonConverter.SerializationContext = null;
					_fOwnsContext = false;
					Monitor.Exit(monitorObj);
				}
			}
		}

		/// <summary>Returns the indentation string for the current nesting level, optionally increasing the level afterwards.</summary>
		protected string Indent(bool incrementAfter = false)
		{
            if (!JsonConverter.SerializationContext.options.Indented)
            {
                return string.Empty;
            }

			StringBuilder sb = new StringBuilder();
			string indent = "  ";
			if (JsonConverter.SerializationContext != null)
			{
				for (int i = 0; i < JsonConverter.SerializationContext.IndentLevel; ++i)
					sb.Append(indent);
				if (incrementAfter)
					++JsonConverter.SerializationContext.IndentLevel;
			}
			return sb.ToString();
		}

		/// <summary>Decreases the current indentation nesting level by one.</summary>
		protected void Outdent()
		{
			--JsonConverter.SerializationContext.IndentLevel;
		}

		/// <summary>Encodes this token as a standalone BSON document and returns the bytes.</summary>
		public byte[] ToBson()
		{
			var size = this.GetBsonSize("") + 5;
			var buffer = new byte[size];
			int offset = 4;
            this.ToBson("", buffer, ref offset);

            // Write the trailing nul
            buffer[offset++] = (byte)0;

            // Write the completed size
            int zero = 0;
            SerializationUtilities.Marshall(buffer, ref zero, offset);
            return buffer;
		}

        /// <summary>Gets the BSON type code for this token.</summary>
        public abstract BsonTypes GetBsonType();

		/// <summary>Gets the number of bytes this token occupies when encoded as BSON.</summary>
		public abstract int GetBsonSize();

		/// <summary>Gets the number of BSON bytes for this token including the given element name.</summary>
		public abstract int GetBsonSize(string ename);

		/// <summary>Writes this token to the buffer as BSON, advancing the offset.</summary>
		public abstract void ToBson(byte[] buffer, ref int offset);

        /// <summary>Writes this token as a named BSON element (type byte, name, then value), advancing the offset.</summary>
        public void ToBson(string ename, byte[] buffer, ref int offset)
        {
#if DEBUG
            int startingOffset = offset;
#endif

            if (buffer!=null)
                buffer[offset] = (byte)this.GetBsonType();
            ++offset;

            MarshallEName(ename, buffer, ref offset);
            ToBson(buffer, ref offset);

#if DEBUG
            if (this.GetBsonSize(ename) != (offset - startingOffset))
                throw new Exception("marshalling error");
#endif
        }

        /// <summary>Writes a BSON element name as a null-terminated UTF-8 string, advancing the offset.</summary>
        protected void MarshallEName(string ename, byte[] buffer, ref int offset)
        {
            var name = Encoding.UTF8.GetBytes(ename);
            if (buffer != null && ename.Length > 0)
                Array.Copy(name, 0, buffer, offset, name.Length);
            offset += name.Length;
            if (buffer != null)
                buffer[offset] = 0;
            ++offset;
        }

        internal static String ConvertToString(Byte[] byteArray, int start, int count)
        {
            var _chars = new char[byteArray.Length];
            bool _completed;
            int _bytesUsed, _charsUsed;
            Encoding.UTF8.GetDecoder().Convert(byteArray, start, count, _chars, 0, byteArray.Length, false, out _bytesUsed, out _charsUsed, out _completed);
            return new string(_chars, 0, _charsUsed);
        }

        internal static int FindNul(byte[] buffer, int start)
        {
            int current = start;
            while (current < buffer.Length)
            {
                if (buffer[current++] == 0)
                    return current - 1;
            }
            return -1;
        }

        /// <summary>Returns the JSON text for this token using the given formatting options.</summary>
        public abstract string ToString(JsonSerializationOptions options);
    }
}
