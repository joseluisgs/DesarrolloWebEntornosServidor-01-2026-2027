using System.Text.Json;
using _10_RepositorioRemoto.Api;
using _10_RepositorioRemoto.Cache;
using _10_RepositorioRemoto.Dto;
using _10_RepositorioRemoto.Errors;
using _10_RepositorioRemoto.Mappers;
using _10_RepositorioRemoto.Models;
using _10_RepositorioRemoto.Notifications;
using _10_RepositorioRemoto.Repositories;
using _10_RepositorioRemoto.Validators;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace _10_RepositorioRemoto.Services;

/// <summary>
/// Servicio de usuarios con 3 niveles de almacenamiento:
/// caché → BD local → API remota.
/// </summary>
public class UserService(
    IUserRepository repository,
    ICacheService cache,
    IJsonPlaceholderApi api,
    INotificationService notifications,
    ILogger<UserService> logger) : IUserService
{
    private const string CacheKeyAll = "users:all";
    private static string CacheKeyById(int id) => $"users:{id}";

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserResponseDto>> GetAllAsync()
    {
        var cached = await cache.GetAsync<IReadOnlyList<User>>(CacheKeyAll);
        if (cached is not null)
        {
            logger.LogDebug("Cache HIT para GetAll");
            return cached.Select(u => u.ToResponse()).ToList();
        }

        logger.LogDebug("Cache MISS para GetAll, consultando BD local");
        var users = await repository.GetAllAsync();
        if (users.Count > 0)
        {
            await cache.SetAsync(CacheKeyAll, users);
            return users.Select(u => u.ToResponse()).ToList();
        }

        logger.LogDebug("BD vacía, sincronizando desde API remota");
        await SyncFromRemoteAsync();
        users = await repository.GetAllAsync();
        await cache.SetAsync(CacheKeyAll, users);
        return users.ToResponse();
    }

    /// <inheritdoc />
    public async Task<Result<UserResponseDto, DomainError>> GetByIdAsync(int id)
    {
        var cached = await cache.GetAsync<User>(CacheKeyById(id));
        if (cached is not null)
        {
            logger.LogDebug("Cache HIT para GetUserById {Id}", id);
            return cached.ToResponse();
        }

        logger.LogDebug("Cache MISS para GetUserById {Id}, consultando BD local", id);
        var user = await repository.GetByIdAsync(id);
        if (user is not null)
        {
            await cache.SetAsync(CacheKeyById(id), user);
            return user.ToResponse();
        }

        logger.LogDebug("BD MISS para GetUserById {Id}, consultando API remota", id);
        try
        {
            var dto = await api.GetUserByIdAsync(id);
            var created = await repository.CreateAsync(new User
            {
                Id = dto.Id,
                Name = dto.Name,
                Username = dto.Username,
                Email = dto.Email
            });
            await cache.SetAsync(CacheKeyById(id), created);
            await cache.RemoveAsync(CacheKeyAll);
            return created.ToResponse();
        }
        catch
        {
            return Result.Failure<UserResponseDto, DomainError>(
                new DomainError.NotFound("Usuario", id));
        }
    }

    /// <inheritdoc />
    public async Task<Result<UserResponseDto, DomainError>> CreateAsync(CreateUserRequest request)
    {
        var validation = CreateUserRequestValidator.Validate(request);
        if (validation.IsFailure)
            return Result.Failure<UserResponseDto, DomainError>(validation.Error);

        try
        {
            var remoteDto = new JsonPlaceholderUserDto
            {
                Id = 0,
                Name = request.Name,
                Username = request.Username,
                Email = request.Email
            };

            var createdDto = await api.CreateUserAsync(remoteDto);
            var user = new User
            {
                Id = createdDto.Id,
                Name = createdDto.Name,
                Username = createdDto.Username,
                Email = createdDto.Email
            };

            await repository.CreateAsync(user);
            await cache.SetAsync(CacheKeyById(user.Id), user);
            await cache.RemoveAsync(CacheKeyAll);

            notifications.NotifyUserCreated(user.Id, user.Name);
            logger.LogInformation("Usuario creado: {Id} - {Name}", user.Id, user.Name);

            return user.ToResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear usuario");
            return Result.Failure<UserResponseDto, DomainError>(
                new DomainError.ApiError(500, "Error al crear el usuario en la API remota"));
        }
    }

    /// <inheritdoc />
    public async Task<Result<UserResponseDto, DomainError>> UpdateAsync(int id, CreateUserRequest request)
    {
        var validation = CreateUserRequestValidator.Validate(request);
        if (validation.IsFailure)
            return Result.Failure<UserResponseDto, DomainError>(validation.Error);

        try
        {
            var remoteDto = new JsonPlaceholderUserDto
            {
                Id = id,
                Name = request.Name,
                Username = request.Username,
                Email = request.Email
            };

            await api.UpdateUserAsync(id, remoteDto);
            var user = new User
            {
                Id = id,
                Name = request.Name,
                Username = request.Username,
                Email = request.Email
            };

            await repository.UpdateAsync(user);
            await cache.SetAsync(CacheKeyById(id), user);
            await cache.RemoveAsync(CacheKeyAll);

            notifications.NotifyUserUpdated(id, user.Name);
            logger.LogInformation("Usuario actualizado: {Id} - {Name}", id, user.Name);

            return user.ToResponse();
        }
        catch
        {
            return Result.Failure<UserResponseDto, DomainError>(
                new DomainError.NotFound("Usuario", id));
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool, DomainError>> DeleteAsync(int id)
    {
        try
        {
            await api.DeleteUserAsync(id);
            await repository.DeleteAsync(id);
            await cache.RemoveAsync(CacheKeyById(id));
            await cache.RemoveAsync(CacheKeyAll);

            notifications.NotifyUserDeleted(id);
            logger.LogInformation("Usuario eliminado: {Id}", id);

            return true;
        }
        catch
        {
            return Result.Failure<bool, DomainError>(
                new DomainError.NotFound("Usuario", id));
        }
    }

    /// <inheritdoc />
    public async Task<string> ExportToJsonAsync()
    {
        var users = await GetAllAsync();
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        var path = Path.Combine("data", "users_export.json");
        Directory.CreateDirectory("data");
        await File.WriteAllTextAsync(path, json);
        logger.LogInformation("Usuarios exportados a {Path}", path);
        return path;
    }

    /// <inheritdoc />
    public async Task SyncFromRemoteAsync()
    {
        logger.LogInformation("Iniciando sincronización desde API remota...");
        await cache.ClearAsync();

        try
        {
            var remoteUsers = await api.GetUsersAsync();
            var users = remoteUsers.Select(dto => dto.ToModel()).ToList();

            await repository.DeleteAllAsync();
            foreach (var user in users)
            {
                await repository.CreateAsync(user);
            }

            logger.LogInformation("Sincronización completada: {Count} usuarios", users.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante la sincronización");
        }
    }
}
