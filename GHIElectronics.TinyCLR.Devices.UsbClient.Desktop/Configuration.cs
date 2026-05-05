using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    public class Configuration {
        public abstract class Descriptor {
            protected Descriptor(byte Index) => this.index = Index;

            protected byte index;
        }

        public class DeviceDescriptor : Descriptor {
            public DeviceDescriptor(ushort Vendor, ushort Product, ushort bcdUsb, ushort bcdDevice)
                : base(0) {
                this.idVendor = Vendor;
                this.idProduct = Product;
                this.bcdUSB = bcdUsb;
                this.bcdDevice = bcdDevice;
                this.iManufacturer = 0;        // Default to no Manufacturer string
                this.iProduct = 0;        // Default to no Product string
                this.iSerialNumber = 0;        // Default to no Serial Number string
                this.bDeviceClass = 0;        // Default to no Device Class
                this.bDeviceSubClass = 0;        // Default to no Device Sub Class
                this.bDeviceProtocol = 0;        // Default to no Device Protocol
                this.bMaxPacketSize0 = 8;        // Default to maximum control packet size of 8                
            }

            public ushort idVendor;
            public ushort idProduct;
            public ushort bcdDevice;
            public byte iManufacturer;
            public byte iProduct;
            public byte iSerialNumber;
            public byte bDeviceClass;
            public byte bDeviceSubClass;
            public byte bDeviceProtocol;
            public byte bMaxPacketSize0;
            public ushort bcdUSB;
        }

        public class ClassDescriptor {
            public ClassDescriptor(byte DescriptorType, byte[] Payload) {
                this.bDescriptorType = DescriptorType;
                this.payload = Payload;
            }

            public byte bDescriptorType;
            private byte[] payload;
        }

        public class Endpoint {
            public const byte ATTRIB_Read = 0;
            public const byte ATTRIB_Write = 0x80;
            public const byte ATTRIB_Isochronous = 0x01;
            public const byte ATTRIB_Bulk = 0x02;
            public const byte ATTRIB_Interrupt = 0x03;
            public const byte ATTRIB_NoSynch = 0;
            public const byte ATTRIB_Asynch = 0x04;
            public const byte ATTRIB_Adaptive = 0x08;
            public const byte ATTRIB_Synchronous = 0x0C;
            public const byte ATTRIB_Data = 0;
            public const byte ATTRIB_Feedback = 0x10;
            public const byte ATTRIB_Implicit = 0x20;

            public Endpoint(byte EndpointAddress, byte Attributes) {
                this.bEndpointAddress = EndpointAddress;
                this.bmAttributes = Attributes;
                this.wMaxPacketSize = 64;                  // Default to 64 byte packet size
                this.bInterval = 0;                   // Default to no interval
            }

            public byte bEndpointAddress;
            public byte bmAttributes;
            public ushort wMaxPacketSize;
            public byte bInterval;
        }

        public class UsbInterface {
            public UsbInterface(byte InterfaceNumber, Endpoint[] Endpoints) {
                this.bInterfaceNumber = InterfaceNumber;
                this.endpoints = Endpoints;
                this.bInterfaceClass = 0xFF;      // Defaults to Vendor class
                this.bInterfaceSubClass = 1;         // Defaults to Sub Class #1
                this.bInterfaceProtocol = 1;         // Defaults to Protocol #1
                this.iInterface = 0;         // Defaults to no Interface string
            }

            public byte bInterfaceNumber;
            public Endpoint[] endpoints;
            public ClassDescriptor[] classDescriptors;
            public byte bInterfaceClass;
            public byte bInterfaceSubClass;
            public byte bInterfaceProtocol;
            public byte iInterface;
        }

        public class ConfigurationDescriptor : Descriptor {
            public const byte ATTRIB_Base = 0x80;
            public const byte ATTRIB_SelfPowered = 0x40;
            public const byte ATTRIB_RemoteWakeup = 0x20;

            private const ushort PowerFactor = 2;

            public ConfigurationDescriptor(ushort MaxPower_mA, UsbInterface[] Interfaces)
                : base(0) {
                this.bMaxPower = (byte)(MaxPower_mA / PowerFactor);
                this.interfaces = Interfaces;
                this.iConfiguration = 0;             // Default to no Configuration string
                this.bmAttributes = ATTRIB_Base;   // Default to no attributes
            }

            public UsbInterface[] interfaces;
            public byte iConfiguration;
            public byte bmAttributes;
            public byte bMaxPower;
        }  // End of ConfigurationDescriptor class

        public class StringDescriptor : Descriptor {
            public StringDescriptor(byte index, string theString)
                : base(index) => this.sString = theString;

            public byte bIndex => this.index;

            public string sString;
        }

        public class GenericDescriptor : Descriptor {
            public const byte REQUEST_OUT = 0;
            public const byte REQUEST_IN = 0x80;
            public const byte REQUEST_Standard = 0;
            public const byte REQUEST_Class = 0x20;
            public const byte REQUEST_Vendor = 0x40;
            public const byte REQUEST_Device = 0;
            public const byte REQUEST_Interface = 0x01;
            public const byte REQUEST_Endpoint = 0x02;
            public const byte REQUEST_Other = 0x03;

            private const byte REQUEST_GET_DESCRIPTOR = 0x06;

            public GenericDescriptor(byte RequestType, ushort Value, byte[] Payload)
                : base(0) {
                this.bmRequestType = (byte)(RequestType | REQUEST_IN);       // The Generic Descriptor only supports "Get" type requests by default
                this.bRequest = REQUEST_GET_DESCRIPTOR;                 // Default to request for descriptor
                this.wValue = Value;
                this.wIndex = 0;                                      // Default to a zero index
                this.payload = Payload;
            }

            public byte bmRequestType;
            public byte bRequest;
            public ushort wValue;
            public ushort wIndex;
            public byte[] payload;
        }

        public Descriptor[] descriptors;
    }
}
