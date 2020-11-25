using UnityEngine;
using System;

public static class Bezier
{
	public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		t = Mathf.Clamp01(t);
		float OneMinusT = 1f - t;
		return
			OneMinusT * OneMinusT * OneMinusT * p0 +
			3f * OneMinusT * OneMinusT * t * p1 +
			3f * OneMinusT * t * t * p2 +
			t * t * t * p3;
	}

	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
			6f * oneMinusT * t * (p2 - p1) +
			3f * t * t * (p3 - p2);
	}
}

public class BezierSpline : MonoBehaviour {

	[SerializeField]
	public Vector3[] points;

	public Color color = Color.green;
	public float thickness = 0.2f;
	private LineRenderer mLineRenderer;
	public bool selected = false;
	private int mLayerOrder = 0;
	private const int SegmentCount = 50;
	private GameObject[] mHandles;

	GameObject CreateHandle(int i)
	{
		var objToSpawn = new GameObject("Handle_" + System.Guid.NewGuid());
		objToSpawn.AddComponent<SplineControlPointMover>();
		objToSpawn.GetComponent<SplineControlPointMover>().Initialize(gameObject, i, points[i]);
		return objToSpawn;
	}

	public int ControlPointCount
	{
		get
		{
			return points.Length;
		}
	}

	public Vector3 GetControlPoint (int index)
	{
		return points[index];
	}

	public void SetControlPoint (int index, Vector3 point)
	{
		if (index % 3 == 0)
		{
			Vector3 delta = point - points[index];
			if (index > 0)
			{
					points[index - 1] += delta;
			}
			if (index + 1 < points.Length)
			{
				points[index + 1] += delta;
			}
		}
		points[index] = point;
	}

	public int CurveCount
	{
		get
		{
			return (points.Length - 1) / 3;
		}
	}

	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f)
		{
			t = 1f;
			i = points.Length - 4;
		}
		else
		{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
	}
	
	public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f)
		{
			t = 1f;
			i = points.Length - 4;
		}
		else
		{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}
	
	public Vector3 GetDirection (float t)
	{
		return GetVelocity(t).normalized;
	}

	public void AddCurve ()
	{
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point.x += 1f;
		points[points.Length - 3] = point;
		point.x += 1f;
		points[points.Length - 2] = point;
		point.x += 1f;
		points[points.Length - 1] = point;
	}
	
	public void Reset ()
	{
		points = new Vector3[]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
		
	}

	public void SetSelected(bool mode)
	{
		if (mHandles != null)
		{
			selected = mode;
			for (int i = 0; i < mHandles.Length; i++)
			{
				mHandles[i].GetComponent<SplineControlPointMover>().visible = mode;
			}
		}
	}

	public void SetPoints(Vector3[] thePoints)
	{
        this.points = thePoints;
        if (mHandles == null)
        {
	        SetupHandles();
        }
        else if (mHandles.Length != thePoints.Length)
        {
	        // TODO: Add handle change
        }
        else
        {
	        // Update position of handles
	        for (int i = 0; i < points.Length; i++)
	        {
				mHandles[i].GetComponent<SplineControlPointMover>().ReSetPosition(points[i]);
	        }
        }
	}

	void Awake()
	{
		mLineRenderer = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
		mLineRenderer.sortingLayerID = this.mLayerOrder;
		mLineRenderer.useWorldSpace = false;
		mLineRenderer.material =  new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

		if (points != null)
		{
			SetupHandles();
		}
	}

	void SetupHandles()
	{
		mHandles = new GameObject[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			mHandles[i] = CreateHandle(i);
		}
	}

	void DrawCurve()
	{
		mLineRenderer.startColor = color;
		mLineRenderer.endColor = color;
		mLineRenderer.startWidth = thickness;
		mLineRenderer.endWidth = thickness;

		for (int j = 0; j < CurveCount; j++)
		{
			for (int i = 1; i <= SegmentCount; i++)
			{
				float t = i / (float)SegmentCount;
				int nodeIndex = j * 3;
				Vector3 pixel = CalculateCubicBezierPoint(t, GetControlPoint(nodeIndex),
					GetControlPoint(nodeIndex + 1), GetControlPoint(nodeIndex + 2),
					GetControlPoint(nodeIndex + 3));
				mLineRenderer.positionCount = (j * SegmentCount) + i;
				mLineRenderer.SetPosition((j * SegmentCount) + (i - 1), pixel);
			}
            
		}
	}
        
	Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1 - t;
		return (Mathf.Pow(u, 3) * p0) + (3 * Mathf.Pow(u, 2) * t * p1) + (3 * u * Mathf.Pow(t, 2) * p2) + (Mathf.Pow(t, 3) * p3);
	}

	// Update is called once per frame
	void Update()
	{
		DrawCurve();
	}
}