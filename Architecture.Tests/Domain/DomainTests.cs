using SharedKernel;
using StockMarketSimulator.Api;
using System.Reflection;
using Types = NetArchTest.Rules.Types;

namespace Architecture.Tests.Domain;

public sealed class DomainTests
{
    [Fact]
    public void Entities_Should_HavePrivateParameterlessConstructor()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(PresentationAssembly.Instance)
            .That()
            .ImplementInterface(typeof(IEntity))
            .GetTypes();

        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            if (!constructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
            {
                failingTypes.Add(entityType);
            }
        }

        Assert.Empty(failingTypes);
    }
}
