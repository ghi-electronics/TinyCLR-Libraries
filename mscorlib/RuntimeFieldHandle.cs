namespace System {
    /**
     *  This value type is used for making classlib type safe.
     *
     *  SECURITY : m_ptr cannot be set to anything other than null by untrusted
     *  code.
     *
     *  This corresponds to EE FieldDesc.
     */
    [Serializable()]
    public struct RuntimeFieldHandle {
    }
}


