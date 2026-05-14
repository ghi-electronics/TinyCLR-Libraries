namespace System.Runtime.CompilerServices {
    // The C# compiler emits `modreq(IsVolatile)` on the field type of every
    // `volatile`-declared field. Without this type in mscorlib the compiler
    // errors with CS0518 ("Predefined type 'System.Runtime.CompilerServices.IsVolatile'
    // is not defined or imported"). The type itself is purely a metadata marker -
    // no instances exist, no methods are called.
    //
    // Runtime behavior: TinyCLR's interpreter recognizes the `volatile.` IL
    // prefix (CEE_VOLATILE, opcode 0xFE 0x13) in its opcode table but has no
    // dispatch handler for it, so reads/writes of volatile fields execute as
    // plain ldfld/stfld with no memory barrier. For single-threaded code or
    // thread-to-thread handoff that crosses a kernel yield (Thread.Sleep,
    // Monitor.Enter, etc.) this is fine. For lock-free patterns or tight
    // ISR-to-managed handoffs, explicit synchronization is required.
    public static class IsVolatile { }
}
