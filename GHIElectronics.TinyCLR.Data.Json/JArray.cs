using System;
using System.Collections;
using System.Text;

namespace GHIElectronics.TinyCLR.Data.Json
{
	public class JArray : JToken
	{
		private readonly JToken[] _contents;

		public JArray()
		{
		}

		public JArray(JToken[] values)
		{
			_contents = values;
		}

		private JArray(Array source)
		{
			_contents = new JToken[source.Length];
			for (int i = 0; i < source.Length; ++i)
			{
				var value = source.GetValue(i);
				var fieldType = value.GetType();

				if (value == null)
				{
					_contents[i] = JValue.Serialize(fieldType, null);
				}
				else if (fieldType.IsValueType || fieldType == typeof(string))
				{
					_contents[i] = JValue.Serialize(fieldType, value);
				}
				else
				{
					if (fieldType.IsArray)
					{
						_contents[i] = JArray.Serialize(fieldType, value);
					}
					else
					{
						_contents[i] = JObject.Serialize(fieldType, value); ;
					}
				}

			}
		}

		public int Length
		{
			get { return _contents.Length; }
		}

		public JToken[] Items
		{
			get { return _contents; }
		}

		public static JArray Serialize(Type type, object oSource)
		{
			return new JArray((Array)oSource);
		}

		public JToken this[int i]
		{
			get { return _contents[i]; }
		}

		public override string ToString()
		{
			EnterSerialization();
			try
			{
				StringBuilder sb = new StringBuilder();

				sb.Append('[');
				Indent(true);
				int prefaceLength = 0;

				bool first = true;
				foreach (var item in _contents)
				{
					if (!first)
					{
						if (sb.Length - prefaceLength > 72)
						{
							sb.AppendLine(",");
							prefaceLength = sb.Length;
						}
						else
						{
							sb.Append(',');
						}
					}
					first = false;
					sb.Append(item);
				}
				sb.Append(']');
				Outdent();
				return sb.ToString();
			}
			finally
			{
				ExitSerialization();
			}
		}

		public override int GetBsonSize()
		{
            int offset = 0;
            this.ToBson(null, ref offset);
            return offset;
        }

        public override int GetBsonSize(string ename)
		{
            return 1 + ename.Length + 1 + this.GetBsonSize();
        }

        public override void ToBson(byte[] buffer, ref int offset)
		{
            int startingOffset = offset;

            // leave space for the size
            offset += 4;

            for (int i = 0; i < _contents.Length; ++i)
            {
                _contents[i].ToBson(i.ToString(), buffer, ref offset);
            }

            // Write the trailing nul
            if (buffer!=null)
                buffer[offset] = (byte)0;
            ++offset;

            // Write the completed size
            if (buffer!=null)
                SerializationUtilities.Marshall(buffer, ref startingOffset, offset - startingOffset);
        }

        public override BsonTypes GetBsonType()
        {
            return BsonTypes.BsonArray;
        }

        internal static JArray FromBson(byte[] buffer, ref int offset, InstanceFactory factory = null)
        {
            BsonTypes elementType = (BsonTypes)0;

            int startingOffset = offset;
            int len = (Int32)SerializationUtilities.Unmarshall(buffer, ref offset, TypeCode.Int32);

            var list = new ArrayList();
            int idx = 0;
            while (offset < startingOffset + len - 1)
            {
                // get the element type
                var bsonType = (BsonTypes)buffer[offset++];
                if (elementType == (BsonTypes)0)
                    elementType = bsonType;
                if (bsonType != elementType)
                    throw new Exception("all array elements must be of the same type");

                // get the element name
                var idxNul = JToken.FindNul(buffer, offset);
                if (idxNul == -1)
                    throw new Exception("Missing ename terminator");
                var ename = JToken.ConvertToString(buffer, offset, idxNul - offset);
                var elemIdx = int.Parse(ename);
                if (elemIdx != idx)
                    throw new Exception("sparse arrays are not supported");
                ++idx;

                offset = idxNul + 1;

                JToken item = null;
                switch (bsonType)
                {
                    case BsonTypes.BsonArray:
                        item = JArray.FromBson(buffer, ref offset, factory);
                        break;
                    case BsonTypes.BsonDocument:
                        item = JObject.FromBson(buffer, ref offset, factory);
                        break;
                    case BsonTypes.BsonNull:
                        item = new JValue();
                        break;
                    case BsonTypes.BsonBoolean:
                    case BsonTypes.BsonDateTime:
                    case BsonTypes.BsonDouble:
                    case BsonTypes.BsonInt32:
                    case BsonTypes.BsonInt64:
                    case BsonTypes.BsonString:
                        item = JValue.FromBson(bsonType, buffer, ref offset);
                        break;
                }
                list.Add(item);
            }
            if (buffer[offset++] != 0)
                throw new Exception("bad format - missing trailing null on bson document");
            return new JArray((JToken[])list.ToArray(typeof(JToken)));
        }
    }
}
