using AstroFarm.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroFarm.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Produtor> Produtores => Set<Produtor>();
        public DbSet<Propriedade> Propriedades => Set<Propriedade>();
        public DbSet<Cultura> Culturas => Set<Cultura>();
        public DbSet<LeituraSatelital> LeiturasSatelitais => Set<LeituraSatelital>();
        public DbSet<Alerta> Alertas => Set<Alerta>();
        public DbSet<HistoricoClima> HistoricoClima => Set<HistoricoClima>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ❌ REMOVIDO (causa erro em Oracle)
            // modelBuilder.HasDefaultSchema("SYSTEM");

            modelBuilder.Entity<Produtor>(entity =>
            {
                entity.ToTable("PRODUTOR");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("ID_PRODUTOR")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Nome)
                    .HasColumnName("NOME")
                    .IsRequired();

                entity.Property(e => e.Cpf)
                    .HasColumnName("CPF")
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasColumnName("EMAIL")
                    .HasMaxLength(200);

                entity.Property(e => e.Senha)
                    .HasColumnName("SENHA")
                    .HasMaxLength(200);

                entity.Property(e => e.Telefone)
                    .HasColumnName("TELEFONE")
                    .HasMaxLength(20);

                entity.Property(e => e.Estado)
                    .HasColumnName("ESTADO")
                    .IsRequired()
                    .HasMaxLength(2);

                entity.Property(e => e.Cidade)
                    .HasColumnName("CIDADE")
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.DtCadastro)
                    .HasColumnName("DT_CADASTRO");
            });

            modelBuilder.Entity<Propriedade>(entity =>
            {
                entity.ToTable("PROPRIEDADE");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("ID_PROPRIEDADE")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdProdutor).HasColumnName("ID_PRODUTOR");
                entity.Property(e => e.NomeFazenda).HasColumnName("NOME_FAZENDA");
                entity.Property(e => e.AreaHectares).HasColumnName("AREA_HECTARES");
                entity.Property(e => e.Latitude).HasColumnName("LATITUDE");
                entity.Property(e => e.Longitude).HasColumnName("LONGITUDE");
                entity.Property(e => e.Estado).HasColumnName("ESTADO");
                entity.Property(e => e.Municipio).HasColumnName("MUNICIPIO");
                entity.Property(e => e.DtRegistro).HasColumnName("DT_REGISTRO");
            });

            modelBuilder.Entity<Cultura>(entity =>
            {
                entity.ToTable("CULTURA");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("ID_CULTURA")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdPropriedade).HasColumnName("ID_PROPRIEDADE");
                entity.Property(e => e.TipoCultura).HasColumnName("TIPO_CULTURA");
                entity.Property(e => e.Safra).HasColumnName("SAFRA");
                entity.Property(e => e.AreaPlantada).HasColumnName("AREA_PLANTADA");
                entity.Property(e => e.DtPlantio).HasColumnName("DT_PLANTIO");
                entity.Property(e => e.DtColheitaPrev).HasColumnName("DT_COLHEITA_PREV");
                entity.Property(e => e.Status).HasColumnName("STATUS");
            });

            modelBuilder.Entity<LeituraSatelital>(entity =>
            {
                entity.ToTable("LEITURA_SATELITAL");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("ID_LEITURA")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdPropriedade).HasColumnName("ID_PROPRIEDADE");
                entity.Property(e => e.DtLeitura).HasColumnName("DT_LEITURA");
                entity.Property(e => e.Ndvi).HasColumnName("NDVI");
                entity.Property(e => e.Temperatura).HasColumnName("TEMPERATURA");
                entity.Property(e => e.Umidade).HasColumnName("UMIDADE");
                entity.Property(e => e.Precipitacao).HasColumnName("PRECIPITACAO");
                entity.Property(e => e.FonteSatelite).HasColumnName("FONTE_SATELITE");
            });

            modelBuilder.Entity<Alerta>(entity =>
            {
                entity.ToTable("ALERTA");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("ID_ALERTA")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdPropriedade).HasColumnName("ID_PROPRIEDADE");
                entity.Property(e => e.IdLeitura).HasColumnName("ID_LEITURA");
                entity.Property(e => e.TipoAlerta).HasColumnName("TIPO_ALERTA");
                entity.Property(e => e.NivelRisco).HasColumnName("NIVEL_RISCO");
                entity.Property(e => e.Descricao).HasColumnName("DESCRICAO");
                entity.Property(e => e.DtAlerta).HasColumnName("DT_ALERTA");
                entity.Property(e => e.Resolvido).HasColumnName("RESOLVIDO");
                entity.Property(e => e.DtResolucao).HasColumnName("DT_RESOLUCAO");
            });

            modelBuilder.Entity<HistoricoClima>(entity =>
            {
                entity.ToTable("HISTORICO_CLIMA");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("ID_CLIMA")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Estado).HasColumnName("ESTADO");
                entity.Property(e => e.Municipio).HasColumnName("MUNICIPIO");
                entity.Property(e => e.AnoMes).HasColumnName("ANO_MES");
                entity.Property(e => e.TempMedia).HasColumnName("TEMP_MEDIA");
                entity.Property(e => e.TempMax).HasColumnName("TEMP_MAX");
                entity.Property(e => e.TempMin).HasColumnName("TEMP_MIN");
                entity.Property(e => e.PrecipitacaoMm).HasColumnName("PRECIPITACAO_MM");
                entity.Property(e => e.UmidadeMedia).HasColumnName("UMIDADE_MEDIA");
            });

            modelBuilder.Entity<Propriedade>()
                .HasOne(x => x.Produtor)
                .WithMany(x => x.Propriedades)
                .HasForeignKey(x => x.IdProdutor);

            modelBuilder.Entity<Cultura>()
                .HasOne(x => x.Propriedade)
                .WithMany(x => x.Culturas)
                .HasForeignKey(x => x.IdPropriedade);

            modelBuilder.Entity<LeituraSatelital>()
                .HasOne(x => x.Propriedade)
                .WithMany(x => x.Leituras)
                .HasForeignKey(x => x.IdPropriedade);

            modelBuilder.Entity<Alerta>()
                .HasOne(x => x.Propriedade)
                .WithMany(x => x.Alertas)
                .HasForeignKey(x => x.IdPropriedade);

            modelBuilder.Entity<Alerta>()
                .HasOne(x => x.Leitura)
                .WithMany(x => x.Alertas)
                .HasForeignKey(x => x.IdLeitura)
                .IsRequired(false);
        }
    }
}