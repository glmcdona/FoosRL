using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderLine : MonoBehaviour
{
    private Color _color = Color.blue;
    public Color Color
    {
        get { return _color; }   // get method
        set
        {
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

    public Vector3 StartOffset = new Vector3(1.0f,0.0f,0.0f);
    public Vector3 EndOffset = new Vector3(-1.0f,0.0f, 0.0f);
    public float HeightDelta = 4f;

    private float _width = 0.2f;
    public float Width
    {
        get { return _width; }   // get method
        set
        {
            _width = value;
            if (_lr != null)
            {
                _lr.startWidth = value;
                _lr.endWidth = value;
            }
        }  // set method
    }

    private GameObject _line_go = null;
    private LineRenderer _lr = null;




    // Start is called before the first frame update
    //void Awake()
    void Start()
    {
        _line_go = new GameObject("render line");
        _line_go.layer = gameObject.layer;
        _line_go.transform.position = gameObject.transform.position;
        
        _lr = _line_go.AddComponent<LineRenderer>();
        _lr.startColor = _color;
        _lr.endColor = _color;
        _lr.startWidth = _width;
        _lr.endWidth = _width;
        Material materialColored = new Material(Shader.Find("Diffuse"));
        materialColored.color = _color;
        _lr.material = materialColored;
    }

    // Update is called once per frame
    void Update()
    {
        _lr.SetPosition(0, gameObject.transform.position + StartOffset + new Vector3(0f, HeightDelta, 0f));
        _lr.SetPosition(1, gameObject.transform.position + EndOffset + new Vector3(0f, HeightDelta, 0f));
    }
}
