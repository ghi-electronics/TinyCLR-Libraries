using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace GHIElectronics.TinyCLR.Drivers.EthernetIP.ObjectLibrary
{
    public class AssemblyObject
    {
        public EthernetIPClient eeipClient;

        /// <summary>
        /// Constructor. </summary>
        /// <param name="eeipClient"> EthernetIPClient Object</param>
        public AssemblyObject(EthernetIPClient eeipClient) => this.eeipClient = eeipClient;

        /// <summary>
        /// Reads the Instance of the Assembly Object (Instance 101 returns the bytes of the class ID 101)
        /// </summary>
        /// <param name="instanceNo"> Instance number to be returned</param>
        /// <returns>bytes of the Instance</returns>
        public byte[] GetInstance(int instanceNo)
        {
            
                var byteArray = this.eeipClient.GetAttributeSingle(4, instanceNo, 3);
                return byteArray;
        }

        /// <summary>
        /// Sets an Instance of the Assembly Object
        /// </summary>
        /// <param name="instanceNo"> Instance number to be returned</param>
        /// <returns>bytes of the Instance</returns>
        public void SetInstance(int instanceNo, byte[] value) => this.eeipClient.SetAttributeSingle(4, instanceNo, 3, value);

    }
}
