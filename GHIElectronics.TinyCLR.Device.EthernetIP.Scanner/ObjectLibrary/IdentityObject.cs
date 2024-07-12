// Copyright (c) 2020 Rossmann Engineering
// Modified by GHI Electronics LLC

using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Device.EthernetIP.Scanner.ObjectLibrary
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
        public EthernetIPClient eeipClient;

        /// <summary>
        /// Constructor. </summary>
        /// <param name="eeipClient"> EthernetIPClient Object</param>
        public IdentityObject(EthernetIPClient eeipClient) => this.eeipClient = eeipClient;

        /// <summary>
        /// gets the Vendor ID / Read "Identity Object" Class Code 0x01 - Attribute ID 1
        /// </summary>
        public ushort VendorID
        {
            get
            {
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 1);
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
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 2);
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
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 3);
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

                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 4);
                var returnValue = new Revison();
                returnValue.MajorRevision = (ushort)(byteArray[0]);
                returnValue.MinorRevision = (ushort)(byteArray[1]);
                return returnValue;
            }
        }

        public struct Revison
        {
            public ushort MajorRevision;
            public ushort MinorRevision;
        }

        /// <summary>
        /// gets the Status / Read "Identity Object" Class Code 0x01 - Attribute ID 5
        /// </summary>
        public ushort Status
        {
            get
            {
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 5);
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
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 6);
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
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 7);
                var returnValue = Encoding.UTF8.GetString(byteArray);
                return returnValue;
            }
        }

        public enum StateEnum
        {
            Nonexistent = 0,
            DeviceSelfTesting = 1,
            Standby = 2,
            Operational = 3,
            MajorRecoverableFault = 4,
            MajorUnrecoverableFault = 5,
            DefaultforGet_Attributes_All_service = 255
        }

        /// <summary>
        /// gets the State / Read "Identity Object" Class Code 0x01 - Attribute ID 8
        /// </summary>
        public StateEnum State
        {
            get
            {
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 8);
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
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 9);
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
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 10);
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
                var byteArray = this.eeipClient.GetAttributeSingle(1, 1, 12);
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
                var byteArray = this.eeipClient.GetAttributeAll(1, 0);
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
                var byteArray = this.eeipClient.GetAttributeAll(1, 1);
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


        public struct ClassAttributesStruct
        {
            public ushort Revision;
            public ushort MaxInstance;
            public ushort MaxIDNumberOfClassAttributes;
            public ushort MaxIDNumberOfInstanceAttributes;
        }

        public struct InstanceAttributesStruct
        {
            public ushort VendorID;
            public ushort DeviceType;
            public ushort ProductCode;
            public Revison Revision;
            public ushort Status;
            public uint SerialNumber;
            public string ProductName;
        }
    }
}
