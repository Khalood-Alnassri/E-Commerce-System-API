namespace E_Commerce_System_API.DTOs
{
    public class OrderDetailsDTO
    {
        public int OId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderProductsDetailsDTO> Products { get; set; }
    }
}
