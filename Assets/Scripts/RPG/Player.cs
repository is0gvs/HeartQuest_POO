using UnityEngine;

namespace AntiBullyingGame.RPG
{
    /// <summary>
    /// Clase del Jugador principal.
    /// Hereda de Character (POO: Herencia).
    /// </summary>
    public class Player : Character
    {
        [Header("Player Settings")]
        [SerializeField] private float moveSpeed = 5f;
        private Animator animator;
        private float lastMoveX = 0;
        private float lastMoveY = -1; // Default looking down

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        // Sobreescritura del método Initialize (POO: Polimorfismo / Override)
        public override void Initialize()
        {
            base.Initialize(); // Llama a la lógica de Initialize del padre (GameEntity)
            
            if (string.IsNullOrEmpty(entityName))
            {
                entityName = "Protagonista Estudiante";
            }
            Debug.Log("Jugador configurado y listo para explorar la escuela.");
        }

        private void Update()
        {
            // El Update de Unity se llama en cada frame
            HandleMovement();
            HandleInteractionInput();
        }

        private void HandleMovement()
        {
            // Movimiento usando WASD
            float moveX = 0f;
            float moveY = 0f;

            if (Input.GetKey(KeyCode.W)) moveY = 1f;
            if (Input.GetKey(KeyCode.S)) moveY = -1f;
            if (Input.GetKey(KeyCode.A)) moveX = -1f;
            if (Input.GetKey(KeyCode.D)) moveX = 1f;

            Vector3 moveDir = new Vector3(moveX, moveY, 0).normalized;
            
            // Actualizar Animator
            if (animator != null)
            {
                bool isMoving = moveDir != Vector3.zero;
                
                if (isMoving)
                {
                    lastMoveX = moveX;
                    lastMoveY = moveY;
                }

                animator.SetFloat("moveX", isMoving ? moveX : lastMoveX);
                animator.SetFloat("moveY", isMoving ? moveY : lastMoveY);
                animator.SetBool("isWalking", isMoving);
            }

            if (moveDir != Vector3.zero)
            {
                // Usamos el método heredado de la clase padre Character
                Move(moveDir, moveSpeed);
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(KeyCode.E)) // Presionar 'E' para hablar/interactuar
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            // Aquí en un futuro lanzaremos un raycast o buscaremos objetos cercanos con la Interfaz IInteractable
            Debug.Log("El jugador está tratando de interactuar con lo que tiene enfrente...");
        }
    }
}
