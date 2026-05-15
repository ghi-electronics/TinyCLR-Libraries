using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Native {
    /// <summary>
    /// CPU interrupt mask. Wrap a tight critical section in
    /// <see cref="Disable"/>/<see cref="Enable"/> to prevent context switches and
    /// hardware interrupts from running during it. Keep the disabled window short —
    /// while interrupts are off, RTOS threads cannot preempt and ISR latencies grow.
    /// </summary>
    public static class Interrupt {
        /// <summary>Re-enables interrupts after a matching <see cref="Disable"/>.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Enable();

        /// <summary>Disables interrupts on the current core. Always pair with <see cref="Enable"/>.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Disable();

        /// <summary>True when interrupts are currently masked.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsDisabled();

    }
}
