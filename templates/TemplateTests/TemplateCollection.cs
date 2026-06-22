namespace TemplateTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class TemplateCollection : ICollectionFixture<TemplateFixture>
{
    public const string Name = "Templates";
}
