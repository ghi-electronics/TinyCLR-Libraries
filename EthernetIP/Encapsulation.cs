using System;
using System.Collections;

//using System.Collections.Generic;
//using System.Linq;
//using System.Net.NetworkInformation;
using System.Text;
//using System.Threading.Tasks;

namespace GHIElectronics.TinyCLR.Drivers.EthernetIP
{
    public class Encapsulation
    {
        public CommandsEnum Command { get; set; }
        public ushort Length { get; set; }
        public uint SessionHandle { get; set; }
        public StatusEnum Status { get; }
        private byte[] senderContext = new byte[8];
        private uint options = 0;
        public ArrayList CommandSpecificData = new ArrayList();

        /// <summary>
        /// Table 2-3.3 Error Codes
        /// </summary>
        public enum StatusEnum : uint
        {
            Success = 0x0000,
            InvalidCommand = 0x0001,
            InsufficientMemory = 0x0002,
            IncorrectData = 0x0003,
            InvalidSessionHandle = 0x0064,
            InvalidLength = 0x0065,
            UnsupportedEncapsulationProtocol = 0x0069
        }





        /// <summary>
        /// Table 2-3.2 Encapsulation Commands
        /// </summary>
        public enum CommandsEnum : ushort
        {
            NOP = 0x0000,
            ListServices = 0x0004,
            ListIdentity = 0x0063,
            ListInterfaces = 0x0064,
            RegisterSession = 0x0065,
            UnRegisterSession = 0x0066,
            SendRRData = 0x006F,
            SendUnitData = 0x0070,
            IndicateStatus = 0x0072,
            Cancel = 0x0073
        }

        public byte[] Tobytes()
        {
            var returnValue = new byte[24 + this.CommandSpecificData.Count];
            returnValue[0] = (byte)this.Command;
            returnValue[1] = (byte)((ushort)this.Command >> 8);
            returnValue[2] = (byte)this.Length;
            returnValue[3] = (byte)((ushort)this.Length >> 8);
            returnValue[4] = (byte)this.SessionHandle;
            returnValue[5] = (byte)((uint)this.SessionHandle >> 8);
            returnValue[6] = (byte)((uint)this.SessionHandle >> 16);
            returnValue[7] = (byte)((uint)this.SessionHandle >> 24);
            returnValue[8] = (byte)this.Status;
            returnValue[9] = (byte)((ushort)this.Status >> 8);
            returnValue[10] = (byte)((ushort)this.Status >> 16);
            returnValue[11] = (byte)((ushort)this.Status >> 24);
            returnValue[12] = this.senderContext[0];
            returnValue[13] = this.senderContext[1];
            returnValue[14] = this.senderContext[2];
            returnValue[15] = this.senderContext[3];
            returnValue[16] = this.senderContext[4];
            returnValue[17] = this.senderContext[5];
            returnValue[18] = this.senderContext[6];
            returnValue[19] = this.senderContext[7];
            returnValue[20] = (byte)this.options;
            returnValue[21] = (byte)((ushort)this.options >> 8);
            returnValue[22] = (byte)((ushort)this.options >> 16);
            returnValue[23] = (byte)((ushort)this.options >> 24);

            var inbytes = this.CommandSpecificData.ToArray();


            for (var i = 0; i < this.CommandSpecificData.Count; i++)
            {
                returnValue[24 + i] = (byte)((int)inbytes[i]);
            }
            return returnValue;
        }


        /// <summary>
        /// Table 2-4.4 CIP Identity Item
        /// </summary>
        public class CIPIdentityItem
        {
            public ushort ItemTypeCode;                                     //Code indicating item type of CIP Identity (0x0C)
            public ushort ItemLength;                                       //Number of bytes in item which follow (length varies depending on Product Name string)
            public ushort EncapsulationProtocolVersion;                     //Encapsulation Protocol Version supported (also returned with Register Sesstion reply).
            public SocketAddress SocketAddress = new SocketAddress();       //Socket Address (see section 2-6.3.2)
            public ushort VendorID1;                                        //Device manufacturers Vendor ID
            public ushort DeviceType1;                                      //Device Type of product
            public ushort ProductCode1;                                     //Product Code assigned with respect to device type
            public byte[] Revision1 = new byte[2];                          //Device revision
            public ushort Status1;                                          //Current status of device
            public uint SerialNumber1;                                      //Serial number of device
            public byte ProductNameLength;                          
            public string ProductName1;                                     //Human readable description of device
            public byte State1;                                             //Current state of device


