﻿using BusinessObject.Models;
using DataAccess;
using Repositories.Interface;

namespace Repositories
{
    public class FamilyProfileRepository : IFamilyProfileRepository
    {
        public async Task<List<Family>> GetAllFamilysAsync() => await FamilyProfileDAO.Instance.GetAllFamilysAsync();

        public async Task<Family> GetFamilyByIDAsync(int fID) => await FamilyProfileDAO.Instance.GetFamilyByIDAsync(fID);

        public async Task AddFamilyAsync(Family Family) => await FamilyProfileDAO.Instance.AddFamilyAsync(Family);

        public async Task DeleteFamilyAsync(int id) => await FamilyProfileDAO.Instance.DeleteFamilyAsync(id);

        public async Task UpdateFamilyAsync(Family Family) => FamilyProfileDAO.Instance.UpdateFamilyAsync(Family);

        public async Task<List<Family>> SearchFamilysByNameAsync(string name) => await FamilyProfileDAO.Instance.SearchFamilysByNameAsync(name);

        public async Task<List<Family>> SearchFamiliesByAccountIDAsync(int accountId) => await FamilyProfileDAO.Instance.SearchFamiliesByAccountIDAsync(accountId);

        public async Task<Family> GetFamilyByAccountIDAsync(int accountId) => await FamilyProfileDAO.Instance.GetFamilyByAccountIDAsync(accountId);
    }
}