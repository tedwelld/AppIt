using System.Collections.Generic;

namespace AppIt.Core.DTOs
{
    public class AccommodationAvailabilityDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<string> Types { get; set; } = new();
        public List<DayAvailabilityDto> Days { get; set; } = new();
    }

    public class DayAvailabilityDto
    {
        public int Day { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public bool InMonth { get; set; }
        public List<TypeAvailabilityDto> TypeAvailability { get; set; } = new();
    }

    public class TypeAvailabilityDto
    {
        public string Type { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public int MinGuestCapacity { get; set; }
        public int MaxGuestCapacity { get; set; }
        public int Booked { get; set; }
        public int Available { get; set; }
    }
}
