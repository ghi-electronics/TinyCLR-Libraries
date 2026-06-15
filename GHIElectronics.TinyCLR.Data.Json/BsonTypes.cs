using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Data.Json
{
    /// <summary>BSON element type codes used when encoding tokens to BSON.</summary>
    public enum BsonTypes : byte
    {
        /// <summary>64-bit floating point value.</summary>
        BsonDouble = 0x01,
        /// <summary>UTF-8 string value.</summary>
        BsonString = 0x02,
        /// <summary>Embedded document (object).</summary>
        BsonDocument = 0x03,
        /// <summary>Array value.</summary>
        BsonArray = 0x04,
        /// <summary>Boolean value.</summary>
        BsonBoolean = 0x08,
        /// <summary>UTC date-time value.</summary>
        BsonDateTime = 0x09,
        /// <summary>Null value.</summary>
        BsonNull = 0x0a,
        /// <summary>32-bit signed integer value.</summary>
        BsonInt32 = 0x10,
        /// <summary>64-bit signed integer value.</summary>
        BsonInt64 = 0x12,
    }
}
