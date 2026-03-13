using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(DialogOption))]
public class DialogOptionDrawer : PropertyDrawer
{
    #region UI Setup

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var foldout = new Foldout();

        foldout.style.paddingTop = 5;
        foldout.style.paddingBottom = 5;
        foldout.style.paddingLeft = 5;
        foldout.style.paddingRight = 5;
        foldout.style.marginBottom = 10;
        foldout.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.25f, 0.25f, 0.25f));

        var optionTextProp = property.FindPropertyRelative("optionText");
        var isIgnoreProp = property.FindPropertyRelative("isIgnoreOption");
        var isMiningProp = property.FindPropertyRelative("isMiningCheck");
        var baseChanceProp = property.FindPropertyRelative("baseSuccessChance");
        var bonusProp = property.FindPropertyRelative("bonusPerMiningPower");
        var successProp = property.FindPropertyRelative("successOutcome");
        var failureProp = property.FindPropertyRelative("failureOutcome");

        UpdateFoldoutTitle(foldout, optionTextProp);

        foldout.TrackPropertyValue(optionTextProp, prop => UpdateFoldoutTitle(foldout, prop));

        var optionTextField = new PropertyField(optionTextProp);
        var isIgnoreField = new PropertyField(isIgnoreProp);
        var isMiningField = new PropertyField(isMiningProp);
        var baseChanceField = new PropertyField(baseChanceProp);
        var bonusField = new PropertyField(bonusProp);
        var successField = new PropertyField(successProp, "Outcome");
        var failureField = new PropertyField(failureProp);

        foldout.Add(optionTextField);
        foldout.Add(isIgnoreField);
        foldout.Add(isMiningField);
        foldout.Add(baseChanceField);
        foldout.Add(bonusField);
        foldout.Add(successField);
        foldout.Add(failureField);

        void UpdateVisibility()
        {
            bool isIgnore = isIgnoreProp.boolValue;
            bool isMining = isMiningProp.boolValue;

            isMiningField.style.display = isIgnore ? DisplayStyle.None : DisplayStyle.Flex;
            successField.style.display = isIgnore ? DisplayStyle.None : DisplayStyle.Flex;

            bool showMiningStats = !isIgnore && isMining;

            baseChanceField.style.display = showMiningStats ? DisplayStyle.Flex : DisplayStyle.None;
            bonusField.style.display = showMiningStats ? DisplayStyle.Flex : DisplayStyle.None;
            failureField.style.display = showMiningStats ? DisplayStyle.Flex : DisplayStyle.None;

            successField.label = isMining ? "Success Outcome" : "Outcome";
        }

        foldout.TrackPropertyValue(isIgnoreProp, _ => UpdateVisibility());
        foldout.TrackPropertyValue(isMiningProp, _ => UpdateVisibility());

        UpdateVisibility();

        return foldout;
    }

    void UpdateFoldoutTitle(Foldout foldout, SerializedProperty textProp)
    {
        string title = textProp.stringValue;
        foldout.text = string.IsNullOrEmpty(title) ? "New Option" : title;
    }

    #endregion
}