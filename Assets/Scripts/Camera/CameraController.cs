using UnityEngine;

/// <summary>
/// Simple 2D orthographic camera controller:
/// - WASD / Arrow keys pan
/// - Middle-mouse drag (optional)
/// - Mouse wheel zoom
/// - Camera stays within configurable world bounds (rect)
/// - Zoom clamped between min/max orthographic size
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Bounds (World Space)")]
    public Rect worldBounds = new Rect(-50, -50, 100, 100);

    [Header("Pan")]
    public float panSpeed = 20f;          // world units per second
    public bool enableMouseDrag = true;
    public int mouseDragButton = 2;       // 0=left,1=right,2=middle

    [Header("Zoom")]
    public float zoomSpeed = 8f;          // orthographic size units per scroll step
    public float minZoom = 3f;            // min orthographicSize
    public float maxZoom = 20f;           // max orthographicSize
    public bool zoomToMouse = true;       // zoom towards mouse position

    private Camera cam;
    private Vector3 lastMouseWorld;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        ClampToBounds();
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
        ClampToBounds();
    }

    private void HandlePan()
    {
        Vector3 delta = Vector3.zero;

        // Keyboard pan
        float x = Input.GetAxisRaw("Horizontal"); // arrows / A-D
        float y = Input.GetAxisRaw("Vertical");   // arrows / W-S
        if (x != 0f || y != 0f)
        {
            delta += new Vector3(x, y, 0f) * (panSpeed * Time.unscaledDeltaTime);
        }

        // Mouse drag pan (drag the world)
        if (enableMouseDrag && Input.GetMouseButtonDown(mouseDragButton))
        {
            lastMouseWorld = MouseWorld();
        }
        if (enableMouseDrag && Input.GetMouseButton(mouseDragButton))
        {
            Vector3 now = MouseWorld();
            Vector3 drag = lastMouseWorld - now; // move camera opposite mouse move
            delta += drag;
            lastMouseWorld = now;
        }

        if (delta != Vector3.zero)
            transform.position += delta;
    }

    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(scroll, 0f))
            return;

        // Store mouse world point before zoom (for zoom-to-mouse)
        Vector3 mouseBefore = MouseWorld();

        float target = cam.orthographicSize - scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(target, minZoom, maxZoom);

        if (zoomToMouse)
        {
            // After zoom, move camera so the mouse points to the same world location
            Vector3 mouseAfter = MouseWorld();
            Vector3 diff = mouseBefore - mouseAfter;
            transform.position += diff;
        }
    }

    private Vector3 MouseWorld()
    {
        // For 2D typically camera looks at Z=0 plane
        Vector3 mp = Input.mousePosition;
        mp.z = -cam.transform.position.z;
        return cam.ScreenToWorldPoint(mp);
    }

    private void ClampToBounds()
    {
        // Camera half extents in world units
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector3 p = transform.position;

        float minX = worldBounds.xMin + halfW;
        float maxX = worldBounds.xMax - halfW;
        float minY = worldBounds.yMin + halfH;
        float maxY = worldBounds.yMax - halfH;

        // If bounds smaller than view, center camera in bounds on that axis
        if (minX > maxX) p.x = worldBounds.center.x;
        else p.x = Mathf.Clamp(p.x, minX, maxX);

        if (minY > maxY) p.y = worldBounds.center.y;
        else p.y = Mathf.Clamp(p.y, minY, maxY);

        // Preserve Z
        transform.position = new Vector3(p.x, p.y, transform.position.z);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 c = new Vector3(worldBounds.center.x, worldBounds.center.y, 0f);
        Vector3 s = new Vector3(worldBounds.size.x, worldBounds.size.y, 0f);
        Gizmos.DrawWireCube(c, s);
    }
#endif
}