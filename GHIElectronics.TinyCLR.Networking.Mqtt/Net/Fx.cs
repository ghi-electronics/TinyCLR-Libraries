using System.Threading;

namespace GHIElectronics.TinyCLR.Networking.Mqtt
{
    /// <summary>
    /// Support methods fos specific framework
    /// </summary>
    public class Fx
    {
        public static void StartThread(ThreadStart threadStart) => new Thread(threadStart).Start();

        public static void SleepThread(int millisecondsTimeout) => Thread.Sleep(millisecondsTimeout);
    }
}
