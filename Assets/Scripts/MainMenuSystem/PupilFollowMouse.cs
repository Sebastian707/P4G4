using UnityEngine;
using UnityEngine.InputSystem;  // ← swap this

public class PupilFollowMouse : MonoBehaviour
{
    [Header("References")]
    public RectTransform pupil;
    public RectTransform eyeBounds;

    [Header("Settings")]
    public float maxRadius = 30f;
    public float followSpeed = 8f;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // New Input System mouse position
        Vector2 mouseScreen = Mouse.current.position.ReadValue();  // ← this line changed

        Vector2 localMouse;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            eyeBounds,
            mouseScreen,
            mainCam,
            out localMouse
        );

        // Clamp to circle
        if (localMouse.magnitude > maxRadius)
            localMouse = localMouse.normalized * maxRadius;

        // Smooth movement
        pupil.localPosition = Vector2.Lerp(
            pupil.localPosition,
            localMouse,
            Time.deltaTime * followSpeed
        );
    }
}