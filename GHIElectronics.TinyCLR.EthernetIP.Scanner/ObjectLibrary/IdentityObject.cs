// Copyright (c) 2020 Rossmann Engineering
// Modified by GHI Electronics, LLC

using System;
using System.Text;

namespace GHIElectronics.TinyCLR.EthernetIP.Scanner.ObjectLibrary
{
    /// <summary>
    /// Identity Object - Class Code: 01 Hex
    /// </summary>
    /// <remarks>
    /// This object provides identification of and general information about the device. The Identity Object shall be present in all CIP products.
    /// If autonomous components of a device exist, use multiple instances of the Identity Object.
    /// </remarks>
    public class IdentityObject
    {
        // Private read-only — see AssemblyObject for rationale.
        private readonly ScannerController scanner;

        internal IdentityObject(ScannerController scanner) => this.scanner = scanner;

        /// <summary>
        /// gets the Vendor ID / Read "Identity Object" Class Code 0x01 - Attribute ID 1
        /// </summary>
        public ushort VendorID
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 1);
                var returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Device Type / Read "Identity Object" Class Code 0x01 - Attribute ID 2
        /// </summary>
        public ushort DeviceType
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 2);
                var returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }


        /// <summary>
        /// gets the Product code / Read "Identity Object" Class Code 0x01 - Attribute ID 3
        /// </summary>
        public ushort ProductCode
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 3);
                var returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Revision / Read "Identity Object" Class Code 0x01 - Attribute ID 4
        /// </summary>
        /// <returns>Revision</returns>
        public Revison Revision
        {
            get
            {

                var byteArray = this.scanner.GetAttributeSingle(1, 1, 4);
                var returnValue = new Revison();
                returnValue.MajorRevision = (ushort)(byteArray[0]);
                returnValue.MinorRevision = (ushort)(byteArray[1]);
                return returnValue;
            }
        }

        /// <summary>Holds a device revision as a major and minor number.</summary>
        public struct Revison
        {
            /// <summary>The major revision number.</summary>
            public ushort MajorRevision;
            /// <summary>The minor revision number.</summary>
            public ushort MinorRevision;
        }

        /// <summary>
        /// gets the Status / Read "Identity Object" Class Code 0x01 - Attribute ID 5
        /// </summary>
        public ushort Status
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 5);
                var returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Serial number / Read "Identity Object" Class Code 0x01 - Attribute ID 6
        /// </summary>
        public uint SerialNumber
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 6);
                var returnValue = ((uint)byteArray[3] << 24 | (uint)byteArray[2] << 16 | (uint)byteArray[1] << 8 | (uint)byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Product Name / Read "Identity Object" Class Code 0x01 - Attribute ID 7
        /// </summary>
        public string ProductName
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 7);
                var returnValue = Encoding.UTF8.GetString(byteArray);
                return returnValue;
            }
        }

        /// <summary>The operational state reported by the Identity Object (attribute 8).</summary>
        public enum StateEnum
        {
            /// <summary>The device or instance does not exist.</summary>
            Nonexistent = 0,
            /// <summary>The device is performing self-test.</summary>
            DeviceSelfTesting = 1,
            /// <summary>The device is in standby (not yet configured).</summary>
            Standby = 2,
            /// <summary>The device is operational.</summary>
            Operational = 3,
            /// <summary>The device has a major recoverable fault.</summary>
            MajorRecoverableFault = 4,
            /// <summary>The device has a major unrecoverable fault.</summary>
            MajorUnrecoverableFault = 5,
            /// <summary>Default value returned by the Get_Attributes_All service.</summary>
            DefaultforGet_Attributes_All_service = 255
        }

        /// <summary>
        /// gets the State / Read "Identity Object" Class Code 0x01 - Attribute ID 8
        /// </summary>
        public StateEnum State
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 8);
                var returnValue = (StateEnum) byteArray[0];
                return returnValue;
            }
        }

        /// <summary>
        /// gets the State / Read "Identity Object" Class Code 0x01 - Attribute ID 9
        /// </summary>
        public ushort ConfigurationConsistencyValue
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 9);
                var returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Heartbeat intervall / Read "Identity Object" Class Code 0x01 - Attribute ID 10
        /// </summary>
        public byte HeartbeatInterval
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 10);
                var returnValue = (byte)byteArray[0];
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Supported Language List / Read "Identity Object" Class Code 0x01 - Attribute ID 12
        /// </summary>
        public string[] SupportedLanguageList
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(1, 1, 12);
                var returnValue = new string[byteArray.Length / 3];
                for (var i = 0; i < returnValue.Length; i++)
                {
                    var byteArray2 = new byte[3];
                    Array.Copy(byteArray, i*3, byteArray2, 0, 3);
                    returnValue[i] = Encoding.UTF8.GetString(byteArray2);
                }
                return returnValue;
            }
        }

        /// <summary>
        /// gets all class attributes
        /// </summary>
        public ClassAttributesStruct ClassAttributes
        {
            get
            {
                var byteArray = this.scanner.GetAttributeAll(1, 0);
                ClassAttributesStruct returnValue;
                returnValue.Revision = (ushort)(byteArray[1] << 8 | byteArray[0]);
                returnValue.MaxInstance = (ushort)(byteArray[3] << 8 | byteArray[2]);
                returnValue.MaxIDNumberOfClassAttributes = (ushort)(byteArray[5] << 8 | byteArray[4]);
                returnValue.MaxIDNumberOfInstanceAttributes = (ushort)(byteArray[7] << 8 | byteArray[6]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets all instance attributes
        /// </summary>
        public InstanceAttributesStruct InstanceAttributes
        {
            get
            {
                var byteArray = this.scanner.GetAttributeAll(1, 1);
                InstanceAttributesStruct returnValue;
                returnValue.VendorID = (ushort)(byteArray[1] << 8 | byteArray[0]);
                returnValue.DeviceType = (ushort)(byteArray[3] << 8 | byteArray[2]);
                returnValue.ProductCode = (ushort)(byteArray[5] << 8 | byteArray[4]);
                returnValue.Revision.MajorRevision = byteArray[6];
                returnValue.Revision.MinorRevision = byteArray[7];
                returnValue.Status = (ushort)(byteArray[9] << 8 | byteArray[8]);
                returnValue.SerialNumber = ((uint)byteArray[13] << 24 | (uint)byteArray[12] << 16 | (uint)byteArray[11] << 8 | (uint)byteArray[10]);
                var productName = new byte[byteArray[14]];
                Array.Copy(byteArray, 15, productName, 0, productName.Length);
                returnValue.ProductName = Encoding.UTF8.GetString(productName);
                return returnValue;
            }
        }


        /// <summary>Holds the class-level attributes of the Identity Object.</summary>
        public struct ClassAttributesStruct
        {
            /// <summary>The object class revision.</summary>
            public ushort Revision;
            /// <summary>The highest instance number created.</summary>
            public ushort MaxInstance;
            /// <summary>The highest class-attribute ID implemented.</summary>
            public ushort MaxIDNumberOfClassAttributes;
            /// <summary>The highest instance-attribute ID implemented.</summary>
            public ushort MaxIDNumberOfInstanceAttributes;
        }

        /// <summary>Holds the instance-level attributes of the Identity Object.</summary>
        public struct InstanceAttributesStruct
        {
            /// <summary>The device manufacturer's vendor ID.</summary>
            public ushort VendorID;
            /// <summary>The CIP device type.</summary>
            public ushort DeviceType;
            /// <summary>The product code.</summary>
            public ushort ProductCode;
            /// <summary>The device revision (major, minor).</summary>
            public Revison Revision;
            /// <summary>The current device status word.</summary>
            public ushort Status;
            /// <summary>The device serial number.</summary>
            public uint SerialNumber;
            /// <summary>The human-readable product name.</summary>
            public string ProductName;
        }
    }
}
