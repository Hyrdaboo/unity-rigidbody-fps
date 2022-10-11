using System.Collections;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [Range(0.001f, 1f)]
    public float displayRate = .05f;
    [Range(1, 240)]
    [Tooltip("Higher values mean more precise results but slower calculation")]
    public int bufferLength = 30;
    [Min(1)]
    public int fontSizePercentage = 3;
    [Header("Style")]
    public TextAnchor alignment = TextAnchor.MiddleLeft;
    public int paddingLeft = 0;
    public int paddingRight = 0;
    public int paddingTop = 0;
    public int paddingBottom = 0;

    private GUIStyle style;
    private void OnEnable()
    {
        style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        StartCoroutine(updateFps());
    }

    private void OnGUI()
    {
        int resW = Screen.width;
        int size = fontSizePercentage * resW / 100;
        style.fontSize = size;
        style.alignment = alignment;
        style.padding.right = paddingRight;
        style.padding.left = paddingLeft;
        style.padding.top = paddingTop;
        style.padding.bottom = paddingBottom;

        averageFps = roundToTenth(averageFps);
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.Label($"Fps: {averageFps}", style);
        GUILayout.EndArea();
    }


    private float averageFps = 0;
    private float[] fpsBuffer;
    private int fpsBufferIndex = 0;

    IEnumerator updateFps()
    {
        while (true)
        {
            yield return new WaitForSeconds(displayRate);

            if (fpsBuffer == null || fpsBuffer.Length != bufferLength)
            {
                InitializeBuffer();
            }

            UpdateBuffer();
            CalculateFps();
        }
    }

    private void InitializeBuffer()
    {
        fpsBuffer = new float[bufferLength];
        fpsBufferIndex = 0;
    }
    private void UpdateBuffer()
    {
        fpsBuffer[fpsBufferIndex++] = 1.0f / Time.unscaledDeltaTime;
        fpsBufferIndex %= bufferLength;
    }
    private void CalculateFps()
    {
        float sum = 0;
        foreach (float f in fpsBuffer)
        {
            sum += f;
        }
        averageFps = sum / bufferLength;
    }

    private float roundToTenth(float num)
    {
        num *= 10;
        return (int)num / 10.0f;
    }
}
