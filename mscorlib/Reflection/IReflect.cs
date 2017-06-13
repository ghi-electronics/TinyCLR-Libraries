namespace System.Reflection {
    // Interface does not need to be marked with the serializable attribute
    public interface IReflect {
        MethodInfo GetMethod(string name, BindingFlags bindingAttr);
        FieldInfo GetField(string name, BindingFlags bindingAttr);
    }
}


