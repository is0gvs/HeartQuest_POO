namespace AntiBullyingGame.Core
{
    /// <summary>
    /// Interfaz para el sujeto que será observado (Ej: GameManager).
    /// </summary>
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void NotifyObservers();
    }

    /// <summary>
    /// Interfaz para las clases que mirarán pasivamente los eventos (Ej: Interfaz Gráfica de Usuario).
    /// </summary>
    public interface IObserver
    {
        void OnMoraleUpdated(int currentMorale);
    }
}
