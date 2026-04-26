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
            // El movimiento físico se hace en FixedUpdate, siempre llamamos a Move para que actualice la velocidad
            Move(currentMoveDir, moveSpeed);
        }

        private void HandleMovementInput()
        {
            // Bloquear movimiento si el diálogo está activo
            var ds = Object.FindAnyObjectByType<HeartQuest.UI.DialogueSystem>(FindObjectsInactive.Include);
            if (ds != null && ds.gameObject.activeSelf)
            {
                currentMoveDir = Vector3.zero;
                if (animator != null)
                {
                    animator.SetFloat("moveX", lastMoveX);
                    animator.SetFloat("moveY", lastMoveY);
                    animator.SetBool("isWalking", false);
                }
                return;
            }

            // Movimiento usando WASD
            float moveX = 0f;
            float moveY = 0f;

            if (Input.GetKey(KeyCode.W)) moveY = 1f;
            if (Input.GetKey(KeyCode.S)) moveY = -1f;
            if (Input.GetKey(KeyCode.A)) moveX = -1f;
            if (Input.GetKey(KeyCode.D)) moveX = 1f;

            currentMoveDir = new Vector3(moveX, moveY, 0).normalized;
            
            // Actualizar Animator
            if (animator != null && animator.runtimeAnimatorController != null)
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
            // Evitar interactuar de nuevo si el diálogo ya está abierto
            var ds = Object.FindAnyObjectByType<HeartQuest.UI.DialogueSystem>(FindObjectsInactive.Include);
            if (ds != null && ds.gameObject.activeSelf) return;

            // Usamos OverlapCircle para detectar NPCs cercanos (más fiable que Raycast)
            float interactRadius = 2.0f;
            Vector2 center = (Vector2)transform.position + new Vector2(0, 0.5f);
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, interactRadius);
            bool interacted = false;

            foreach (var hit in hits)
            {
                if (hit != null && hit.gameObject != this.gameObject)
                {
                    IInteractable interactableObj = hit.GetComponent<IInteractable>();
                    
                    if (interactableObj != null)
                    {
                        Debug.Log($"Interactuando con: {hit.gameObject.name}");
                        interactableObj.Interact(); // Aplicamos polimorfismo
                        interacted = true;
                        break; // Solo interactuamos con el primero que encontremos
                    }
                }
            }

            if (!interacted)
            {
                Debug.Log("No hay nada con qué interactuar aquí.");
            }
        }
    }
}
