using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    private SerializedProperty unitRole;

    // General
    private SerializedProperty unitPrefab;
    private SerializedProperty unitIcon;
    private SerializedProperty isMainCharacter;
    private SerializedProperty unitName;
    private SerializedProperty unitDescription;
    private SerializedProperty abilityDescription;

    // Common Stats
    private SerializedProperty maxHP;
    private SerializedProperty maxEnergy;
    private SerializedProperty moveSpeed;
    private SerializedProperty damage;
    private SerializedProperty attacksPerSecond;
    private SerializedProperty miningPower;
    private SerializedProperty carryCapacity;

    // Immunities and Attacks
    private SerializedProperty typeImmunities;
    private SerializedProperty deliveryImmunities;
    private SerializedProperty possibleAttacks;

    // Audio
    private SerializedProperty audioSettings;

    private Color defaultColor;
    private readonly Color activeColor = new Color(0.6f, 1f, 0.6f);

    private void OnEnable()
    {
        unitRole = serializedObject.FindProperty("unitRole");

        unitPrefab = serializedObject.FindProperty("unitPrefab");
        unitIcon = serializedObject.FindProperty("unitIcon");
        isMainCharacter = serializedObject.FindProperty("isMainCharacter");
        unitName = serializedObject.FindProperty("unitName");
        unitDescription = serializedObject.FindProperty("unitDescription");
        abilityDescription = serializedObject.FindProperty("abilityDescription");

        maxHP = serializedObject.FindProperty("maxHP");
        maxEnergy = serializedObject.FindProperty("maxEnergy");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        damage = serializedObject.FindProperty("damage");
        attacksPerSecond = serializedObject.FindProperty("attacksPerSecond");
        miningPower = serializedObject.FindProperty("miningPower");
        carryCapacity = serializedObject.FindProperty("carryCapacity");
        typeImmunities = serializedObject.FindProperty("typeImmunities");
        deliveryImmunities = serializedObject.FindProperty("deliveryImmunities");
        possibleAttacks = serializedObject.FindProperty("possibleAttacks");

        audioSettings = serializedObject.FindProperty("audioSettings");

        defaultColor = GUI.color;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(unitRole);
        EditorGUILayout.Space(12);

        DrawGeneralSettings();
        EditorGUILayout.Space(12);

        DrawCommonStats();
        EditorGUILayout.Space(12);

        DrawAudio();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGeneralSettings()
    {
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(unitPrefab);
        EditorGUILayout.PropertyField(unitIcon);
        EditorGUILayout.PropertyField(isMainCharacter);
        EditorGUILayout.PropertyField(unitName);
        EditorGUILayout.PropertyField(unitDescription);
        EditorGUILayout.PropertyField(abilityDescription);
    }

    private void DrawCommonStats()
    {
        EditorGUILayout.LabelField("Common Stats", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(maxHP);
        EditorGUILayout.PropertyField(maxEnergy);
        EditorGUILayout.PropertyField(moveSpeed);

        SetColor(UnitRole.Warrior);
        EditorGUILayout.PropertyField(damage);
        EditorGUILayout.PropertyField(attacksPerSecond);
        ResetColor();

        SetColor(UnitRole.Miner);
        EditorGUILayout.PropertyField(miningPower);
        ResetColor();

        EditorGUILayout.PropertyField(carryCapacity);

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Others", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(typeImmunities);
        EditorGUILayout.PropertyField(deliveryImmunities);

        SetColor(UnitRole.Warrior);
        EditorGUILayout.PropertyField(possibleAttacks);
        ResetColor();
    }

    private void DrawAudio()
    {
        EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(audioSettings);
    }

    private void SetColor(UnitRole targetRole)
    {
        if ((UnitRole)unitRole.enumValueIndex == targetRole)
            GUI.color = activeColor;
    }

    private void ResetColor() => GUI.color = defaultColor;
}