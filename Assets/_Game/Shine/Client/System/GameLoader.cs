using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

//TODO: Add error checking to functions and timeouts if data is not loading
public class GameLoader : MonoBehaviour
{
    [SerializeField]
    private Text loadingTextDisplay;
    [SerializeField]
    private GameObject gameLoadingPanel;
    [SerializeField]
    private GameObject gamePanel;
    
    private Queue<LoadStep> loadSteps = new Queue<LoadStep>();

    private List<string[]> loadedUnitAbilities = new List<string[]>();

    public UnityEvent GameLoadedEvent = new UnityEvent();
    public static GameLoader Instance { get; set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        //TODO: Create a system of random clever descriptions for these loading steps :)
        loadSteps.Enqueue(new LoadStep(LoadAbilitiesFromDatabase, "Loading abilities from database"));
        loadSteps.Enqueue(new LoadStep(LoadAbilityEffects, "Adding effects to abilities"));
        loadSteps.Enqueue(new LoadStep(LoadUnitsFromDatabase, "Creating game units"));
        //loadSteps.Enqueue(new LoadStep(PrintResults, "Printing results (temporary)")); //FOR TESTING

        //NEXT STEPS
        //Add references to all the animations and aesthetics to the unit
        //Figure out how to store collider size
        //Create finishing touches on units
    }

	public void Load()
	{
        NextLoadStep();
	}

	private void NextLoadStep()
    {
        if(loadSteps.Count > 0)
        {
            LoadStep nextStep = loadSteps.Dequeue();
            Debug.Log(nextStep.description);
            loadingTextDisplay.text = nextStep.description;
            nextStep.method.Invoke();       
        }
        else
        {
            gameLoadingPanel.SetActive(false);
            gamePanel.SetActive(true);
            GameLoadedEvent.Invoke();
        }
    }

