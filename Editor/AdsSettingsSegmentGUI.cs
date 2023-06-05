using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DGames.Ads
{
    public class AdsSettingsSegmentGUI : SettingSegmentSetGUIWithSymbols
    {
        private readonly SerializedObject _adsSettings;

        public AdsSettingsSegmentGUI(SerializedObject adsSettings) : base(
            new AdmobSettingsSegmentGUI(adsSettings.FindProperty(AdsSettings.IOS_ADMOB_SETTING_FIELD),
                adsSettings.FindProperty(AdsSettings.ANDROID_ADMOB_SETTING_FIELD)), new UnitySettingsSegmentGUI(
                adsSettings.FindProperty(AdsSettings.IOS_UNITY_ADS_SETTING_FIELD),
                adsSettings.FindProperty(AdsSettings.ANDROID_UNITY_ADS_SETTING_FIELD)))
        {
            _adsSettings = adsSettings;
        }

        protected override void DrawGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ads Settings", EditorStyles.boldLabel);
            // _adsSettings.isExpanded = EditorGUILayout.ToggleLeft("", _adsSettings.isExpanded);
            EditorGUILayout.EndHorizontal();
            // if (_adsSettings.isExpanded)
            // {
            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox("Interval between interstitial ads at Game Over will between this two value",
                MessageType.Info);
            EditorGUILayout.PropertyField(
                _adsSettings.FindProperty(AdsSettings.MIN_AND_MAX_GAME_OVERS_BETWEEN_INTERSTITIAL_ADS_FIELD));


            DrawChildrenInOrder();
            EditorGUI.indentLevel--;
            // }

            EditorGUILayout.EndVertical();
        }

        protected override void ApplyModifiedProperties()
        {
            _adsSettings.ApplyModifiedProperties();
        }

    }
    
    public class AdmobSettingsSegmentGUI : SettingSegmentSetGUIWithSymbols
    {
        private readonly SerializedProperty _iosAdmobSetting;
        private readonly AdmobPlatformSegmentGUI _androidSegmentGUI;
        private readonly AdmobPlatformSegmentGUI _iOSSegmentGUI;

        public AdmobSettingsSegmentGUI(SerializedProperty iosAdmobSetting, SerializedProperty androidAdmobSetting) :
            base(
                new AdmobPlatformSegmentGUI(androidAdmobSetting, "Android", BuildTargetGroup.Android),
                new AdmobPlatformSegmentGUI(iosAdmobSetting, "iOS", BuildTargetGroup.iOS))
        {
            _iosAdmobSetting = iosAdmobSetting;
        }


        protected override void DrawGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            _iosAdmobSetting.isExpanded = EditorGUILayout.Foldout(_iosAdmobSetting.isExpanded, "Admob Setting");

            if (_iosAdmobSetting.isExpanded)
            {
                EditorGUILayout.Space();
                DrawChildrenInOrder();
            }

            EditorGUILayout.EndVertical();
        }

        protected override void ApplyModifiedProperties()
        {
            _iosAdmobSetting.serializedObject.ApplyModifiedProperties();
        }
    }
    
    public class UnitySettingsSegmentGUI : SettingSegmentSetGUIWithSymbols
    {
        private readonly SerializedProperty _iosSetting;

        public UnitySettingsSegmentGUI(SerializedProperty iosSetting, SerializedProperty androidSetting) : base(
            new UnityPlatformSegmentGUI(androidSetting, "Android", BuildTargetGroup.Android),
            new UnityPlatformSegmentGUI(iosSetting, "iOS", BuildTargetGroup.iOS))
        {
            _iosSetting = iosSetting;
        }

        protected override void DrawGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            _iosSetting.isExpanded = EditorGUILayout.Foldout(_iosSetting.isExpanded, "Unity Setting");

            if (_iosSetting.isExpanded)
            {
                EditorGUILayout.Space();
                DrawChildrenInOrder();
            }

            EditorGUILayout.EndVertical();
        }

        protected override void ApplyModifiedProperties()
        {
            _iosSetting.serializedObject.ApplyModifiedProperties();
        }
    }
    
    public class UnityPlatformSegmentGUI : SettingSegmentGUIWithSymbols
    {
        private readonly SerializedProperty _adsSetting;
        private readonly string _title;
        private readonly BuildTargetGroup _group;

        public UnityPlatformSegmentGUI(SerializedProperty adsSetting, string title,
            BuildTargetGroup group)
        {
            _adsSetting = adsSetting;
            _title = title;
            _group = group;
        }

        protected override void DrawGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            var enableProperty = _adsSetting.FindPropertyRelative(nameof(UnityAdsSetting.enable));
            var sdkProperty = _adsSetting.FindPropertyRelative(nameof(UnityAdsSetting.sdkType));


            var enable = EditorGUILayout.Toggle(_title, enableProperty.boolValue);

            if (enable != enableProperty.boolValue)
            {
                enableProperty.boolValue = enable;
                UpdateUnityAdsSdkTypeSymbols(_group, enable, sdkProperty);
            }


            if (enableProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sdkProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateUnityAdsSdkTypeSymbols(_group, enable, sdkProperty);
                }

                EditorGUILayout.HelpBox(
                    "Ads Provider will select for each ads based on Priority. Higher values is most likely to select. Eg - if admob priority of ads 2 and unity ads is 1 then admob have two times chance to show up compared to unity",
                    MessageType.Info);
                _adsSetting.DrawChildrenDefault(nameof(UnityAdsSetting.enable), nameof(UnityAdsSetting.sdkType));
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private static void UpdateUnityAdsSdkTypeSymbols(BuildTargetGroup group, bool enable,
            SerializedProperty sdkProperty)
        {
            foreach (UnityAdsSdkType value in Enum.GetValues(typeof(UnityAdsSdkType)))
            {
                ScriptingDefineSymbolHandler.HandleScriptingSymbol(group,
                    enable && value == (UnityAdsSdkType)sdkProperty.enumValueIndex, $"UNITY_ADS_{value}");
            }
        }

        public override bool HasSymbolsProblem()
        {
            var enable = _adsSetting.FindPropertyRelative(nameof(UnityAdsSetting.enable)).boolValue;
            var sdkType =
                (UnityAdsSdkType)_adsSetting.FindPropertyRelative(nameof(UnityAdsSetting.sdkType)).enumValueIndex;
            foreach (UnityAdsSdkType value in Enum.GetValues(typeof(UnityAdsSdkType)))
            {
                if (enable &&
                    ScriptingDefineSymbolHandler.HaveBuildSymbol(_group,
                        $"UNITY_ADS_{value}") != (value == sdkType)

                    ||

                    !enable && ScriptingDefineSymbolHandler.HaveBuildSymbol(_group, $"UNITY_ADS_{value}")
                   )
                    return true;
            }

            return false;
        }

        public override void FixSymbolsProblem()
        {
            var unityAdsSetting = _adsSetting.ToObjectValue<UnityAdsSetting>();
            foreach (UnityAdsSdkType sdkType in Enum.GetValues(typeof(UnityAdsSdkType)))
            {
                ScriptingDefineSymbolHandler.HandleScriptingSymbol(_group,
                    unityAdsSetting.enable && unityAdsSetting.sdkType == sdkType,
                    $"UNITY_ADS_{sdkType}");
            }
        }

        protected override void ApplyModifiedProperties()
        {
            _adsSetting.serializedObject.ApplyModifiedProperties();
        }
    }
    
     public class AdmobPlatformSegmentGUI : SettingSegmentGUIWithSymbols
    {
        private readonly BuildTargetGroup _group;
        private readonly string _title;
        private readonly SerializedProperty _admobSetting;

        public AdmobPlatformSegmentGUI(SerializedProperty admobSetting, string title, BuildTargetGroup group)
        {
            _group = group;
            _title = title;
            _admobSetting = admobSetting;
        }

        protected override void DrawGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            var enableProperty = _admobSetting.FindPropertyRelative(nameof(AdmobSetting.enable));
            var enable = EditorGUILayout.Toggle(_title, enableProperty.boolValue);

            if (enable != enableProperty.boolValue)
            {
                enableProperty.boolValue = enable;
                ScriptingDefineSymbolHandler.HandleScriptingSymbol(_group, enable, "ADMOB");
            }

            if (enableProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "Ads Provider will select for each ads based on Priority. Higher values is most likely to select. Eg - if admob priority of ads 2 and unity ads is 1 then admob have two times chance to show up compared to unity",
                    MessageType.Info);
                _admobSetting.DrawChildrenDefault(nameof(AdmobSetting.enable));
                EditorGUI.indentLevel--;
            }


            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        public override bool HasSymbolsProblem()
        {
            return _admobSetting.FindPropertyRelative(nameof(AdmobSetting.enable)).boolValue &&
                   !ScriptingDefineSymbolHandler.HaveBuildSymbol(_group, "ADMOB");
        }

        public override void FixSymbolsProblem()
        {
            ScriptingDefineSymbolHandler.HandleScriptingSymbol(_group,
                _admobSetting.FindPropertyRelative(nameof(AdmobSetting.enable)).boolValue, "ADMOB");
        }

        protected override void ApplyModifiedProperties()
        {
            _admobSetting.serializedObject.ApplyModifiedProperties();
        }
    }
     
     public abstract class SettingSegmentSetGUIWithSymbols : SettingSegmentSetGUI, ISettingSegmentGUIWithSymbols
     {
         protected SettingSegmentSetGUIWithSymbols(params ISettingSegmentGUI[] segmentGuis) : base(segmentGuis)
         {
         }

         public bool HasSymbolsProblem()
         {
             return settingSegmentGuis.OfType<ISettingSegmentGUIWithSymbols>().Any(s => s.HasSymbolsProblem());
         }

         public void FixSymbolsProblem()
         {
             foreach (var guiWithSymbols in settingSegmentGuis.OfType<ISettingSegmentGUIWithSymbols>()
                          .Where(s => s.HasSymbolsProblem()))
             {
                 guiWithSymbols.FixSymbolsProblem();
             }
         }
     }
     
     public abstract class SettingSegmentSetGUI : SettingSegmentGUI, IEnumerable<ISettingSegmentGUI>
     {
         protected readonly List<ISettingSegmentGUI> settingSegmentGuis;


         protected SettingSegmentSetGUI(params ISettingSegmentGUI[] segmentGuis)
         {
             settingSegmentGuis = segmentGuis.ToList();
         }


         protected void DrawChildrenInOrder()
         {
             for (var index = 0; index < settingSegmentGuis.Count; index++)
             {
                 var settingSegmentGui = settingSegmentGuis[index];
                 settingSegmentGui.OnGUI();
                 if (index < settingSegmentGuis.Count - 2)
                     DrawSpaceBetweenChildren();
             }
         }

         protected virtual void DrawSpaceBetweenChildren()
         {
         }

         public IEnumerator<ISettingSegmentGUI> GetEnumerator()
         {
             return settingSegmentGuis.GetEnumerator();
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
             return GetEnumerator();
         }
     }
     public abstract class SettingSegmentGUI : ISettingSegmentGUI
     {
         public void OnGUI()
         {
             EditorGUI.BeginChangeCheck();

             DrawGUI();

             if (EditorGUI.EndChangeCheck())
             {
                 ApplyModifiedProperties();
             }

         }

         protected abstract void ApplyModifiedProperties();
         protected abstract void DrawGUI();
     }
     
     public interface ISettingSegmentGUI
     {
         void OnGUI();
     }
     
     public abstract class SettingSegmentGUIWithSymbols : SettingSegmentGUI, ISettingSegmentGUIWithSymbols
     {
         public abstract bool HasSymbolsProblem();
         public abstract void FixSymbolsProblem();
     }
     
     public interface ISettingSegmentGUIWithSymbols : ISettingSegmentGUI
     {
         bool HasSymbolsProblem();
         void FixSymbolsProblem();
     }
     
      public class ConsentSegmentGUI : SettingSegmentGUI
    {
        private readonly SerializedProperty _consentSetting;

        public ConsentSegmentGUI(SerializedProperty consentSetting)
        {
            _consentSetting = consentSetting;
        }

        protected override void ApplyModifiedProperties()
        {
            _consentSetting.serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.HelpBox("Enable/Disable Consent Panel Show Up at Start.", MessageType.Info);

            var enableProperty = _consentSetting.FindPropertyRelative(nameof(ConsentSetting.enable));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Consent", EditorStyles.boldLabel);
            enableProperty.boolValue = EditorGUILayout.ToggleLeft("", enableProperty.boolValue);
            EditorGUILayout.EndHorizontal();


            if (enableProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                var enablePolicy = _consentSetting.FindPropertyRelative(nameof(ConsentSetting.privatePolicy));
                EditorGUILayout.PropertyField(enablePolicy);
                if (enablePolicy.boolValue)
                    EditorGUILayout.PropertyField(
                        _consentSetting.FindPropertyRelative(nameof(ConsentSetting.privatePolicyUrl)));
                EditorGUI.indentLevel--;
            }


            EditorGUILayout.EndVertical();
        }
    }

   
    
    public static class SerializePropertyExtensions
{
    public static IEnumerable<T> ToValueEnumerable<T>(this SerializedProperty property)
    {
        if (!property.isArray)
        {
            yield break;
        }

        foreach (var val in property.ToValueEnumerable(typeof(T)))
        {
            yield return val == null ? default(T) : (T)val;
        }
    }

