namespace AntiBullyingGame.Interfaces
{
    /// <summary>
    /// Interfaz para cualquier entidad que pueda recibir daño (físico o emocional)
    /// Se utilizará principalmente en el modo Tower Defense.
    /// </summary>
    public interface IDamageable
    {
        // Propiedad de salud actual
        int Health { get; }

        // Método para procesar el daño recibido
        void TakeDamage(int amount);
    }
}
