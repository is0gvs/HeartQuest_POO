using UnityEngine;
namespace AntiBullyingGame.Core
{
    [System.Serializable]
    public struct DialogueOption
    {
        [Tooltip("Texto corto mostrado en el botón del submenú (ej: 'Hablar con calma')")]
        public string label;
        
        [Tooltip("Línea que dice el jugador en el cuadro de diálogo principal")]
        [TextArea(2, 4)]
        public string playerLine;
        
        [Tooltip("Puntos de perdón/empatía que otorga esta opción")]
        public int mercyValue;
        
        [Tooltip("¿Esta opción hace que el enemigo te ataque en su turno?")]
        public bool startsBattle;
        
        [Tooltip("Línea de respuesta del enemigo si la opción inicia combate")]
        public string enemyResponse;
    }
    [CreateAssetMenu(fileName = "NewEnemyCombatData", menuName = "POO Game/Enemy Combat Data")]
    public class EnemyCombatData : ScriptableObject
    {
        [Header("Datos Generales del Enemigo")]
        public string enemyName = "Carlos";
        [Tooltip("ID único usado en el SaveManager para marcar al enemigo como resuelto")]
        public string resolvedSaveId = "bully_carlos_reformed";
        public Sprite enemySprite;
        [Header("Transición Post-Combate")]
        [Tooltip("Escena a cargar al ganar el combate. Si está vacío, vuelve a la escena anterior.")]
        public string nextSceneName = "";
        [Header("Estadísticas de Combate")]
        public float maxHP = 32f;
        public float attackValue = 5f;
        public float defendValue = 0f;
        [Header("Comportamiento de Paciencia / Perdón")]
        public string spareMessage = "* Carlos baja los brazos y decide cambiar.";
        public string[] flavorTexts = {
            "* Intuitivamente, sientes la tensión en el aire.",
            "* Carlos parece dudar por un segundo.",
            "* Tratas de mantener la calma."
        };
        [Header("Opciones del submenú HABLAR")]
        public DialogueOption[] hablarOpciones;
        [Header("Minijuego de Esquivar (Corazón)")]
        [Tooltip("Palabras que el enemigo te lanzará en el minijuego")]
        public string[] attackWords = { "Burla", "Rumor", "Insulto", "Empujon", "Amenaza", "Risa" };
        public float minigameDuration = 6.5f;
        public float pelletSpeed = 2.65f;
        public float spawnInterval = 0.4f;
    }
}