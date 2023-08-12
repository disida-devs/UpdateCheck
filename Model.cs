#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UpdateCheckContext : DbContext
{
    public DbSet<Site> Sites { get; set; } = null!;
    public DbSet<Link> Links { get; set; } = null!;
    public DbSet<Param> Params { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // TODO try catch если файла нет 
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        string host = configuration["DBHost"];
        string port = configuration["DBPort"];
        string baseName = configuration["DBName"];
        string user = configuration["DBUser"];
        string password = configuration["DBPassword"];

        optionsBuilder.UseNpgsql($"Host={host};Port={port};Database={baseName};Username={user};Password={password}");
    }
}

[Index(nameof(Site.Name), IsUnique = true)]
[Index(nameof(Site.Url), IsUnique = true)]
public class Site
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "varchar(25)")]
    public string Name { get; set; }

    [Column(TypeName = "text")]
    [Url]
    public string Url { get; set; }
}

public class Link
{
    [Key]
    public int Id { get; set; }

    public Site Site { get; set; }

    [Column(TypeName = "text")]
    [Url]
    public string Url { get; set; }

    public bool Posted { get; set; }
}

public class Param
{
    [Key]
    [Column(TypeName = "text")]
    public string Parameter { get; set; }

    [Column(TypeName = "text")]
    public string Value { get; set; }
}
