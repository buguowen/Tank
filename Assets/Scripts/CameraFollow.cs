using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ns
{
    public class CameraFollow : MonoBehaviour
    {
        public Camera followCamera;
        public Vector3 distance;
        public float offset;
        public float speed = 5f;

        public Vector3 startPos;
        private void Start()
        {
            startPos = new Vector3(0f, 5f, -16f);
            distance = new Vector3(0f, 5f, -8f);
            offset = 3f;

            followCamera = Camera.main;
            followCamera.transform.position = transform.position + transform.forward*startPos.z + Vector3.up*startPos.y;
            followCamera.transform.LookAt(transform.position + Vector3.up * offset);
        }

        private void LateUpdate()
        {
            Vector3 targetPos = transform.position + transform.forward * distance.z + Vector3.up * distance.y;
            Vector3 cameraPos = Vector3.MoveTowards(followCamera.transform.position, targetPos, speed * Time.deltaTime);
            followCamera.transform.position = cameraPos;
            followCamera.transform.LookAt(transform.position + Vector3.up * offset);
        }
    }
}