using UnityEngine;
using AntiBullyingGame.Interfaces;

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
        private Vector3 currentMoveDir;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        // Sobreescritura del método Initialize (POO: Polimorfismo / Override)
        public override void Initialize()
        {
            base.Initialize(); // Llama a la lógica de Initialize del padre
            
            if (string.IsNullOrEmpty(entityName))
            {
                entityName = "Protagonista Estudiante";
            }
            Debug.Log("Jugador configurado y listo para explorar la escuela.");
        }

        private void Update()
        {
            HandleMovementInput();
            HandleInteractionInput();
        }

        private void FixedUpdate()
        {
            // El movimiento físico se hace en FixedUpdate
            if (currentMoveDir != Vector3.zero)
            {
                Move(currentMoveDir, moveSpeed);
            }
        }

        private void HandleMovementInput()
        {
            // Movimiento usando WASD
            float moveX = 0f;
            float moveY = 0f;

            if (Input.GetKey(KeyCode.W)) moveY = 1f;
            if (Input.GetKey(KeyCode.S)) moveY = -1f;
            if (Input.GetKey(KeyCode.A)) moveX = -1f;
            if (Input.GetKey(KeyCode.D)) moveX = 1f;

            currentMoveDir = new Vector3(moveX, moveY, 0).normalized;
            
            // Actualizar Animator
            if (animator != null)
            {
                bool isMoving = currentMoveDir != Vector3.zero;
                
                if (isMoving)
                {
                    lastMoveX = moveX;
                    lastMoveY = moveY;
                }

                animator.SetFloat("moveX", isMoving ? moveX : lastMoveX);
                animator.SetFloat("moveY", isMoving ? moveY : lastMoveY);
                animator.SetBool("isWalking", isMoving);
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Z)) // Presionar 'E' o 'Z' para interactuar
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            // Lanzamos un rayo invisible (Raycast) en la dirección a la que mira el jugador
            Vector2 facingDir = new Vector2(lastMoveX, lastMoveY).normalized;
            if (facingDir == Vector2.zero) facingDir = Vector2.down; // Por defecto abajo

            // Punto de origen ajustado al centro del jugador
            Vector2 origin = transform.position + new Vector3(0, 0.5f, 0); 
            
            Debug.DrawRay(origin, facingDir * 1.5f, Color.green, 1f); // Para depuración en el Editor

            RaycastHit2D hit = Physics2D.Raycast(origin, facingDir, 1.5f);

            if (hit.collider != null)
            {
                // Si chocamos con algo, buscamos si tiene el componente (Interfaz) IInteractable
                IInteractable interactableObj = hit.collider.GetComponent<IInteractable>();
                
                if (interactableObj != null)
                {
                    Debug.Log($"Interactuando con: {hit.collider.gameObject.name}");
                    interactableObj.Interact(); // Aplicamos polimorfismo
                }
            }
            else
            {
                Debug.Log("No hay nada con qué interactuar aquí.");
            }
        }
    }
}
