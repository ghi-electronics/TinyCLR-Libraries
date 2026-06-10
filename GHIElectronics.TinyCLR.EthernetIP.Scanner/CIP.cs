// Copyright (c) 2020 Rossmann Engineering
// Modified by GHI Electronics LLC 
using System;

namespace GHIElectronics.TinyCLR.EthernetIP.Scanner
{

    /// <summary>
    /// Table A-3.1 Volume 1 Chapter A-3
    /// </summary>
    public enum CIPCommonServices : byte
    {
        /// <summary>Reads all attributes of the object in a single request (service 0x01).</summary>
        Get_Attributes_All = 0x01,
        /// <summary>Writes all attributes of the object in a single request (service 0x02).</summary>
        Set_Attributes_All_Request = 0x02,
        /// <summary>Reads a specified list of attributes (service 0x03).</summary>
        Get_Attribute_List = 0x03,
        /// <summary>Writes a specified list of attributes (service 0x04).</summary>
        Set_Attribute_List = 0x04,
        /// <summary>Resets the object to its power-up state (service 0x05).</summary>
        Reset = 0x05,
        /// <summary>Starts the object (service 0x06).</summary>
        Start = 0x06,
        /// <summary>Stops the object (service 0x07).</summary>
        Stop = 0x07,
        /// <summary>Creates a new instance of the object (service 0x08).</summary>
        Create = 0x08,
        /// <summary>Deletes an instance of the object (service 0x09).</summary>
        Delete = 0x09,
        /// <summary>Sends multiple service requests in one packet (service 0x0A).</summary>
        Multiple_Service_Packet = 0x0A,
        /// <summary>Applies previously set attribute values (service 0x0D).</summary>
        Apply_Attributes = 0x0D,
        /// <summary>Reads a single attribute (service 0x0E).</summary>
        Get_Attribute_Single = 0x0E,
        /// <summary>Writes a single attribute (service 0x10).</summary>
        Set_Attribute_Single = 0x10,
        /// <summary>Finds the next existing object instance (service 0x11).</summary>
        Find_Next_Object_Instance = 0x11,
        /// <summary>Indicates an error response (service 0x14).</summary>
        Error_Response = 0x14,
        /// <summary>Restores saved attribute values (service 0x15).</summary>
        Restore = 0x15,
        /// <summary>Saves current attribute values to non-volatile storage (service 0x16).</summary>
        Save = 0x16,
        /// <summary>No-operation service (service 0x17).</summary>
        NOP = 0x17,
        /// <summary>Reads a member of a structured attribute (service 0x18).</summary>
        Get_Member = 0x18,
        /// <summary>Writes a member of a structured attribute (service 0x19).</summary>
        Set_Member = 0x19,
        /// <summary>Inserts a member into a structured attribute (service 0x1A).</summary>
        Insert_Member = 0x1A,
        /// <summary>Removes a member from a structured attribute (service 0x1B).</summary>
        Remove_Member = 0x1B,
        /// <summary>Synchronizes a group of devices (service 0x1C).</summary>
        GroupSync = 0x1C
    }


    /// <summary>Thrown when the target returns a non-success CIP general status code.</summary>
    public class CIPException : Exception
    {
        /// <summary>Initializes a new CIP exception with no message.</summary>
        public CIPException()
        {
        }

        /// <summary>Initializes a new CIP exception with the given message.</summary>
        public CIPException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new CIP exception with the given message and inner exception.</summary>
        public CIPException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Table B-1.1 CIP General Status Codes
    /// </summary>
    internal static class GeneralStatusCodes
    {
        static internal string GetStatusCode(byte code)
        {
            switch (code)
            {
                case 0x00: return "Success";
                case 0x01: return "Connection failure";
                case 0x02: return "Resource unavailable";
                case 0x03: return "Invalid Parameter value";
                case 0x04: return "Path segment error";
                case 0x05: return "Path destination unknown";
                case 0x06: return "Partial transfer";
                case 0x07: return "Connection lost";
                case 0x08: return "Service not supported";
                case 0x09: return "Invalid attribute value";
                case 0x0A: return "Attribute List error";
                case 0x0B: return "Already in requested mode/state";
                case 0x0C: return "Object state conflict";
                case 0x0D: return "Object already exists";
                case 0x0E: return "Attribute not settable";
                case 0x0F: return "Privilege violation";
                case 0x10: return "Device state conflict";
                case 0x11: return "Reply data too large";
                case 0x12: return "Fragmentation of a primitive value";
                case 0x13: return "Not enough data";
                case 0x14: return "Attribute not supported";
                case 0x15: return "Too much data";
                case 0x16: return "Object does not exist";
                case 0x17: return "Service fragmentation sequence not in progress";
                case 0x18: return "No stored attribute data";
                case 0x19: return "Store operation failure";
                case 0x1A: return "Routing failure, request packet too large";
                case 0x1B: return "Routing failure, response packet too large";
                case 0x1C: return "Missing attribute list entry data";
                case 0x1D: return "Invalid attribute value list";
                case 0x1E: return "Embedded service error";
                case 0x1F: return "Vendor specific error";
                case 0x20: return "Invalid parameter";
                case 0x21: return "Write-once value or medium atready written";
                case 0x22: return "Invalid Reply Received";
                case 0x23: return "Buffer overflow";
                case 0x24: return "Message format error";
                case 0x25: return "Key failure path";
                case 0x26: return "Path size invalid";
                case 0x27: return "Unecpected attribute list";
                case 0x28: return "Invalid Member ID";
                case 0x29: return "Member not settable";
                case 0x2A: return "Group 2 only Server failure";
                case 0x2B: return "Unknown Modbus Error";
                default: return "unknown";
            }
        }
    }
}
