using System.Reflection;

namespace ContextualProgramming;

/// <summary>
/// Defines the details of mutual states.
/// </summary>
/// <param name="HostPropertyInfo">The host's mutual property info.</param>
/// <param name="MutualistPropertyName">The name of the mutualist's mutual property.</param>
/// <param name="MutualistPropertyInfo">The mutualist's mutual property info.</param>
public record struct MutualStateInfo(PropertyInfo HostPropertyInfo, string MutualistPropertyName,
        PropertyInfo MutualistPropertyInfo);
