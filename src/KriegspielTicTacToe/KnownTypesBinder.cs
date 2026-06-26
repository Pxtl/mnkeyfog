using KriegspielTicTacToe.Model.Template;
using System.Reflection;
using System.Runtime.Serialization;

namespace KriegspielTicTacToe;

/// <summary>
/// Serialization binder ensuring that unexpected complex types are not deserialized.
/// </summary>
public class KnownTypesBinder : Newtonsoft.Json.Serialization.ISerializationBinder {
    public static KnownTypesBinder Instance {get;} = new KnownTypesBinder();
    public KnownTypesBinder() {
        foreach(var type in typeof(ModelSerializableAttribute).Assembly.GetTypes()) {
            if(type.GetCustomAttributes<ModelSerializableAttribute>().Any()) {
                KnownTypes.Add(type.Name, type);
            }
        }
    }
    public Dictionary<string, Type> KnownTypes {get;} = new Dictionary<string, Type>();
 
    public Type BindToType(string? assemblyName, string? typeName) {
        var allowedAssembly = typeof(ModelSerializableAttribute).Assembly;
        ArgumentNullException.ThrowIfNull(typeName, nameof(typeName));
        var type = KnownTypes[typeName];
        if(type.Assembly.Equals(allowedAssembly)) {
            return type;
        } else {
            throw new SerializationException($"Only classes within the assembly {allowedAssembly} are permitted.");
        }
    }

    public void BindToName(Type serializedType, out string? assemblyName, out string typeName) {
        assemblyName = null;
        typeName = serializedType.Name;
    }
}
