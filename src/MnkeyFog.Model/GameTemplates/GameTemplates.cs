
using System.Reflection;
using MnkeyFog.Model.Template;

namespace MnkeyFog.Model;
public static partial class GameTemplates {
    public static IEnumerable<IGameTemplate> GetBuiltInGameTemplates()
        => typeof(GameTemplates)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(propInfo => propInfo.PropertyType.IsAssignableTo(typeof(IGameTemplate)))
            .Select(propInfo => (IGameTemplate)propInfo.GetValue(null)!);
}