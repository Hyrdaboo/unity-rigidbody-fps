using UnityEditor;
using UnityEngine;

// Usage:
// [PreviewTexture(100, 100)]
// public Texture2D someTexture;

#if UNITY_EDITOR

public enum TextureAlignment {left, center, right}

public class PreviewTextureAttribute : PropertyAttribute
{
    public readonly int width;
    public readonly int height;
    public readonly TextureAlignment alignment;

    /// <summary>
    /// Create a preview of a Texture2D
    /// </summary>
    /// <param name="width">Width of the preview texture</param>
    /// <param name="height">Height of the preview texture</param>
    public PreviewTextureAttribute(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
    /// <summary>
    /// Create a preview of a Texture2D with alignment
    /// </summary>
    /// <param name="width">Width of the preview texture</param>
    /// <param name="height">Height of the preview texture</param>
    /// <param name="alignment">Alignment of the preview texture</param>
    public PreviewTextureAttribute(int width, int height, TextureAlignment alignment)
    {
        this.width = width;
        this.height = height;
        this.alignment = alignment;
    }
}

[CustomPropertyDrawer(typeof(PreviewTextureAttribute))]
public class PreviewTextureDrawer : PropertyDrawer
{
    System.Type fieldType;
    Texture2D preview;
    int width;
    int height;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        fieldType = property.serializedObject.targetObject.GetType().GetField(property.name).FieldType;

        if (fieldType == typeof(Texture2D))
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;

            var texAttribute = attribute as PreviewTextureAttribute;
            width = texAttribute.width;
            height = texAttribute.height;

            Rect imageRect = new Rect(position.x, position.y, width, height);
            Rect rect = new Rect(position.x, position.y + height, position.width, lineHeight);

            preview = (Texture2D)property.objectReferenceValue;
            if (preview != null)
            {
                GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                skin.label.alignment = TextAnchor.MiddleCenter;
                imageRect.x = GetXBasedOnAlignment(texAttribute.alignment, position);
                EditorGUI.DrawPreviewTexture(imageRect, preview);
                EditorGUI.PropertyField(rect, property, label);
                return;
            }
        }
        EditorGUI.PropertyField(position, property, label);
    }

    private float GetXBasedOnAlignment(TextureAlignment alignment, Rect position)
    {

        if (alignment == TextureAlignment.center)
        {
            return position.x + (position.width - width) / 2;
        }
        else if (alignment == TextureAlignment.right)
        {
            return position.width - width;
        }
        else
        {
            return position.x;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (fieldType != typeof(Texture2D) || preview == null)
        {
            return base.GetPropertyHeight(property, label);
        }

        return height + EditorGUIUtility.singleLineHeight;
    }
}

#endif
