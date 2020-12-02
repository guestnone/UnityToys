using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMover : MonoBehaviour
{
    public GameObject connectedPolygon;
    public int pointIndex;
    public Vector3 currentPosition;
    public bool visible = true;

    private SpriteRenderer mRenderer;
    private BoxCollider2D mCollider;
    private Camera mMainCamera;
    private bool mIsInUse = false;
    private int mMode = 0;

    public PointMover Initialize(GameObject polygon, int index, Vector3 position)
    {
        this.connectedPolygon = polygon;
        this.pointIndex = index;
        this.currentPosition = position;
        this.transform.position = currentPosition + polygon.transform.position;
        transform.position -= new Vector3(0, 0, 2); // Fix z ordering
        return this;
    }

    public void ReSetPosition(Vector3 position)
    {
        this.currentPosition = position;
        this.transform.position = currentPosition + connectedPolygon.transform.position;
        transform.position -= new Vector3(0, 0, 2); // Fix z ordering
    }

    void Awake()
    {
        mRenderer = gameObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        mRenderer.sprite = Resources.Load("Picker", typeof(Sprite)) as Sprite;
        mRenderer.material = new Material(Shader.Find("Sprites/Default"));
        mCollider = gameObject.AddComponent<BoxCollider2D>();
        mMainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            mIsInUse = true;
            if (Input.GetKey(KeyCode.Z))
            {
                mMode = 1;
            }
            else if (Input.GetKey(KeyCode.X))
            {
                mMode = 2;
            }
            else
            {
                mMode = 0;
            }
        }
    }

    void OnMouseDrag()
    {
        if (mIsInUse)
        {
            // Rotate
            if (mMode == 1)
            {
                float axis = Input.GetAxis("Mouse X");

                // Positive means move right
                if (axis > 0)
                {
                    connectedPolygon.GetComponent<Polygon>().OnHandleRotate(pointIndex, 0.02f);
                }
                // Negative means move right
                else if (axis < 0)
                {
                    connectedPolygon.GetComponent<Polygon>().OnHandleRotate(pointIndex, -0.02f);
                }
                // No move
                else
                {
                    return;
                }
            }

            // Scale (Very Finnicky, byt kinda scales???)
            if (mMode == 2)
            {
                float axis = Input.GetAxis("Mouse X");
                // Positive means move right
                if (axis > 0)
                {
                    connectedPolygon.GetComponent<Polygon>().OnHandleScale(pointIndex, 0.0001f);
                }
                // Negative means move right
                else if (axis < 0)
                {
                    connectedPolygon.GetComponent<Polygon>().OnHandleScale(pointIndex, -0.0001f);
                }
                // No move
                else
                {
                    return;
                }
            }

            if (mMode == 0)
            {
                // Translate
                Vector3 pos = mMainCamera.ScreenToWorldPoint(Input.mousePosition);

                Vector3 forHandle = new Vector3(pos.x, pos.y, -2);
                Vector3 forPoints = new Vector3(pos.x - transform.position.x, pos.y - transform.position.y, 0);
                transform.position = forHandle;
                connectedPolygon.GetComponent<Polygon>().OnHandleMove(pointIndex, forPoints);
            }
        }
    }

    void OnMouseUp()
    {
        mIsInUse = false;
        mMode = 0;
    }

    void Start()
    {
    }

    void Update()
    {
 
    }
}
