using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace GHIElectronics.TinyCLR.Data.Json
{
	public class JObject : JToken
	{
		private readonly Hashtable _members = new Hashtable();

		public JProperty this[string name]
		{
			get { return (JProperty)_members[name.ToLower()]; }
			set
			{
				if (name.ToLower() != value.Name.ToLower())
					throw new ArgumentException("index value must match property name");
                _members[value.Name.ToLower()] = value;
            }
        }

        public bool Contains(string name) => this._members.Contains(name.ToLower());

        public ICollection Members
		{
			get { return _members.Values; }
		}

		public void Add(string name, JToken value)
		{
			_members[name.ToLower()] = new JProperty(name, value);
		}

		public static JObject Serialize(Type type, object oSource, JsonSerializerSettings settings = null)
		{
            if (settings == null)
            {
                settings = new JsonSerializerSettings();
            }

            var result = new JObject();
			var methods = type.GetMethods();
			foreach (var m in methods)
			{
				if (!m.IsPublic)
					continue;

				if (m.Name.IndexOf("get_") == 0)
				{
					var name = m.Name.Substring(4);
					var methodResult = m.Invoke(oSource, null);
					if (methodResult == null)
						result._members.Add(name.ToLower(), new JProperty(name, JValue.Serialize(m.ReturnType, null)));
					if (m.ReturnType.IsArray)
						result._members.Add(name.ToLower(), new JProperty(name, JArray.Serialize(m.ReturnType, methodResult, settings)));
					else
                    {
                        if (m.ReturnType.IsValueType || m.ReturnType == typeof(string))
                        {
                            result._members.Add(name.ToLower(), new JProperty(name, JValue.Serialize(m.ReturnType, methodResult)));
                        }
                        else
                        {
                            var methodResultType = methodResult.GetType();
                            var child = JObject.Serialize(methodResultType, methodResult, settings);
                            if (settings.TypeNameHandling == TypeNameHandling.Objects ||
                               (settings.TypeNameHandling == TypeNameHandling.Auto && methodResultType != m.ReturnType))
                            {
                                child.Add("$type", new JValue(methodResultType.FullName + ", " + methodResultType.Assembly.GetName().Name));
                            }

                            result._members.Add(name.ToLower(), new JProperty(name, child));
                        }
                    }
                }
			}

			var fields = type.GetFields();
			foreach (var f in fields)
			{
				if (f.FieldType.IsNotPublic)
					continue;

				switch (f.MemberType)
				{
					case MemberTypes.Field:
					case MemberTypes.Property:
						var value = f.GetValue(oSource);
						if (value == null)
						{
							result._members.Add(f.Name, new JProperty(f.Name, JValue.Serialize(f.FieldType, null)));
						}
						else if (f.FieldType.IsValueType || f.FieldType == typeof(string))
						{
							result._members.Add(f.Name.ToLower(),
								new JProperty(f.Name, JValue.Serialize(f.FieldType, value)));
						}
						else
						{
							if (f.FieldType.IsArray)
							{
								result._members.Add(f.Name.ToLower(),
									new JProperty(f.Name, JArray.Serialize(f.FieldType, value, settings)));
							}
							else
							{
								result._members.Add(f.Name.ToLower(),
									new JProperty(f.Name, JObject.Serialize(f.FieldType, value, settings)));
							}
						}
						break;
					default:
						break;
				}
			}

			return result;
		}

		public override string ToString()
        {
            return this.ToString(null);
        }

        public override string ToString(JsonSerializationOptions options)
		{
			EnterSerialization(options);
			try
			{
				StringBuilder sb = new StringBuilder();

                sb.Append(Indent(true) + "{");
                if (JsonConverter.SerializationContext.options.Indented)
                    sb.AppendLine();
                bool first = true;
				foreach (var member in _members.Values)
				{
					if (!first)
                    {
                        sb.Append(",");
                        if (JsonConverter.SerializationContext.options.Indented)
                            sb.AppendLine();
                    }
					first = false;
					sb.Append(Indent() + ((JProperty)member).ToString(options));
				}
                if (JsonConverter.SerializationContext.options.Indented)
                    sb.AppendLine();
                Outdent();
				sb.Append(Indent() + "}");
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

            foreach (DictionaryEntry member in _members)
            {
                ((JProperty)member.Value).ToBson(((JProperty)member.Value).Name, buffer, ref offset);
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
            return BsonTypes.BsonDocument;
        }

        internal static JObject FromBson(byte[] buffer, ref int offset, InstanceFactory factory = null)
        {
            JObject result = new JObject();

            int startingOffset = offset;
            int len = (Int32)SerializationUtilities.Unmarshall(buffer, ref offset, TypeCode.Int32);

            while (offset < startingOffset + len - 1)
            {
                // get the element type
                var bsonType = (BsonTypes)buffer[offset++];
                // get the element name
                var idxNul = JToken.FindNul(buffer, offset);
                if (idxNul == -1)
                    throw new Exception("Missing ename terminator");
                var ename = JToken.ConvertToString(buffer, offset, idxNul - offset);
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
                result.Add(ename, item);
            }

            if (buffer[offset++] != 0)
                throw new Exception("bad format - missing trailing null on bson document");

            return result;
        }

    }
}
