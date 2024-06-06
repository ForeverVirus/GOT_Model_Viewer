using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public Main main;
    private Camera mainCamera;
    public float HorizonSpeed = 8;
    public float VerticalSpeed = 8;

    private Vector3 preMousePosition = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
        
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (mainCamera.fieldOfView <= 100)
            {
                mainCamera.fieldOfView += 2;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (mainCamera.fieldOfView > 2)
            {
                mainCamera.fieldOfView -= 2;
            }
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay((Input.mousePosition));
            if (Physics.Raycast(ray, out var hit))
            {
                main.SelectPartOfMesh(hit.collider.gameObject);
            }
            // Physics.Raycast(transform.position, )
        }

        if (Input.GetMouseButton(0))
        {
            var curPos = Input.mousePosition;
            var deltaDir = (curPos - preMousePosition).normalized;
            var right = mainCamera.transform.right * -deltaDir.x * HorizonSpeed;
            var up = mainCamera.transform.up * -deltaDir.y * VerticalSpeed;
        
            mainCamera.transform.position += right + up;
            // mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, newPosition, 0.1f * Speed);
            preMousePosition = curPos;
        }
        
        if (main.CurrentMeshData != null && main.ModelRoot != null)
        {
            mainCamera.transform.LookAt(main.ModelRoot.transform);
        }
    }
}
