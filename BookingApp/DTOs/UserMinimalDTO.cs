﻿
namespace BookingApp.DTOs
{
    public class UserMinimalDto
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool? ApprovalStatus { get; set; }
        public bool? IsBlocked { get; set; }
    }
}
