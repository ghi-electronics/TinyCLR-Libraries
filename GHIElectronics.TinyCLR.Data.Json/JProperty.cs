using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Data.Json
{
	/// <summary>A name/value pair inside a <see cref="JObject"/>.</summary>
	public class JProperty : JToken
	{
		/// <summary>Initializes a new empty property.</summary>
		public JProperty()
		{
		}

		/// <summary>Initializes a new property with the given name and value.</summary>
		public JProperty(string name, JToken value)
		{
			this.Name = name;
			this.Value = value;
		}

		/// <summary>Gets or sets the property name.</summary>
		public string Name { get; set; }
		/// <summary>Gets or sets the property value.</summary>
		public JToken Value { get; set; }

		/// <summary>Returns the JSON text for this property.</summary>
		public override string ToString()
        {
            return this.ToString(null);
        }

        /// <summary>Returns the JSON text for this property using the given formatting options.</summary>
        public override string ToString(JsonSerializationOptions options)
		{
			EnterSerialization(options);
			try
			{
				StringBuilder sb = new StringBuilder();
				sb.Append('"');
				sb.Append(this.Name);
                if (JsonConverter.SerializationContext.options.Indented)
                {
                    sb.Append("\" : ");
                }
                else
                {
                    sb.Append("\":");
                }
                sb.Append(this.Value.ToString(options));
				return sb.ToString();
			}
			finally
			{
				ExitSerialization();
			}
		}

		/// <summary>Gets the number of bytes the property value occupies when encoded as BSON.</summary>
		public override int GetBsonSize()
		{
            if (this.Value == null)
                return 0;
            else
                return this.Value.GetBsonSize();
		}

		/// <summary>Gets the number of BSON bytes for this property including the given element name.</summary>
		public override int GetBsonSize(string ename)
		{
            return 1 + ename.Length + 1 + this.GetBsonSize();
        }

		/// <summary>Writes the property value to the buffer as BSON, advancing the offset.</summary>
		public override void ToBson(byte[] buffer, ref int offset)
		{
            if (this.Value != null)
                this.Value.ToBson(buffer, ref offset);
		}

        /// <summary>Gets the BSON type code of the property value.</summary>
        public override BsonTypes GetBsonType()
        {
            return this.Value.GetBsonType();
        }
    }
}
