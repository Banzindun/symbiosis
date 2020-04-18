using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float rightBound;
    private float leftBound;
    private float topBound;
    private float bottomBound;
    private Vector3 max;
    private Vector3 min;
    private Vector3 pos;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private BoxCollider2D boxCollider;

    private Camera camera;

    private float horizontalMax;
    private float horizontalMin;
    private float verticalMax;
    private float verticalMin;

    // Use this for initialization
    void Start()
    {
        //transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float vertExtent = camera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        max = boxCollider.bounds.max;
        min = boxCollider.bounds.min;

        horizontalMax = max.x - horzExtent;
        horizontalMin = min.x + horzExtent;

        verticalMax = max.y - vertExtent;
        verticalMin = min.y + vertExtent;

        var pos = new Vector3(target.position.x, target.position.y, transform.position.z);
        pos.x = Mathf.Clamp(pos.x, horizontalMin, horizontalMax);
        pos.y = Mathf.Clamp(pos.y, verticalMin, verticalMax);
        transform.position = pos;
    }
}
