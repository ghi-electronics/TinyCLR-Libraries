// Copyright (c) 2020 Rossmann Engineering
// Modified by GHI Electronics, LLC

using System;
using System.Text;

namespace GHIElectronics.TinyCLR.EthernetIP.Scanner.ObjectLibrary
{
    /// <summary>Provides explicit-message access to the Message Router Object (Class Code 0x02) on the target.</summary>
    public class MessageRouterObject
    {
        // Private read-only — see AssemblyObject for rationale.
        private readonly ScannerController scanner;

        internal MessageRouterObject(ScannerController scanner) => this.scanner = scanner;

        /// <summary>Holds the list of object classes supported by the target's Message Router.</summary>
        public struct ObjectListStruct
        {
            /// <summary>The number of class codes in the list.</summary>
            public ushort Number;
            /// <summary>The supported object class codes.</summary>
            public ushort[] Classes;
        }

        /// <summary>
        /// gets the Object List / Read "Message Router Object" Class Code 0x02 - Attribute ID 1
        /// </summary>
        public ObjectListStruct ObjectList
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(2, 1, 1);
                ObjectListStruct returnValue;
                returnValue.Number = (ushort)(byteArray[1] << 8 | byteArray[0]);
                returnValue.Classes = new ushort[returnValue.Number];
                for (var i = 0; i < returnValue.Classes.Length; i++)
                {
                    returnValue.Classes[i] = (ushort)(byteArray[i*2+3] << 8 | byteArray[i*2+2]);
                }
                return returnValue;
            }
        }

        /// <summary>
        /// gets the Maximum of connections supported / Read "Message Router Object" Class Code 0x02 - Attribute ID 2
        /// </summary>
        public ushort NumberAvailable
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(2, 1, 2);
                ushort returnValue;
                returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the number of active connections / Read "Message Router Object" Class Code 0x02 - Attribute ID 3
        /// </summary>
        public ushort NumberActive
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(2, 1, 3);
                ushort returnValue;
                returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
                return returnValue;
            }
        }

        /// <summary>
        /// gets the active connections / Read "Message Router Object" Class Code 0x02 - Attribute ID 4
        /// </summary>
        public ushort[] ActiveConnections
        {
            get
            {
                var byteArray = this.scanner.GetAttributeSingle(2, 1, 4);
                var returnValue = new ushort[byteArray.Length / 2];
                for (var i = 0; i < returnValue.Length; i++)
                {
                    returnValue[i] = (ushort)(byteArray[1 + 2*i] << 8 | byteArray[0 + 2*i]);
                }
                return returnValue;
            
            }
        }

    }
}
