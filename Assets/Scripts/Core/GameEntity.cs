using UnityEngine;

namespace AntiBullyingGame.Core
{
    /// <summary>
    /// Clase Base Absoluta (POO: Herencia).
    /// Todos los elementos interactivos o personajes en el juego heredarán de aquí.
    /// </summary>
    public abstract class GameEntity : MonoBehaviour
    {
        [Header("Entity Base Info")]
        [SerializeField] protected string entityName; // Encapsulamiento: protected (accesible aquí y en clases hijas)
        public string EntityName => entityName; // Propiedad pública tipo lectura (Encapsulamiento)

        // Método virtual (POO: Polimorfismo) que puede ser sobreescrito por las clases hijas
        public virtual void Initialize()
        {
            Debug.Log($"Iniciando entidad base: {entityName}");
        }

        protected virtual void Start()
        {
            Initialize(); // Llama a la inicialización al empezar el juego
        }
    }
}
