using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineControlPointMover : MonoBehaviour
{
    public GameObject connectedBezierSpline;
    public int controlPointIndex;
    public Vector3 currentPosition;
    public bool visible = false;

    private SpriteRenderer mRenderer;
    private bool mCurrVisible = false;
    private BoxCollider2D mCollider;
    private Camera mMainCamera;
    private bool mIsDragging = false;

    public SplineControlPointMover Initialize(GameObject spline, int index, Vector3 position)
    {
        this.connectedBezierSpline = spline;
        this.controlPointIndex = index;
        this.currentPosition = position;
        this.transform.position = currentPosition + spline.transform.position;
        transform.position -= new Vector3(0, 0, 2); // Fix z ordering
        return this;
    }

    void Awake()
    {
        mRenderer = gameObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        mRenderer.sprite = Resources.Load("Picker", typeof(Sprite)) as Sprite;
        mRenderer.material = new Material(Shader.Find("Sprites/Default"));
        mCollider = gameObject.AddComponent<BoxCollider2D>();
        mMainCamera = Camera.main;
    }

    public void ReSetPosition(Vector3 position)
    {
        this.currentPosition = position;
        this.transform.position = currentPosition + connectedBezierSpline.transform.position;
        transform.position -= new Vector3(0, 0, 2); // Fix z ordering
    }

    void OnMouseDown()
    {
        if (mCurrVisible)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                mIsDragging = true;
            }
        }
    }

    void OnMouseDrag()
    {
        if (mIsDragging)
        {
            Vector3 pos = mMainCamera.ScreenToWorldPoint(Input.mousePosition);

            Vector3 forHandle = new Vector3(pos.x, pos.y, -2);
            Vector3 forPoint = new Vector3(pos.x, pos.y, 0);

            transform.position = forHandle;
            connectedBezierSpline.GetComponent<BezierSpline>().SetControlPoint(controlPointIndex, forPoint);
        }
    }

    void OnMouseUp()
    {
        mIsDragging = false;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (mCurrVisible == visible)
            return;

        if (!visible)
        {
            mRenderer.enabled = false;
            mCurrVisible = false;
            return;
        }

        if (visible)
        {
            mRenderer.enabled = true;
            mCurrVisible = true;
        }
    }
}
