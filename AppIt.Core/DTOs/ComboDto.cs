namespace AppIt.Core.DTOs
{
    public class ComboReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? SupplierId { get; set; }
        public int? ProductCategoryId { get; set; }
        public int MaxProducts { get; set; }
        public bool IsActive { get; set; }
        public List<ComboProductReadDto> Products { get; set; } = new();
    }

    public class ComboProductReadDto
    {
        public int Id { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
    }

    public class CreateComboDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? SupplierId { get; set; }
        public int? ProductCategoryId { get; set; }
        public int MaxProducts { get; set; } = 2;
        public List<CreateComboProductDto> Products { get; set; } = new();
    }

    public class CreateComboProductDto
    {
        public string ServiceType { get; set; } = "Product";
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
    }

    public class UpdateComboDto : CreateComboDto
    {
        public bool IsActive { get; set; } = true;
    }

    public class ReservationServiceItemSplitDto
    {
        public string ServiceType { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public bool IsMandatory { get; set; }
    }
}
