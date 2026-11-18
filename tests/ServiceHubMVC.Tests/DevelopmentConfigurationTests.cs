using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using ServiceHubMVC.Configuration;
using ServiceHubMVC.Data;


namespace ServiceHubMVC.Tests;

public sealed class DevelopmentConfigurationTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "servicehub-config-" + Guid.NewGuid().ToString("N"));

    public DevelopmentConfigurationTests() => Directory.CreateDirectory(root);
    public void Dispose() => Directory.Delete(root, recursive: true);

    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "ServiceHubMVC";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private ConfigurationManager Configuration()
    {
        var configuration = new ConfigurationManager();
        configuration.SetBasePath(root);
        return configuration;
    }

    [Fact]
    public void Development_reads_private_file_and_command_line_wins()
    {
        File.WriteAllText(Path.Combine(root, DevelopmentConfiguration.FileName),
            """{"SeedAccounts":{"SharedDemo":{"Enabled":true}},"BootstrapTestValue":"file"}""");
        using var configuration = Configuration();
        configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["BootstrapTestValue"] = "old-secret" });
        DevelopmentConfiguration.AddInternalFile(configuration, new TestEnvironment("Development"), ["--BootstrapTestValue=cli"]);
        Assert.True(configuration.GetValue<bool>("SeedAccounts:SharedDemo:Enabled"));
        Assert.Equal("cli", configuration["BootstrapTestValue"]);
    }

    [Fact]
    public void Production_does_not_read_even_a_malformed_private_file()
    {
        File.WriteAllText(Path.Combine(root, DevelopmentConfiguration.FileName), "broken secret json");
        using var configuration = Configuration();
        DevelopmentConfiguration.AddInternalFile(configuration, new TestEnvironment("Production"), []);
        Assert.Null(configuration["SeedAccounts:SharedDemo:Enabled"]);
    }

    [Fact]
    public void Missing_file_is_optional_and_invalid_file_does_not_echo_its_contents()
    {
        using var configuration = Configuration();
        DevelopmentConfiguration.AddInternalFile(configuration, new TestEnvironment("Development"), []);
        File.WriteAllText(Path.Combine(root, DevelopmentConfiguration.FileName), "private-value invalid json");
        var error = Assert.Throws<InvalidOperationException>(() => DevelopmentConfiguration.AddInternalFile(configuration, new TestEnvironment("Development"), []));
        Assert.DoesNotContain("private-value", error.ToString());
    }

    internal static IConfiguration DemoConfiguration(string? ownerEmail = null, string? password = null)
    {
        // Per-test ephemeral credentials, never shared team credentials.
        var secret = password ?? "Aa1!" + Guid.NewGuid().ToString("N");
        return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SeedAccounts:SharedDemo:Enabled"] = "true",
            ["SeedAccounts:SharedDemo:AdminPassword"] = secret,
            ["SeedAccounts:SharedDemo:OwnerPassword"] = secret,
            ["SeedAccounts:SharedDemo:CustomerPassword"] = secret,
            ["SeedAccounts:SharedDemo:OwnerEmail"] = ownerEmail ?? "owner@servicehub.test"
        }).Build();
    }

    [Fact]
    public void Shared_demo_validates_count_duplicates_and_placeholders()
    {
        Assert.Equal(7, SharedDemoAccounts.Read(DemoConfiguration()).Length);
        Assert.Throws<InvalidOperationException>(() => SharedDemoAccounts.Read(DemoConfiguration("ADMIN1@servicehub.test")));
        Assert.Throws<InvalidOperationException>(() => SharedDemoAccounts.Read(DemoConfiguration(password: "<PRIVATE>")));
        var invalid = new ConfigurationBuilder().AddConfiguration(DemoConfiguration())
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SeedAccounts:SharedDemo:AdminEmails:0"] = "one@test.invalid" }).Build();
        Assert.Throws<InvalidOperationException>(() => SharedDemoAccounts.Read(invalid));
    }
}
