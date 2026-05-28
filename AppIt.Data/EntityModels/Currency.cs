using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Currency
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(60)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
