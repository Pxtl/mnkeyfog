namespace MnkeyFog.Model;

/// <summary>
/// Apply to a class to mark it as legal for the KnownTypes serializer.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class ModelSerializableAttribute : Attribute {   
    //internal constructor ensures only classes in this lib can make this.
    internal ModelSerializableAttribute() { }
}