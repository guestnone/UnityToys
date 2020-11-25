using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EditMode
{
    Dormant,
    AddPos,
    AddControlPoint,
}

public class UserGUI : MonoBehaviour
{
    public List<GameObject> splines = new List<GameObject>();

    private int mCurrMode = 0;
    private string[] mToolbarStrings = {"Create", "Edit"};
    private string mGlobalText = "Ready";

    // Create Mode
    private Vector3[] mControlPointsForNew;
    private uint mNumMainPoints = 2;
    private bool mMouseInputMode = false;
    private Vector2 mScrollViewVectorCreateMode = Vector2.zero;
    private Vector2 mScrollViewVectorEditMode = Vector2.zero;

    // Edit Mode
    private int mNumCurrEditedSpline = -1;
    private Vector3[] mControlPointsForEdit;
    private BezierSpline mCurrEditSplineComponent;

    // Edit with mouse
    private int mCurrentControlPointToAdd = 0;

    void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(10, Screen.height - 400, 400, 400), "Editor Menu");
        var switchedMode = GUI.Toolbar(new Rect(25, Screen.height - 380, 150, 20), mCurrMode, mToolbarStrings);
        bool switching = false;
        if (switchedMode != mCurrMode)
        {
            switching = true;
            mCurrMode = switchedMode;
        }

        if (mCurrMode == 0) // Create
        {
            GUI.Label (new Rect (20, Screen.height - 360, 50, 50), "Degree");
            mNumMainPoints =
                (uint)Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(70, Screen.height - 357, 70, 10), mNumMainPoints, 2f,
                    20f));
            if (GUI.changed)
            {
                ChangeCreateSize();
            }
            GUI.Label (new Rect (150, Screen.height - 360, 50, 50), mNumMainPoints.ToString());

            mScrollViewVectorCreateMode = GUI.BeginScrollView (new Rect (20, Screen.height - 340, 380, 270),
                mScrollViewVectorCreateMode, new Rect (0, 0, 1000, 400));
            for (int i = 0; i < mControlPointsForNew.Length; i++)
            {
                // TODO: This is a shit hack that makes this program only run on Unity editor, but this is for showing and it works so IDC.
                //       The only thing that it doesn't works is the actual text input but I can move the handles around and it saves so there's that. 
                //       Blame Unity for not providing proper Vector3Field for non-editor use.
                mControlPointsForNew[i] = EditorGUI.Vector3Field(new Rect(0, i * 20, 300, 20),
                    i.ToString(),
                    mControlPointsForNew[i]);
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(20, Screen.height - 65, 100, 20), "Mouse Add"))
            {
                mCurrentControlPointToAdd = 0;
                mGlobalText = "RMB to set control point " + (mCurrentControlPointToAdd + 1).ToString();
                mMouseInputMode = true;
            }

            if (GUI.Button(new Rect(120, Screen.height - 65, 100, 20), "Input Add"))
            {
                var newSpline = new GameObject("Spline_" + System.Guid.NewGuid());
                newSpline.AddComponent<BezierSpline>();
                newSpline.GetComponent<BezierSpline>().SetPoints(mControlPointsForNew);
                splines.Add(newSpline);
                ChangeCreateSize();
                if (mNumCurrEditedSpline == -1)
                {
                    Switch(splines.Count - 1);
                }
            }
        }
        else // Edit
        {
            if (splines.Count != 0)
            {
                if (mNumCurrEditedSpline == -1)
                {
                    mNumCurrEditedSpline = 0;
                }
                GUI.Label (new Rect (20, Screen.height - 360, 80, 50), "Spline ID");
                mNumCurrEditedSpline =
                    Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(70, Screen.height - 357, 70, 10), mNumCurrEditedSpline, 0f,
                        (float)splines.Count-1));
                if (GUI.changed)
                {
                    Switch(mNumCurrEditedSpline);
                }
                GUI.Label (new Rect (150, Screen.height - 360, 50, 50), mNumCurrEditedSpline.ToString());

                mScrollViewVectorEditMode = GUI.BeginScrollView (new Rect (20, Screen.height - 340, 380, 270),
                    mScrollViewVectorEditMode, new Rect (0, 0, 1000, 400));
                for (int i = 0; i < mControlPointsForEdit.Length; i++)
                {
                    // TODO: This is a shit hack that makes this program only run on Unity editor, but this is for showing and it works so IDC.
                    //       The only thing that it doesn't works is the actual text input but I can move the handles around and it saves so there's that. 
                    //       Blame Unity for not providing proper Vector3Field for non-editor use.
                    mControlPointsForEdit[i] = EditorGUI.Vector3Field(new Rect(0, i * 20, 300, 20),
                        i.ToString(),
                        mControlPointsForEdit[i]);
                }
                GUI.EndScrollView();

                if (GUI.Button(new Rect(20, Screen.height - 65, 100, 20), "Apply"))
                {
                    splines[mNumCurrEditedSpline].GetComponent<BezierSpline>().SetPoints(mControlPointsForEdit);
                    mGlobalText = "Applied";
                }
                GUI.Label (new Rect (20, Screen.height - 40, 390, 20), "WARNING: To update handles, remember to click Apply!");
            }
            else
            {
                GUI.Label (new Rect (20, Screen.height - 360, 50, 50), "No splines added");
            }
        }
        GUI.Label (new Rect (20, Screen.height - 20, 300, 300), mGlobalText);
    }

    void AddFromText()
    {
        
    }

    void Switch(int i)
    {
        if (mCurrEditSplineComponent != null)
        {
            mCurrEditSplineComponent.SetSelected(false);
        }

        mNumCurrEditedSpline = i;
        mCurrEditSplineComponent = splines[mNumCurrEditedSpline].GetComponent<BezierSpline>();
        mControlPointsForEdit = mCurrEditSplineComponent.points;
        mCurrEditSplineComponent.SetSelected(true);
    }

    uint ComputeNumControlPoints(uint mainPoints)
    {
        return mainPoints + ((mainPoints - 1) * 2);
    }

    void ChangeCreateSize()
    {
        mControlPointsForNew = new Vector3[ComputeNumControlPoints(mNumMainPoints)];
        for(int i = 0; i < mControlPointsForNew.Length; i++)
        {
            mControlPointsForNew[i] = Vector3.zero;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeCreateSize();
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
                mControlPointsForNew[mCurrentControlPointToAdd] = pos;
                mCurrentControlPointToAdd++;

                if (mCurrentControlPointToAdd == mControlPointsForNew.Length)
                {
                    var newSpline = new GameObject("Spline_" + System.Guid.NewGuid());
                    newSpline.AddComponent<BezierSpline>();
                    newSpline.GetComponent<BezierSpline>().SetPoints(mControlPointsForNew);
                    splines.Add(newSpline);
                    ChangeCreateSize();
                    mMouseInputMode = false;
                    Switch(splines.Count - 1);
                    mGlobalText = "Added";
                }
                else
                {
                    mGlobalText = "RMB to set control point " + (mCurrentControlPointToAdd + 1).ToString();
                }
            }
        }
    }
}
