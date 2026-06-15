// Copyright (c) 2020 Rossmann Engineering
// Modified by GHI Electronics, LLC

namespace GHIElectronics.TinyCLR.EthernetIP.Scanner.ObjectLibrary
{
    public class AssemblyObject
    {
        // Private read-only — user shouldn't be able to swap the owning ScannerController
        // mid-session. Previously this was a `public` mutable field that named the
        // upstream library ("eeipClient"); both characteristics removed in Phase 3.
        private readonly ScannerController scanner;

        internal AssemblyObject(ScannerController scanner) => this.scanner = scanner;

        /// <summary>
        /// Reads the Instance of the Assembly Object (Instance 101 returns the bytes of the class ID 101)
        /// </summary>
        /// <param name="instanceNo"> Instance number to be returned</param>
        /// <returns>bytes of the Instance</returns>
        public byte[] GetInstance(int instanceNo) => this.scanner.GetAttributeSingle(4, instanceNo, 3);

        /// <summary>
        /// Sets an Instance of the Assembly Object
        /// </summary>
        /// <param name="instanceNo"> Instance number to be returned</param>
        /// <returns>bytes of the Instance</returns>
        public void SetInstance(int instanceNo, byte[] value) => this.scanner.SetAttributeSingle(4, instanceNo, 3, value);

    }
}
