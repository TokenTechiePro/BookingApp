﻿using BookingApp.Data;
using BookingApp.Data.Models;
using BookingApp.DTOs;
using BookingApp.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Repositories
{
    public class ResourcesRepository : IRepositoryAsync<Resource, int>
    {
        ApplicationDbContext dbContext;

        /// <summary>
        /// IQueryable shorthand for DbContext.Resources
        /// </summary>
        readonly IQueryable<Resource> Resources;

        /// <summary>
        /// IQueryable shorthand for only active DbContext.Resources
        /// </summary>
        readonly IQueryable<Resource> ActiveResources;

        /// <summary>
        /// Not found exception factory
        /// </summary>
        CurrentEntryNotFoundException NewNotFoundException => new CurrentEntryNotFoundException("Specified resource not found");

        public ResourcesRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            Resources = dbContext.Resources;
            ActiveResources = Resources.Where(r => r.IsActive == true);
        }

        #region Standard repository operations
        public async Task<IEnumerable<Resource>> GetListAsync() => await Resources.ToListAsync();

        public async Task<Resource> GetAsync(int resourceId)
        {
            if (await Resources.SingleOrDefaultAsync(p => p.ResourceId == resourceId) is Resource resource)
                return resource;
            else
                throw NewNotFoundException;
        }

        public async Task CreateAsync(Resource resource)
        {
            dbContext.Resources.Add(resource);
            await SaveAsync("Resource Creation");
        }

        public async Task UpdateAsync(Resource resource)
        {
            if (! await ExistsAsync(resource))
                throw NewNotFoundException;

            //invalidating the exact properties for updating
            var updatedProps = typeof(ResourceDetailedModdedDto).GetProperties().Select(prop => prop.Name);
            foreach (var propName in updatedProps)
                dbContext.Entry(resource).Property(propName).IsModified = true;

            await SaveAsync("Resource Update");
        }

        public async Task DeleteAsync(int resourceId)
        {
            if (await GetAsync(resourceId) is Resource resource)
            {
                dbContext.Resources.Remove(resource);

                await SaveAsync("Resource Deletion");
            }
            else
                throw NewNotFoundException;
        }

        /// <summary>
        /// Save changes do storage, SAFE.
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbuException)
            {
                Helpers.DbUpdateExceptionTranslator.ReThrow(dbuException);
            }
        }
        /// <summary>
        /// Save changes do storage. SAFE, verbose.
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync(string message)
        {
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbuException)
            {
                Helpers.DbUpdateExceptionTranslator.ReThrow(dbuException, message);
            }
        }
        #endregion

        #region Extensions
        /// <summary>
        /// Checks whether sepcified resource is active.
        /// </summary>
        public async Task<bool> IsActiveAsync(int resourceId)
        {
            var result = await Resources.Where(r => r.ResourceId == resourceId).Select(r => new { r.IsActive }).SingleOrDefaultAsync();

            if (result != null)
                return result.IsActive == true;
            else
                throw NewNotFoundException;
        }

        /// <summary>
        /// Lists active resources.
        /// </summary>
        public async Task<IEnumerable<Resource>> ListActiveAsync() => await ActiveResources.ToListAsync();

        /// <summary>
        /// Lists identifiers of all resources.
        /// </summary>
        public async Task<IEnumerable<int>> ListIDsAsync() =>        await Resources.Select(r => r.ResourceId).ToListAsync();

        /// <summary>
        /// Lists identifiers of all active resources.
        /// </summary>
        public async Task<IEnumerable<int>> ListActiveIDsAsync() =>  await ActiveResources.Select(r => r.ResourceId).ToListAsync();

        /// <summary>
        /// Lists all resources adhering to the specified rule.
        /// </summary>
        public async Task<IEnumerable<Resource>> ListByRuleAsync(int ruleId) => await Resources.Where(r => r.RuleId == ruleId).ToListAsync();

        /// <summary>
        /// Lists all resources which have the specified user as a creator or updater.
        /// </summary>
        public async Task<IEnumerable<Resource>> ListByAssociatedUser(string userId) => await Resources.Where(r => r.CreatedUserId == userId || r.UpdatedUserId == userId).ToListAsync();

        /// <summary>
        /// Checks whether specified resource exists.
        /// </summary>
        public async Task<bool> ExistsAsync(int resourceId) => await Resources.AnyAsync(e => e.ResourceId == resourceId);

        /// <summary>
        /// Checks whether specified resource exists.
        /// </summary>
        public async Task<bool> ExistsAsync(Resource resource) => await ExistsAsync(resource.ResourceId);
        #endregion
    }
}