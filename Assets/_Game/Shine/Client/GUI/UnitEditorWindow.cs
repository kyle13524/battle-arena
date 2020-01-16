using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;

#if UNITY_EDITOR
public class UnitEditorWindow : EditorWindow
{
    private struct UnitEditorSettings
    {
        public int selectedUnitType;
        public int selectedRarity;
        public string IDField;
        public string nameField;
        public string healthField;
        public string speedField;
        //public string selectedBasicAbility;
        //public string selectedUltimateAbility;
        public string basic;
        public string ultimate;
    }

    private struct AbilityEditorSettings
    {
        public string IDField;
        public string nameField;
        public string damageField;
        public string cooldownField;
        public string rangeField;
        public string maxHitsField;
        public AbilityType attackType;
        public AbilityEffect.Type effectType;
        public string effectDurationField;
        public string effectRateField;
        public string effectValueField;

        public int selectedType;
        public bool displayEffects;
    }

    private List<UnitEditorSettings> previousUnitSettings = new List<UnitEditorSettings>();
    private List<AbilityEditorSettings> previousAbilitySettings = new List<AbilityEditorSettings>();

    private List<UnitEditorSettings> currentUnitSettings = new List<UnitEditorSettings>();
    private List<AbilityEditorSettings> currentAbilitySettings = new List<AbilityEditorSettings>();

    private int selectedUnit;
    private int selectedUnitBasic;
    private int selectedUnitUltimate;
    private int selectedAbility;

    private string[] unitTypes = { "Hero", "Artillery", "Minion" };
    private string[] rarityTypes = { "Basic", "Rare", "Epic" };
    private string[] abilityTypes = { "Basic", "Ultimate" };

    private bool loaded = false;
    private bool addingUnit = false;
    private bool addingAbility = false;
    private bool basicAbilityToggle = false;
    private bool ultimateAbilityToggle = false;

    private int currentWindow;

    private FirebaseDBManager dbManager;

    private float lastDeleteClick = 0f;

    [MenuItem("Window/ProjectDK/Unit Editor")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindowWithRect(typeof(UnitEditorWindow), new Rect(0, 0, 410, 380), false, "Unit Editor");
    }

