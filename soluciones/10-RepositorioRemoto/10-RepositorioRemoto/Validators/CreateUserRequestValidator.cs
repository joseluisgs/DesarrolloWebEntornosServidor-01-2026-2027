using _10_RepositorioRemoto.Dto;
using _10_RepositorioRemoto.Errors;
using CSharpFunctionalExtensions;

namespace _10_RepositorioRemoto.Validators;

/// <summary>
/// Validador de CreateUserRequest.
/// Devuelve Result con errores de dominio en vez de lanzar excepciones.
/// </summary>
public static class CreateUserRequestValidator
{
    /// <summary>
    /// Valida que el request de creación de usuario sea correcto.
    /// </summary>
    public static Result<CreateUserRequest, DomainError> Validate(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<CreateUserRequest, DomainError>(
                new DomainError.ValidationError(nameof(request.Name), "El nombre es obligatorio"));

        if (request.Name.Length < 2 || request.Name.Length > 100)
            return Result.Failure<CreateUserRequest, DomainError>(
                new DomainError.ValidationError(nameof(request.Name), "El nombre debe tener entre 2 y 100 caracteres"));

        if (string.IsNullOrWhiteSpace(request.Username))
            return Result.Failure<CreateUserRequest, DomainError>(
                new DomainError.ValidationError(nameof(request.Username), "El username es obligatorio"));

        if (request.Username.Length < 3 || request.Username.Length > 50)
            return Result.Failure<CreateUserRequest, DomainError>(
                new DomainError.ValidationError(nameof(request.Username), "El username debe tener entre 3 y 50 caracteres"));

        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure<CreateUserRequest, DomainError>(
                new DomainError.ValidationError(nameof(request.Email), "El email es obligatorio"));

        if (!request.Email.Contains('@') || !request.Email.Contains('.'))
            return Result.Failure<CreateUserRequest, DomainError>(
                new DomainError.ValidationError(nameof(request.Email), "El email no tiene un formato válido"));

        return Result.Success<CreateUserRequest, DomainError>(request);
    }
}
