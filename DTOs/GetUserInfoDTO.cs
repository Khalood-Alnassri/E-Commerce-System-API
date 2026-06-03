namespace E_Commerce_System_API.DTOs
{
    public class GetUserInfoDTO
    {
        public int UId { get; set; }
        public string UName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}
