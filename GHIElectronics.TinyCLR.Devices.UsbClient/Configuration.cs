using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    /// <summary>Holds the set of USB descriptors that define a device.</summary>
    public class Configuration {
        /// <summary>Base class for all USB descriptors.</summary>
        public abstract class Descriptor {
            /// <summary>Creates a new descriptor with the given index.</summary>
            protected Descriptor(byte Index) => this.index = Index;

            /// <summary>The index of the descriptor.</summary>
            protected byte index;
        }

        /// <summary>The USB device descriptor.</summary>
        public class DeviceDescriptor : Descriptor {
            /// <summary>Creates a new device descriptor.</summary>
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

            /// <summary>The vendor id.</summary>
            public ushort idVendor;
            /// <summary>The product id.</summary>
            public ushort idProduct;
            /// <summary>The device release number.</summary>
            public ushort bcdDevice;
            /// <summary>The string index of the manufacturer.</summary>
            public byte iManufacturer;
            /// <summary>The string index of the product.</summary>
            public byte iProduct;
            /// <summary>The string index of the serial number.</summary>
            public byte iSerialNumber;
            /// <summary>The device class code.</summary>
            public byte bDeviceClass;
            /// <summary>The device subclass code.</summary>
            public byte bDeviceSubClass;
            /// <summary>The device protocol code.</summary>
            public byte bDeviceProtocol;
            /// <summary>The maximum packet size for endpoint zero.</summary>
            public byte bMaxPacketSize0;
            /// <summary>The USB specification release number.</summary>
            public ushort bcdUSB;
        }

        /// <summary>A class-specific descriptor.</summary>
        public class ClassDescriptor {
            /// <summary>Creates a new class descriptor.</summary>
            public ClassDescriptor(byte DescriptorType, byte[] Payload) {
                this.bDescriptorType = DescriptorType;
                this.payload = Payload;
            }

            /// <summary>The descriptor type.</summary>
            public byte bDescriptorType;
            private byte[] payload;
        }

        /// <summary>A USB endpoint descriptor.</summary>
        public class Endpoint {
            /// <summary>Attribute marking the endpoint as a read endpoint.</summary>
            public const byte ATTRIB_Read = 0;
            /// <summary>Attribute marking the endpoint as a write endpoint.</summary>
            public const byte ATTRIB_Write = 0x80;
            /// <summary>Attribute for an isochronous transfer type.</summary>
            public const byte ATTRIB_Isochronous = 0x01;
            /// <summary>Attribute for a bulk transfer type.</summary>
            public const byte ATTRIB_Bulk = 0x02;
            /// <summary>Attribute for an interrupt transfer type.</summary>
            public const byte ATTRIB_Interrupt = 0x03;
            /// <summary>Attribute for no synchronization.</summary>
            public const byte ATTRIB_NoSynch = 0;
            /// <summary>Attribute for asynchronous synchronization.</summary>
            public const byte ATTRIB_Asynch = 0x04;
            /// <summary>Attribute for adaptive synchronization.</summary>
            public const byte ATTRIB_Adaptive = 0x08;
            /// <summary>Attribute for synchronous synchronization.</summary>
            public const byte ATTRIB_Synchronous = 0x0C;
            /// <summary>Attribute for a data usage type.</summary>
            public const byte ATTRIB_Data = 0;
            /// <summary>Attribute for a feedback usage type.</summary>
            public const byte ATTRIB_Feedback = 0x10;
            /// <summary>Attribute for an implicit feedback data usage type.</summary>
            public const byte ATTRIB_Implicit = 0x20;

            /// <summary>Creates a new endpoint descriptor.</summary>
            public Endpoint(byte EndpointAddress, byte Attributes) {
                this.bEndpointAddress = EndpointAddress;
                this.bmAttributes = Attributes;
                this.wMaxPacketSize = 64;                  // Default to 64 byte packet size
                this.bInterval = 0;                   // Default to no interval
            }

            /// <summary>The address of the endpoint.</summary>
            public byte bEndpointAddress;
            /// <summary>The endpoint attributes.</summary>
            public byte bmAttributes;
            /// <summary>The maximum packet size for the endpoint.</summary>
            public ushort wMaxPacketSize;
            /// <summary>The polling interval for the endpoint.</summary>
            public byte bInterval;
        }

        /// <summary>A USB interface descriptor.</summary>
        public class UsbInterface {
            /// <summary>Creates a new interface descriptor.</summary>
            public UsbInterface(byte InterfaceNumber, Endpoint[] Endpoints) {
                this.bInterfaceNumber = InterfaceNumber;
                this.endpoints = Endpoints;
                this.bInterfaceClass = 0xFF;      // Defaults to Vendor class
                this.bInterfaceSubClass = 1;         // Defaults to Sub Class #1
                this.bInterfaceProtocol = 1;         // Defaults to Protocol #1
                this.iInterface = 0;         // Defaults to no Interface string
            }

            /// <summary>The interface number.</summary>
            public byte bInterfaceNumber;
            /// <summary>The endpoints belonging to this interface.</summary>
            public Endpoint[] endpoints;
            /// <summary>The class-specific descriptors for this interface.</summary>
            public ClassDescriptor[] classDescriptors;
            /// <summary>The interface class code.</summary>
            public byte bInterfaceClass;
            /// <summary>The interface subclass code.</summary>
            public byte bInterfaceSubClass;
            /// <summary>The interface protocol code.</summary>
            public byte bInterfaceProtocol;
            /// <summary>The string index of the interface.</summary>
            public byte iInterface;
        }

        /// <summary>A USB configuration descriptor.</summary>
        public class ConfigurationDescriptor : Descriptor {
            /// <summary>The base configuration attribute.</summary>
            public const byte ATTRIB_Base = 0x80;
            /// <summary>Attribute marking the device as self powered.</summary>
            public const byte ATTRIB_SelfPowered = 0x40;
            /// <summary>Attribute marking the device as supporting remote wakeup.</summary>
            public const byte ATTRIB_RemoteWakeup = 0x20;

            private const ushort PowerFactor = 2;

            /// <summary>Creates a new configuration descriptor.</summary>
            public ConfigurationDescriptor(ushort MaxPower_mA, UsbInterface[] Interfaces)
                : base(0) {
                this.bMaxPower = (byte)(MaxPower_mA / PowerFactor);
                this.interfaces = Interfaces;
                this.iConfiguration = 0;             // Default to no Configuration string
                this.bmAttributes = ATTRIB_Base;   // Default to no attributes
            }

            /// <summary>The interfaces belonging to this configuration.</summary>
            public UsbInterface[] interfaces;
            /// <summary>The string index of the configuration.</summary>
            public byte iConfiguration;
            /// <summary>The configuration attributes.</summary>
            public byte bmAttributes;
            /// <summary>The maximum power the configuration uses, in 2 mA units.</summary>
            public byte bMaxPower;
        }  // End of ConfigurationDescriptor class

        /// <summary>A USB string descriptor.</summary>
        public class StringDescriptor : Descriptor {
            /// <summary>Creates a new string descriptor.</summary>
            public StringDescriptor(byte index, string theString)
                : base(index) => this.sString = theString;

            /// <summary>The index of the string descriptor.</summary>
            public byte bIndex => this.index;

            /// <summary>The string value.</summary>
            public string sString;
        }

        /// <summary>A generic descriptor returned in response to a control request.</summary>
        public class GenericDescriptor : Descriptor {
            /// <summary>Request direction marking an out (host to device) transfer.</summary>
            public const byte REQUEST_OUT = 0;
            /// <summary>Request direction marking an in (device to host) transfer.</summary>
            public const byte REQUEST_IN = 0x80;
            /// <summary>Standard request type.</summary>
            public const byte REQUEST_Standard = 0;
            /// <summary>Class request type.</summary>
            public const byte REQUEST_Class = 0x20;
            /// <summary>Vendor request type.</summary>
            public const byte REQUEST_Vendor = 0x40;
            /// <summary>Request recipient is the device.</summary>
            public const byte REQUEST_Device = 0;
            /// <summary>Request recipient is an interface.</summary>
            public const byte REQUEST_Interface = 0x01;
            /// <summary>Request recipient is an endpoint.</summary>
            public const byte REQUEST_Endpoint = 0x02;
            /// <summary>Request recipient is another element.</summary>
            public const byte REQUEST_Other = 0x03;

            private const byte REQUEST_GET_DESCRIPTOR = 0x06;

            /// <summary>Creates a new generic descriptor.</summary>
            public GenericDescriptor(byte RequestType, ushort Value, byte[] Payload)
                : base(0) {
                this.bmRequestType = (byte)(RequestType | REQUEST_IN);       // The Generic Descriptor only supports "Get" type requests by default
                this.bRequest = REQUEST_GET_DESCRIPTOR;                 // Default to request for descriptor
                this.wValue = Value;
                this.wIndex = 0;                                      // Default to a zero index
                this.payload = Payload;
            }

            /// <summary>The request type.</summary>
            public byte bmRequestType;
            /// <summary>The request code.</summary>
            public byte bRequest;
            /// <summary>The request value.</summary>
            public ushort wValue;
            /// <summary>The request index.</summary>
            public ushort wIndex;
            /// <summary>The descriptor data returned for the request.</summary>
            public byte[] payload;
        }

        /// <summary>All of the descriptors that make up the configuration.</summary>
        public Descriptor[] descriptors;
    }
}
