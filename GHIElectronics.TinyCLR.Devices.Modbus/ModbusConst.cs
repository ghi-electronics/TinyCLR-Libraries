namespace GHIElectronics.TinyCLR.Devices.Modbus {
    /// <summary>
    /// Some general constants for Modbus
    /// </summary>
    public static class ModbusConst {
        /// <summary>
        /// Modbus broadcast address
        /// </summary>
        public const byte BroadcastAddress = 0x00;

        /// <summary>
        /// The default device address for Modbus TCP devices (248)
        /// </summary>
        public const byte TcpDeviceAddress = 0xF8;

        /// <summary>
        /// The dafult TCP port for Modbus TCP devices
        /// </summary>
        public const int TcpDefaultPort = 502;
    }


    /// <summary>
    /// Modbus function codes
    /// </summary>
    public enum ModbusFunctionCode : byte {
        /// <summary>
        /// Read colis (input/output bits)
        /// </summary>
        ReadCoils = 0x01,

        /// <summary>
        /// Read discrete inputs (input bits)
        /// </summary>
        ReadDiscreteInputs = 0x02,

        /// <summary>
        /// Read holding registers (input/output registers)
        /// </summary>
        ReadHoldingRegisters = 0x03,

        /// <summary>
        /// Read input registers
        /// </summary>
        ReadInputRegisters = 0x04,

        /// <summary>
        /// Write single coil (input/output bit)
        /// </summary>
        WriteSingleCoil = 0x05,

        /// <summary>
        /// Write single registers (input/output register)
        /// </summary>
        WriteSingleRegister = 0x06,

        /// <summary>
        /// Read exception status
        /// </summary>
        ReadExceptionStatus = 0x07,

        /// <summary>
        /// Diagnostics
        /// </summary>
        Diagnostics = 0x08,

        /// <summary>
        /// Get comm event counter
        /// </summary>
        GetCommEventCounter = 0x0b,

        /// <summary>
        /// Get comm event log
        /// </summary>
        GetCommEventLog = 0x0c,

        /// <summary>
        /// Write multiple coils (input/output bits)
        /// </summary>
        WriteMultipleCoils = 0x0f,

        /// <summary>
        /// Write multiple registers (input/output registers)
        /// </summary>
        WriteMultipleRegisters = 0x10,

        /// <summary>
        /// Read write multiple registers (input/output regsiters)
        /// The Write operation is performed first.
        /// </summary>
        ReadWriteMultipleRegisters = 0x17,

        /// <summary>
        /// Read device identification
        /// </summary>
        ReadDeviceIdentification = 0x2b,

        /// <summary>
        /// Read device identification (again !?)
        /// </summary>
        ReadDeviceIdentification2 = 0x0e,
    }

}
