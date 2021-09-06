namespace System.Threading {
    /**
     * A constant used by methods that take a timeout (Object.Wait, Thread.Sleep
     * etc) to indicate that no timeout should occur.
     *
     * this should become an enum.
     */
    //This class has only static members and does not require serialization.
    public static class Timeout {
        public const int Infinite = -1;
        public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);
    }

}


