using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float smoothSpeed = 0.1f;
    [SerializeField]
    private float minViewDistance = 1.75f;
    [SerializeField]
    private float maxViewDistance = 4f;

    [SerializeField]
    private Vector3 cameraOffset = new Vector3(0f, 0f, -7.5f);
    private new Camera camera;

    private float minCameraX, minCameraY;
    private float maxCameraX;

    void Start()
    {
        camera = this.GetComponent<Camera>();
    }

    void LateUpdate()
    {
        SetCameraBounds();

        if (target)
        {
            Vector3 tempPos = transform.position;
            tempPos = Vector3.Lerp(transform.position, target.position, smoothSpeed * Time.deltaTime);
            tempPos.x = Mathf.Clamp(tempPos.x, minCameraX, maxCameraX);
            tempPos.y = minCameraY;
            tempPos.z = cameraOffset.z;
            transform.position = tempPos;
        }

        //camera.orthographicSize = (Screen.height / 100f) / scale;
    }

    private void SetCameraBounds()
    {
        //float verticalExtent = camera.orthographicSize;
        //float horizontalExtent = verticalExtent * Screen.width / Screen.height;
		float verticalExtent = 3.6f;
		float horizontalExtent = 6.4f;

        Bounds mapBounds = Map.Instance.mapBounds;
        minCameraX = mapBounds.min.x + horizontalExtent;
        maxCameraX = mapBounds.max.x - horizontalExtent;
        minCameraY = mapBounds.min.y + verticalExtent;
    }

	public void SetTarget(Transform target)
	{
		this.target = target;
	}
}
