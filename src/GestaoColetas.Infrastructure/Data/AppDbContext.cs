using GestaoColetas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestaoColetas.Infrastructure.Data;

/// <summary>
/// Ponte entre o código e o banco de dados (SQL Server).
/// Cada DbSet vira uma tabela; o OnModelCreating define os detalhes do mapeamento.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<SolicitacaoColeta> Coletas => Set<SolicitacaoColeta>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Motorista> Motoristas => Set<Motorista>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).IsRequired().HasMaxLength(150);
            e.Property(c => c.Documento).HasMaxLength(20);
        });

        modelBuilder.Entity<Motorista>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Nome).IsRequired().HasMaxLength(150);
            e.Property(m => m.Cnh).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Veiculo>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Placa).IsRequired().HasMaxLength(10);
            e.Property(v => v.Modelo).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Ocorrencia>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Descricao).IsRequired().HasMaxLength(500);
            e.Property(o => o.UsuarioResponsavel).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<SolicitacaoColeta>(e =>
        {
            e.HasKey(s => s.Id);

            e.Property(s => s.Numero).IsRequired().HasMaxLength(30);
            e.HasIndex(s => s.Numero).IsUnique(); // número de identificação único

            e.Property(s => s.RemetenteNome).IsRequired().HasMaxLength(150);
            e.Property(s => s.RemetenteEndereco).IsRequired().HasMaxLength(250);
            e.Property(s => s.DestinatarioNome).IsRequired().HasMaxLength(150);
            e.Property(s => s.DestinatarioEndereco).IsRequired().HasMaxLength(250);
            e.Property(s => s.Observacoes).HasMaxLength(1000);

            // Guarda os enums como texto legível no banco ("Aberta", "Alta"...) em vez de números.
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(s => s.Prioridade).HasConversion<string>().HasMaxLength(20);

            // Relacionamentos: uma coleta tem um cliente, e (depois) um motorista e um veículo.
            e.HasOne(s => s.Cliente).WithMany()
                .HasForeignKey(s => s.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Motorista).WithMany()
                .HasForeignKey(s => s.MotoristaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Veiculo).WithMany()
                .HasForeignKey(s => s.VeiculoId).OnDelete(DeleteBehavior.Restrict);

            // Uma coleta tem várias ocorrências (apagar a coleta apaga as ocorrências dela).
            e.HasMany(s => s.Ocorrencias).WithOne()
                .HasForeignKey(o => o.SolicitacaoColetaId).OnDelete(DeleteBehavior.Cascade);

            // Faz o EF usar o campo privado _ocorrencias (a lista é exposta só pra leitura).
            e.Navigation(s => s.Ocorrencias).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
