
using SlayTheSpireMechanics.VisualLogic.CardContainer;
using UnityEngine;
using UnityEngine.Serialization;

public class Bezier : MonoBehaviour
{
    [Header("настройки кривой")]
    [SerializeField] private bool enableVisual;
    [SerializeField] private int pointCount = 19;
    [SerializeField] private float exp;
    [Header("Префабы")]
    [SerializeField] private GameObject arrowHead;

    [SerializeField] private Transform startPoint;
    [SerializeField] private GameObject container;

    private Camera _camera;
    private GameObject _controlVisual1;
    private GameObject _controlVisual2;
    private GameObject _controlVisual3;
    

    
    public bool isDrawing = false;
    
    public void Init(CardHoverSystem  cardHoverSystem)
    {
        _camera = Camera.main;
        cardHoverSystem.cardActiveEnter += EnableLine;
        cardHoverSystem.cardActiveExit += DisableLine;

        if (enableVisual)
        {
            _controlVisual1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _controlVisual1.transform.localScale = Vector3.one * 0.3f;
            _controlVisual1.GetComponent<Renderer>().material.color = Color.yellow;
        
            _controlVisual2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _controlVisual2.transform.localScale = Vector3.one * 0.3f;
            _controlVisual2.GetComponent<Renderer>().material.color = Color.yellow;
        
            _controlVisual3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _controlVisual3.transform.localScale = Vector3.one * 0.3f;
            _controlVisual3.GetComponent<Renderer>().material.color = Color.yellow;
        }

        

        


    }

    public void Update()
    {
        if (!isDrawing) {return;}
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -_camera.transform.position.z;
        Vector3 worldPos = _camera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        
        RedrawLine(startPoint.position, worldPos);
    }
    public Vector3[] CalculateBezierPoints(Vector3 start, Vector3 end, Vector3 bezier, int count, float exp)
    {
        Vector3[] points = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float step = (float)i / (count - 1);
            step = Mathf.Clamp01(Mathf.Pow(step, exp));
            
            points[i] = CalculateBezier(step, start, bezier, end);
        }
        return points;
    }
    
    public void RedrawLine(Vector3 startPoint, Vector3 endPoint)
    {

        Vector3 bezierPoint = GetBezierPoint(startPoint, endPoint);
        Vector3[]  bezierPoints = CalculateBezierPoints(startPoint, endPoint, bezierPoint, pointCount + 1, exp);
        
        Quaternion euler = Quaternion.identity;
        for (int i = 0; i < bezierPoints.Length; i++)
        {
            if (i == bezierPoints.Length - 1) 
            {
                
                arrowHead.transform.position = bezierPoints[i];
                arrowHead.transform.rotation = euler;
                break;
            }
            euler = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward,bezierPoints[i+1] - bezierPoints[i]));

        }
        
        if (enableVisual)
        {
            _controlVisual1.transform.position = startPoint;
            _controlVisual2.transform.position = endPoint;
            _controlVisual3.transform.position = bezierPoint;
        }
    }
    
    
    
    
    public Vector3 GetBezierPoint(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 midPoint = Vector3.Lerp(startPoint, endPoint, 0.8f);
        Vector3 bezier = midPoint - Vector3.right * (endPoint.x - startPoint.x);
        return bezier;
    }
    
    Vector3 CalculateBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    public void EnableLine()
    {
        Debug.Log("EnableLine");
        container?.SetActive(true);
        isDrawing = true;
    }
    public void DisableLine()
    {
        Debug.Log("DisableLine");
        container?.SetActive(false);
        isDrawing = false;
    }

    
}
