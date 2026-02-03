using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 0.4f;
    [SerializeField] private float gravity = -20f;

    [Header("Kamera Ayarları")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Zemin Kontrolü")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float cameraPitch = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool isSprinting;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Eğer kamera atanmamışsa, alt obje olarak ara
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }

        // Mouse'u kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleCamera();
        CheckGround();
    }

    void HandleInput()
    {
        // Keyboard null kontrolü
        if (Keyboard.current == null)
        {
            Debug.LogWarning("Keyboard bulunamadı!");
            return;
        }

        // Hareket girişi (WASD)
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.dKey.isPressed) horizontal += 1f;
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.wKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed) vertical -= 1f;

        moveInput = new Vector2(horizontal, vertical);

        // Kamera girişi (Mouse Delta)
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            lookInput = mouseDelta * 0.1f; // Delta değeri çok büyük olduğu için küçültüyoruz
        }

        // Zıplama - Space tuşu
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpPressed = true;
            Debug.Log("Zıplama tuşuna basıldı!");
        }

        // Koşma (Shift)
        isSprinting = Keyboard.current.leftShiftKey.isPressed;

        // ESC ile mouse kilidini aç/kapat
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void HandleMovement()
    {
        // Hız seçimi
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Hareket yönü hesaplama (oyuncunun baktığı yöne göre)
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Zıplama
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            // Debug.Log($"Zıplandı! Velocity.y: {velocity.y}, isGrounded: {isGrounded}");
        }

        // jumpPressed'i sıfırla (tek seferlik input)
        jumpPressed = false;

        // Yerçekimi uygula
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Yere değdiğinde velocity'yi sıfırla
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleCamera()
    {
        // Yatay dönüş (Yaw - oyuncuyu döndür)
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        // Dikey bakış (Pitch - kamerayı döndür)
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void CheckGround()
    {
        // Zemin kontrolü - küçük bir küre ile raycast
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            // GroundCheck yoksa controller'ın isGrounded'ını kullan
            isGrounded = controller.isGrounded;
        }

        // Eğer hala havada değilse ve velocity negatifse yere değmiştir
        if (controller.isGrounded && velocity.y < 0)
        {
            isGrounded = true;
        }
    }

    // Debug için zemin kontrolünü göster
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    // Debug bilgisi
    void OnGUI()
    {
        float fps = GetFps();

        // Stil ayarları
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        // FPS gösterimi (renk kodlu)
        GUIStyle fpsStyle = new GUIStyle(style);
        if (fps >= 60)
            fpsStyle.normal.textColor = Color.green;
        else if (fps >= 30)
            fpsStyle.normal.textColor = Color.yellow;
        else
            fpsStyle.normal.textColor = Color.red;

        GUI.Label(new Rect(10, 90, 300, 20), $"FPS: {Mathf.Ceil(fps)}", fpsStyle);
        GUI.Label(new Rect(10, 110, 300, 20), $"Yerde mi: {isGrounded}");
        GUI.Label(new Rect(10, 130, 300, 20), $"Velocity.y: {velocity.y:F2}");
        // GUI.Label(new Rect(10, 150, 300, 20), $"Space Basılı: {(Keyboard.current != null && Keyboard.current.spaceKey.isPressed)}");
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public Vector3 GetVelocity()
    {
        return controller.velocity;
    }

    private float deltaTime = 0.0f;
    public int GetFps()
    {
        // FPS hesaplama
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        return (int)fps;
    }
}