using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class BetweenExt
{
    public static bool IsBetween<T>(this T item, T start, T end)
    {
        return Comparer<T>.Default.Compare(item, start) >= 0
               && Comparer<T>.Default.Compare(item, end) <= 0;
    }
}

public class ColorConvertGui : MonoBehaviour
{
    private int mRed = 0;
    private int mGreen = 0;
    private int mBlue = 0;
    private float mCyan = 0;
    private float mMagenta = 0;
    private float mYellow = 0;
    private float mBlack = 1;

    // string versions used for text box input
    private string mRedStr = "0";
    private string mGreenStr = "0";
    private string mBlueStr = "0";
    private string mCyanStr = "0";
    private string mMagentaStr ="0";
    private string mYellowStr = "0";
    private string mBlackStr = "1";
    private string mInfoBarText = "OK!";

    private int mCurrMode = 0;
    private string[] mToolbarStrings = {"RGB", "CMYK"};
    private GUIStyle mRgbGuiStyle;
    private bool mIsChange = false;
    private Regex mRegex = new Regex("^[a-zA-Z ]*$");

    private static float clampCmyk(float value)
    {
        if (value < 0 || float.IsNaN(value))
        {
            value = 0;
        }

        return value;
    }

    private void rgbToCmyk()
    {
        var redFloat = mRed / 255F;
        var greenFloat = mGreen / 255F;
        var blueFloat = mBlue / 255F;

        mBlack = clampCmyk(1 - Mathf.Max(new float[]{redFloat, greenFloat, blueFloat}));
        mCyan = clampCmyk((1 - redFloat - mBlack / (1 - mBlack)));
        mMagenta =  clampCmyk((1 - greenFloat - mBlack) / (1 - mBlack));
        mYellow = clampCmyk((1 - blueFloat - mBlack) / (1 - mBlack));

    }

    private void cmykToRgb()
    {
        mRed = Mathf.RoundToInt(255 * (1 - mCyan) * (1 - mBlack));
        mGreen = Mathf.RoundToInt((255 * (1 - mMagenta) * (1 - mBlack)));
        mBlue = Mathf.RoundToInt(255 * (1 - mYellow) * (1 - mBlack));
    }

    private Texture2D makeGuiTexture( int width, int height, Color col )
    {
        Color[] rawTextureColors = new Color[width * height];
        for( int i = 0; i < rawTextureColors.Length; ++i )
        {
            rawTextureColors[ i ] = col;
        }
        Texture2D texture = new Texture2D( width, height );
        texture.SetPixels( rawTextureColors );
        texture.Apply();
        return texture;
    }