    public static IEnumerable<object> ToValueEnumerable(this SerializedProperty property, Type type)
    {
        if (!property.isArray)
        {
            yield break;
        }

        //        Debug.Log(nameof(ToValueEnumerable) + type.Name +" "+property.arraySize);
        for (var i = 0; i < property.arraySize; i++)
        {
            var elementAtIndex = property.GetArrayElementAtIndex(i);
            yield return elementAtIndex.ToObjectValue(type);
        }
    }

    public static T ToObjectValue<T>(this SerializedProperty property)
    {
        var value = ToObjectValue(property, typeof(T));
        return value == null ? default(T) : (T)value;
    }

    public static object ToObjectValue(this SerializedProperty property, Type type)
    {
        if (property.isArray && property.propertyType != SerializedPropertyType.String)
            return null;

        if (property.propertyType.IsBuildInSerializableField())
        {
            return GetBuildInFieldValue(property);
        }

        var instance = Activator.CreateInstance(type);
        if (property.type != type.Name)
        {
            throw new InvalidOperationException($"Value MisMatched Property-{property.type} Type-{type.Name}");
        }

        foreach (var fieldInfo in type
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(info => info.IsPublic ||
                           info.CustomAttributes.Any(data => data.AttributeType == typeof(SerializeField))))
        {
            var fieldProperty = property.FindPropertyRelative(fieldInfo.Name);

            var value = fieldProperty.isArray && fieldProperty.propertyType != SerializedPropertyType.String
                ? fieldProperty.ToValueEnumerable(fieldInfo.FieldType.GetInterfaces()
                        .First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .GenericTypeArguments[0])
                    .ConvertToTypeEnumerable(fieldInfo.FieldType)
                : fieldProperty.hasChildren && fieldProperty.propertyType != SerializedPropertyType.String
                    ? fieldProperty.ToObjectValue(fieldInfo.FieldType)
                    : GetBuildInFieldValue(fieldProperty);

            fieldInfo.SetValue(instance, value);
        }

