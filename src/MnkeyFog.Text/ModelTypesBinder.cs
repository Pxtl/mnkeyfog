using System.Reflection;
using System.Runtime.Serialization;
using MnkeyFog.Text.Persistence;
using Newtonsoft.Json.Serialization;

namespace MnkeyFog.Text;

/// <summary>
/// Serialization binder for GameState persistence, restricting polymorphic
/// deserialization to types in the MnkeyFog.Model assembly marked
/// <see cref="ModelSerializableAttribute"/>.
/// </summary>
public class ModelTypesBinder : ISerializationBinder {
    public static ModelTypesBinder Instance { get; } = new();

    private readonly Dictionary<string, Type> _knownTypes;

    private ModelTypesBinder() {
        _knownTypes = typeof(ModelSerializableAttribute).Assembly
            .GetTypes()
            .Where(t => t.GetCustomAttributes<ModelSerializableAttribute>().Any())
            .ToDictionary(t => t.Name, StringComparer.Ordinal);

        // Persistence DTOs from this library.
        _knownTypes[nameof(PersistedSession)] = typeof(PersistedSession);
        _knownTypes[nameof(PersistedPlayerDid)] = typeof(PersistedPlayerDid);
    }

    public Type BindToType(string? assemblyName, string? typeName) {
        ArgumentNullException.ThrowIfNull(typeName, nameof(typeName));

        if (_knownTypes.TryGetValue(typeName, out var type)
            && (type.Assembly.Equals(typeof(ModelSerializableAttribute).Assembly)
                || type.Assembly.Equals(typeof(ModelTypesBinder).Assembly))) {
            return type;
        }

        throw new SerializationException($"Type '{typeName}' is not a known serializable MnkeyFog type.");
    }

    public void BindToName(Type serializedType, out string? assemblyName, out string typeName) {
        assemblyName = null;
        typeName = serializedType.Name;
    }
}
