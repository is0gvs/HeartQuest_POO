using UnityEngine;
using AntiBullyingGame.Core;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// Clase que representa a cualquier personaje vivo (Jugador o NPCs).
    /// Hereda de GameEntity (POO: Herencia simple).
    /// </summary>
    public abstract class Character : GameEntity
    {
        [Header("Character Stats")]
        [SerializeField] protected int morale = 50; // Atributo protegido

        // Propiedad pública de lectura
        public int Morale => morale;

        // Método virtual que puede cambiar la forma de moverse en el futuro (POO: Polimorfismo)
        public virtual void Move(Vector3 direction, float speed)
        {
            // Lógica base de movimiento usando Transform de Unity
            transform.Translate(direction * speed * Time.deltaTime);
        }

        public virtual void Speak(string message)
        {
            Debug.Log($"[{entityName}] dice: {message}");
        }

        // Método para modificar la moral interna del personaje de forma controlada (POO: Encapsulamiento)
        public void ChangeMorale(int amount)
        {
            morale += amount;
            morale = Mathf.Clamp(morale, 0, 100); // Evitar que la moral sea menor a 0 o mayor a 100
            Debug.Log($"{entityName} cambió su moral a {morale}");
        }
    }
}
