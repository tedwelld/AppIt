using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class RoleService : IRoleService
    {
        private readonly AppItDbContext _db;

        public RoleService(AppItDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResponse<RoleDto>> CreateAsync(CreateRoleDto dto)
        {
            var response = new ServiceResponse<RoleDto>();
            try
            {
                var role = new Role { Name = dto.Name };
                _db.Roles.Add(role);
                await _db.SaveChangesAsync();

                response.Data = MapToDto(role);
                response.Success = true;
                response.Message = "Role created successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating role: {ex.Message} {ex.InnerException?.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto)
        {
            var response = new ServiceResponse<RoleDto>();
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
                return new ServiceResponse<RoleDto>(null, "Role not found") { Success = false };

            role.Name = dto.Name;
            await _db.SaveChangesAsync();

            response.Data = MapToDto(role);
            response.Success = true;
            response.Message = "Role updated successfully";
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
                return new ServiceResponse<bool>(false, "Role not found") { Success = false };

            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();

            response.Success = true;
            response.Data = true;
            response.Message = "Role deleted successfully";
            return response;
        }

        public async Task<ServiceResponse<List<RoleDto>>> GetAllAsync()
        {
            var roles = await _db.Roles
                .AsNoTracking()
                .ToListAsync();

            var dtos = roles.Select(MapToDto).ToList();

            return new ServiceResponse<List<RoleDto>>(dtos, "Roles retrieved");
        }


        public async Task<ServiceResponse<RoleDto>> GetByIdAsync(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
                return new ServiceResponse<RoleDto>(null, "Role not found") { Success = false };

            return new ServiceResponse<RoleDto>(MapToDto(role), "Role retrieved");
        }

        private static RoleDto MapToDto(Role role) => new RoleDto
        {
            RoleId = role.RoleId,
            Name = role.Name
        };
    }
}