        return instance;
    }


    public static void SetObjectValue<T>(this SerializedProperty property, T value)
    {
        property.SetObjectValue(value, typeof(T));
    }

    public static void Foreach(this SerializedProperty property, Action<SerializedProperty> action)
    {
        if (!property.isArray)
        {
            throw new InvalidOperationException();
        }

        for (var i = 0; i < property.arraySize; i++)
        {
            action?.Invoke(property.GetArrayElementAtIndex(i));
        }
    }

    public static IEnumerable<SerializedProperty> ToEnumerable(this SerializedProperty property)
    {
        if (!property.isArray)
        {
            throw new InvalidOperationException();
        }

        for (var i = 0; i < property.arraySize; i++)
        {
            yield return property.GetArrayElementAtIndex(i);
        }
    }

    public static void SetObjectValue(this SerializedProperty property, object value, Type type)
    {
        //        Debug.Log(type.Name);
        if (property.type.ToLower() != type.Name.ToLower())
            throw new ArgumentException($"Type Mismatched Property Type:{property.type} Value Type:{type.Name}");

        if (property.isArray && property.propertyType != SerializedPropertyType.String)
        {
            //            property.SetObjectValueEnumerable(value as IEnumerable,type);
            return;
        }

        if (property.propertyType.IsBuildInSerializableField())
        {
            SetBuildInFieldValue(property, value);
            return;
        }

        foreach (var fieldInfo in type
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(info => info.IsPublic ||
                           info.CustomAttributes.Any(data => data.AttributeType == typeof(SerializeField))))
        {
            var fieldProperty = property.FindPropertyRelative(fieldInfo.Name);
            if (fieldProperty.isArray && fieldProperty.propertyType != SerializedPropertyType.String)
            {
                fieldProperty.SetObjectValueEnumerable(
                    fieldInfo.GetValue(value) as IEnumerable
                    , fieldInfo.FieldType.GetInterfaces()
                        .First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .GenericTypeArguments[0]);
            }
            else if (fieldProperty.hasChildren && fieldProperty.propertyType != SerializedPropertyType.String)
            {
                fieldProperty.SetObjectValue(fieldInfo.GetValue(value));
            }
            else
            {
                SetBuildInFieldValue(fieldProperty, fieldInfo.GetValue(value));
            }
        }
    }

