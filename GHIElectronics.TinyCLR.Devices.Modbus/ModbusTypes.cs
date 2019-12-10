using System;

namespace GHIElectronics.TinyCLR.Devices.Modbus {
    /// <summary>
    /// Identification conformity level of the device and type of supported access.
    /// </summary>
    public enum ModbusConformityLevel : byte {
        /// <summary>
        /// Basic Device Identification. All objects of this category are mandatory : VendorName, Product code, and revision number.
        /// </summary>
        Basic = 0x01,
        /// <summary>
        /// Regular Device Identification. In addition to Basic data objects, the device provides additional and optional identification and description data objects. All of the objects of this category are defined in the standard but their implementation is optional .
        /// </summary>
        Regular = 0x02,
        /// <summary>
        /// Extended Device Identification. In addition to regular data objects, the device provides additional and optional identification and description private data about the physical device itself. All of these data are device dependent.
        /// </summary>
        Extended = 0x03
    }

    /// <summary>
    /// Identifiers for the Device Identification elements.
    /// </summary>
    public enum ModbusObjectId : byte {
        /// <summary>
        /// VendorName (mandatory)
        /// </summary>
        VendorName = 0x00,
        /// <summary>
        /// ProductCode (mandatory)
        /// </summary>
        ProductCode = 0x01,
        /// <summary>
        /// MajorMinorRevision (mandatory)
        /// </summary>
        MajorMinorRevision = 0x02,
        /// <summary>
        /// VendorUrl (optional)
        /// </summary>
        VendorUrl = 0x03,
        /// <summary>
        /// ProductName (optional)
        /// </summary>
        ProductName = 0x04,
        /// <summary>
        /// ModelName (optional)
        /// </summary>
        ModelName = 0x05,
        /// <summary>
        /// UserApplicationName (optional)
        /// </summary>
        UserApplicationName = 0x06,
        /// <summary>
        /// Fisrt reserved id (0x07 .. 0x7f are reserved)
        /// </summary>
        ReservedFirst = 0x07,
        /// <summary>
        /// Last reserved id (0x07 .. 0x7f are reserved)
        /// </summary>
        ReservedLast = 0x7f,
        /// <summary>
        /// First private (extended) id (0x80 .. 0xff are private)
        /// </summary>
        PrivareFirst = 0x80,
        /// <summary>
        /// Last private (extended) id (0x80 .. 0xff are private)
        /// </summary>
        PrivateLast = 0xff
    }

    /// <summary>
    /// Defines a device identification by id and value
    /// </summary>
    public class DeviceIdentification {
        /// <summary>
        /// Gets sets the object id of the device identification 
        /// </summary>
        public ModbusObjectId ObjectId { get; set; }

        /// <summary>
        /// Gets sets the value of the divide identification
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Modbus error codes
    /// </summary>
    /// <remarks>
    /// Errors received from modbus devices have the high byte set to zero.
    /// Internal errors like Timeout have the high byte != zero
    /// </remarks>
    public enum ModbusErrorCode : ushort {
        /// <summary>
        /// No error
        /// </summary>
        NoError = 0x0000,

        // standrad modbus exception codes
        /// <summary>
        /// The function code received in the query is not an allowable action for the server.
        /// </summary>
        /// <remarks>This may be because the function code is only applicable to newer devices, and was not implemented in the unit selected. It could also indicate that the server is in the wrong state to process a request of this type, for example because it is unconfigured and is being asked to return register values.</remarks>
        IllegalFunction = 0x0001,

        /// <summary>
        /// The data address received in the query is not an allowable address for the server.
        /// </summary>
        /// <remarks>
        /// More specifically, the combination of reference number and transfer length is invalid. For a controller with 100 registers, the PDU addresses the first register as 0, and the last one as 99. If a request is submitted with a starting register address of 96 and a quantity of registers of 4, then this request will successfully operate (address-wise at least) on registers 96, 97, 98, 99. If a request is submitted with a starting register address of 96 and a quantity of registers of 5, then this request will fail with Exception Code 0x02 “Illegal Data Address” since it attempts to operate on registers 96, 97, 98, 99 and 100, and there is no register with address 100.
        /// </remarks>
        IllegalDataAddress = 0x0002,

        /// <summary>
        /// A value contained in the query data field is not an allowable value for server.
        /// </summary>
        /// <remarks>
        /// This indicates a fault in the structure of the remainder of a complex request, such as that the implied length is incorrect. It specifically does NOT mean that a data item submitted for storage in a register has a value outside the expectation of the application program, since the MODBUS protocol is unaware of the significance of any particular value of any particular register.
        /// </remarks>
        IllegalDataValue = 0x0003,

        /// <summary>
        /// An unrecoverable error occurred while the server was attempting to perform the requested action.
        /// </summary>
        ServerDeviceFailure = 0x0004,

        /// <summary>
        /// Specialized use in conjunction with programming commands.
        /// </summary>
        /// <remarks>
        /// The server has accepted the request and is processing it, but a long duration of time will be required to do so. This response is returned to prevent a timeout error from occurring in the client. The client can next issue a Poll Program Complete message to determine if processing is completed.
        /// </remarks>
        Acknowledge = 0x0005,

        /// <summary>
        /// Specialized use in conjunction with programming commands.
        /// </summary>
        /// <remarks>
        /// The server is engaged in processing a long–duration program command. The client should retransmit the message later when the server is free.
        /// </remarks>
        ServerDeviceBusy = 0x0006,

        /// <summary>
        /// Specialized use in conjunction with programming commands.
        /// </summary>
        NegativeAcknowledgement = 0x0007,

        /// <summary>
        /// Specialized use in conjunction with function codes 20 and 21 and reference type 6, to indicate that the extended file area failed to pass a consistency check.
        /// </summary>
        /// <remarks>
        /// The server attempted to read record file, but detected a parity error in the memory. The client can retry the request, but service may be required on the server device.
        /// </remarks>
        MemoryParityError = 0x0008,

        /// <summary>
        /// Specialized use in conjunction with gateways.
        /// </summary>
        /// <remarks>
        /// Indicates that the gateway was unable to allocate an internal communication path from the input port to the output port for processing the request. Usually means that the gateway is misconfigured or overloaded.
        /// </remarks>
        GatewayPathUnavailable = 0x000a,

        /// <summary>
        /// Specialized use in conjunction with gateways.
        /// </summary>
        /// <remarks>
        /// Indicates that no response was obtained from the target device. Usually means that the device is not present on the network.
        /// </remarks>
        GatewayTargetDeviceFailedToRespond = 0x000b,

        // implementation specific errors

        /// <summary>
        /// Unspecifed internal error
        /// </summary>
        Unspecified = 0x0100,

        /// <summary>
        /// Device response timeout
        /// </summary>
        Timeout = 0x0101,

        /// <summary>
        /// Check summ error
        /// </summary>
        CrcError = 0x0102,

        /// <summary>
        /// Response data to short.
        /// </summary>
        ResponseTooShort = 0x0103,
    }
}
