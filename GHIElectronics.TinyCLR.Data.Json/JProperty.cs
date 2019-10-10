using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Data.Json
{
	public class JProperty : JToken
	{
		public JProperty()
		{
		}

		public JProperty(string name, JToken value)
		{
			this.Name = name;
			this.Value = value;
		}

		public string Name { get; set; }
		public JToken Value { get; set; }

		public override string ToString()
		{
			EnterSerialization();
			try
			{
				StringBuilder sb = new StringBuilder();
				sb.Append('"');
				sb.Append(this.Name);
				sb.Append("\" : ");
				sb.Append(this.Value.ToString());
				return sb.ToString();
			}
			finally
			{
				ExitSerialization();
			}
		}

		public override int GetBsonSize()
		{
            if (this.Value == null)
                return 0;
            else
                return this.Value.GetBsonSize();
		}

		public override int GetBsonSize(string ename)
		{
            return 1 + ename.Length + 1 + this.GetBsonSize();
        }

		public override void ToBson(byte[] buffer, ref int offset)
		{
            if (this.Value != null)
                this.Value.ToBson(buffer, ref offset);
		}

        public override BsonTypes GetBsonType()
        {
            return this.Value.GetBsonType();
        }
    }
}