    public static void SetObjectValueEnumerable<T>(this SerializedProperty property, IEnumerable<T> value)
    {
        property.SetObjectValueEnumerable(value, typeof(T));
    }

    public static void SetObjectValueEnumerable(this SerializedProperty property, IEnumerable value, Type elementType)
    {
        property.ClearArray();
        var i = 0;



        foreach (var v in value)
        {
            property.InsertArrayElementAtIndex(i);
            property.GetArrayElementAtIndex(i).SetObjectValue(v, elementType);
            i++;
        }
    }

    //    public static object ConvertToTypeList(this IEnumerable<object> list, Type type)
    //    {
    //        return list.Select(item => Convert.ChangeType(item, type)).ToList();
    //    }

    public static object ConvertToTypeEnumerable(this IEnumerable value, Type type)
    {
        var list = (IList)Activator.CreateInstance(type);
        foreach (var item in value)
        {
            list.Add(item);
        }

        return list;
    }

    private static object GetBuildInFieldValue(SerializedProperty property)
    {
        if (property.propertyType == SerializedPropertyType.Generic)
        {
            Debug.LogError("Invalid Property Field:" + property.propertyType);
            return null;
        }

        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                return property.intValue;
            case SerializedPropertyType.Boolean:
                return property.boolValue;
            case SerializedPropertyType.Float:
                return property.floatValue;
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Color:
                return property.colorValue;
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue;
            case SerializedPropertyType.LayerMask:
                return property.intValue;
            case SerializedPropertyType.Enum:
                return property.enumValueIndex;
            case SerializedPropertyType.Vector2:
                return property.vector2Value;
            case SerializedPropertyType.Vector3:
                return property.vector3Value;
            case SerializedPropertyType.Vector4:
                return property.vector4Value;
            case SerializedPropertyType.Rect:
                return property.rectValue;
            case SerializedPropertyType.ArraySize:
                return property.arraySize;
            case SerializedPropertyType.Character:
                return property.stringValue;
            case SerializedPropertyType.AnimationCurve:
                return property.animationCurveValue;
            case SerializedPropertyType.Bounds:
                return property.boundsValue;
            //            case SerializedPropertyType.Gradient:
            //                return property.;
            case SerializedPropertyType.Quaternion:
                return property.quaternionValue;
            case SerializedPropertyType.ExposedReference:
                return property.exposedReferenceValue;

