using AccidentesMadrid.Models;

namespace AccidentesMadrid.Repositories;

public interface IAccidentesRepository
{
    Task<IReadOnlyList<Accidente>> GetAllAsync();
}