    void OnGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperCenter;
            EditorGUI.LabelField(Rect.MinMaxRect(50f, 170f, 350f, 190f), new GUIContent("Game must be running."), style);
            selectedAbility = 0;
            selectedUnit = 0;
            loaded = false;
            addingUnit = false;
            addingAbility = false;
            return;
        }

        if(!loaded)
        {
            LoadFromDatabase();
            loaded = true;
        }

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontStyle = FontStyle.Bold;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Units"))
        {
            currentWindow = 0;
        }
        else if (GUILayout.Button("Abilities"))
        {
            currentWindow = 1;
        }
        GUILayout.EndHorizontal();

        switch (currentWindow)
        {
            case 0:
                #region Currently Selected Unit
                EditorGUI.LabelField(Rect.MinMaxRect(5f, 25f, 50f, 45f), new GUIContent("Units"));

                selectedUnit = EditorGUI.Popup(Rect.MinMaxRect(40f, 25f, 350f, 45f), selectedUnit, currentUnitSettings.Select(x => x.IDField).ToArray());

                GUILayout.BeginArea(new Rect(358f, 24f, 18f, 18f));
                if (GUILayout.Button("+"))
                {
                    AddNewUnit();
                }
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(382f, 24f, 18f, 18f));
                if (GUILayout.Button("x"))
                {
                    DeleteUnit();
                }
                GUILayout.EndArea();

                if (currentUnitSettings.Count <= 0)
                    return;
                
                UnitEditorSettings unitSettings = currentUnitSettings[selectedUnit];
                string selectedUnitID = previousUnitSettings[selectedUnit].IDField;

                EditorGUI.LabelField(Rect.MinMaxRect(5f, 50f, 80f, 70f), new GUIContent("Currently Selected Unit"), labelStyle);
                EditorGUI.DrawPreviewTexture(Rect.MinMaxRect(10f, 70f, 90f, 150f), GetUnitTexture(unitSettings.IDField));

                EditorGUI.LabelField(Rect.MinMaxRect(95f, 70f, 145f, 90f), new GUIContent("Unit ID"));
                unitSettings.IDField = EditorGUI.TextField(Rect.MinMaxRect(140f, 70f, 250f, 85f), unitSettings.IDField);

                EditorGUI.LabelField(Rect.MinMaxRect(95f, 100f, 145f, 120f), new GUIContent("Name"));
                unitSettings.nameField = EditorGUI.TextField(Rect.MinMaxRect(140f, 100f, 250f, 115f), unitSettings.nameField);

                EditorGUI.LabelField(Rect.MinMaxRect(95f, 130f, 145f, 150f), new GUIContent("Health"));
                unitSettings.healthField = EditorGUI.TextField(Rect.MinMaxRect(140f, 130f, 250f, 145f), unitSettings.healthField);

                EditorGUI.LabelField(Rect.MinMaxRect(270f, 70f, 320f, 90f), new GUIContent("Type"));
                unitSettings.selectedUnitType = EditorGUI.Popup(Rect.MinMaxRect(310f, 70f, 400f, 90f), unitSettings.selectedUnitType, unitTypes);

                EditorGUI.LabelField(Rect.MinMaxRect(265f, 100f, 315f, 120f), new GUIContent("Speed"));
                unitSettings.speedField = EditorGUI.TextField(Rect.MinMaxRect(310f, 100f, 400f, 115f), unitSettings.speedField);

                EditorGUI.LabelField(Rect.MinMaxRect(265f, 130f, 315f, 150f), new GUIContent("Rarity"));
                unitSettings.selectedRarity = EditorGUI.Popup(Rect.MinMaxRect(310f, 130f, 400f, 150f), unitSettings.selectedRarity, rarityTypes);
                #endregion

                #region Basic Ability Slot
                EditorGUI.LabelField(Rect.MinMaxRect(5f, 160f, 90f, 180f), new GUIContent("Basic Ability Info"), labelStyle);

                var basicAbilities = currentAbilitySettings.Where(x => x.selectedType == 0).Select(x => x.IDField).ToArray();
                selectedUnitBasic = EditorGUI.Popup(Rect.MinMaxRect(50f, 180f, 370f, 200f), selectedUnitBasic, basicAbilities);
                var selectedBasicAbility = currentAbilitySettings.FirstOrDefault(x => x.IDField == basicAbilities[selectedUnitBasic]);

                basicAbilityToggle = EditorGUI.ToggleLeft(Rect.MinMaxRect(380f, 180f, 400f, 200f), string.Empty, basicAbilityToggle);
                unitSettings.basic = (basicAbilityToggle) ? selectedBasicAbility.IDField : string.Empty;

                EditorGUI.DrawPreviewTexture(Rect.MinMaxRect(10f, 180f, 40f, 210f), GetAbilityTexture(selectedBasicAbility.IDField));

                EditorGUI.BeginDisabledGroup(basicAbilityToggle == false);

                EditorGUI.LabelField(Rect.MinMaxRect(50f, 200f, 100f, 220f), new GUIContent("Damage"));
                selectedBasicAbility.damageField = EditorGUI.TextField(Rect.MinMaxRect(110f, 200f, 160f, 215f), selectedBasicAbility.damageField);

                EditorGUI.LabelField(Rect.MinMaxRect(170f, 200f, 220f, 220f), new GUIContent("Range"));
                selectedBasicAbility.rangeField = EditorGUI.TextField(Rect.MinMaxRect(220f, 200f, 270f, 215f), selectedBasicAbility.rangeField);

                EditorGUI.LabelField(Rect.MinMaxRect(280f, 200f, 340f, 220f), new GUIContent("Cooldown"));
                selectedBasicAbility.cooldownField = EditorGUI.TextField(Rect.MinMaxRect(350f, 200f, 400f, 215f), selectedBasicAbility.cooldownField);

                selectedBasicAbility.displayEffects = EditorGUI.ToggleLeft(Rect.MinMaxRect(10f, 225f, 60f, 245f), "Effect", selectedBasicAbility.displayEffects);

                EditorGUI.BeginDisabledGroup(selectedBasicAbility.displayEffects == false);

                selectedBasicAbility.effectType = (AbilityEffect.Type)EditorGUI.EnumPopup(Rect.MinMaxRect(65f, 225f, 130f, 245f), selectedBasicAbility.effectType);

                EditorGUI.LabelField(Rect.MinMaxRect(140f, 225f, 190f, 245f), new GUIContent("Duration"));
                selectedBasicAbility.effectDurationField = EditorGUI.TextField(Rect.MinMaxRect(195f, 225f, 230f, 240f), selectedBasicAbility.effectDurationField);

                EditorGUI.LabelField(Rect.MinMaxRect(240f, 225f, 270f, 245f), new GUIContent("Rate"));
                selectedBasicAbility.effectRateField = EditorGUI.TextField(Rect.MinMaxRect(275f, 225f, 310f, 240f), selectedBasicAbility.effectRateField);

                EditorGUI.LabelField(Rect.MinMaxRect(320f, 225f, 360f, 245f), new GUIContent("Value"));
                selectedBasicAbility.effectValueField = EditorGUI.TextField(Rect.MinMaxRect(360f, 225f, 400f, 240f), selectedBasicAbility.effectValueField);

                if (basicAbilities.Length > 0)
                {
                    int index = currentAbilitySettings.FindIndex(x => x.IDField == basicAbilities[selectedUnitBasic]);
                    currentAbilitySettings[index] = selectedBasicAbility;
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();
                #endregion

                #region Ultimate Ability Slot
                EditorGUI.LabelField(Rect.MinMaxRect(5f, 260f, 100f, 280f), new GUIContent("Ultimate Ability Info"), labelStyle);

                var ultimateAbilities = currentAbilitySettings.Where(x => x.selectedType == 1).Select(x => x.IDField).ToArray();
                selectedUnitUltimate = EditorGUI.Popup(Rect.MinMaxRect(50f, 280f, 370f, 300f), selectedUnitUltimate, ultimateAbilities);
                var selectedUltimateAbility = currentAbilitySettings.FirstOrDefault(x => x.IDField == ultimateAbilities[selectedUnitUltimate]);

                ultimateAbilityToggle = EditorGUI.ToggleLeft(Rect.MinMaxRect(380f, 280f, 400f, 300f), string.Empty, ultimateAbilityToggle);
                unitSettings.ultimate = (ultimateAbilityToggle) ? selectedUltimateAbility.IDField : string.Empty;

                EditorGUI.DrawPreviewTexture(Rect.MinMaxRect(10f, 280f, 40f, 310f), GetAbilityTexture(selectedUltimateAbility.IDField));

                EditorGUI.BeginDisabledGroup(ultimateAbilityToggle == false);

                EditorGUI.LabelField(Rect.MinMaxRect(50f, 300f, 100f, 320f), new GUIContent("Damage"));
                selectedUltimateAbility.damageField = EditorGUI.TextField(Rect.MinMaxRect(110f, 300f, 160f, 315f), selectedUltimateAbility.damageField);

                EditorGUI.LabelField(Rect.MinMaxRect(170f, 300f, 220f, 320f), new GUIContent("Range"));
                selectedUltimateAbility.rangeField = EditorGUI.TextField(Rect.MinMaxRect(220f, 300f, 270f, 315f), selectedUltimateAbility.rangeField);

                EditorGUI.LabelField(Rect.MinMaxRect(280f, 300f, 340f, 320f), new GUIContent("Cooldown"));
                selectedUltimateAbility.cooldownField = EditorGUI.TextField(Rect.MinMaxRect(350f, 300f, 400f, 315f), selectedUltimateAbility.cooldownField);

                selectedUltimateAbility.displayEffects = EditorGUI.ToggleLeft(Rect.MinMaxRect(10f, 325f, 60f, 345f), "Effect", selectedUltimateAbility.displayEffects);

                EditorGUI.BeginDisabledGroup(selectedUltimateAbility.displayEffects == false);

                selectedUltimateAbility.effectType = (AbilityEffect.Type)EditorGUI.EnumPopup(Rect.MinMaxRect(65f, 325f, 130f, 345f), selectedUltimateAbility.effectType);

                EditorGUI.LabelField(Rect.MinMaxRect(140f, 325f, 190f, 345f), new GUIContent("Duration"));
                selectedUltimateAbility.effectDurationField = EditorGUI.TextField(Rect.MinMaxRect(195f, 325f, 230f, 340f), selectedUltimateAbility.effectDurationField);

                EditorGUI.LabelField(Rect.MinMaxRect(240f, 325f, 270f, 345f), new GUIContent("Rate"));
                selectedUltimateAbility.effectRateField = EditorGUI.TextField(Rect.MinMaxRect(275f, 325f, 310f, 340f), selectedUltimateAbility.effectRateField);

                EditorGUI.LabelField(Rect.MinMaxRect(320f, 325f, 360f, 345f), new GUIContent("Value"));
                selectedUltimateAbility.effectValueField = EditorGUI.TextField(Rect.MinMaxRect(360f, 325f, 400f, 340f), selectedUltimateAbility.effectValueField);

                if (ultimateAbilities.Length > 0)
                {
                    int index = currentAbilitySettings.FindIndex(x => x.IDField == ultimateAbilities[selectedUnitUltimate]);
                    currentAbilitySettings[index] = selectedUltimateAbility;
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();
                #endregion             

                currentUnitSettings[selectedUnit] = unitSettings;

                break;
            case 1:

                #region Currently Selected Ability
                EditorGUI.LabelField(Rect.MinMaxRect(5f, 25f, 50f, 45f), new GUIContent("Abilities"));

                selectedAbility = EditorGUI.Popup(Rect.MinMaxRect(55f, 25f, 350f, 45f), selectedAbility, currentAbilitySettings.Select(x => x.IDField).ToArray());

                GUILayout.BeginArea(new Rect(358f, 24f, 18f, 18f));
                if (GUILayout.Button("+"))
                {
                    AddNewAbility();
                }
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(382f, 24f, 18f, 18f));
                if (GUILayout.Button("x"))
                {
                    DeleteAbility();
                }
                GUILayout.EndArea();

                if (currentAbilitySettings.Count <= 0)
                    return;

                AbilityEditorSettings abilitySettings = currentAbilitySettings[selectedAbility];
                string selectedAbilityID = previousAbilitySettings[selectedAbility].IDField;

                EditorGUI.LabelField(Rect.MinMaxRect(5f, 50f, 80f, 70f), new GUIContent("Currently Selected Ability"), labelStyle);
                EditorGUI.DrawPreviewTexture(Rect.MinMaxRect(10f, 70f, 90f, 150f), GetAbilityTexture(currentAbilitySettings[selectedAbility].IDField));

                EditorGUI.LabelField(Rect.MinMaxRect(95f, 70f, 150f, 90f), new GUIContent("Ability ID"));
                abilitySettings.IDField = EditorGUI.TextField(Rect.MinMaxRect(153f, 70f, 250f, 85f), abilitySettings.IDField);

                EditorGUI.LabelField(Rect.MinMaxRect(110f, 100f, 145f, 120f), new GUIContent("Name"));
                abilitySettings.nameField = EditorGUI.TextField(Rect.MinMaxRect(153f, 100f, 250f, 115f), abilitySettings.nameField);

                EditorGUI.LabelField(Rect.MinMaxRect(95f, 130f, 145f, 150f), new GUIContent("Damage"));
                abilitySettings.damageField = EditorGUI.TextField(Rect.MinMaxRect(153f, 130f, 250f, 145f), abilitySettings.damageField);

                EditorGUI.LabelField(Rect.MinMaxRect(275f, 155f, 330f, 170f), new GUIContent("Max Hits"));
                abilitySettings.maxHitsField = EditorGUI.TextField(Rect.MinMaxRect(330f, 155f, 400f, 170f), abilitySettings.maxHitsField);

                EditorGUI.LabelField(Rect.MinMaxRect(270f, 70f, 330f, 90f), new GUIContent("Cooldown"));
                abilitySettings.cooldownField = EditorGUI.TextField(Rect.MinMaxRect(330f, 70f, 400f, 85f), abilitySettings.cooldownField);

                EditorGUI.LabelField(Rect.MinMaxRect(290f, 100f, 330f, 120f), new GUIContent("Range"));
                abilitySettings.rangeField = EditorGUI.TextField(Rect.MinMaxRect(330f, 100f, 400f, 115f), abilitySettings.rangeField);


                EditorGUI.LabelField(Rect.MinMaxRect(260f, 130f, 330f, 150f), new GUIContent("Attack Type"));
                abilitySettings.attackType = (AbilityType)EditorGUI.EnumPopup(Rect.MinMaxRect(330f, 130f, 400f, 150f), abilitySettings.attackType);

                abilitySettings.selectedType = EditorGUI.Popup(Rect.MinMaxRect(10f, 155f, 90f, 175f), abilitySettings.selectedType, abilityTypes);

                #endregion

                #region Effect Settings
                EditorGUI.LabelField(Rect.MinMaxRect(5f, 180f, 80f, 200f), new GUIContent("Effect Settings"), labelStyle);

                abilitySettings.displayEffects = EditorGUI.ToggleLeft(Rect.MinMaxRect(90f, 180f, 110f, 200f), string.Empty, abilitySettings.displayEffects);

                EditorGUI.BeginDisabledGroup(abilitySettings.displayEffects == false);

                EditorGUI.LabelField(Rect.MinMaxRect(10f, 200f, 50f, 215f), new GUIContent("Type"));
                abilitySettings.effectType = (AbilityEffect.Type)EditorGUI.EnumPopup(Rect.MinMaxRect(50f, 200f, 130f, 215f), abilitySettings.effectType);

                EditorGUI.LabelField(Rect.MinMaxRect(140f, 200f, 190f, 215f), new GUIContent("Duration"));
                abilitySettings.effectDurationField = EditorGUI.TextField(Rect.MinMaxRect(195f, 200f, 230f, 215f), abilitySettings.effectDurationField);

                EditorGUI.LabelField(Rect.MinMaxRect(240f, 200f, 270f, 215f), new GUIContent("Rate"));
                abilitySettings.effectRateField = EditorGUI.TextField(Rect.MinMaxRect(275f, 200f, 310f, 215f), abilitySettings.effectRateField);

                EditorGUI.LabelField(Rect.MinMaxRect(320f, 200f, 360f, 215f), new GUIContent("Value"));
                abilitySettings.effectValueField = EditorGUI.TextField(Rect.MinMaxRect(360f, 200f, 400f, 215f), abilitySettings.effectValueField);

                EditorGUI.EndDisabledGroup();

                #endregion

                currentAbilitySettings[selectedAbility] = abilitySettings;

                break;
        }

        #region Save Area
        GUILayout.BeginArea(new Rect(10f, 350f, (Screen.width / 2f) - 20f, 30f));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Revert"))
        {
            RevertChanges();
        }
        else if (GUILayout.Button("Save"))
        {
            SaveChanges();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        #endregion
    }

    void LoadFromDatabase()
    {
        Debug.Log("Load units from database");
        dbManager = FindObjectOfType<FirebaseDBManager>();

        LoadAllUnits();
        LoadAllAbilities();
    }

    #region Menu Functions
    void AddNewUnit()
    {
        if(!addingUnit)
        {
            var newUnit = new UnitEditorSettings();
            newUnit.IDField = string.Empty;
            newUnit.nameField = string.Empty;
            newUnit.healthField = "0";
            newUnit.speedField = "0.0";
            newUnit.selectedUnitType = 0;
            newUnit.selectedRarity = 0;
            selectedUnitBasic = 0;
            selectedUnitUltimate = 0;
            selectedUnit = currentUnitSettings.Count;
            currentUnitSettings.Add(newUnit);
            previousUnitSettings.Add(newUnit);
            addingUnit = true;
        }
    }


    void AddNewAbility()
    {
        if (!addingAbility)
        {
            var newAbility = new AbilityEditorSettings();
            newAbility.IDField = string.Empty;
            newAbility.nameField = string.Empty;
            newAbility.damageField = "0";
            newAbility.cooldownField = "0.0";
            newAbility.rangeField = "0.0";
            newAbility.maxHitsField = "1";
            newAbility.attackType = AbilityType.Melee;
            newAbility.effectType = AbilityEffect.Type.DOT;
            newAbility.effectDurationField = "0.0";
            newAbility.effectRateField = "0.0";
            newAbility.effectValueField = "0.0";
            newAbility.selectedType = 0;
            selectedAbility = currentAbilitySettings.Count;
            currentAbilitySettings.Add(newAbility);
            previousAbilitySettings.Add(newAbility);
            addingAbility = true;
        }
    }

    void RevertChanges()
    {
        Debug.Log("Revert the changes");

        switch(currentWindow)
        {
            case 0:
                RevertUnitChanges();
                break;
            case 1:
                RevertAbilityChanges();
                break;
        }
    }

    void RevertUnitChanges()
    {
        if (addingUnit)
        {
            selectedUnit = 0;
            addingUnit = false;
            currentUnitSettings.RemoveAt(currentUnitSettings.Count - 1);
            return;
        }

        var prev = previousUnitSettings[selectedUnit];
        var curr = currentUnitSettings[selectedUnit];

        curr.IDField = prev.IDField;
        curr.nameField = prev.nameField;
        curr.healthField = prev.healthField;
        curr.speedField = prev.speedField;
        curr.selectedUnitType = prev.selectedUnitType;
        curr.selectedRarity = prev.selectedRarity;

        //TODO: Selected abilities reset

        currentUnitSettings[selectedUnit] = curr;
    }

    private void RevertAbilityChanges()
    {
        if (addingAbility)
        {
            selectedAbility = 0;
            addingAbility = false;
            currentAbilitySettings.RemoveAt(currentAbilitySettings.Count - 1);
        }

        var prev = previousAbilitySettings[selectedAbility];
        var curr = currentAbilitySettings[selectedAbility];

        Debug.Log("Curr: "+curr.IDField + " Prev: " + prev.IDField);

        curr.IDField = prev.IDField;
        curr.nameField = prev.nameField;
        curr.damageField = prev.damageField;
        curr.cooldownField = prev.cooldownField;
        curr.rangeField = prev.rangeField;
        curr.attackType = prev.attackType;
        curr.effectType = prev.effectType;
        curr.effectDurationField = prev.effectDurationField;
        curr.effectRateField = prev.effectRateField;
        curr.effectValueField = prev.effectValueField;
        curr.selectedType = prev.selectedType;

        currentAbilitySettings[selectedAbility] = curr;
    }

    #endregion

    #region Data Functions

    void LoadAllUnits()
    {
        var unitTableRef = dbManager.GetUnitTable();
        unitTableRef.GetValueAsync().ContinueWith(task =>
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach(var unit in snapshot.Children)
                {
                    var unitSettings = new UnitEditorSettings();
                    unitSettings.IDField = unit.Key;
                    unitSettings.nameField = (string)unit.Child("name").Value;
                    unitSettings.healthField = (string)unit.Child("health").Value;
                    unitSettings.speedField = (string)unit.Child("move_speed").Value;
                    unitSettings.selectedUnitType = Convert.ToInt32(unit.Child("type").Value);
                    unitSettings.selectedRarity = Convert.ToInt32(unit.Child("rarity").Value);
                    currentUnitSettings.Add(unitSettings);
                    previousUnitSettings.Add(unitSettings);
                }
            }
        });
    }

    void LoadAllAbilities()
    {
        var abilityTableRef = dbManager.GetAbilityTable();
        var effectTableRef = dbManager.GetAbilityEffectTable();

        abilityTableRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (var ability in snapshot.Children)
                {
                    var abilitySettings = new AbilityEditorSettings();
                    abilitySettings.IDField = ability.Key;
                    abilitySettings.nameField = (string)ability.Child("name").Value;
                    abilitySettings.selectedType = Convert.ToInt32(ability.Child("type").Value);
                    abilitySettings.damageField = (string)ability.Child("damage").Value;
                    abilitySettings.cooldownField = (string)ability.Child("cooldown").Value;
                    abilitySettings.rangeField = (string)ability.Child("range").Value;
                    abilitySettings.attackType = (AbilityType)Convert.ToInt32(ability.Child("attack_type").Value);
                    abilitySettings.maxHitsField = (string)ability.Child("max _hits").Value;
                    currentAbilitySettings.Add(abilitySettings);
                    previousAbilitySettings.Add(abilitySettings);
                }
            }
        });

        effectTableRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (var effect in snapshot.Children)
                {
                    int index = currentAbilitySettings.FindIndex(c => c.IDField == effect.Key);
                    var abilitySettings = currentAbilitySettings[index];
                    abilitySettings.effectType = (AbilityEffect.Type)Convert.ToInt32(effect.Child("type").Value);
                    abilitySettings.effectDurationField = (string)effect.Child("duration").Value;
                    abilitySettings.effectRateField = (string)effect.Child("rate").Value;
                    abilitySettings.effectValueField = (string)effect.Child("value").Value;
                    abilitySettings.displayEffects = true;
                    currentAbilitySettings[index] = abilitySettings;
                    previousAbilitySettings[index] = abilitySettings;
                }
            }
        });

    }


    UnitEditorSettings LoadUnit(string unitID)
    {
        var unitSettings = new UnitEditorSettings();
        var unitRef = dbManager.GetUnitReference(unitID);

        unitRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                unitSettings.nameField = (string)snapshot.Child("name").Value;
                unitSettings.healthField = (string)snapshot.Child("health").Value;
                unitSettings.speedField = (string)snapshot.Child("move_speed").Value;
                unitSettings.selectedUnitType = Convert.ToInt32(snapshot.Child("type").Value);
                unitSettings.selectedRarity = Convert.ToInt32(snapshot.Child("rarity").Value);
            }
        });

        return unitSettings;
    }

    //Load in from database
    AbilityEditorSettings LoadAbility(string abilityID)
    {
        var abilitySettings = new AbilityEditorSettings();
        var abilityRef = dbManager.GetAbilityReference(abilityID);
        var effectRef = dbManager.GetAbilityEffectReference(abilityID);

        abilityRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                abilitySettings.nameField = (string)snapshot.Child("name").Value;
                abilitySettings.selectedType = (int)snapshot.Child("type").Value;
                abilitySettings.damageField = (string)snapshot.Child("damage").Value;
                abilitySettings.cooldownField = (string)snapshot.Child("cooldown").Value;
                abilitySettings.rangeField = (string)snapshot.Child("range").Value;
                abilitySettings.attackType = (AbilityType)snapshot.Child("attack_type").Value;
                abilitySettings.maxHitsField = (string)snapshot.Child("max_hits").Value;
            }
        });

        effectRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                abilitySettings.effectType = (AbilityEffect.Type)snapshot.Child("type").Value;
                abilitySettings.effectDurationField = (string)snapshot.Child("duration").Value;
                abilitySettings.effectRateField = (string)snapshot.Child("rate").Value;
                abilitySettings.effectValueField = (string)snapshot.Child("value").Value;
            }
        });

        return abilitySettings;
    }

    void SaveChanges()
    {
        switch(currentWindow)
        {
            case 0:
                SaveUnit();
                break;

            case 1:
                SaveAbility();
                break;
        }
    }

    void SaveUnit()
    {
        var prev = previousUnitSettings[selectedUnit];
        var curr = currentUnitSettings[selectedUnit];

        //Replace with input validation
        if (string.IsNullOrEmpty(curr.IDField)
           || string.IsNullOrEmpty(curr.nameField)
           || string.IsNullOrEmpty(curr.healthField)
           || string.IsNullOrEmpty(curr.speedField))
        {
            Debug.Log("String invalid");
            return;
        }

        //TODO: Add validators to input fields

        if (!addingUnit && prev.IDField != curr.IDField)
            dbManager.GetUnitReference(prev.IDField).SetValueAsync(curr.IDField);

        var unitRef = dbManager.GetUnitReference(curr.IDField);
        unitRef.Child("name").SetValueAsync(curr.nameField);
        unitRef.Child("health").SetValueAsync(curr.healthField);
        unitRef.Child("move_speed").SetValueAsync(curr.speedField);
        unitRef.Child("type").SetValueAsync(curr.selectedUnitType);
        unitRef.Child("rarity").SetValueAsync(curr.selectedRarity);
        unitRef.Child("basic").SetValueAsync(curr.basic);
        unitRef.Child("ultimate").SetValueAsync(curr.ultimate);

        //TODO: Reload abilities if they have been changed in unit editor
        if(!string.IsNullOrEmpty(curr.basic))
        {
            var currAbility = currentAbilitySettings.FirstOrDefault(x => x.IDField == curr.basic);
            var basicAbilityRef = dbManager.GetAbilityReference(currAbility.IDField);
            basicAbilityRef.Child("damage").SetValueAsync(currAbility.damageField);
            basicAbilityRef.Child("cooldown").SetValueAsync(currAbility.cooldownField);
            basicAbilityRef.Child("range").SetValueAsync(currAbility.rangeField);

            var basicAbilityEffectRef = dbManager.GetAbilityEffectReference(currAbility.IDField);
            if(currAbility.displayEffects)
            {
                basicAbilityEffectRef.Child("type").SetValueAsync((int)currAbility.effectType);
                basicAbilityEffectRef.Child("duration").SetValueAsync(currAbility.effectDurationField);
                basicAbilityEffectRef.Child("rate").SetValueAsync(currAbility.effectRateField);
                basicAbilityEffectRef.Child("value").SetValueAsync(currAbility.effectValueField);
            }
            else
            {
                basicAbilityEffectRef.RemoveValueAsync();
            }
        }

        if (curr.ultimate != string.Empty)
        {
            var currAbility = currentAbilitySettings.FirstOrDefault(x => x.IDField == curr.ultimate);
            var ultAbilityRef = dbManager.GetAbilityReference(currAbility.IDField);
            ultAbilityRef.Child("damage").SetValueAsync(currAbility.damageField);
            ultAbilityRef.Child("cooldown").SetValueAsync(currAbility.cooldownField);
            ultAbilityRef.Child("range").SetValueAsync(currAbility.rangeField);

            if (currAbility.displayEffects)
            {
                var ultAbilityEffectRef = dbManager.GetAbilityEffectReference(currAbility.IDField);
                ultAbilityEffectRef.Child("type").SetValueAsync((int)currAbility.effectType);
                ultAbilityEffectRef.Child("duration").SetValueAsync(currAbility.effectDurationField);
                ultAbilityEffectRef.Child("rate").SetValueAsync(currAbility.effectRateField);
                ultAbilityEffectRef.Child("value").SetValueAsync(currAbility.effectValueField);
            }
        }

        previousUnitSettings = currentUnitSettings;
        previousAbilitySettings = currentAbilitySettings;
        addingUnit = false;
    }

    void SaveAbility()
    {
        var prev = previousAbilitySettings[selectedAbility];
        var curr = currentAbilitySettings[selectedAbility];

        //Replace with input validation
        if (string.IsNullOrEmpty(curr.IDField)
            || string.IsNullOrEmpty(curr.nameField)
            || string.IsNullOrEmpty(curr.damageField)
            || string.IsNullOrEmpty(curr.cooldownField)
            || string.IsNullOrEmpty(curr.rangeField)
            || (curr.displayEffects && string.IsNullOrEmpty(curr.effectDurationField))
            || (curr.displayEffects && string.IsNullOrEmpty(curr.effectValueField))
            || (curr.displayEffects && string.IsNullOrEmpty(curr.effectValueField)))
        {
            Debug.Log("Fields cannot be empty");
            return;
        }

        if (!addingAbility && prev.IDField != curr.IDField)
        {
            dbManager.GetAbilityReference(prev.IDField).RemoveValueAsync();
        }

        var abilityRef = dbManager.GetAbilityReference(curr.IDField);
        abilityRef.Child("name").SetValueAsync(curr.nameField);
        abilityRef.Child("type").SetValueAsync(curr.selectedType);
        abilityRef.Child("damage").SetValueAsync(curr.damageField);
        abilityRef.Child("cooldown").SetValueAsync(curr.cooldownField);
        abilityRef.Child("range").SetValueAsync(curr.rangeField);
        abilityRef.Child("attack_type").SetValueAsync((int)curr.attackType);
        abilityRef.Child("max_hits").SetValueAsync(curr.maxHitsField);

        var effectRef = dbManager.GetAbilityEffectReference(curr.IDField);
        if(curr.displayEffects)
        {
            effectRef.Child("type").SetValueAsync((int)curr.effectType);
            effectRef.Child("duration").SetValueAsync(curr.effectDurationField);
            effectRef.Child("rate").SetValueAsync(curr.effectRateField);
            effectRef.Child("value").SetValueAsync(curr.effectValueField);
        }
        else
        {
            effectRef.RemoveValueAsync();
        }
       
        previousAbilitySettings[selectedAbility] = currentAbilitySettings[selectedAbility];
        addingAbility = false;
    }

    //Look over this
    /*void ReloadAbility(string abilityID)
    {
        var curr = currentUnitSettings[selectedUnit];
        if(curr.basic.IDField != abilityID)
            curr.basic = LoadAbility(abilityID);
        if(curr.ultimate.IDField != abilityID)
            curr.ultimate = LoadAbility(abilityID);
        currentUnitSettings[selectedUnit] = curr;
    }*/

    //NEXT STEPS: 
    //Load units in from database
    //Load abilities in from database
    //Load abilities into unit editor based on type
    //Link up all image paths and get some images for new units and abilities
    //Test and try to break things, specifically on transitions between unit and ability

    void DeleteUnit()
    {
        if(Time.time - lastDeleteClick < 0.25f)
        {
            Debug.Log("Delete Unit");
        }
        lastDeleteClick = Time.time;
    }

    void DeleteAbility()
    {
        if (Time.time - lastDeleteClick < 0.25f)
        {
            Debug.Log("Delete Ability");
        }
        lastDeleteClick = Time.time;
    }

    #endregion

    private Texture GetUnitTexture(string unitID)
    {
        Texture texture = Resources.Load<Texture>("Units/Textures/" + unitID);
        if(texture == null)
        {
            return Resources.Load<Texture>("Units/Textures/none");
        }
        return texture;          
    }

    private Texture GetAbilityTexture(string abilityID)
    {
        Texture texture = Resources.Load<Texture>("Abilities/Textures/" + abilityID);
        if(texture == null)
        {
            return Resources.Load<Texture>("Abilities/Textures/none");
        }
        return texture;   
    }

    private RuntimeAnimatorController GetAnimatorController(string unitID)
    {
        return Resources.Load<RuntimeAnimatorController>("Units/Anim/" + unitID);
    }
}
#endif