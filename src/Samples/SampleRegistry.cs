using System.Reflection;

namespace Osdu.Samples;

/// <summary>
/// Discovers all <see cref="ISample"/> implementations in this assembly via
/// reflection, so adding a sample is just dropping in a new class.
/// </summary>
public static class SampleRegistry
{
    public static IReadOnlyList<ISample> All { get; } = Discover();

    public static ISample? Find(string name) =>
        All.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    private static List<ISample> Discover() =>
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ISample).IsAssignableFrom(t)
                        && t is { IsInterface: false, IsAbstract: false })
            .Select(t => (ISample)Activator.CreateInstance(t)!)
            .OrderBy(s => s.Name, StringComparer.Ordinal)
            .ToList();
}
