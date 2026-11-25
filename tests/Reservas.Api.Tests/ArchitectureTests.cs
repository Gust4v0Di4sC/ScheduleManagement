using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reservas.Api.Tests;

[TestClass]
public sealed class ArchitectureTests
{
    [TestMethod]
    public void DomainNamespaces_DoNotReferencePresentationOrInfrastructureNamespaces()
    {
        var violatingTypes = typeof(Program).Assembly.GetTypes()
            .Where(type => type.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true)
            .Where(type => type.GetMembers()
                .SelectMany(member => GetReferencedTypes(member))
                .Any(IsForbiddenDependency))
            .Select(type => type.FullName)
            .ToArray();

        violatingTypes.Should().BeEmpty();
    }

    [TestMethod]
    public void DomainNamespaces_DoNotReferenceAnotherBusinessModuleDomain()
    {
        var violations = typeof(Program).Assembly.GetTypes()
            .Where(type => type.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true)
            .SelectMany(type => type.GetMembers().SelectMany(GetReferencedTypes)
                .Select(reference => new { Source = type, Target = reference }))
            .Where(pair => IsCrossModuleDomainReference(pair.Source, pair.Target))
            .Select(pair => $"{pair.Source.FullName} -> {pair.Target.FullName}")
            .ToArray();

        violations.Should().BeEmpty();
    }

    private static IEnumerable<Type> GetReferencedTypes(System.Reflection.MemberInfo member) => member switch
    {
        System.Reflection.PropertyInfo property => [property.PropertyType],
        System.Reflection.FieldInfo field => [field.FieldType],
        System.Reflection.MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType)
            .Append(method.ReturnType),
        _ => []
    };

    private static bool IsForbiddenDependency(Type type) =>
        type.Namespace?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true ||
        type.Namespace?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true ||
        type.Namespace?.Contains(".Presentation", StringComparison.Ordinal) == true ||
        type.Namespace?.Contains(".Infrastructure", StringComparison.Ordinal) == true;

    private static bool IsCrossModuleDomainReference(Type source, Type target)
    {
        var sourceModule = GetModule(source.Namespace);
        var targetModule = GetModule(target.Namespace);
        return sourceModule is not null && targetModule is not null &&
               targetModule != "Platform" &&
               !string.Equals(sourceModule, targetModule, StringComparison.Ordinal);
    }

    private static string? GetModule(string? namespaceName)
    {
        const string marker = "Reservas.Api.Modules.";
        if (namespaceName?.StartsWith(marker, StringComparison.Ordinal) != true)
        {
            return null;
        }

        return namespaceName[marker.Length..].Split('.')[0];
    }
}
