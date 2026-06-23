namespace AppIt.Core.DTOs
{
    public class AgentProductPriceReadDto
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string? ProductType { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal? NetRate { get; set; }
        public decimal? RackRate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsAgentApproved { get; set; }
        public bool IsVerified { get; set; }
        public int? YearEffected { get; set; }
        public string? ApprovalKey { get; set; }
    }

    public class CreateAgentProductPriceDto
    {
        public int? CompanyId { get; set; }
        public string? ProductType { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal? NetRate { get; set; }
        public decimal? RackRate { get; set; }
        public int? YearEffected { get; set; }
    }

    public class AgentApprovalDto
    {
        public string ApprovalKey { get; set; } = string.Empty;
        public string? AgentSignature { get; set; }
    }
}