            public static CIPIdentityItem GetCIPIdentityItem(int startingbyte, byte[] receivedData)
            {
                startingbyte = startingbyte + 2;            //Skipped ItemCount
                var cipIdentityItem = new CIPIdentityItem();
                cipIdentityItem.ItemTypeCode = (ushort)(receivedData[0+startingbyte]
                                                                    | (receivedData[1 + startingbyte] << 8));
                cipIdentityItem.ItemLength = (ushort)(receivedData[2 + startingbyte]
                                                                    | (receivedData[3 + startingbyte] << 8));
                cipIdentityItem.EncapsulationProtocolVersion = (ushort)(receivedData[4 + startingbyte]
                                                                    | (receivedData[5 + startingbyte] << 8));
                cipIdentityItem.SocketAddress.SIN_family = (ushort)(receivedData[7 + startingbyte]
                                                    | (receivedData[6 + startingbyte] << 8));
                cipIdentityItem.SocketAddress.SIN_port = (ushort)(receivedData[9 + startingbyte]
                                                    | (receivedData[8 + startingbyte] << 8));
                cipIdentityItem.SocketAddress.SIN_Address = (uint)(receivedData[13 + startingbyte]
                                                    | (receivedData[12 + startingbyte] << 8)
                                                    | (receivedData[11 + startingbyte] << 16)
                                                    | (receivedData[10 + startingbyte] << 24)
                                                    );
                cipIdentityItem.VendorID1 = (ushort)(receivedData[22 + startingbyte]
                                    | (receivedData[23 + startingbyte] << 8));
                cipIdentityItem.DeviceType1 = (ushort)(receivedData[24 + startingbyte]
                                    | (receivedData[25 + startingbyte] << 8));
                cipIdentityItem.ProductCode1 = (ushort)(receivedData[26 + startingbyte]
                    | (receivedData[27 + startingbyte] << 8));
                cipIdentityItem.Revision1[0] = receivedData[28 + startingbyte];
                cipIdentityItem.Revision1[1] = receivedData[29 + startingbyte];
                cipIdentityItem.Status1 = (ushort)(receivedData[30 + startingbyte]
                    | (receivedData[31 + startingbyte] << 8));
                cipIdentityItem.SerialNumber1 = (uint)(receivedData[32 + startingbyte]
                                                    | (receivedData[33 + startingbyte] << 8)
                                                    | (receivedData[34 + startingbyte] << 16)
                                                    | (receivedData[35 + startingbyte] << 24));
                cipIdentityItem.ProductNameLength = receivedData[36 + startingbyte];
                cipIdentityItem.ProductName1 = Encoding.UTF8.GetString(receivedData, 37 + startingbyte, cipIdentityItem.ProductNameLength);
                cipIdentityItem.State1 = receivedData[receivedData.Length - 1];
                return cipIdentityItem;
            }
            /// <summary>
            /// Converts an IP-Address in UIint32 Format (Received by Device)
            /// </summary>
            public static string GetIPAddress(uint address) => ((byte)(address >> 24)).ToString() + "." + ((byte)(address >> 16)).ToString() + "." + ((byte)(address >> 8)).ToString() + "." + ((byte)(address)).ToString();


        }




        /// <summary>
        /// Socket Address (see section 2-6.3.2)
        /// </summary>
        public class SocketAddress
        {
            public ushort SIN_family;
            public ushort SIN_port;
            public uint SIN_Address;
            public byte[] SIN_Zero = new byte[8];
        }

        public class CommonPacketFormat
        {
            public ushort ItemCount = 2;
            public ushort AddressItem = 0x0000;
            public ushort AddressLength = 0;
            public ushort DataItem = 0xB2; //0xB2 = Unconnected Data Item
            public ushort DataLength = 8;
            public ArrayList Data = new ArrayList();
            public ushort SockaddrInfoItem_O_T = 0x8001; //8000 for O->T and 8001 for T->O - Volume 2 Table 2-6.9
            public ushort SockaddrInfoLength = 16;
            public SocketAddress SocketaddrInfo_O_T = null;


