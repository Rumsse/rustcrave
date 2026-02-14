using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitSO))]
public class UnitSOEditor : Editor
{
    SerializedProperty unitType;

    SerializedProperty moveSpeed;
    SerializedProperty maxHP;
    SerializedProperty possibleAttacks;

    SerializedProperty damage;
    SerializedProperty attacksPerSecond;

    SerializedProperty miningPower;

    SerializedProperty carryCapacity;

    Color defaultColor;
    Color activeColor = new Color(0.6f, 1f, 0.6f);

    void OnEnable()
    {
        unitType = serializedObject.FindProperty("unitType");

        moveSpeed = serializedObject.FindProperty("moveSpeed");
        maxHP = serializedObject.FindProperty("maxHP");
        possibleAttacks = serializedObject.FindProperty("possibleAttacks");

        damage = serializedObject.FindProperty("damage");
        attacksPerSecond = serializedObject.FindProperty("attacksPerSecond");

        miningPower = serializedObject.FindProperty("miningPower");

        carryCapacity = serializedObject.FindProperty("carryCapacity");

        defaultColor = GUI.color;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawUnitType();
        Space();

        DrawCommon();
        Space();

        DrawWarrior();
        Space();

        DrawMiner();
        Space();

        DrawToter();
        Space();

        DrawAttacks();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawUnitType()
    {
        EditorGUILayout.PropertyField(unitType);
    }

    void DrawCommon()
    {
        EditorGUILayout.LabelField("Common Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(moveSpeed);
        EditorGUILayout.PropertyField(maxHP);
    }

    void DrawWarrior()
    {
        SetColor(UnitType.Warrior);
        EditorGUILayout.LabelField("Warrior Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(damage);
        EditorGUILayout.PropertyField(attacksPerSecond);
        ResetColor();
    }

    void DrawMiner()
    {
        SetColor(UnitType.Miner);
        EditorGUILayout.LabelField("Miner Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(miningPower);
        ResetColor();
    }

    void DrawToter()
    {
        SetColor(UnitType.Toter);
        EditorGUILayout.LabelField("Toter Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(carryCapacity);
        ResetColor();
    }

    void DrawAttacks()
    {
        SetColor(UnitType.Warrior);
        EditorGUILayout.LabelField("Attacks", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(possibleAttacks);
        ResetColor();
    }

    void SetColor(UnitType type)
    {
        GUI.color = (UnitType)unitType.enumValueIndex == type ? activeColor: defaultColor;
    }

    void ResetColor()
    {
        GUI.color = defaultColor;
    }

    void Space()
    {
        EditorGUILayout.Space(8);
    }
}
