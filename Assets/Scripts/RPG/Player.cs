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
        private bool wasDialogueActiveLastFrame = false;

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

            // Registrar si el diálogo estaba activo en este frame
            var ds = HeartQuest.UI.DialogueSystem.Instance;
            wasDialogueActiveLastFrame = ds != null && ds.IsDialogueActive();
        }

        private void FixedUpdate()
        {
            // El movimiento físico se hace en FixedUpdate, siempre llamamos a Move para que actualice la velocidad
            Move(currentMoveDir, moveSpeed);
        }

        private void HandleMovementInput()
        {
            // Bloquear movimiento si el diálogo está activo
            var ds = HeartQuest.UI.DialogueSystem.Instance;
            if (ds != null && ds.IsDialogueActive())
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

            // Movimiento usando los ejes de Input (soporta teclado WASD/flechas y joystick/control analógico)
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            // Limitar la magnitud del vector a 1 para evitar velocidad diagonal extra,
            // pero conservar la sensibilidad analógica para empujes suaves del joystick
            Vector3 inputDir = new Vector3(moveX, moveY, 0);
            if (inputDir.sqrMagnitude > 1f)
            {
                inputDir.Normalize();
            }
            currentMoveDir = inputDir;
            
            // Actualizar Animator
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                // Umbral pequeño para considerar que hay movimiento (evita ruidos de palanca analógica)
                bool isMoving = currentMoveDir.sqrMagnitude > 0.01f;
                
                if (isMoving)
                {
                    // Registramos la última dirección para el blend tree de Idle.
                    // Usamos Sign para asegurar que el personaje mire completamente en la dirección del movimiento al detenerse.
                    if (Mathf.Abs(moveX) > 0.1f) lastMoveX = Mathf.Sign(moveX);
                    if (Mathf.Abs(moveY) > 0.1f) lastMoveY = Mathf.Sign(moveY);
                }

                animator.SetFloat("moveX", isMoving ? moveX : lastMoveX);
                animator.SetFloat("moveY", isMoving ? moveY : lastMoveY);
                animator.SetBool("isWalking", isMoving);
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.JoystickButton0)) // Presionar 'E', 'Z' o Botón A (Joystick) para interactuar
            {
                if (wasDialogueActiveLastFrame) return; // Si el diálogo estaba activo al inicio del frame, no interactuamos en este frame
                TryInteract();
            }
        }

        private void TryInteract()
        {
            // Evitar interactuar de nuevo si el diálogo ya está abierto
            var ds = HeartQuest.UI.DialogueSystem.Instance;
            if (ds != null && ds.IsDialogueActive()) return;

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
