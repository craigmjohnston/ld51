namespace Oatsbarley.LD51
{
    using System;
    using UnityEngine;

    public class CameraZoom : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float minZoom;
        [SerializeField] private float maxZoom;
        [SerializeField] private float smoothTime;

        [SerializeField] private Vector2 minPosition = new Vector2(-16f, -8.6f);
        [SerializeField] private Vector2 maxPosition = new Vector2(16f, 8.6f);

        private float targetZoom;
        private float velocity;
        private Vector2 targetPosition;
        private Vector2 positionVelocity;

        private Vector3 mousePosition;
        private Vector3 lastMousePosition;

        private void Start()
        {
            this.targetZoom = this.mainCamera.orthographicSize;
            this.targetPosition = this.mainCamera.transform.position;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Mouse2))
            {
                if (Input.mousePosition != this.lastMousePosition)
                {
                    var worldDelta = this.mainCamera.ScreenToWorldPoint(Input.mousePosition) - this.mainCamera.ScreenToWorldPoint(this.lastMousePosition);
                    var newPosition = this.mainCamera.transform.position - worldDelta;

                    newPosition = new Vector3(
                        Mathf.Clamp(newPosition.x, this.minPosition.x, this.maxPosition.x),
                        Mathf.Clamp(newPosition.y, this.minPosition.y, this.maxPosition.y),
                        this.mainCamera.transform.position.z);

                    this.mainCamera.transform.position = newPosition;
                }

                this.lastMousePosition = Input.mousePosition;

                return;
            }

            this.lastMousePosition = Input.mousePosition;

            if (Input.mouseScrollDelta.y != 0)
            {
                this.targetZoom -= Input.mouseScrollDelta.y;
                this.targetZoom = Mathf.Clamp(this.targetZoom, this.minZoom, this.maxZoom);

                // todo position snapping
                // this.mousePosition = Input.mousePosition;
                //
                // var cameraPosition = this.mainCamera.transform.position;
                // var mouseWorldPos = this.mainCamera.ScreenToWorldPoint(this.mousePosition);

                // var cameraOffset = (mouseWorldPos + cameraPosition) / this.mainCamera.orthographicSize;
                // Debug.Log($"(worldPosition: {mouseWorldPos} + mainCameraPosition: {cameraPosition}) / orthographicSize: {this.mainCamera.orthographicSize} = {cameraOffset}");

                // this.targetPosition = cameraPosition - ((cameraOffset * this.targetZoom - mouseWorldPos) - cameraPosition);
                // Debug.Log($"cameraOffset: {cameraOffset} * targetZoom: {this.targetZoom} - worldPosition: {mouseWorldPos} = {this.targetPosition}");
            }

            float zoomResult = Mathf.SmoothDamp(this.mainCamera.orthographicSize, this.targetZoom, ref this.velocity, this.smoothTime);

            if (zoomResult == this.mainCamera.orthographicSize)
            {
                return;
            }

            // Vector2 positionResult = Vector2.SmoothDamp(this.mainCamera.transform.position, this.targetPosition, ref this.positionVelocity, this.smoothTime);

            this.mainCamera.orthographicSize = zoomResult;
            // this.mainCamera.transform.position = new Vector3(positionResult.x, positionResult.y, this.mainCamera.transform.position.z);
        }
    }
}