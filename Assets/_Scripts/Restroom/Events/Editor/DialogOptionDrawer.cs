using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(DialogOption))]
public class DialogOptionDrawer : PropertyDrawer
{
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

        var reqItemProp = property.FindPropertyRelative("requiredItem");
        var reqAmountProp = property.FindPropertyRelative("requiredItemAmount");

        var isLuckProp = property.FindPropertyRelative("isLuckCheck");

        var isMiningProp = property.FindPropertyRelative("isMiningCheck");
        var bonusMiningProp = property.FindPropertyRelative("bonusPerMiningPower");

        var isAttackProp = property.FindPropertyRelative("isAttackCheck");
        var bonusAttackProp = property.FindPropertyRelative("bonusPerDamage");

        var isCapacityProp = property.FindPropertyRelative("isCapacityCheck");
        var bonusCapacityProp = property.FindPropertyRelative("bonusPerCapacity");

        var baseChanceProp = property.FindPropertyRelative("baseSuccessChance");
        var successProp = property.FindPropertyRelative("successOutcome");
        var failureProp = property.FindPropertyRelative("failureOutcome");

        UpdateFoldoutTitle(foldout, optionTextProp);
        foldout.TrackPropertyValue(optionTextProp, prop => UpdateFoldoutTitle(foldout, prop));

        var optionTextField = new PropertyField(optionTextProp);
        var isIgnoreField = new PropertyField(isIgnoreProp);
        var reqItemField = new PropertyField(reqItemProp, "Required Item");
        var reqAmountField = new PropertyField(reqAmountProp, "Required Amount");

        var isLuckField = new PropertyField(isLuckProp);
        var isMiningField = new PropertyField(isMiningProp);
        var bonusMiningField = new PropertyField(bonusMiningProp);
        var isAttackField = new PropertyField(isAttackProp);
        var bonusAttackField = new PropertyField(bonusAttackProp);
        var isCapacityField = new PropertyField(isCapacityProp);
        var bonusCapacityField = new PropertyField(bonusCapacityProp);

        var baseChanceField = new PropertyField(baseChanceProp);
        var successField = new PropertyField(successProp, "Outcome");
        var failureField = new PropertyField(failureProp);

        foldout.Add(optionTextField);
        foldout.Add(isIgnoreField);
        foldout.Add(reqItemField);
        foldout.Add(reqAmountField);
        foldout.Add(isLuckField);
        foldout.Add(isMiningField);
        foldout.Add(bonusMiningField);
        foldout.Add(isAttackField);
        foldout.Add(bonusAttackField);
        foldout.Add(isCapacityField);
        foldout.Add(bonusCapacityField);
        foldout.Add(baseChanceField);
        foldout.Add(successField);
        foldout.Add(failureField);

        void UpdateVisibility()
        {
            bool isIgnore = isIgnoreProp.boolValue;
            bool isLuck = isLuckProp.boolValue;
            bool isMining = isMiningProp.boolValue;
            bool isAttack = isAttackProp.boolValue;
            bool isCapacity = isCapacityProp.boolValue;

            if (isLuck && (isMining || isAttack || isCapacity))
            {
                isMiningProp.boolValue = false;
                isAttackProp.boolValue = false;
                isCapacityProp.boolValue = false;
                property.serializedObject.ApplyModifiedProperties();
                isMining = isAttack = isCapacity = false;
            }
            else if (isMining && (isLuck || isAttack || isCapacity))
            {
                isLuckProp.boolValue = false;
                isAttackProp.boolValue = false;
                isCapacityProp.boolValue = false;
                property.serializedObject.ApplyModifiedProperties();
                isLuck = isAttack = isCapacity = false;
            }
            else if (isAttack && (isLuck || isMining || isCapacity))
            {
                isLuckProp.boolValue = false;
                isMiningProp.boolValue = false;
                isCapacityProp.boolValue = false;
                property.serializedObject.ApplyModifiedProperties();
                isLuck = isMining = isCapacity = false;
            }
            else if (isCapacity && (isLuck || isMining || isAttack))
            {
                isLuckProp.boolValue = false;
                isMiningProp.boolValue = false;
                isAttackProp.boolValue = false;
                property.serializedObject.ApplyModifiedProperties();
                isLuck = isMining = isAttack = false;
            }

            reqItemField.style.display = isIgnore ? DisplayStyle.None : DisplayStyle.Flex;
            reqAmountField.style.display = (isIgnore || reqItemProp.objectReferenceValue == null) ? DisplayStyle.None : DisplayStyle.Flex;

            isLuckField.style.display = isIgnore ? DisplayStyle.None : DisplayStyle.Flex;
            isMiningField.style.display = isIgnore ? DisplayStyle.None : DisplayStyle.Flex;
            isAttackField.style.display = isIgnore ? DisplayStyle.None : DisplayStyle.Flex;
            isCapacityField.style.display = isIgnore ? DisplayStyle.None : DisplayStyle.Flex;
            successField.style.display = isIgnore ? DisplayStyle.None : DisplayStyle.Flex;

            bool isAnyCheck = !isIgnore && (isLuck || isMining || isAttack || isCapacity);

            baseChanceField.style.display = isAnyCheck ? DisplayStyle.Flex : DisplayStyle.None;
            failureField.style.display = isAnyCheck ? DisplayStyle.Flex : DisplayStyle.None;

            bonusMiningField.style.display = (!isIgnore && isMining) ? DisplayStyle.Flex : DisplayStyle.None;
            bonusAttackField.style.display = (!isIgnore && isAttack) ? DisplayStyle.Flex : DisplayStyle.None;
            bonusCapacityField.style.display = (!isIgnore && isCapacity) ? DisplayStyle.Flex : DisplayStyle.None;

            successField.label = isAnyCheck ? "Success Outcome" : "Outcome";
        }

        foldout.TrackPropertyValue(isIgnoreProp, _ => UpdateVisibility());
        foldout.TrackPropertyValue(reqItemProp, _ => UpdateVisibility());
        foldout.TrackPropertyValue(isLuckProp, _ => UpdateVisibility());
        foldout.TrackPropertyValue(isMiningProp, _ => UpdateVisibility());
        foldout.TrackPropertyValue(isAttackProp, _ => UpdateVisibility());
        foldout.TrackPropertyValue(isCapacityProp, _ => UpdateVisibility());

        UpdateVisibility();

        return foldout;
    }

    void UpdateFoldoutTitle(Foldout foldout, SerializedProperty textProp)
    {
        string title = textProp.stringValue;
        foldout.text = string.IsNullOrEmpty(title) ? "New Option" : title;
    }
}