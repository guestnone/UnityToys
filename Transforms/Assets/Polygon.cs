using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    [SerializeField]
    public Vector3[] points;

    public bool createDefaultValuesForTesting = false;
    public Color color = Color.green;
    public float thickness = 0.1f;
    public bool selected = false;

    private GameObject[] mHandles;
    private LineRenderer mLineRenderer;
    private int mLayerOrder = 0;
    private float mScaleX = 1.0f;
    private float mScaleY = 1.0f;

    public float ScaleX()
    {
        return mScaleX;

    }

    public float ScaleY()
    {
        return mScaleY;

    }

    GameObject CreateHandle(int i)
    {
        var objToSpawn = new GameObject("Handle_" + System.Guid.NewGuid());
        objToSpawn.AddComponent<PointMover>();
        objToSpawn.GetComponent<PointMover>().Initialize(gameObject, i, points[i]);
        return objToSpawn;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void SetupHandles()
    {
        mHandles = new GameObject[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            mHandles[i] = CreateHandle(i);
        }
    }

    public void OnHandleMove(int id, Vector3 vector)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] += vector;
            if (i != id)
                mHandles[i].GetComponent<PointMover>().ReSetPosition(points[i]);
        }

    }

    public void Move(Vector3 vector)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] += vector;
            mHandles[i].GetComponent<PointMover>().ReSetPosition(points[i]);
        }

    }

    public void OnHandleRotate(int id, float degree)
    {
        for (int i = 0; i < points.Length; i++)
        {
            // Only apply to not selected points.
            if (i != id)
            {
                // Where: x', y' - final position of rotated point
                //        xr, yr - points from which will be rotating
                //        omega  - degree of rotation
                // Note: Due to how Unity computes cos/sin, the rotation has to be represented using floats with 
                //       0.01 = 1 Degree and 3.60 = 360 degrees

                //x' = xr + (x - xr)cos(omega) - (y - yr)sin(omega)
                float xPrim = points[id].x + (points[i].x - points[id].x) * Mathf.Cos(degree) -
                              (points[i].y - points[id].y) * Mathf.Sin(degree);

                // y' = yr + (x - xr)sin(omega) + (y - yr)cos(omega)
                float yPrim = points[id].y + (points[i].x - points[id].x) * Mathf.Sin(degree) +
                              (points[i].y - points[id].y) * Mathf.Cos(degree);

                // Apply to points
                points[i].x = xPrim;
                points[i].y = yPrim;

                // Apply to handle
                mHandles[i].GetComponent<PointMover>().ReSetPosition(points[i]);
            }
        }
    }

    public void OnHandleScale(int id, float scaleToAdd)
    { 
        mScaleX = mScaleX + scaleToAdd;
        mScaleY = mScaleY + scaleToAdd;
        Debug.Log("x: " + mScaleX.ToString(CultureInfo.InvariantCulture) + " y: " + mScaleY.ToString(CultureInfo.InvariantCulture));
        for (int i = 0; i < points.Length; i++)
        {
                // Where: x', y' - final position of rotated point
                //        xr, yr - points from which will be scaling

                //x' = xr + (x - xr) * scale
                float xPrim = points[id].x + (points[i].x - points[id].x) * mScaleX;

                // y' = yr + (y - yr) * scale
                float yPrim = points[id].y + (points[i].y - points[id].y) * mScaleY;

                // Apply to points
                points[i].x = xPrim;
                points[i].y = yPrim;

                // Apply to handle
                mHandles[i].GetComponent<PointMover>().ReSetPosition(points[i]);
        }
    }

    public void Scale(int id, float scaleX, float scaleY)
    { 
        mScaleX = scaleX;
        mScaleY = scaleY;
        for (int i = 0; i < points.Length; i++)
        {
            // Where: x', y' - final position of rotated point
            //        xr, yr - points from which will be scaling

            //x' = xr + (x - xr) * scale
            float xPrim = points[id].x + (points[i].x - points[id].x) * mScaleX;

            // y' = yr + (y - yr) * scale
            float yPrim = points[id].y + (points[i].y - points[id].y) * mScaleY;

            // Apply to points
            points[i].x = xPrim;
            points[i].y = yPrim;

            // Apply to handle
            mHandles[i].GetComponent<PointMover>().ReSetPosition(points[i]);
        }
    }

    void Awake()
    {
        mLineRenderer = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
        mLineRenderer.sortingLayerID = this.mLayerOrder;
        mLineRenderer.useWorldSpace = false;
        mLineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        if (createDefaultValuesForTesting)
        {
            points = new Vector3[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(3f, 3f, 0f),
                new Vector3(1f, 3f, 0f)
            };
        }

        if (points != null)
        {
            SetupHandles();
        }
    }

    public void SetPoints(Vector3[] thePoints)
    {
        this.points = thePoints;
        if (mHandles == null)
        {
            SetupHandles();
        }
        else
        {
            // Update position of handles
            for (int i = 0; i < points.Length; i++)
            {
                mHandles[i].GetComponent<PointMover>().ReSetPosition(points[i]);
            }
        }
    }


    void Draw()
    {
        if (points != null)
        {
            mLineRenderer.startColor = color;
            mLineRenderer.endColor = color;
            mLineRenderer.startWidth = thickness;
            mLineRenderer.endWidth = thickness;

            mLineRenderer.positionCount = points.Length;
            mLineRenderer.SetPositions(points);
            mLineRenderer.loop = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Draw();
    }
}
