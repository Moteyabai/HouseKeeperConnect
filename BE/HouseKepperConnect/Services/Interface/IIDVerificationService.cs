﻿using BusinessObject.Models;

namespace Services.Interface
{
    public interface IIDVerificationService
    {
        Task<List<IDVerification>> GetAllIDVerifysAsync();

        Task<IDVerification> GetIDVerifyByIDAsync(int id);

        Task<int> AddIDVerifyAsync(IDVerification veri);

        Task UpdateIDVerifyAsync(IDVerification veri);
    }
}