using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformView : MonoBehaviour
{
    [SerializeField] private float _shiftNextPoint = 0.35f;

    private BoxCollider2D _collider;

    public float LeftEdge => _collider.bounds.min.x;
    public float RightEdge => _collider.bounds.max.x;
    public float TopEdge => _collider.bounds.max.y;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    public Vector3 StickSpawnPosition => new Vector3(
        RightEdge, 
        TopEdge,
        transform.position.z);


    public Vector3 GetLandingPosition(float playerOffset)
    {
        return new Vector3(
            RightEdge - _shiftNextPoint,
            TopEdge + playerOffset,
            transform.position.z);
    }
}
