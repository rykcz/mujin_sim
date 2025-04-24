using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float dragSpeed = 0.01f;
    public float scrollSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 10f;

    private Camera cam;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // ダブルクリック認識時間

    private int tapCount = 0;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f; // ダブルタップ認識時間

    // カメラをドラッグ移動できる範囲（例：X -10〜10, Y -5〜5）
    public Vector2 minPosition = new Vector2(-10f, -5f);
    public Vector2 maxPosition = new Vector2(10f, 15f);

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleMouseInput();
        HandleTouchInput();
        HandleZoom();
        HandleKeyboardInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float timeSinceLastClick = Time.unscaledTime - lastClickTime;
            lastClickTime = Time.unscaledTime;

            if (timeSinceLastClick <= doubleClickThreshold)
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * dragSpeed;
            transform.Translate(move, Space.World);
            lastMousePosition = Input.mousePosition;

            ClampPosition();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                float timeSinceLastTap = Time.unscaledTime - lastTapTime;
                lastTapTime = Time.unscaledTime;

                if (timeSinceLastTap <= doubleTapThreshold)
                {
                    tapCount++;

                    if (tapCount == 2)
                    {
                        // ダブルタップ成功
                        isDragging = true;
                        lastMousePosition = touch.position;
                        tapCount = 0;
                    }
                }
                else
                {
                    tapCount = 1;
                }
            }

            if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 delta = (Vector3)touch.position - lastMousePosition;
                Vector3 move = new Vector3(-delta.x, -delta.y, 0) * dragSpeed;
                transform.Translate(move, Space.World);
                lastMousePosition = touch.position;

                ClampPosition();
            }

            if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * scrollSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    private void ClampPosition() //カメラ移動範囲制限
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);

        transform.position = pos;
    }

    private void HandleKeyboardInput()
    {
        float moveSpeed = 8f;
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move.x -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            move.x += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            move.y += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            move.y -= moveSpeed * Time.deltaTime;
        }

        if (move != Vector3.zero)
        {
            transform.Translate(move, Space.World);
            ClampPosition(); // 移動後に範囲制限
        }
    }

}
