using BookingWepApp.Data;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingWepApp
{
    public class MongoRoleStore : IRoleStore<IdentityRole>
    {
        private readonly IMongoCollection<IdentityRole> _rolesCollection;

        public MongoRoleStore(MongoDbContext context)
        {
            _rolesCollection = context.Roles ?? throw new ArgumentNullException(nameof(context.Roles)); // Correct MongoDbContext's property reference
        }


        public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _rolesCollection.InsertOneAsync(role, cancellationToken: cancellationToken)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        return IdentityResult.Failed(new IdentityError { Description = "Could not create role." });
                    return IdentityResult.Success;
                });
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var result = await _rolesCollection.ReplaceOneAsync(
                r => r.Id == role.Id,
                role,
                cancellationToken: cancellationToken);

            return result.MatchedCount > 0 ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var result = await _rolesCollection.DeleteOneAsync(
                r => r.Id == role.Id,
                cancellationToken);

            return result.DeletedCount > 0 ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await _rolesCollection.Find(r => r.Id == roleId)
                                         .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _rolesCollection.Find(r => r.NormalizedName == normalizedRoleName)
                                         .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            role.Name = roleName;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Nothing to dispose for now
        }
    }
}
