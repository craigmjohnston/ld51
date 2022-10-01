namespace Oatsbarley.LD51
{
    using UnityEngine;

    public class CameraZoom : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float minZoom;
        [SerializeField] private float maxZoom;
    }
}