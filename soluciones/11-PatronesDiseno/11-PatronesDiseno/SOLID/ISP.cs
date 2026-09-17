// ============================================================
// SOLID - I: Interface Segregation Principle (ISP)
// Ningún cliente debería verse forzado a depender de métodos
// que no usa. Interfaces pequeñas y específicas.
// ============================================================

namespace _11_PatronesDiseno.SOLID;

// ❌ MALO: Interfaz gigante — un Repositorio de solo lectura
// se ve forzado a implementar métodos de escritura
public interface IRepositoryMal<T>
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Create(T entity);
    void Update(T entity);
    void Delete(int id);
    void SaveChanges();
    IEnumerable<T> Find(Func<T, bool> predicate);
    void BulkInsert(IEnumerable<T> entities);
    // Un repositorio de solo lectura NO debería implementar
    // Create, Update, Delete, SaveChanges, BulkInsert...
}

// ✅ BUENO: Interfaces pequeñas y específicas
public interface IReadRepository<T>
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    IEnumerable<T> Find(Func<T, bool> predicate);
}

public interface IWriteRepository<T>
{
    void Create(T entity);
    void Update(T entity);
    void Delete(int id);
}

public interface IUnitOfWork
{
    void SaveChanges();
}

// Un repositorio de solo lectura implementa solo lo que necesita
public class PersonaReadOnlyRepository : IReadRepository<Models.Persona>
{
    private readonly List<Models.Persona> _personas = [];

    public Models.Persona? GetById(int id) => _personas.FirstOrDefault(p => p.Id == id);
    public IEnumerable<Models.Persona> GetAll() => _personas;
    public IEnumerable<Models.Persona> Find(Func<Models.Persona, bool> predicate) =>
        _personas.Where(predicate);
}

// Un repositorio completo implementa todo
public class PersonaFullRepository : IReadRepository<Models.Persona>, IWriteRepository<Models.Persona>, IUnitOfWork
{
    private readonly List<Models.Persona> _personas = [];

    public Models.Persona? GetById(int id) => _personas.FirstOrDefault(p => p.Id == id);
    public IEnumerable<Models.Persona> GetAll() => _personas;
    public IEnumerable<Models.Persona> Find(Func<Models.Persona, bool> predicate) =>
        _personas.Where(predicate);
    public void Create(Models.Persona entity) => _personas.Add(entity);
    public void Update(Models.Persona entity) { /* update */ }
    public void Delete(int id) => _personas.RemoveAll(p => p.Id == id);
    public void SaveChanges() { /* save */ }
}
