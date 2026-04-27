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

        protected Rigidbody2D rb;

        public override void Initialize()
        {
            base.Initialize();
            rb = GetComponent<Rigidbody2D>();
        }

        // Método virtual que puede cambiar la forma de moverse en el futuro (POO: Polimorfismo)
        public virtual void Move(Vector3 direction, float speed)
        {
            // Movimiento usando Físicas 2D para que las paredes y colisiones funcionen
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
            else
            {
                transform.Translate(direction * speed * Time.deltaTime);
            }
        }

        public virtual void Speak(string message)
        {
            Debug.Log($"[{entityName}] dice: {message}");
            
            // Si el sistema de diálogo Cyberpunk está en la escena, lo usamos
            var dialogueSystem = Object.FindFirstObjectByType<HeartQuest.UI.DialogueSystem>();
            if (dialogueSystem != null)
            {
                dialogueSystem.ShowDialogue($"[{entityName}]: {message}");
            }
        }

        // Método para modificar la moral interna del personaje de forma controlada (POO: Encapsulamiento)
        public void ChangeMorale(int amount)
        {
            morale += amount;
            morale = Mathf.Clamp(morale, 0, 100); // Evitar que la moral sea menor a 0 o mayor a 100
            Debug.Log($"{entityName} cambió su moral a {morale}");
        }

        // Método para establecer la moral directamente (útil al cargar la partida)
        public void SetMorale(int value)
        {
            morale = Mathf.Clamp(value, 0, 100);
        }
    }
}
