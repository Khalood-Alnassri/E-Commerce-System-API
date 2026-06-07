namespace E_Commerce_System_API.DTOs
{
    public class GetOrdersDTO
    {
        public int OId { get; set; }

        public DateTime? OrderDate { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