    void OnGUI ()
    {
        initStyles();
        GUI.Box(new Rect(10,10,400,420), "Color Converted Menu");
        GUI.Label(new Rect(20,30,400,20), "Cube can be rotated by pressing space and moving the mouse.");
        var switchedMode = GUI.Toolbar (new Rect (25, 50, 250, 30), mCurrMode, mToolbarStrings);

        bool switching = false;
        if (switchedMode != mCurrMode)
        {
            switching = true;
            mCurrMode = switchedMode;
        }


        if (mCurrMode == 0) // RGB
        {
            GUI.Label (new Rect(20, 80, 100, 20), "Red");
            mRed = Mathf.RoundToInt(GUI.HorizontalSlider (new Rect(20, 100, 100, 20), mRed, 0f, 255f));
            if (GUI.changed && !switching)
            {
                rgbToCmyk();
                mIsChange = true;
            }


            GUI.Label (new Rect(20, 120, 100, 20), "Green");
            mGreen = Mathf.RoundToInt(GUI.HorizontalSlider (new Rect(20, 140, 100, 20), mGreen, 0f, 255f));
            if (GUI.changed  && !switching)
            {
                rgbToCmyk();
                mIsChange = true;
            }

            GUI.Label (new Rect(20, 160, 100, 20), "Blue");
            mBlue = Mathf.RoundToInt(GUI.HorizontalSlider (new Rect(20, 180, 100, 20), mBlue, 0f, 255f));
            if (GUI.changed  && !switching)
            {
                rgbToCmyk();
                mIsChange = true;
            }

        }
        else // CMYK
        {
            GUI.Label (new Rect(20, 80, 100, 20), "Cyan");
            mCyan = GUI.HorizontalSlider (new Rect(20, 100, 100, 20), mCyan, 0.0f, 1.0f);
            if (GUI.changed  && !switching)
            {
                cmykToRgb();
                mIsChange = true;
            }

            GUI.Label (new Rect(20, 120, 100, 20), "Magenta");
            mMagenta = GUI.HorizontalSlider (new Rect(20, 140, 100, 20), mMagenta, 0f, 1f);
            if (GUI.changed  && !switching)
            {
                cmykToRgb();
                mIsChange = true;
            }

            GUI.Label (new Rect(20, 160, 100, 20), "Yellow");
            mYellow = GUI.HorizontalSlider (new Rect(20, 180, 100, 20), mYellow, 0f, 1f);
            if (GUI.changed && !switching)
            {
                cmykToRgb();
                mIsChange = true;
            }

            GUI.Label (new Rect(20, 200, 100, 20), "Black");
            mBlack = GUI.HorizontalSlider (new Rect(20, 220, 100, 20), mBlack, 0f, 1f);
            if (GUI.changed  && !switching)
            {
                cmykToRgb();
                mIsChange = true;
            }
        }

        if (mIsChange)
        {
            mRgbGuiStyle.normal.background = makeGuiTexture(2, 2, new Color(mRed / 255f, mGreen / 255f, mBlue / 255f, 1.0f));
            if (mCurrMode == 0)
            {
                mRedStr = GUI.TextField(new Rect(120, 80, 100, 20), mRed.ToString(), 3);
                mGreenStr = GUI.TextField(new Rect(120, 120, 100, 20), mGreen.ToString(), 3);
                mBlueStr = GUI.TextField(new Rect(120, 160, 100, 20), mBlue.ToString(), 3);
            }
            else
            {
                mCyanStr = GUI.TextField(new Rect(120, 80, 100, 20), mCyan.ToString(), 16);
                mMagentaStr = GUI.TextField(new Rect(120, 120, 100, 20), mMagenta.ToString(), 16);
                mYellowStr = GUI.TextField(new Rect(120, 160, 100, 20), mYellow.ToString(), 16);
                mBlackStr = GUI.TextField(new Rect(120, 200, 100, 20), mBlack.ToString(), 16);
            }
            mIsChange = false;
        }
        else
        {
            if (mCurrMode == 0) // RGB
            {
                mRedStr = GUI.TextField(new Rect(120, 80, 100, 20), mRedStr, 3);
                mGreenStr = GUI.TextField(new Rect(120, 120, 100, 20), mGreenStr, 3);
                mBlueStr = GUI.TextField(new Rect(120, 160, 100, 20), mBlueStr, 3);
            }
            else // CMYK
            {
                mCyanStr = GUI.TextField(new Rect(120, 80, 100, 20), mCyanStr, 16);
                mMagentaStr = GUI.TextField(new Rect(120, 120, 100, 20), mMagentaStr, 16);
                mYellowStr = GUI.TextField(new Rect(120, 160, 100, 20), mYellowStr, 16);
                mBlackStr = GUI.TextField(new Rect(120, 200, 100, 20), mBlackStr, 16);
            }
        }

        if (GUI.Button(new Rect(240, 200, 150, 20), "Apply from value box"))
        {
            if (mCurrMode == 0) // RGB
            {
                var ret = applyFromValueBoxRgb();
                if (ret.Item1)
                {
                    rgbToCmyk();
                    mRgbGuiStyle.normal.background = makeGuiTexture(2, 2, new Color(mRed / 255f, mGreen / 255f, mBlue / 255f, 1.0f));
                }
                mInfoBarText = ret.Item2;
            }
            else // CMYK
            {
                var ret = applyFromValueBoxCmyk();
                if (ret.Item1)
                {
                    cmykToRgb();
                    mRgbGuiStyle.normal.background = makeGuiTexture(2, 2, new Color(mRed / 255f, mGreen / 255f, mBlue / 255f, 1.0f));
                }
                mInfoBarText = ret.Item2;
            }
        }

        var rgbWindowRect = GUI.Window (0, new Rect(20, 240, 300, 70), WindowFunctionRGB, "RGB", mRgbGuiStyle);
        var cmykWindowRect = GUI.Window (1, new Rect(20, 320, 390, 70), WindowFunctionCmyk, "CMYK (Values only, Unity only supports RGB colors)          ");
        GUI.Label (new Rect(20, 400, 250, 20), mInfoBarText);
    }

