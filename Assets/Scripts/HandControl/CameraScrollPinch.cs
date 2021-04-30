using UnityEngine;

class CameraScrollPinch : MonoBehaviour
{
    public Camera Camera;
    private bool Rotate;
    protected Plane Plane;
    [SerializeField] private float maxZoom = 5;

    float maxX = 150;
    float minX = -150;
    float minY = 39;
    float maxY = 200;
    private void Awake()
    {
        if (Camera == null)
            Camera = Camera.main;

        minY = HandGestureUtils.minYCameraPos();
    }

    private void Update()
    {
        //Update Plane
        if (Input.touchCount >= 1)
            Plane.SetNormalAndPosition(transform.up, transform.position);

        //Zoom in Pinch
        if (Input.touchCount >= 2)
        {
            var pos1 = PlanePosition(Input.GetTouch(0).position);
            var pos2 = PlanePosition(Input.GetTouch(1).position);
            var pos1b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
            var pos2b = PlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);

            if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                //calc zoom
                var zoom = Vector3.Distance(pos1, pos2) / Vector3.Distance(pos1b, pos2b);

                // edge case
                if (zoom == 0 || zoom > maxZoom)
                    return;

                //Move cam amount the mid ray
                 Camera.transform.position = Vector3.LerpUnclamped(pos1, Camera.transform.position, 1/ zoom);

                if (Rotate && pos2b != pos2)
                   Camera.transform.RotateAround(pos1, Plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, Plane.normal));

                CameraInBounds();
            }
        }
    }

    void CameraInBounds()
    {
        if (Camera != null)
        {
            float camX = Mathf.Clamp(Camera.transform.position.x, minX, maxX);
            float camY = Mathf.Clamp(Camera.transform.position.y, minY, maxY);
            float camZ = Mathf.Clamp(Camera.transform.position.z, minX, maxX);
            Camera.transform.position = new Vector3(camX, camY, camZ);
        }
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        //position
        var rayNow = Camera.ScreenPointToRay(screenPos);
        if (Plane.Raycast(rayNow, out var enterNow))
            return rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }
}