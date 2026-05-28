using System;

namespace AppIt.Core.DTOs
{
    public class ExchangeRateDto
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateExchangeRateDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    public class UpdateExchangeRateDto
    {
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
