using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Moving : MonoBehaviour
{
    public Vector3 displacement;
    public float duration;
    public bool flipSpriteX;
    private float startTime;

	private Vector3 startPos;
    private bool forward;
	private Rigidbody2D rb;

    private const float epsilon = 1e-6f;
	
	void Awake()
	{
        startTime = Time.time;
		startPos = gameObject.transform.position;
		rb = GetComponent<Rigidbody2D>(); 
	}

    void Start()
    {
        startTime = Time.time;
        if( duration < epsilon && displacement == Vector3.zero){
            duration = 1f;
            displacement = Vector3.zero;
        }
        InvokeRepeating("FlipDirection", duration, duration);
    }

    void Update() 
    {   

    }
    void FixedUpdate()
    {
        float t = duration > epsilon ? (Time.time - startTime) / duration : 0;
        rb.position = createSmoothTransform(t);
    }

    public void FlipDirection()
    {
        if( flipSpriteX ){
            
        }
        displacement = -displacement;
        startTime = Time.time;
        startPos = gameObject.transform.position;
    }

    Vector3 createSmoothTransform(float t){
        float smoothX, smoothY, smoothZ;
        Vector3 smooth;
        if( startPos != null && t > 0 ){
            smoothX = Mathf.SmoothStep(startPos.x, startPos.x + displacement.x, t);
            smoothY = Mathf.SmoothStep(startPos.y, startPos.y + displacement.y, t);
            smoothZ = Mathf.SmoothStep(startPos.z, startPos.z + displacement.z, t);    
            smooth = new Vector3(smoothX, smoothY, smoothZ);
        }else{
            smooth = Vector3.zero;
        }
        return smooth;
    }
}
