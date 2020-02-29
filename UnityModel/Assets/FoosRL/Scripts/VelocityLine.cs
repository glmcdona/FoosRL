using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityLine : MonoBehaviour
{

    private Color _color = Color.blue;
    public Color Color
    {
        get { return _color; }   // get method
        set {
            _color = value;
            if (_lr != null)
            {
                Material materialColored = new Material(Shader.Find("Diffuse"));
                materialColored.color = value;
                _lr.material = materialColored;
                _lr.startColor = value;
                _lr.endColor = value;
            }
        }  // set method
    }

    public float VelocityLengthFactor = 2.0f;
    public float VelocityLogBase = 4f;
    public float HeightDelta = 4f;

    private float _width = 0.2f;
    public float Width
    {
        get { return _width; }   // get method
        set {
            _width = value;
            if (_lr != null)
            {
                _lr.startWidth = value;
                _lr.endWidth = 0.0f;
            }
        }  // set method
    }

    public Vector3 VelocityAxes = new Vector3(1.0f, 1.0f, 1.0f);


    private GameObject _line_go = null;
    private Rigidbody _vel_rb = null;
    private LineRenderer _lr = null;


    

    // Start is called before the first frame update
    //void Awake()
    void Start()
    {
        _line_go = new GameObject("velocity trail");
        _line_go.layer = gameObject.layer;
        _line_go.transform.position = gameObject.transform.position;
        
        // Find the rigidbody to track
        _vel_rb = null;
        GameObject go = gameObject;
        while (_vel_rb == null)
        {
            if( go.GetComponent<Rigidbody>() != null )
            {
                _vel_rb = go.GetComponent<Rigidbody>();
            }

            if (go.transform.parent == null)
                break;
            
            go = go.transform.parent.gameObject;
        }

        _lr = _line_go.AddComponent<LineRenderer>();
        _lr.startColor = _color;
        _lr.endColor = _color;
        _lr.startWidth = _width;
        _lr.endWidth = 0.0f;
        Material materialColored = new Material(Shader.Find("Diffuse"));
        materialColored.color = _color;
        _lr.material = materialColored;
    }

    // Update is called once per frame
    void Update()
    {
        _lr.SetPosition(0, gameObject.transform.position + new Vector3(0f, HeightDelta, 0f));
        _lr.SetPosition(1, gameObject.transform.position - Mathf.Log(_vel_rb.velocity.magnitude + 1, VelocityLogBase) * Vector3.Scale(_vel_rb.velocity.normalized, VelocityAxes) * VelocityLengthFactor + new Vector3(0f, HeightDelta, 0f));
    }
}
