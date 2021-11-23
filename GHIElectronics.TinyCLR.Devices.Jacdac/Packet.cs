using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.Jacdac {
    public class Packet {
        // Registers 0x001-0x07f - r/w common to all services
        // Registers 0x080-0x0ff - r/w defined per-service
        // Registers 0x100-0x17f - r/o common to all services
        // Registers 0x180-0x1ff - r/o defined per-service
        // Registers 0x200-0xeff - custom, defined per-service
        // Registers 0xf00-0xfff - reserved for implementation, should not be on the wire

        // this is either binary (0 or non-zero), or can be gradual (eg. brightness of neopixel)
        const int REG_INTENSITY = 0x01;
        // the primary value of actuator (eg. servo angle)
        const int REG_VALUE = 0x02;
        // enable/disable streaming
        const int REG_IS_STREAMING = 0x03;
        // streaming interval in miliseconds
        const int REG_STREAMING_INTERVAL = 0x04;
        // for analog sensors
        const int REG_LOW_THRESHOLD = 0x05;
        const int REG_HIGH_THRESHOLD = 0x06;
        // limit power drawn; in mA
        const int REG_MAX_POWER = 0x07;

        // eg. one number for light sensor, all 3 coordinates for accelerometer
        const int REG_READING = 0x101;

        const int CMD_GET_REG = 0x1000;
        const int CMD_SET_REG = 0x2000;

        const int CMD_TOP_MASK = 0xf000;
        const int CMD_REG_MASK = 0x0fff;


        // Commands 0x000-0x07f - common to all services
        // Commands 0x080-0xeff - defined per-service
        // Commands 0xf00-0xfff - reserved for implementation
        // enumeration data for CTRL, ad-data for other services
        const int CMD_ADVERTISEMENT_DATA = 0x00;
        // event from sensor or on broadcast service
        const int CMD_EVENT = 0x01;
        // request to calibrate sensor
        const int CMD_CALIBRATE = 0x02;
        // request human-readable description of service
        const int CMD_GET_DESCRIPTION = 0x03;

        // Commands specific to control service
        // do nothing
        const int CMD_CTRL_NOOP = 0x80;
        // blink led or otherwise draw user's attention
        const int CMD_CTRL_IDENTIFY = 0x81;
        // reset device
        const int CMD_CTRL_RESET = 0x82;

        const int STREAM_PORT_SHIFT = 7;
        const int STREAM_COUNTER_MASK = 0x001f;
        const int STREAM_CLOSE_MASK = 0x0020;
        const int STREAM_METADATA_MASK = 0x0040;

        const int JD_SERIAL_HEADER_SIZE = 16;
        const int JD_SERIAL_MAX_PAYLOAD_SIZE = 236;
        const int JD_SERVICE_NUMBER_MASK = 0x3f;
        const int JD_SERVICE_NUMBER_INV_MASK = 0xc0;
        const int JD_SERVICE_NUMBER_CRC_ACK = 0x3f;
        const int JD_SERVICE_NUMBER_STREAM = 0x3e;
        const int JD_SERVICE_NUMBER_CONTROL = 0x00;

        // the COMMAND flag signifies that the device_identifier is the recipent
        // (i.e., it's a command for the peripheral); the bit clear means device_identifier is the source
        // (i.e., it's a report from peripheral or a broadcast message)
        const int JD_FRAME_FLAG_COMMAND = 0x01;
        // an ACK should be issued with CRC of this package upon reception
        const int JD_FRAME_FLAG_ACK_REQUESTED = 0x02;
        // the device_identifier contains target service class number
        const int JD_FRAME_FLAG_IDENTIFIER_IS_SERVICE_CLASS = 0x04;

        //GHI
        const uint UNDEFINED = 0xFFFFFFFF;
        const int UNDEFINED_INT = -1;

        // microbit
        const uint CMD_EVENT_MASK = 0x8000;
        const uint CMD_EVENT_CODE_MASK = 0xff;
        const uint CMD_EVENT_COUNTER_MASK = 0x7f;
        const byte CMD_EVENT_COUNTER_POS = 8;

        byte[] header;
        byte[] data;
        public TimeSpan Timestamp { get; set; }
        Device dev;

        public Packet() {

        }

        public static Packet FromBinary(byte[] buffer) {
            var p = new Packet {
                header = Util.Slice(buffer, 0, JD_SERIAL_HEADER_SIZE),
                data = Util.Slice(buffer, JD_SERIAL_HEADER_SIZE)
            };

            return p;

        }

        public static Packet From(uint service_command, byte[] buffer) {
            var p = new Packet {
                header = new byte[JD_SERIAL_HEADER_SIZE],
                data = buffer,
                ServiceCommand = (ushort)service_command
            };

            return p;
        }

        public static Packet OnlyHeader(uint service_command) {
            var data = new byte[0];
            return Packet.From(service_command, data);
        }

        public byte[] ToBuffer() => Util.BufferConcat(this.header, this.data);

        public string DeviceIdentifier {
            get => Util.ToHex(Util.Slice(this.header, 4, 4 + 8));
            set {
                var idb = Util.FromHex(value);

                if (idb.Length != 8) {
                    throw new Exception("Invalid id");
                }

                Util.Set(this.header, idb, 4);
            }
        }



        public byte FrameFlags => this.header[3];


        public uint MulticommandClass {
            get {
                if ((this.FrameFlags & JD_FRAME_FLAG_IDENTIFIER_IS_SERVICE_CLASS) != 0)
                    return Util.Read32(this.header, 4);
                return UNDEFINED;
            }
        }

        public uint Size => this.header[12];


        public bool IsRequiresAck {
            get => (this.FrameFlags & JD_FRAME_FLAG_ACK_REQUESTED) != 0 ? true : false;

            set {
                if (value != this.IsRequiresAck)
                    this.header[3] ^= JD_FRAME_FLAG_ACK_REQUESTED;
            }
        }


        public uint ServiceNumber {
            get => (uint)(this.header[13] & JD_SERVICE_NUMBER_MASK);
            set => this.header[13] = (byte)((this.header[13] & JD_SERVICE_NUMBER_INV_MASK) | value);
        }


        public uint ServiceClass {
            get {
                if (this.dev != null)
                    return this.dev.ServiceAt(this.ServiceNumber);

                return UNDEFINED;
            }
        }
        public ushort Crc => Util.Read16(this.header, 0);


        public ushort ServiceCommand {
            get => Util.Read16(this.header, 14);
            set => Util.Write16(this.header, 14, value);
        }

        public bool IsEvent => this.IsReport && this.ServiceNumber <= 0x30 && ((this.ServiceCommand & CMD_EVENT_MASK) != 0);

        public uint EventCode {
            get {
                if (this.IsEvent == true)
                    return this.ServiceCommand & CMD_EVENT_CODE_MASK;
                else
                    return UNDEFINED;
            }
        }

        public uint EventCounter {
            get {
                if (this.IsEvent == true)
                    return ((uint)this.ServiceCommand >> CMD_EVENT_COUNTER_POS) & CMD_EVENT_COUNTER_MASK;
                else
                    return UNDEFINED;
            }
        }

        public uint RegisterCode => (uint)(this.ServiceCommand & CMD_REG_MASK);
        public bool IsRegisterSet => (this.ServiceCommand >> 12) == (CMD_SET_REG >> 12) ? true : false;
        public bool IsRegisterGet => (this.ServiceCommand >> 12) == (CMD_GET_REG >> 12) ? true : false;

        public byte[] Header {
            get => this.header;
            set {
                if (value.Length > JD_SERIAL_HEADER_SIZE)
                    throw new Exception("Too big");

                this.header = value;
            }
        }
        public byte[] Data {
            get => this.data;
            set {
                if (value.Length > JD_SERIAL_MAX_PAYLOAD_SIZE)
                    throw new Exception("Too big");
                this.header[12] = (byte)value.Length;
                this.data = value;
            }
        }

        public uint UintData {
            get {
                var buf = this.data;

                if (buf == null || buf.Length == 0)
                    return UNDEFINED;

                if (buf.Length < 4)
                    buf = Util.BufferConcat(buf, new byte[4]);

                return Util.Read32(buf, 0);
            }
        }

        public int IntData {
            get {
                Util.NumberFormat fmt;
                switch (this.data.Length) {
                    case 0:
                        return UNDEFINED_INT;
                    case 1:
                        fmt = Util.NumberFormat.Int8LE;
                        break;
                    case 2:
                    case 3:
                        fmt = Util.NumberFormat.Int16LE;
                        break;
                    default:
                        fmt = Util.NumberFormat.Int32LE;
                        break;
                }
                return (int)this.GetNumber(fmt, 0);
            }
        }

        public uint GetNumber(Util.NumberFormat fmt, int offset) => Util.GetNumber(this.data, fmt, offset);

        public bool IsCommand => (this.FrameFlags & JD_FRAME_FLAG_COMMAND) != 0 ? true : false;
        public bool IsReport => !this.IsCommand;
    }
}
