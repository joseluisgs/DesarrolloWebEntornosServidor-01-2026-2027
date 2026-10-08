using _10_RepositorioRemoto.Dto;
using _10_RepositorioRemoto.Errors;
using _10_RepositorioRemoto.Models;
using CSharpFunctionalExtensions;

namespace _10_RepositorioRemoto.Services;

/// <summary>
/// Interfaz del servicio de gestión de usuarios.
/// </summary>
public interface IUserService
{
    Task<IReadOnlyList<UserResponseDto>> GetAllAsync();
    Task<Result<UserResponseDto, DomainError>> GetByIdAsync(int id);
    Task<Result<UserResponseDto, DomainError>> CreateAsync(CreateUserRequest request);
    Task<Result<UserResponseDto, DomainError>> UpdateAsync(int id, CreateUserRequest request);
    Task<Result<bool, DomainError>> DeleteAsync(int id);
    Task<string> ExportToJsonAsync();
    Task SyncFromRemoteAsync();
}
