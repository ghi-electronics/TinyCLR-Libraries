using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Data.Json
{
    public enum BsonTypes : byte
    {
        BsonDouble = 0x01,
        BsonString = 0x02,
        BsonDocument = 0x03,
        BsonArray = 0x04,
        BsonBoolean = 0x08,
        BsonDateTime = 0x09,
        BsonNull = 0x0a,
        BsonInt32 = 0x10,
        BsonInt64 = 0x12,
    }
}
