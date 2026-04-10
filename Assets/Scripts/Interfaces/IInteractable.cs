namespace AntiBullyingGame.Interfaces
{
    /// <summary>
    /// Interfaz que implementarán todos los objetos y personajes con los que el jugador pueda interactuar 
    /// en el modo RPG (estudiantes, profesores, objetos).
    /// </summary>
    public interface IInteractable
    {
        // Método que se llamará cuando el jugador decida interactuar con la entidad
        void Interact();
    }
}
