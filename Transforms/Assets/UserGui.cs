using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UserGui : MonoBehaviour
{
    public List<GameObject> polygons = new List<GameObject>();

    private int mCurrMode = 0;
    private int mCurrEditMode = 0;
    private string[] mToolbarStrings = {"Create", "Edit"};
    private string[] mEditToolbarStrings = {"Translate", "Rotate", "Scale"};
    private string mGlobalText = "Ready";
    
    private Vector3[] mPointsForNew;
    private uint mNumPoints = 2;
    private bool mMouseInputMode = false;
    private Vector2 mScrollViewVectorCreateMode = Vector2.zero;
    private Vector2 mScrollViewVectorEditMode = Vector2.zero;

    private Polygon mCurrEditPolygonComponent;
    private int mNumCurrEditedPolygon = -1;
    private int mNumSelectedPoint = 0;
    private string mVariable1 = "0";
    private string mVariable2 = "0";
    float mVariable1Parsed = 0f;
    float mVariable2Parsed = 0f;


    // Edit with mouse
    private int mCurrentPointToAdd = 0;

    void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(Screen.width - 410, Screen.height - 400, 400, 400), "Editor Menu");
        var switchedMode = GUI.Toolbar(new Rect(Screen.width - 410 + 25, Screen.height - 380, 150, 20), mCurrMode, mToolbarStrings);
        bool switching = false;
        if (switchedMode != mCurrMode)
        {
            switching = true;
            mCurrMode = switchedMode;
        }

        if (mCurrMode == 0) // Create
        {
            GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 360, 50, 50), "Points");
            mNumPoints =
                (uint)Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(Screen.width - 410 + 70, Screen.height - 357, 70, 10), mNumPoints, 2f,
                    20f));
            if (GUI.changed)
            {
                ChangeCreateSize();
            }
            GUI.Label (new Rect (Screen.width - 410 + 150, Screen.height - 360, 50, 50), mNumPoints.ToString());

            mScrollViewVectorCreateMode = GUI.BeginScrollView (new Rect (Screen.width - 410 + 20, Screen.height - 340, 380, 270),
                mScrollViewVectorCreateMode, new Rect (0, 0, 1000, 400));
            for (int i = 0; i < mPointsForNew.Length; i++)
            {
                // TODO: This is a shit hack that makes this program only run on Unity editor, but this is for showing and it works so IDC.
                //       The only thing that it doesn't works is the actual text input but I can move the handles around and it saves so there's that. 
                //       Blame Unity for not providing proper Vector3Field for non-editor use.
                mPointsForNew[i] = EditorGUI.Vector3Field(new Rect(0, i * 20, 300, 20),
                    i.ToString(),
                    mPointsForNew[i]);
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(Screen.width - 410 + 20, Screen.height - 65, 100, 20), "Mouse Add"))
            {
                mCurrentPointToAdd = 0;
                mGlobalText = "RMB to set control point " + (mCurrentPointToAdd + 1).ToString();
                mMouseInputMode = true;
            }

            if (GUI.Button(new Rect( Screen.width - 410 + 120, Screen.height - 65, 100, 20), "Input Add"))
            {
                var newPolygon = new GameObject("Polygon_" + System.Guid.NewGuid());
                newPolygon.AddComponent<Polygon>();
                newPolygon.GetComponent<Polygon>().SetPoints(mPointsForNew);
                polygons.Add(newPolygon);
                ChangeCreateSize();
                if (mNumCurrEditedPolygon == -1)
                {
                    Switch(polygons.Count - 1);
                }
            }

        }
        else // Edit
        {
            if (polygons.Count != 0)
            {
                GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 360, 80, 50), "Spline ID");
                mNumCurrEditedPolygon =
                    Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(Screen.width - 410 + 73, Screen.height - 357, 70, 10), mNumCurrEditedPolygon, 0f,
                        (float)polygons.Count-1));
                if (GUI.changed)
                {
                    Switch(mNumCurrEditedPolygon);
                }
                mScrollViewVectorEditMode = GUI.BeginScrollView (new Rect (Screen.width - 410 + 20, Screen.height - 340, 380, 150),
                    mScrollViewVectorEditMode, new Rect (0, 0, 200, 1000));
                GUI.Label(new Rect(0, 0, 300, 20),
                    "Scale: X: " + mCurrEditPolygonComponent.ScaleX() + " Y: " + mCurrEditPolygonComponent.ScaleY());
                for (int i = 0; i < mCurrEditPolygonComponent.points.Length; i++)
                {
                    GUI.Label(new Rect(0, 20 + i * 20, 300, 20),
                        i.ToString() + ": " + mCurrEditPolygonComponent.points[i].ToString());
                }
                GUI.EndScrollView();
                GUI.Label (new Rect (Screen.width - 410 + 150, Screen.height - 360, 50, 50), mNumCurrEditedPolygon.ToString());
                mCurrEditMode = GUI.Toolbar(new Rect(Screen.width - 410 + 20, Screen.height - 180, 200, 20), mCurrEditMode, mEditToolbarStrings);

                if (mCurrEditMode == 0) // Translate
                {
                    GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 150, 20, 50), "X:");
                    mVariable1 = GUI.TextField(new Rect(Screen.width - 410 + 40, Screen.height - 150, 60, 20), mVariable1, 16);
                    GUI.Label (new Rect (Screen.width - 410 + 120, Screen.height - 150, 20, 50), "Y:");
                    mVariable2 = GUI.TextField(new Rect(Screen.width - 410 + 140, Screen.height - 150, 60, 20), mVariable2, 16);
                    if (GUI.Button(new Rect(Screen.width - 410 + 240, Screen.height - 150, 50, 20), "Apply"))
                    {
                        var ret = parseVars();
                        if (ret.Item1)
                        {
                            mCurrEditPolygonComponent.Move(new Vector3(mVariable1Parsed, mVariable2Parsed, 0));
                            mGlobalText = "Applied!";
                            CleanUpApplied();
                        }
                        else
                        {
                            mGlobalText = ret.Item2;
                        }
                    }
                }
                else if (mCurrEditMode == 1) // Rotate
                {
                    GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 150, 39, 50), "Point:");
                    mNumSelectedPoint =
                        Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(Screen.width - 410 + 60, Screen.height - 150, 100, 10), mNumSelectedPoint, 0f,
                            (float)mCurrEditPolygonComponent.points.Length-1));
                    GUI.Label (new Rect (Screen.width - 410 + 170, Screen.height - 150, 50, 50), mNumSelectedPoint.ToString());
                    GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 130, 50, 50), "Angle:");
                    mVariable1 = GUI.TextField(new Rect(Screen.width - 410 + 80, Screen.height - 130, 60, 20), mVariable1, 16);
                    if (GUI.Button(new Rect(Screen.width - 410 + 140, Screen.height - 130, 50, 20), "Apply"))
                    {
                        var ret = parseVars();
                        if (ret.Item1)
                        {
                            mCurrEditPolygonComponent.OnHandleRotate(mNumSelectedPoint, mVariable1Parsed/100);
                            mGlobalText = "Applied!";
                            CleanUpApplied();
                        }
                        else
                        {
                            mGlobalText = ret.Item2;
                        }
                    }

                }
                else //Scale
                {
                    GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 150, 39, 50), "Point:");
                    mNumSelectedPoint =
                        Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(Screen.width - 410 + 60, Screen.height - 150, 100, 10), mNumSelectedPoint, 0f,
                            (float)mCurrEditPolygonComponent.points.Length-1));
                    GUI.Label (new Rect (Screen.width - 410 + 170, Screen.height - 150, 50, 50), mNumSelectedPoint.ToString());
                    
                    GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 120, 40, 50), "Scale X:");
                    mVariable1 = GUI.TextField(new Rect(Screen.width - 410 + 60, Screen.height - 120, 60, 20), mVariable1, 16);
                    GUI.Label (new Rect (Screen.width - 410 + 140, Screen.height - 120, 40, 50), "Scale Y:");
                    mVariable2 = GUI.TextField(new Rect(Screen.width - 410 + 180, Screen.height - 120, 60, 20), mVariable2, 16);
                    
                    if (GUI.Button(new Rect(Screen.width - 410 + 240, Screen.height - 120, 50, 20), "Apply"))
                    {
                        var ret = parseVars();
                        if (ret.Item1)
                        {
                            mCurrEditPolygonComponent.Scale(mNumSelectedPoint, mVariable1Parsed, mVariable2Parsed);
                            mGlobalText = "Applied!";
                            CleanUpApplied();
                        }
                        else
                        {
                            mGlobalText = ret.Item2;
                        }
                    }
                }

            }
            else
            {
                GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 360, 50, 50), "No polygons added");
            }
        }

        GUI.Label (new Rect (Screen.width - 410 + 20, Screen.height - 20, 300, 300), mGlobalText);
    }

    void ChangeCreateSize()
    {
        mPointsForNew = new Vector3[mNumPoints];
        for(int i = 0; i < mPointsForNew.Length; i++)
        {
            mPointsForNew[i] = Vector3.zero;
        }
    }

    private Tuple<bool, string> parseVars()
    {
        float var1Parsed;
        float var2Parsed;

        try
        {
            var1Parsed = Convert.ToSingle(mVariable1);
        }
        catch
        {
            return new Tuple<bool, string>(false, "First input box contains illegal values"); 
        }

        try
        {
            var2Parsed = Convert.ToSingle(mVariable2);
        }
        catch
        {
            return new Tuple<bool, string>(false, "Second input box contains illegal values"); 
        }

        mVariable1Parsed = var1Parsed;
        mVariable2Parsed = var2Parsed;

        return new Tuple<bool, string>(true, "OK!"); 
    }

    private void CleanUpApplied()
    {
        mVariable1Parsed = 0f;
        mVariable2Parsed = 0f;
        mVariable1 = "0";
        mVariable2 = "0";
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeCreateSize();
    }

    void Switch(int i)
    {
        mNumCurrEditedPolygon = i;
        mCurrEditPolygonComponent = polygons[mNumCurrEditedPolygon].GetComponent<Polygon>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mMouseInputMode)
        {
            if (Input.GetButtonDown("Fire2"))
            {
                Vector3 p = Input.mousePosition;
                Vector3 pos = Camera.main.ScreenToWorldPoint(p);
                pos.z = 0;
                mPointsForNew[mCurrentPointToAdd] = pos;
                mCurrentPointToAdd++;

                if (mCurrentPointToAdd == mPointsForNew.Length)
                {
                    var newPolygon = new GameObject("Polygon_" + System.Guid.NewGuid());
                    newPolygon.AddComponent<Polygon>();
                    newPolygon.GetComponent<Polygon>().SetPoints(mPointsForNew);
                    polygons.Add(newPolygon);
                    ChangeCreateSize();
                    mMouseInputMode = false;
                    Switch(polygons.Count - 1);
                    mGlobalText = "Added";
                }
                else
                {
                    mGlobalText = "RMB to set control point " + (mCurrentPointToAdd + 1).ToString();
                }
            }
        }
    }
}
