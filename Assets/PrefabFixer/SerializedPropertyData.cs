using UnityEditor;
using UnityEngine;

namespace Kamgam.PF
{
    public class SerializedPropertyData
    {
        public bool HasKnownDataType;
        public SerializedPropertyType PropertyType;

        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public string StringValue;
        public Vector2 Vector2Value;
        public Vector3 Vector3Value;
        public Vector4 Vector4Value;
        public Vector2Int Vector2IntValue;
        public Vector3Int Vector3IntValue;
        public Color ColorValue;
        public Object ObjectReferenceValue;
        public int EnumValueIndex;
        public Rect RectValue;
        public RectInt RectIntValue;
        public int ArraySize;
        public AnimationCurve AnimationCurveValue;
        public Bounds BoundsValue;
        public BoundsInt BoundsIntValue;
        public Object ExposedReferenceValue;
        // public int FixedBufferSize; // Can not be set. :(
        public Quaternion QuaternionValue;
        public Gradient GradientValue;

        public SerializedPropertyData()
        {
            HasKnownDataType = false;
        }

        public SerializedPropertyData(SerializedProperty property)
        {
            SetData(property);
        }

        public void SetData(SerializedProperty property)
        {
            PropertyType = property.propertyType;
            HasKnownDataType = true;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    HasKnownDataType = false;
                    break;
                case SerializedPropertyType.Integer:
                    IntValue = property.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    BoolValue = property.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    FloatValue = property.floatValue;
                    break;
                case SerializedPropertyType.String:
                    StringValue = property.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    ColorValue = property.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    ObjectReferenceValue = property.objectReferenceValue;
                    break;
                case SerializedPropertyType.LayerMask:
                    IntValue = property.intValue; // Unity Source does the same.
                    break;
                case SerializedPropertyType.Enum:
                    EnumValueIndex = property.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    Vector2Value = property.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    Vector3Value = property.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    Vector4Value = property.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    RectValue = property.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    HasKnownDataType = false;
                    break;
                case SerializedPropertyType.Character:
                    IntValue = property.intValue; // Unity Source does the same.
                    break;
                case SerializedPropertyType.AnimationCurve:
                    AnimationCurveValue = property.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    BoundsValue = property.boundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    GradientValue = GetGradient(property);
                    break;
                case SerializedPropertyType.Quaternion:
                    QuaternionValue = property.quaternionValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    ExposedReferenceValue = property.exposedReferenceValue;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    // Can not be set. Thus we also have to mark it as unkown here.
                    HasKnownDataType = false;
                    /*
                    if (property.isFixedBuffer)
                        FixedBufferSize = property.fixedBufferSize;
                    else
                        FixedBufferSize = 0;
                    */
                    break;
                case SerializedPropertyType.Vector2Int:
                    Vector2IntValue = property.vector2IntValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    Vector3IntValue = property.vector3IntValue;
                    break;
                case SerializedPropertyType.RectInt:
                    RectIntValue = property.rectIntValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    BoundsIntValue = property.boundsIntValue;
                    break;
                case SerializedPropertyType.ManagedReference:
                    HasKnownDataType = false;
                    break;
                default:
                    HasKnownDataType = false;
                    break;
            }
        }

