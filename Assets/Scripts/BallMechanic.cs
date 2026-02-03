using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BallMechanic : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public Transform holdPosition;

    [Header("Throw Settings")]
    public float maxThrowForce = 10f;
    public float chargeSpeed = 10f;

    [Header("Bounce Settings")]
    public float bounceForce = 8f;
    public float bounceReturnSpeed = 5f;

    [Header("UI Settings")]
    public float uiUpdateDistance = 2f; // UI gösterim mesafesi

    private GameObject heldBall;
    private GameObject nearbyBall; // Yakındaki top

    private bool isHoldingBall = false;
    private bool isBouncing = false;
    private float currentThrowForce = 0f;
    private bool isCharging = false;
    private bool showPickupUI = false;

    void Update()
    {
        // Yakındaki topu kontrol et
        CheckNearbyBall();

        // E tuşu - Top al/bırak
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isHoldingBall && !isBouncing)
            {
                TryPickupBall();
            }
            else if (isHoldingBall && !isBouncing)
            {
                DropBall();
            }
        }
        // H tuşu - Topu yanına getir veya spawn et
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (!isHoldingBall && !isBouncing)
            {
                GameObject existingBall = GameObject.FindGameObjectWithTag("Ball");
                if(existingBall != null)
                {
                    existingBall.transform.position = holdPosition.position;
                }
                // Topun fizik özelliklerini sıfırla
                Rigidbody ballRb = existingBall.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    ballRb.linearVelocity = Vector3.zero;
                    ballRb.angularVelocity = Vector3.zero;
                    ballRb.isKinematic = false;
                    ballRb.useGravity = true;
                }
            }
            else if (isHoldingBall && !isBouncing)
            {
                // Eğer top zaten eldeyse, sadece konumunu düzelt
                heldBall.transform.position = holdPosition.position;
            }
        }


        // Sadece top tutuluyorsa ve sektirme yapılmıyorsa atış mekanikleri çalışsın
        if (isHoldingBall && heldBall != null && !isBouncing)
        {
            // SAĞ TIK - Sektirme
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                StartCoroutine(BounceBall());
            }

            // SOL TIK - Güç yükleme ve fırlatma
            if (Mouse.current.leftButton.isPressed)
            {
                isCharging = true;
                currentThrowForce += chargeSpeed * Time.deltaTime;
                currentThrowForce = Mathf.Clamp(currentThrowForce, 0f, maxThrowForce);

                Debug.Log("Güç yükleniyor: " + currentThrowForce.ToString("F1"));
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && isCharging)
            {
                ThrowBall(currentThrowForce);
                currentThrowForce = 0f;
                isCharging = false;
            }
        }
    }

    void OnGUI()
    {
        // Pickup UI (Ekranın sağ üstü)
        if (showPickupUI && !isHoldingBall)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleRight;

            GUI.Label(new Rect(Screen.width - 200, 50, 180, 30), "[E] Topu Al", style);
        }

        DrawCrosshair();

        // Topun üzerinde 3D space'de label
        //if (showPickupUI && nearbyBall != null && !isHoldingBall)
        //{
        //    Vector3 ballScreenPos = Camera.main.WorldToScreenPoint(nearbyBall.transform.position + Vector3.up * 1.5f);

        //    // Ekranın içindeyse göster
        //    if (ballScreenPos.z > 0)
        //    {
        //        GUIStyle worldStyle = new GUIStyle(GUI.skin.label);
        //        worldStyle.fontSize = 20;
        //        worldStyle.normal.textColor = Color.green;
        //        worldStyle.alignment = TextAnchor.MiddleCenter;
        //        worldStyle.fontStyle = FontStyle.Bold;

        //        // Y koordinatını ters çevir (GUI koordinat sistemi)
        //        GUI.Label(new Rect(ballScreenPos.x - 50, Screen.height - ballScreenPos.y - 25, 100, 30), "E", worldStyle);
        //    }
        //}
    }

    void CheckNearbyBall()
    {
        if (isHoldingBall)
        {
            showPickupUI = false;
            nearbyBall = null;
            return;
        }

        // Yakındaki topları kontrol et
        Collider[] colliders = Physics.OverlapSphere(transform.position, uiUpdateDistance);
        GameObject closestBall = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Ball"))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBall = col.gameObject;
                }
            }
        }

        // En yakın topu bulduk
        if (closestBall != null)
        {
            nearbyBall = closestBall;
            showPickupUI = true;
        }
        else
        {
            nearbyBall = null;
            showPickupUI = false;
        }
    }

    void TryPickupBall()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Ball"))
            {
                heldBall = col.gameObject;
                isHoldingBall = true;

                Rigidbody ballRb = heldBall.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    ballRb.isKinematic = true;
                    ballRb.useGravity = false;
                    ballRb.linearVelocity = Vector3.zero;
                    ballRb.angularVelocity = Vector3.zero;
                }

                heldBall.transform.position = holdPosition.position;
                heldBall.transform.parent = holdPosition;

                Debug.Log("Top alındı!");
                showPickupUI = false;
                break;
            }
        }
    }

    void DropBall()
    {
        if (heldBall != null)
        {
            heldBall.transform.parent = null;

            Rigidbody ballRb = heldBall.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                ballRb.isKinematic = false;
                ballRb.useGravity = true;
            }

            Debug.Log("Top bırakıldı!");
            heldBall = null;
            isHoldingBall = false;
            currentThrowForce = 0f;
            isCharging = false;
        }
    }

    void DrawCrosshair()
    {
        float crosshairSize = 20f;
        float crosshairThickness = 2f;
        float centerX = Screen.width / 2;
        float centerY = Screen.height / 2;

        // Kırmızı renk
        Color crosshairColor = Color.red;

        // Dikey çizgi
        GUI.color = crosshairColor;
        GUI.DrawTexture(
            new Rect(centerX - crosshairThickness / 2, centerY - crosshairSize / 2,
                    crosshairThickness, crosshairSize),
            Texture2D.whiteTexture
        );

        // Yatay çizgi
        GUI.DrawTexture(
            new Rect(centerX - crosshairSize / 2, centerY - crosshairThickness / 2,
                    crosshairSize, crosshairThickness),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white; // Rengi sıfırla
    }

    IEnumerator BounceBall()
    {
        if (heldBall == null) yield break;

        isBouncing = true;
        Debug.Log("Top sektiriliyor...");

        heldBall.transform.parent = null;

        Rigidbody ballRb = heldBall.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.isKinematic = false;
            ballRb.useGravity = true;
            ballRb.linearVelocity = Vector3.down * bounceForce;
        }

        yield return new WaitForSeconds(0.05f);

        float returnTime = 0f;
        float returnDuration = 0.5f;

        while (returnTime < returnDuration && heldBall != null)
        {
            returnTime += Time.deltaTime;
            float progress = returnTime / returnDuration;

            if (ballRb != null)
            {
                ballRb.linearVelocity = Vector3.Lerp(ballRb.linearVelocity, Vector3.zero, progress);
                heldBall.transform.position = Vector3.Lerp(
                    heldBall.transform.position,
                    holdPosition.position,
                    progress * bounceReturnSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        if (heldBall != null && ballRb != null)
        {
            ballRb.isKinematic = true;
            ballRb.useGravity = false;
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;

            heldBall.transform.position = holdPosition.position;
            heldBall.transform.parent = holdPosition;
        }

        isBouncing = false;
        Debug.Log("Sektirme tamamlandı!");
    }

    void ThrowBall(float force)
    {
        if (heldBall != null)
        {
            heldBall.transform.parent = null;

            Rigidbody ballRb = heldBall.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                ballRb.isKinematic = false;
                ballRb.useGravity = true;

                Vector3 throwDirection = Camera.main.transform.forward;
                ballRb.AddForce(throwDirection * force, ForceMode.Impulse);
            }

            Debug.Log("Top fırlatıldı! Güç: " + force.ToString("F1"));
            heldBall = null;
            isHoldingBall = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, uiUpdateDistance);
    }
}