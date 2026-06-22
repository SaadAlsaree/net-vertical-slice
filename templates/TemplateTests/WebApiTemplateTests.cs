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
}