        public void CopyDataTo(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = IntValue;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = BoolValue;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = FloatValue;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = StringValue;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = ColorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = ObjectReferenceValue;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = IntValue; // Unity Source does the same.
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = EnumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = Vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = Vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = Vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = RectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = IntValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = AnimationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = BoundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    SetGradient(property, GradientValue);
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = QuaternionValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = ExposedReferenceValue;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    // Can not be set. :(
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = Vector2IntValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = Vector3IntValue;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = RectIntValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = BoundsIntValue;
                    break;
                case SerializedPropertyType.ManagedReference:
                    break;
                default:
                    break;
            }
        }

        public void Clear()
        {
            switch (PropertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    IntValue = 0;
                    break;
                case SerializedPropertyType.Boolean:
                    BoolValue = false;
                    break;
                case SerializedPropertyType.Float:
                    FloatValue = 0f;
                    break;
                case SerializedPropertyType.String:
                    StringValue = null;
                    break;
                case SerializedPropertyType.Color:
                    ColorValue = Color.white;
                    break;
                case SerializedPropertyType.ObjectReference:
                    ObjectReferenceValue = null;
                    break;
                case SerializedPropertyType.LayerMask:
                    IntValue = default;
                    break;
                case SerializedPropertyType.Enum:
                    EnumValueIndex = -1;
                    break;
                case SerializedPropertyType.Vector2:
                    Vector2Value = default;
                    break;
                case SerializedPropertyType.Vector3:
                    Vector3Value = default;
                    break;
                case SerializedPropertyType.Vector4:
                    Vector4Value = default;
                    break;
                case SerializedPropertyType.Rect:
                    RectValue = default;
                    break;
                case SerializedPropertyType.ArraySize:
                    HasKnownDataType = false;
                    break;
                case SerializedPropertyType.Character:
                    IntValue = default;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    AnimationCurveValue = default;
                    break;
                case SerializedPropertyType.Bounds:
                    BoundsValue = default;
                    break;
                case SerializedPropertyType.Gradient:
                    GradientValue = default;
                    break;
                case SerializedPropertyType.Quaternion:
                    QuaternionValue = default;
                    break;
                case SerializedPropertyType.ExposedReference:
                    ExposedReferenceValue = default;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    // FixedBufferSize = default;
                    break;
                case SerializedPropertyType.Vector2Int:
                    Vector2IntValue = default;
                    break;
                case SerializedPropertyType.Vector3Int:
                    Vector3IntValue = default;
                    break;
                case SerializedPropertyType.RectInt:
                    RectIntValue = default;
                    break;
                case SerializedPropertyType.BoundsInt:
                    BoundsIntValue = default;
                    break;
                case SerializedPropertyType.ManagedReference:
                    HasKnownDataType = false;
                    break;
                default:
                    HasKnownDataType = false;
                    break;
            }
        }

        public object GetData()
        {
            switch (PropertyType)
            {
                case SerializedPropertyType.Generic:
                    return null;
                case SerializedPropertyType.Integer:
                    return IntValue;
                case SerializedPropertyType.Boolean:
                    return BoolValue;
                case SerializedPropertyType.Float:
                    return FloatValue;
                case SerializedPropertyType.String:
                    return StringValue;
                case SerializedPropertyType.Color:
                    return ColorValue;
                case SerializedPropertyType.ObjectReference:
                    return ObjectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return IntValue;
                case SerializedPropertyType.Enum:
                    return EnumValueIndex;
                case SerializedPropertyType.Vector2:
                    return Vector2Value;
                case SerializedPropertyType.Vector3:
                    return Vector3Value;
                case SerializedPropertyType.Vector4:
                    return Vector4Value;
                case SerializedPropertyType.Rect:
                    return RectValue;
                case SerializedPropertyType.ArraySize:
                    return ArraySize;
                case SerializedPropertyType.Character:
                    return IntValue;
                case SerializedPropertyType.AnimationCurve:
                    return AnimationCurveValue;
                case SerializedPropertyType.Bounds:
                    return BoundsValue;
                case SerializedPropertyType.Gradient:
                    return GradientValue;
                case SerializedPropertyType.Quaternion:
                    return QuaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return ExposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    //return FixedBufferSize == property.fixedBufferSize; 
                    return null;
                case SerializedPropertyType.Vector2Int:
                    return Vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return Vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return RectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return BoundsIntValue;
                case SerializedPropertyType.ManagedReference:
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns false if the data type is unkown (use HasKnownDataType to check that beforehand).
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public bool DataEquals(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return false;
                case SerializedPropertyType.Integer:
                    return IntValue == property.intValue;
                case SerializedPropertyType.Boolean:
                    return BoolValue == property.boolValue;
                case SerializedPropertyType.Float:
                    return FloatValue == property.floatValue;
                case SerializedPropertyType.String:
                    return StringValue == property.stringValue;
                case SerializedPropertyType.Color:
                    return ColorValue == property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return ObjectReferenceValue == property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return IntValue == property.intValue;
                case SerializedPropertyType.Enum:
                    return EnumValueIndex == property.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return Vector2Value == property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return Vector3Value == property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return Vector4Value == property.vector4Value;
                case SerializedPropertyType.Rect:
                    return RectValue == property.rectValue;
                case SerializedPropertyType.ArraySize:
                    if (property.isArray)
                        return ArraySize == property.arraySize;
                    else
                        return false;
                case SerializedPropertyType.Character:
                    return IntValue == property.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return AnimationCurveValue == property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return BoundsValue == property.boundsValue;
                case SerializedPropertyType.Gradient:
                    return GradientValue == GetGradient(property);
                case SerializedPropertyType.Quaternion:
                    return QuaternionValue == property.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return ExposedReferenceValue == property.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    //return FixedBufferSize == property.fixedBufferSize; 
                    return false;
                case SerializedPropertyType.Vector2Int:
                    return Vector2IntValue == property.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return Vector3IntValue == property.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return RectIntValue.Equals(property.rectIntValue);
                case SerializedPropertyType.BoundsInt:
                    return BoundsIntValue == property.boundsIntValue;
                case SerializedPropertyType.ManagedReference:
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get a Gradient from a SerializedProperty of a Gradient.
        /// Thanks to: https://forum.unity.com/threads/get-a-gradient-from-a-property.1189453/#post-7609402
        /// </summary>
        /// <param name="gradientProperty"></param>
        /// <returns></returns>
        public static Gradient GetGradient(SerializedProperty gradientProperty)
        {
            var propertyInfo = getGradientPropertyInfo();
            if (propertyInfo == null) { return null; }
            else { return propertyInfo.GetValue(gradientProperty, null) as Gradient; }
        }

        public static void SetGradient(SerializedProperty gradientProperty, Gradient gradient)
        {
            var propertyInfo = getGradientPropertyInfo();
            if (propertyInfo == null) { return; }
            else { propertyInfo.SetValue(gradientProperty, gradient); }
        }

        static System.Reflection.PropertyInfo getGradientPropertyInfo()
        {
            return typeof(SerializedProperty).GetProperty("gradientValue", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
    }
}