            public byte[] Tobytes()
            {
                if (this.SocketaddrInfo_O_T != null)
                    this.ItemCount =3;
                var returnValue = new byte[10 + this.Data.Count + (this.SocketaddrInfo_O_T == null ? 0 : 20)];
                returnValue[0] = (byte)this.ItemCount;
                returnValue[1] = (byte)((ushort)this.ItemCount >> 8);
                returnValue[2] = (byte)this.AddressItem;
                returnValue[3] = (byte)((ushort)this.AddressItem >> 8);
                returnValue[4] = (byte)this.AddressLength;
                returnValue[5] = (byte)((ushort)this.AddressLength >> 8);
                returnValue[6] = (byte)this.DataItem;
                returnValue[7] = (byte)((ushort)this.DataItem >> 8);
                returnValue[8] = (byte)this.DataLength;
                returnValue[9] = (byte)((ushort)this.DataLength >> 8);

                var inbytes = this.Data.ToArray();

                for (var i = 0; i < this.Data.Count; i++)
                {
                    var d = int.Parse(inbytes[i].ToString());
                    returnValue[10 + i] = (byte)(d);
                }


                // Add Socket Address Info Item
                if (this.SocketaddrInfo_O_T != null)
                {
                    returnValue[10 + this.Data.Count + 0] = (byte)this.SockaddrInfoItem_O_T;
                    returnValue[10 + this.Data.Count + 1] = (byte)((ushort)this.SockaddrInfoItem_O_T >> 8);
                    returnValue[10 + this.Data.Count + 2] = (byte)this.SockaddrInfoLength;
                    returnValue[10 + this.Data.Count + 3] = (byte)((ushort)this.SockaddrInfoLength >> 8);
                    returnValue[10 + this.Data.Count + 5] = (byte)this.SocketaddrInfo_O_T.SIN_family;
                    returnValue[10 + this.Data.Count + 4] = (byte)((ushort)this.SocketaddrInfo_O_T.SIN_family >> 8);
                    returnValue[10 + this.Data.Count + 7] = (byte)this.SocketaddrInfo_O_T.SIN_port;
                    returnValue[10 + this.Data.Count + 6] = (byte)((ushort)this.SocketaddrInfo_O_T.SIN_port >> 8);
                    returnValue[10 + this.Data.Count + 11] = (byte)this.SocketaddrInfo_O_T.SIN_Address;
                    returnValue[10 + this.Data.Count + 10] = (byte)((uint)this.SocketaddrInfo_O_T.SIN_Address >> 8);
                    returnValue[10 + this.Data.Count + 9] = (byte)((uint)this.SocketaddrInfo_O_T.SIN_Address >> 16);
                    returnValue[10 + this.Data.Count + 8] = (byte)((uint)this.SocketaddrInfo_O_T.SIN_Address >> 24);
                    returnValue[10 + this.Data.Count + 12] = this.SocketaddrInfo_O_T.SIN_Zero[0];
                    returnValue[10 + this.Data.Count + 13] = this.SocketaddrInfo_O_T.SIN_Zero[1];
                    returnValue[10 + this.Data.Count + 14] = this.SocketaddrInfo_O_T.SIN_Zero[2];
                    returnValue[10 + this.Data.Count + 15] = this.SocketaddrInfo_O_T.SIN_Zero[3];
                    returnValue[10 + this.Data.Count + 16] = this.SocketaddrInfo_O_T.SIN_Zero[4];
                    returnValue[10 + this.Data.Count + 17] = this.SocketaddrInfo_O_T.SIN_Zero[5];
                    returnValue[10 + this.Data.Count + 18] = this.SocketaddrInfo_O_T.SIN_Zero[6];
                    returnValue[10 + this.Data.Count + 19] = this.SocketaddrInfo_O_T.SIN_Zero[7];
                }
                    return returnValue;
            }
        }
    }
}