    private void LoadAbilitiesFromDatabase()
    {
        var databaseManager = FindObjectOfType<FirebaseDBManager>();
        var abilityTask = databaseManager.GetAbilityTable().GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot abilityTableRef = task.Result;

                foreach (var abilityRef in abilityTableRef.Children)
                {
                    Ability ability = null;
                    AbilityType basicAbilityType = (AbilityType)Convert.ToInt32(abilityRef.Child("type").Value);
                    if (basicAbilityType == AbilityType.Melee)
                        ability = new MeleeAbility();
                    else if (basicAbilityType == AbilityType.MeleeAOE)
                        ability = new MeleeAOEAbility();
                    else if (basicAbilityType == AbilityType.Projectile)
                        ability = new ProjectileAbility();
                    else if (basicAbilityType == AbilityType.ProjectileAOE)
                        ability = new ProjectileAOEAbility();

                    ability.id = abilityRef.Key;
                    ability.name = (string)abilityRef.Child("name").Value;
                    ability.damage = float.Parse((string)abilityRef.Child("damage").Value);
                    ability.attackRange = float.Parse((string)abilityRef.Child("range").Value);
                    ability.attackSpeed = float.Parse((string)abilityRef.Child("cooldown").Value);
                    ability.maxHits = Convert.ToInt32(abilityRef.Child("max_hits").Value);
                    DataLibrary.Instance.Abilities.Add(ability);
                }
                NextLoadStep();
            }
            else
            {
                Debug.Log("Failed to load abilities from database");
            }
        });
    }

    private void LoadAbilityEffects()
    {
        var databaseManager = FindObjectOfType<FirebaseDBManager>();

        var effectTask = databaseManager.GetAbilityEffectTable().GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot effectTableRef = task.Result;

                foreach(var effectRef in effectTableRef.Children)
                {
                    AbilityEffect effect = new AbilityEffect();
                    effect.type = (AbilityEffect.Type)Convert.ToInt32(effectRef.Child("type").Value);
                    effect.duration = float.Parse((string)effectRef.Child("duration").Value);
                    effect.rate = float.Parse((string)effectRef.Child("rate").Value);
                    effect.value = float.Parse((string)effectRef.Child("value").Value);

                    DataLibrary.Instance.GetAbilityById(effectRef.Key).effect = effect;
                }
                NextLoadStep();
            }
            else
            {
                Debug.Log("Failed to load ability effects from database");
            }
        });
    }

    private void LoadUnitsFromDatabase()
    {
        var databaseManager = FindObjectOfType<FirebaseDBManager>();

        var unitTask = databaseManager.GetUnitTable().GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (var unit in snapshot.Children)
                {
                    string unitID = unit.Key;
                    string unitName = (string)unit.Child("name").Value;
                    int unitHealth = Convert.ToInt32(unit.Child("health").Value);
                    float unitSpeed = float.Parse((string)unit.Child("move_speed").Value);

                    int unitRarity = Convert.ToInt32(unit.Child("rarity").Value);
                    string basicID = (string)unit.Child("basic").Value;
                    string ultimateID = (string)unit.Child("ultimate").Value;

                    BaseUnit.Type unitType = (BaseUnit.Type)Convert.ToInt32(unit.Child("type").Value);

                    GameObject unitObject = new GameObject();
                    if (unitType == BaseUnit.Type.Hero)
                        unitObject.AddComponent<Hero>();
                    //else if (unitType == BaseUnit.Type.Artillery)
                    //    unitObject.AddComponent<Artillery>();
                    //else if (unitType == BaseUnit.Type.Minion)
                    //    unitObject.AddComponent<Minion>();

                    BaseUnit baseUnit = unitObject.GetComponent<BaseUnit>();

                    baseUnit.name = unitName;
                    baseUnit.id = unitID;
                    baseUnit.maxHealth = unitHealth;
                    baseUnit.currentHealth = unitHealth;
                    baseUnit.moveSpeed = unitSpeed;
                    baseUnit.rarity = unitRarity;
                    baseUnit.basicAbility = DataLibrary.Instance.GetAbilityById(basicID);
                    baseUnit.ultimateAbility = DataLibrary.Instance.GetAbilityById(ultimateID);
                    DataLibrary.Instance.Units.Add(baseUnit);
                }
                NextLoadStep();
            }
            else
            {
                Debug.Log("Failed to load units from database");
            }
        });
    }

    private void PrintResults()
    {
        foreach(BaseUnit unit in DataLibrary.Instance.GetUnits<Hero>())
        {
            Debug.Log(">>> Unit: " + unit.id);
            Debug.Log("Name: " + unit.name);
            Debug.Log("Max Health: " + unit.maxHealth);
            Debug.Log("Move Speed: " + unit.moveSpeed);
            Debug.Log("Rarity: " + unit.rarity);
            if(unit.basicAbility != null)
            {
                Debug.Log(">> Basic Ability: " + unit.basicAbility.id);
                Debug.Log("Name: " + unit.basicAbility.name);
                Debug.Log("Damage: " + unit.basicAbility.damage);
                Debug.Log("Range: " + unit.basicAbility.attackRange);
                Debug.Log("Speed: " + unit.basicAbility.attackSpeed);
                Debug.Log("Max Hits: " + unit.basicAbility.maxHits);
                if(unit.basicAbility.effect != null)
                {
                    Debug.Log("> Effect: " + unit.basicAbility.effect.type);
                    Debug.Log("Duration: " + unit.basicAbility.effect.duration);
                    Debug.Log("Rate: " + unit.basicAbility.effect.rate);
                    Debug.Log("Value: " + unit.basicAbility.effect.value);
                }
            }

            if (unit.ultimateAbility != null)
            {
                Debug.Log(">> Ultimate Ability: " + unit.ultimateAbility.id);
                Debug.Log("Name: " + unit.ultimateAbility.name);
                Debug.Log("Damage: " + unit.ultimateAbility.damage);
                Debug.Log("Range: " + unit.ultimateAbility.attackRange);
                Debug.Log("Speed: " + unit.ultimateAbility.attackSpeed);
                Debug.Log("Max Hits: " + unit.ultimateAbility.maxHits);

                if (unit.ultimateAbility.effect != null)
                {
                    Debug.Log("> Effect: " + unit.ultimateAbility.effect.type);
                    Debug.Log("Duration: " + unit.ultimateAbility.effect.duration);
                    Debug.Log("Rate: " + unit.ultimateAbility.effect.rate);
                    Debug.Log("Value: " + unit.ultimateAbility.effect.value);
                }
            }
        }

        NextLoadStep();
    }

    private class LoadStep
    {
        public delegate void LoadMethod();
        public LoadMethod method;
        public string description;

        public LoadStep(LoadMethod method, string desc)
        {
            this.method = method;
            this.description = desc;
        }
    }
}
