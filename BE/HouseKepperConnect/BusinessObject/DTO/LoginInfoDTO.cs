﻿namespace BusinessObject.DTO
{
    public class LoginInfoDTO
    {
        public int AccountID { get; set; }
        public string FullName { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string Token { get; set; }
    }
}