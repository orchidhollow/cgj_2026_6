using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetAssets : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Planet Assets")]
    [SerializeField] private float gravityScale = 10.0f;
    public float GravityScale => gravityScale;
   public Vector2 GetGravityDirection(Vector2 objectPosition)
    {
        return ((Vector2)transform.position - objectPosition).normalized;
    }
    public Vector2 GetMovementDirection(Vector2 objectPosition)
    {
        return new Vector2(-GetGravityDirection(objectPosition).y, GetGravityDirection(objectPosition).x);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