    private Tuple<bool, string> applyFromValueBoxRgb()
    {
        int redParsed;
        int greenParsed;
        int blueParsed;

        // Validate if we can parse
        try
        {
            redParsed = Convert.ToInt32(mRedStr);
        }
        catch
        {
            return new Tuple<bool, string>(false, "Red input box contains illegal values"); 
        }

        try
        {
            greenParsed = Convert.ToInt32(mGreenStr);
        }
        catch
        {
            return new Tuple<bool, string>(false, "Green input box contains illegal values"); 
        }

        try
        {
            blueParsed = Convert.ToInt32(mBlueStr);
        }
        catch
        {
            return new Tuple<bool, string>(false, "Blue input box contains illegal values"); 
        }

        // Validate if this is in the range
        if (!redParsed.IsBetween(0, 255))
        {
            return new Tuple<bool, string>(false, "Red input box is not between 0 and 255"); 
        }
        if (!greenParsed.IsBetween(0, 255))
        {
            return new Tuple<bool, string>(false, "Green input box is not between 0 and 255"); 
        }
        if (!blueParsed.IsBetween(0, 255))
        {
            return new Tuple<bool, string>(false, "Blue input box is not between 0 and 255"); 
        }

        mRed = redParsed;
        mGreen = greenParsed;
        mBlue = blueParsed;
        return new Tuple<bool, string>(true, "OK!"); 
    }

    private Tuple<bool, string> applyFromValueBoxCmyk()
    {
        float cyanParsed;
        float magentaParsed;
        float yellowParsed;
        float blackParsed;

        // Validate if we can parse
        try
        {
            cyanParsed = Convert.ToSingle(mCyanStr);
        }
        catch
        {
            return new Tuple<bool, string>(false, "Cyan input box contains illegal values"); 
        }

        try
        {
            magentaParsed = Convert.ToSingle(mMagentaStr);
        }
        catch
        {
            return new Tuple<bool, string>(false, "Magenta input box contains illegal values"); 
        }

        try
        {
            yellowParsed = Convert.ToSingle(mYellowStr);
        }
        catch
        {
            return new Tuple<bool, string>(false, "Yellow input box contains illegal values"); 
        }

        try
        {
            blackParsed = Convert.ToSingle(mBlackStr);
        }
        catch
        {
            return new Tuple<bool, string>(false, "Black input box contains illegal values"); 
        }

        // Validate if this is in the range
        if (!cyanParsed.IsBetween(0.000f, 1f))
        {
            return new Tuple<bool, string>(false, "Cyan input box is not between 0.0 and 1.0"); 
        }
        if (!magentaParsed.IsBetween(0.000f, 1f))
        {
            return new Tuple<bool, string>(false, "Magenta input box is not between 0.0 and 1.0"); 
        }
        if (!yellowParsed.IsBetween(0.000f, 1f))
        {
            return new Tuple<bool, string>(false, "Yellow input box is not between 0.0 and 1.0"); 
        }
        if (!blackParsed.IsBetween(0.000f, 1f))
        {
            return new Tuple<bool, string>(false, "Black input box is not between 0.0 and 1.0"); 
        }

        mCyan = cyanParsed;
        mMagenta = magentaParsed;
        mYellow = yellowParsed;
        mBlack = blackParsed;
        cmykToRgb();

        return new Tuple<bool, string>(true, "OK!"); 
    }

    void WindowFunctionRGB (int windowID) 
    {
        // Draw any Controls inside the window here
        GUI.Label (new Rect(20, 20, 150, 30), "r: " + mRed.ToString() + " g: " + mGreen.ToString() + " b: " + mBlue.ToString());
    }

    void WindowFunctionCmyk(int windowID)
    {
        GUI.Label (new Rect(20, 20, 390, 30), "c: " + mCyan.ToString() + " m: " + mMagenta.ToString() + " y: " + mYellow.ToString() + " k: " + mBlack.ToString());
    }

    void initStyles()
    {
        if (mRgbGuiStyle == null)
        {
            mRgbGuiStyle = new GUIStyle( GUI.skin.box );
            mRgbGuiStyle.normal.background = makeGuiTexture( 2, 2, new Color( 0f, 0f, 0f) );
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
