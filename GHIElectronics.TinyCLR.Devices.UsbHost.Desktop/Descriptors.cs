using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.Devices.UsbHost.Descriptors {

	/// <summary>The possible types of descriptors.</summary>
	public enum DescriptorType : byte {

		/// <summary>The device descriptor type.</summary>
		Device = 0x01,

		/// <summary>The configuration descriptor type.</summary>
		Configuration = 0x02,

		/// <summary>The string descriptor type.</summary>
		String = 0x03,

		/// <summary>The interface descriptor type.</summary>
		Interface = 0x04,

		/// <summary>The endpoint descriptor type.</summary>
		Endpoint = 0x05
	}

	/// <summary>The base USB descriptor.</summary>
	public class BaseDescriptor {

		/// <summary>Length of descriptor in bytes.</summary>
		public byte Length { get; set; }

        /// <summary>The type of the descriptor.</summary>
        public DescriptorType Type { get; set; }

        /// <summary>Creates a new base descriptor</summary>
        /// <param name="type">The type of the derived descriptor</param>
        /// <param name="length">The length of the descriptor.</param>
        protected BaseDescriptor(DescriptorType type, byte length) {
			this.Type = type;
			this.Length = length;
		}
	}

	/// <summary>Device descriptor.</summary>
	public class Device : BaseDescriptor {

		/// <summary>The length of the descriptor in bytes.</summary>
		public const byte LENGTH = 18;

		/// <summary>USB specification number that the device implements as a BCD.</summary>
		public ushort UsbSpecificationNumber { get; set; }

		/// <summary>The class code of the device.</summary>
		public byte ClassCode { get; set; }

        /// <summary>The subclass code.</summary>
        public byte SubclassCode { get; set; }

        /// <summary>The protocal code.</summary>
        public byte ProtocalCode { get; set; }

        /// <summary>The max packet size for endpoint zero.</summary>
        public byte MaximumPacketSize { get; set; }

        /// <summary>The vendor id.</summary>
        public ushort VendorId { get; set; }

        /// <summary>The product id.</summary>
        public ushort ProductId { get; set; }

        /// <summary>The device release number as a BCD.</summary>
        public ushort ReleaseNumber { get; set; }

        /// <summary>The manufacturer string descriptor index.</summary>
        public byte ManufacturerIndex { get; set; }

        /// <summary>The product string descriptor index.</summary>
        public byte ProductIndex { get; set; }

        /// <summary>The serial number string descriptor index.</summary>
        public byte SerialNumberIndex { get; set; }

        /// <summary>The number of possible configurations.</summary>
        public byte NumberOfConfigurations { get; set; }

        /// <summary>Constructs a new descriptor.</summary>
        /// <param name="buffer">The buffer with which to populate the descriptor.</param>
        public Device(byte[] buffer)
			: this(buffer, 0) {
		}

		/// <summary>Constructs a new descriptor.</summary>
		/// <param name="buffer">The buffer with which to populate the descriptor.</param>
		/// <param name="offset">The offset into the buffer at which to start.</param>
		public Device(byte[] buffer, int offset)
			: this() {
			if (this.Length != buffer[offset]) throw new ArgumentException("Invalid length received for a USB descriptor.", "buffer");
			if (this.Type != (Descriptors.DescriptorType)buffer[offset + 1]) throw new ArgumentException("Invalid type received for a USB descriptor.", "buffer");

			this.UsbSpecificationNumber = (ushort)(buffer[offset + 2] | (buffer[offset + 3] << 8));
			this.ClassCode = buffer[offset + 4];
			this.SubclassCode = buffer[offset + 5];
			this.ProtocalCode = buffer[offset + 6];
			this.MaximumPacketSize = buffer[offset + 7];
			this.VendorId = (ushort)(buffer[offset + 8] | (buffer[offset + 9] << 8));
			this.ProductId = (ushort)(buffer[offset + 10] | (buffer[offset + 11] << 8));
			this.ReleaseNumber = (ushort)(buffer[offset + 12] | (buffer[offset + 13] << 8));
			this.ManufacturerIndex = buffer[offset + 14];
			this.ProductIndex = buffer[offset + 15];
			this.SerialNumberIndex = buffer[offset + 16];
			this.NumberOfConfigurations = buffer[offset + 17];
		}

		/// <summary>Constructs a new descriptor.</summary>
		protected Device()
			: base(DescriptorType.Device, Device.LENGTH) {
		}
	}

	/// <summary>Configuration descriptor.</summary>
	public class Configuration : BaseDescriptor {

		/// <summary>The length of the descriptor in bytes.</summary>
		public const byte LENGTH = 9;

		/// <summary>Total length of the the data returned in bytes.</summary>
		public ushort TotalLength { get; set; }

        /// <summary>The number of interfaces.</summary>
        public byte NumberOfInterfaces { get; set; }

        /// <summary>The value to be used as an argument to select this configuration.</summary>
        public byte Value { get; set; }

        /// <summary>The configuration string descriptor index.</summary>
        public byte Index { get; set; }

        /// <summary>A bitmap representing the attributes of the device.</summary>
        public byte Attributes { get; set; }

        /// <summary>The maximum power consumption in 2mA units.</summary>
        public byte MaxPower { get; set; }

        /// <summary>Child auxiliary descriptors.</summary>
        public Auxiliary[] AuxiliaryDescriptors { get; set; }

        /// <summary>Child interface descriptors.</summary>
        public Interface[] Interfaces { get; set; }

        /// <summary>Constructs a new descriptor.</summary>
        /// <param name="buffer">The buffer with which to populate the descriptor.</param>
        public Configuration(byte[] buffer)
			: this(buffer, 0) {
		}

		/// <summary>Constructs a new descriptor.</summary>
		/// <param name="buffer">The buffer with which to populate the descriptor.</param>
		/// <param name="offset">The offset into the buffer at which to start.</param>
		public Configuration(byte[] buffer, int offset)
			: this() {
			if (this.Length != buffer[offset]) throw new ArgumentException("Invalid length received for a USB descriptor.", "buffer");
			if (this.Type != (Descriptors.DescriptorType)buffer[offset + 1]) throw new ArgumentException("Invalid type received for a USB descriptor.", "buffer");

			this.TotalLength = (ushort)(buffer[offset + 2] | (buffer[offset + 3] << 8));
			this.NumberOfInterfaces = buffer[offset + 4];
			this.Value = buffer[offset + 5];
			this.Index = buffer[offset + 6];
			this.Attributes = buffer[offset + 7];
			this.MaxPower = buffer[offset + 8];
		}

		/// <summary>Constructs a new descriptor.</summary>
		protected Configuration()
			: base(DescriptorType.Configuration, Configuration.LENGTH) {
		}

		internal void FillChildren(byte[] bytes, int offset) {
			var auxiliaries = new ArrayList();
			var interfaces = new ArrayList();

			while (offset < bytes.Length) {
				var type = (DescriptorType)bytes[offset + 1];

				if (type == DescriptorType.Interface) {
					var newInterface = new Interface(bytes, offset);
					interfaces.Add(newInterface);
					offset += Interface.LENGTH;
					newInterface.FillChildren(bytes, ref offset);
				}
				else {
					var newAuxiliary = new Auxiliary(bytes, offset);
					auxiliaries.Add(newAuxiliary);
					offset += newAuxiliary.Length;
				}
			}

			this.Interfaces = (Interface[])interfaces.ToArray(typeof(Interface));
			this.AuxiliaryDescriptors = (Auxiliary[])auxiliaries.ToArray(typeof(Auxiliary));
		}
	}

	/// <summary>Interface descriptor.</summary>
	public class Interface : BaseDescriptor {

		/// <summary>The length of the descriptor in bytes.</summary>
		public const byte LENGTH = 9;

		/// <summary>The number of the interface.</summary>
		public byte Number { get; set; }

        /// <summary>The value used to select alternate settings.</summary>
        public byte AlternateSetting { get; set; }

        /// <summary>The number of endpoints for this interface.</summary>
        public byte NumberEndpoints { get; set; }

        /// <summary>The class code.</summary>
        public byte ClassCode { get; set; }

        /// <summary>The subclass code.</summary>
        public byte SubclassCode { get; set; }

        /// <summary>The protocal code.</summary>
        public byte ProtocolCode { get; set; }

        /// <summary>The interface string descriptor index.</summary>
        public byte Index { get; set; }

        /// <summary>Child auxiliary descriptors.</summary>
        public Auxiliary[] AuxiliaryDescriptors { get; set; }

        /// <summary>Child endpoint descriptors.</summary>
        public Endpoint[] Endpoints { get; set; }

        /// <summary>Constructs a new descriptor.</summary>
        /// <param name="buffer">The buffer with which to populate the descriptor.</param>
        public Interface(byte[] buffer)
			: this(buffer, 0) {
		}

		/// <summary>Constructs a new descriptor.</summary>
		/// <param name="buffer">The buffer with which to populate the descriptor.</param>
		/// <param name="offset">The offset into the buffer at which to start.</param>
		public Interface(byte[] buffer, int offset)
			: this() {
			if (this.Length != buffer[offset]) throw new ArgumentException("Invalid length received for a USB descriptor.", "buffer");
			if (this.Type != (Descriptors.DescriptorType)buffer[offset + 1]) throw new ArgumentException("Invalid type received for a USB descriptor.", "buffer");

			this.Number = buffer[offset + 2];
			this.AlternateSetting = buffer[offset + 3];
			this.NumberEndpoints = buffer[offset + 4];
			this.ClassCode = buffer[offset + 5];
			this.SubclassCode = buffer[offset + 6];
			this.ProtocolCode = buffer[offset + 7];
			this.Index = buffer[offset + 8];
		}

		/// <summary>Constructs a new descriptor.</summary>
		protected Interface()
			: base(DescriptorType.Interface, Interface.LENGTH) {
		}

		internal void FillChildren(byte[] bytes, ref int offset) {
			var auxiliaries = new ArrayList();
			var endpoints = new ArrayList();

			while (offset < bytes.Length) {
				var type = (DescriptorType)bytes[offset + 1];

				if (type == DescriptorType.Interface) {
					break;
				}
				else if (type == DescriptorType.Endpoint) {
					var newEndpoint = new Endpoint(bytes, offset);
					endpoints.Add(newEndpoint);
					offset += Endpoint.LENGTH;
					newEndpoint.FillChildren(bytes, ref offset);
				}
				else {
					var newAuxiliary = new Auxiliary(bytes, offset);
					auxiliaries.Add(newAuxiliary);
					offset += newAuxiliary.Length;
				}
			}

			this.Endpoints = (Endpoint[])endpoints.ToArray(typeof(Endpoint));
			this.AuxiliaryDescriptors = (Auxiliary[])auxiliaries.ToArray(typeof(Auxiliary));
		}
	}

	/// <summary>Endpoint descriptor.</summary>
	public class Endpoint : BaseDescriptor {

		/// <summary>The length of the descriptor in bytes.</summary>
		public const byte LENGTH = 7;

		/// <summary>The endpoint address.</summary>
		public byte Address { get; set; }

        /// <summary>The attributes of the endpoint.</summary>
        public byte Attributes { get; set; }

        /// <summary>The maximum packet size this endpoint can transmit.</summary>
        public ushort MaximumPacketSize { get; set; }

        /// <summary>The interval for data transfer pooling in frame counts.</summary>
        public byte Interval { get; set; }

        /// <summary>Child auxiliary descriptors.</summary>
        public Auxiliary[] AuxiliaryDescriptors { get; set; }

        /// <summary>Constructs a new descriptor.</summary>
        /// <param name="buffer">The buffer with which to populate the descriptor.</param>
        public Endpoint(byte[] buffer)
			: this(buffer, 0) {
		}

		/// <summary>Constructs a new descriptor.</summary>
		/// <param name="buffer">The buffer with which to populate the descriptor.</param>
		/// <param name="offset">The offset into the buffer at which to start.</param>
		public Endpoint(byte[] buffer, int offset)
			: this() {
			if (this.Length != buffer[offset]) throw new ArgumentException("Invalid length received for a USB descriptor.", "buffer");
			if (this.Type != (Descriptors.DescriptorType)buffer[offset + 1]) throw new ArgumentException("Invalid type received for a USB descriptor.", "buffer");

			this.Address = buffer[offset + 2];
			this.Attributes = buffer[offset + 3];
			this.MaximumPacketSize = (ushort)(buffer[offset + 4] | (buffer[offset + 5] << 8));
			this.Interval = buffer[offset + 6];
		}

		/// <summary>Constructs a new descriptor.</summary>
		protected Endpoint()
			: base(DescriptorType.Endpoint, Endpoint.LENGTH) {
		}

		internal void FillChildren(byte[] bytes, ref int offset) {
			var auxiliaries = new ArrayList();

			while (offset < bytes.Length) {
				var type = (DescriptorType)bytes[offset + 1];

				if (type == DescriptorType.Interface || type == DescriptorType.Endpoint) {
					break;
				}
				else {
					var newAuxiliary = new Auxiliary(bytes, offset);
					auxiliaries.Add(newAuxiliary);
					offset += newAuxiliary.Length;
				}
			}

			this.AuxiliaryDescriptors = (Auxiliary[])auxiliaries.ToArray(typeof(Auxiliary));
		}
	}

	/// <summary>Represents additional auxiliary descriptors.</summary>
	public class Auxiliary : BaseDescriptor {

		/// <summary>The payload of the descriptor excluding length and type.</summary>
		public byte[] Payload { get; set; }

        /// <summary>Constructs a new descriptor.</summary>
        /// <param name="buffer">The buffer with which to populate the descriptor.</param>
        public Auxiliary(byte[] buffer)
			: this(buffer, 0) {
		}

		/// <summary>Constructs a new descriptor.</summary>
		/// <param name="buffer">The buffer with which to populate the descriptor.</param>
		/// <param name="offset">The offset into the buffer at which to start.</param>
		public Auxiliary(byte[] buffer, int offset)
			: base((Descriptors.DescriptorType)buffer[offset + 1], buffer[offset]) {
			this.Payload = new byte[buffer[offset] - 2];

			Array.Copy(buffer, offset + 2, this.Payload, 0, this.Payload.Length);
		}
	}
}
