using JetBrains.Application.BuildScript.Application.Zones;

namespace ReSharperPlugin.FluentValidation
{
    [ZoneMarker]
    public class ZoneMarker : IRequire<IFluentValidationZone>
    {
    }
}