            case SerializedPropertyType.FixedBufferSize:
                return property.fixedBufferSize;
            case SerializedPropertyType.Vector2Int:
                return property.vector2IntValue;
            case SerializedPropertyType.Vector3Int:
                return property.vector3IntValue;
            case SerializedPropertyType.RectInt:
                return property.rectIntValue;
            case SerializedPropertyType.BoundsInt:
                return property.boundsIntValue;
            default:
                Debug.LogError("Invalid Property Field:" + property.propertyType);
                return null;
        }
    }

    private static void SetBuildInFieldValue(SerializedProperty property, object value)
    {
        if (property.hasChildren && property.propertyType != SerializedPropertyType.String)
        {
            Debug.LogError("Invalid Property Field:" + property.propertyType);
        }

        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                property.intValue = (int)value;
                break;
            case SerializedPropertyType.Boolean:
                property.boolValue = (bool)value;
                break;
            case SerializedPropertyType.Float:
                property.floatValue = (float)value;
                break;
            case SerializedPropertyType.String:
                property.stringValue = (string)value;
                break;
            case SerializedPropertyType.Color:
                property.colorValue = (Color)value;
                break;
            case SerializedPropertyType.ObjectReference:
                property.objectReferenceValue = value as UnityEngine.Object;
                break;
            case SerializedPropertyType.LayerMask:
                property.intValue = (int)value;
                break;
            case SerializedPropertyType.Enum:
                property.enumValueIndex = (int)value;
                break;
            case SerializedPropertyType.Vector2:
                property.vector2Value = (Vector2)value;
                break;
            case SerializedPropertyType.Vector3:
                property.vector3Value = (Vector3)value;
                break;
            case SerializedPropertyType.Vector4:
                property.vector4Value = (Vector4)value;
                break;
            case SerializedPropertyType.Rect:
                property.rectValue = (Rect)value;
                break;
            case SerializedPropertyType.ArraySize:
                property.arraySize = (int)value;
                break;
            case SerializedPropertyType.Character:
                property.stringValue = (string)value;
                break;
            case SerializedPropertyType.AnimationCurve:
                property.animationCurveValue = (AnimationCurve)value;
                break;
            case SerializedPropertyType.Bounds:
                property.boundsValue = (Bounds)value;
                break;
            //            case SerializedPropertyType.Gradient:
            //                return property.;
            case SerializedPropertyType.Quaternion:
                property.quaternionValue = (Quaternion)value;
                break;
            case SerializedPropertyType.ExposedReference:
                property.exposedReferenceValue = value as UnityEngine.Object;
                break;
            //            case SerializedPropertyType.FixedBufferSize:
            //                property.fixedBufferSize = value;
            case SerializedPropertyType.Vector2Int:
                property.vector2IntValue = (Vector2Int)value;
                break;
            case SerializedPropertyType.Vector3Int:
                property.vector3IntValue = (Vector3Int)value;
                break;
            case SerializedPropertyType.RectInt:
                property.rectIntValue = (RectInt)value;
                break;
            case SerializedPropertyType.BoundsInt:
                property.boundsIntValue = (BoundsInt)value;
                break;

            default:
                Debug.LogError("Invalid Property Field:" + property.propertyType);
                break;
        }
    }

    public static bool IsBuildInSerializableField(this SerializedPropertyType type)
    {
        return type != SerializedPropertyType.ArraySize && type != SerializedPropertyType.Generic;
    }
}
    
    public static class ScriptingDefineSymbolHandler
    {
        public static bool HaveBuildSymbol(BuildTargetGroup group, string symbol)
        {
            var scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var strings = scriptingDefineSymbolsForGroup.Split(';').ToList();

            return strings.Contains(symbol);
        }

        public static void AddBuildSymbol(BuildTargetGroup group, string symbol)
        {
            if (HaveBuildSymbol(group, symbol))
                return;
            var scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var strings = scriptingDefineSymbolsForGroup.Split(';').ToList();
            strings.Add(symbol);
            var str = "";
            foreach (var s in strings)
            {
                str += s + ";";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, str);
        }

        public static void RemoveBuildSymbol(BuildTargetGroup group, string symbol)
        {
            if (!HaveBuildSymbol(group, symbol))
                return;
            var scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var strings = scriptingDefineSymbolsForGroup.Split(';').ToList();
            strings.Remove(symbol);
            var str = "";
            foreach (var s in strings)
            {
                str += s + ";";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, str);
        }

        public static void HandleScriptingSymbol(BuildTargetGroup buildTargetGroup, bool enable, string key)
        {
            var scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var strings = scriptingDefineSymbolsForGroup.Split(';').ToList();

            if (enable)
            {
                strings.Add(key);
            }
            else
            {
                strings.Remove(key);
            }


            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", strings.Distinct()));
        }
    }
    
    public static class EditorExtensions
    {


        public static void DrawChildrenDefault(this SerializedProperty property,
            params string[] exceptChildren)
        {
            var exceptList = exceptChildren?.ToList() ?? new List<string>();

            property = property.Copy();

            var parentDepth = property.depth;
            if (property.NextVisible(true) && parentDepth < property.depth)
            {
                do
                {
                    if (exceptList.Contains(property.name))
                        continue;
                    EditorGUILayout.PropertyField(property, true);
                } while (property.NextVisible(false) && parentDepth < property.depth);
            }
        }
    }

}