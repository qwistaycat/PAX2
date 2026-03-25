using UnityEngine;

public class ProximityOrigin : MonoBehaviour
{
    [SerializeField] private Material _material;

    private static readonly int Origin = Shader.PropertyToID("_Origin");

    private void Update()
    {
        _material.SetVector(Origin, transform.position);
    }
}
