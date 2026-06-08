namespace E_Commerce_System_API.DTOs
{
    public class FilteringProductDTO
    {
        public string? PName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
