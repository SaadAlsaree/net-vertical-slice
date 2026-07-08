namespace TemplateTests;

[Collection(TemplateCollection.Name)]
public sealed class WebApiTemplateTests(TemplateFixture fixture)
{
    [Fact]
    public void WebApiTemplateBuilds()
    {
        var root = fixture.CreateTempDirectory("webapi");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C OutputCache -Au None --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet($"build \"{Path.Combine(root, "TestApi.csproj")}\" --no-restore", root);
    }

    [Fact]
    public void WebApiTemplateBuildsWithKeycloak()
    {
        var root = fixture.CreateTempDirectory("webapi-keycloak");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D Sqlite -A Scalar -C OutputCache -Au Keycloak --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet($"build \"{Path.Combine(root, "TestApi.csproj")}\" --no-restore", root);
    }

    [Theory]
    [InlineData("PostgreSQL")]
    [InlineData("SqlServer")]
    [InlineData("Sqlite")]
    public void WebApiTemplateBuildsWithIdentity(string dbProvider)
    {
        var root = fixture.CreateTempDirectory($"webapi-identity-{dbProvider}");

        fixture.RunDotnet(
            $"new vsa-webapi -n TestApi -D {dbProvider} -A Scalar -C OutputCache -Au Identity --output \"{root}\" --no-update-check",
            fixture.TemplatesRoot);

        fixture.RunDotnet($"build \"{Path.Combine(root, "TestApi.csproj")}\" --no-restore", root);
    }
}
