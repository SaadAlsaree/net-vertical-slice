namespace TemplateTests;

[Collection(TemplateCollection.Name)]
public sealed class FeatureTemplateTests(TemplateFixture fixture)
{
    [Theory]
    [InlineData("Category", "categories")]
    [InlineData("Box", "boxes")]
    public void FeatureTemplateBuildsForIrregularNames(string entityName, string resourceName)
    {
        var root = fixture.CreateTempDirectory($"feature-{entityName.ToLowerInvariant()}");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C OutputCache -Au None --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet(
            $"new vsa-feature -n {entityName} -Re {resourceName} -C Catalog -R TestApi -D AppDbContext --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet($"build \"{Path.Combine(root, "TestApi.csproj")}\" --no-restore", root);
    }
}
