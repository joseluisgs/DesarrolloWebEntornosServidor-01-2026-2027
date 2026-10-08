using _10_RepositorioRemoto.Cache;
using _10_RepositorioRemoto.Dto;
using _10_RepositorioRemoto.Notifications;
using _10_RepositorioRemoto.Services;

namespace _10_RepositorioRemoto.Infrastructure;

/// <summary>
/// Aplicación principal que demuestra todos los flujos de caché:
/// Cache HIT, Cache MISS → BD, Cache MISS → BD MISS → API remota.
/// </summary>
public class App(
    IUserService userService,
    INotificationService notifications)
{
    /// <summary>
    /// Ejecuta la demostración completa del repositorio remoto.
    /// </summary>
    public async Task RunAsync()
    {
        using var subscription = notifications.Events.Subscribe(evt =>
        {
            var mensaje = evt.Type switch
            {
                UserEventType.Created => $"  🟢 [NOTIFICACIÓN] Usuario creado: ID={evt.UserId}, Nombre={evt.UserName}",
                UserEventType.Updated => $"  🟡 [NOTIFICACIÓN] Usuario actualizado: ID={evt.UserId}, Nombre={evt.UserName}",
                UserEventType.Deleted => $"  🔴 [NOTIFICACIÓN] Usuario eliminado: ID={evt.UserId}",
                _ => string.Empty
            };
            Console.WriteLine(mensaje);
        });

        // ============================================================
        // FASE 1: Sincronización inicial
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 1: Sincronización inicial desde API remota");
        Console.WriteLine("═══════════════════════════════════════════════════");
        await userService.SyncFromRemoteAsync();
        Console.WriteLine("  ✅ BD local poblada con 10 usuarios desde jsonplaceholder\n");

        // ============================================================
        // FASE 2: GetAll — Cache MISS → BD (primera vez)
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 2: GetAll — Cache MISS → BD local");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  📋 Buscando todos los usuarios...");
        var usuarios = await userService.GetAllAsync();
        Console.WriteLine($"  ✅ Resultado: {usuarios.Count} usuarios (leídos de BD, guardados en caché)");
        foreach (var u in usuarios.Take(3))
            Console.WriteLine($"     [{u.Id}] {u.Name} ({u.Username})");
        Console.WriteLine("     ...\n");

        // ============================================================
        // FASE 3: GetAll otra vez — Cache HIT
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 3: GetAll — Cache HIT (datos en caché)");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  📋 Buscando todos los usuarios (2ª vez)...");
        var usuarios2 = await userService.GetAllAsync();
        Console.WriteLine($"  ✅ Resultado: {usuarios2.Count} usuarios (salen de caché, NO se consulta BD)\n");

        // ============================================================
        // FASE 4: GetById — Cache MISS → BD
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 4: GetById(1) — Cache MISS → BD local");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  🔍 Buscando usuario ID=1...");
        var user1 = await userService.GetByIdAsync(1);
        if (user1.IsSuccess)
            Console.WriteLine($"  ✅ Encontrado: {user1.Value.Name} - {user1.Value.Email} (de BD, guardado en caché)\n");

        // ============================================================
        // FASE 5: GetById otra vez — Cache HIT
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 5: GetById(1) — Cache HIT");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  🔍 Buscando usuario ID=1 (2ª vez)...");
        var user1b = await userService.GetByIdAsync(1);
        if (user1b.IsSuccess)
            Console.WriteLine($"  ✅ Encontrado: {user1b.Value.Name} (salen de caché, NO se consulta BD)\n");

        // ============================================================
        // FASE 6: GetById inexistente — Cache MISS → BD MISS → API
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 6: GetById(9999) — Cache MISS → BD MISS → API");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  🔍 Buscando usuario ID=9999 (no existe)...");
        var noExiste = await userService.GetByIdAsync(9999);
        if (noExiste.IsFailure)
            Console.WriteLine($"  ❌ No encontrado: {noExiste.Error}\n");

        // ============================================================
        // FASE 7: Crear usuario — invalida caché de lista
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 7: POST — Crear usuario (invalida caché)");
        Console.WriteLine("═══════════════════════════════════════════════════");
        var nuevoUsuario = new CreateUserRequest
        {
            Name = "José Luis González",
            Username = "joseluisgs",
            Email = "jose.luis@iesluisvives.es"
        };
        Console.WriteLine("  📝 Creando: José Luis González...");
        var creado = await userService.CreateAsync(nuevoUsuario);
        if (creado.IsSuccess)
            Console.WriteLine($"  ✅ Creado: [{creado.Value.Id}] {creado.Value.Name}");
        Console.WriteLine("  🗑️ Caché de lista invalidada (próxima GetAll irá a BD)\n");

        // ============================================================
        // FASE 8: Validación — error de dominio
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 8: POST — Validación (datos inválidos)");
        Console.WriteLine("═══════════════════════════════════════════════════");
        var invalido = new CreateUserRequest { Name = "", Username = "test", Email = "test@test.com" };
        Console.WriteLine("  📝 Creando usuario con nombre vacío...");
        var validacion = await userService.CreateAsync(invalido);
        if (validacion.IsFailure)
            Console.WriteLine($"  ❌ Error de validación: {validacion.Error}\n");

        // ============================================================
        // FASE 9: Actualizar usuario
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 9: PUT — Actualizar usuario");
        Console.WriteLine("═══════════════════════════════════════════════════");
        var actualizacion = new CreateUserRequest
        {
            Name = "José Luis González Sánchez",
            Username = "joseluisgs",
            Email = "jose.luis@iesluisvives.es"
        };
        Console.WriteLine("  📝 Actualizando ID=1...");
        var actualizado = await userService.UpdateAsync(1, actualizacion);
        if (actualizado.IsSuccess)
            Console.WriteLine($"  ✅ Actualizado: [{actualizado.Value.Id}] {actualizado.Value.Name}\n");

        // ============================================================
        // FASE 10: Eliminar usuario
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 10: DELETE — Eliminar usuario");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  🗑️ Eliminando ID=1...");
        var eliminado = await userService.DeleteAsync(1);
        if (eliminado.IsSuccess)
            Console.WriteLine("  ✅ Eliminado correctamente\n");

        // ============================================================
        // FASE 11: GetAll tras borrado — Cache invalidada
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 11: GetAll tras DELETE — Cache invalidada");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  📋 Buscando todos los usuarios (tras delete)...");
        var usuarios3 = await userService.GetAllAsync();
        Console.WriteLine($"  ✅ Resultado: {usuarios3.Count} usuarios (caché invalidada, se relee de BD)\n");

        // ============================================================
        // FASE 12: Exportar a JSON
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 12: EXPORT — Exportar a JSON");
        Console.WriteLine("═══════════════════════════════════════════════════");
        var ruta = await userService.ExportToJsonAsync();
        Console.WriteLine($"  ✅ Exportados a: {ruta}\n");

        // ============================================================
        // FASE 13: BackgroundService — refrescos periódicos
        // ============================================================
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  FASE 13: BackgroundService — refrescos cada 10s");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  ⏳ Esperando 35 segundos para ver refrescos...\n");

        for (int i = 1; i <= 3; i++)
        {
            await Task.Delay(10_000);
            Console.WriteLine($"  🔄 Refresco #{i} detectado a los {i * 10}s");
        }

        Console.WriteLine("\n═══════════════════════════════════════════════════");
        Console.WriteLine("  Demo completada. BackgroundService sigue corriendo.");
        Console.WriteLine("═══════════════════════════════════════════════════");
    }
}
