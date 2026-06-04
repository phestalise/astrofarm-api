namespace AstroFarm.Api.Models
{
    public class LeituraDto
    {
        public int IdPropriedade { get; set; }
        public decimal? Ndvi { get; set; }
        public decimal? Temperatura { get; set; }
        public decimal? Umidade { get; set; }
        public decimal? Precipitacao { get; set; }
        public string? FonteSatelite { get; set; }
    }
}