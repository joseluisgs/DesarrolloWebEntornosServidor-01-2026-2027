using Refit;
using _07_Refit.Api;
using _07_Refit.Dto;
using _07_Refit.Errors;
using _07_Refit.Mappers;
using _07_Refit.Models;

namespace _07_Refit.Services;

/// <summary>
/// Resultado funcional tipo Result&lt;T, E&gt; para manejo de errores sin excepciones.
/// Patrón ROP (Result Object Pattern).
/// </summary>
public abstract record Result<T, E> where E : DomainError
{
    public sealed record Success(T Value) : Result<T, E>;
    public sealed record Failure(E Error) : Result<T, E>;

    public static Result<T, E> Ok(T value) => new Success(value);
    public static Result<T, E> Fail(E error) => new Failure(error);

    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;
}

/// <summary>
/// Servicio de usuarios que consume JSONPlaceholder usando Refit.
/// Implementa CRUD completo con manejo de errores funcional (Result).
/// </summary>
public class UsuarioService(IJsonPlaceholderApi api)
{
    /// <summary>
    /// Obtiene todos los usuarios de la API.
    /// </summary>
    public async Task<List<Usuario>> GetAllAsync()
    {
        return await api.GetUsuariosAsync();
    }

    /// <summary>
    /// Obtiene un usuario por su ID. Devuelve NotFound si no existe.
    /// </summary>
    public async Task<Result<Usuario, DomainError>> GetByIdAsync(int id)
    {
        try
        {
            var usuario = await api.GetUsuarioByIdAsync(id);
            return usuario is not null
                ? Result<Usuario, DomainError>.Ok(usuario)
                : Result<Usuario, DomainError>.Fail(
                    new DomainError.NotFound("Usuario", id));
        }
        catch (ApiException ex)
        {
            return Result<Usuario, DomainError>.Fail(
                new DomainError.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    /// <summary>
    /// Crea un nuevo usuario. Valida los campos obligatorios antes de enviar.
    /// </summary>
    public async Task<Result<Usuario, DomainError>> CreateAsync(CreateUserRequest request)
    {
        // Validación de campos obligatorios
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<Usuario, DomainError>.Fail(
                new DomainError.ValidationError("Name", "El nombre es obligatorio"));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<Usuario, DomainError>.Fail(
                new DomainError.ValidationError("Email", "El email es obligatorio"));
        }

        try
        {
            var creado = await api.CreateUsuarioAsync(request);
            return Result<Usuario, DomainError>.Ok(creado);
        }
        catch (ApiException ex)
        {
            return Result<Usuario, DomainError>.Fail(
                new DomainError.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    public async Task<Result<Usuario, DomainError>> UpdateAsync(int id, UpdateUserRequest request)
    {
        try
        {
            var actualizado = await api.UpdateUsuarioAsync(id, request);
            return Result<Usuario, DomainError>.Ok(actualizado);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<Usuario, DomainError>.Fail(
                new DomainError.NotFound("Usuario", id));
        }
        catch (ApiException ex)
        {
            return Result<Usuario, DomainError>.Fail(
                new DomainError.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    /// <summary>
    /// Elimina un usuario por su ID.
    /// </summary>
    public async Task<Result<bool, DomainError>> DeleteAsync(int id)
    {
        try
        {
            await api.DeleteUsuarioAsync(id);
            return Result<bool, DomainError>.Ok(true);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<bool, DomainError>.Fail(
                new DomainError.NotFound("Usuario", id));
        }
        catch (ApiException ex)
        {
            return Result<bool, DomainError>.Fail(
                new DomainError.ApiError((int)ex.StatusCode, ex.Message));
        }
    }
}
