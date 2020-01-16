using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FirebaseDBManager : MonoBehaviour
{
    private const string kPlayerTable = "players";
    private const string kUnitTable = "units";
    private const string kUnitOwnershipTable = "unit_ownership";
    private const string kAbilityTable = "abilities";
    private const string kAbilityEffectTable = "ability_effects";

    void Awake()
    {
        CheckForFirebaseUpdate();
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://projectdk-5df4a.firebaseio.com/");
        FirebaseApp.DefaultInstance.SetEditorP12FileName("ProjectDK-16ff83eff5d8.p12");
        FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("firebase-adminsdk-nn61x@projectdk-5df4a.iam.gserviceaccount.com");
        FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");
        DontDestroyOnLoad(this.gameObject);
    }

    void CheckForFirebaseUpdate()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }

    private void CreatePlayer(string userID, string nickname)
    {
        PlayerModel player = new PlayerModel(nickname);
        string json = JsonUtility.ToJson(player);
        //databaseReference.Child(kPlayerTable).Child(userID).SetRawJsonValueAsync(json);
        //databaseReference.Child(kPlayerTable).Child(userID).Child("experience").SetValueAsync(200);
    }

    public DatabaseReference GetUnitTable()
    {
        return FirebaseDatabase.DefaultInstance.RootReference.Child(kUnitTable);
    }

    public DatabaseReference GetUnitReference(string unitID)
    {
        return FirebaseDatabase.DefaultInstance.RootReference.Child(kUnitTable).Child(unitID);
    }

    public DatabaseReference GetAbilityTable()
    {
        return FirebaseDatabase.DefaultInstance.RootReference.Child(kAbilityTable);
    }

    public DatabaseReference GetAbilityReference(string abilityID)
    {
        return FirebaseDatabase.DefaultInstance.RootReference.Child(kAbilityTable).Child(abilityID);
    }

    public DatabaseReference GetAbilityEffectTable()
    {
        return FirebaseDatabase.DefaultInstance.RootReference.Child(kAbilityEffectTable);
    }

    public DatabaseReference GetAbilityEffectReference(string abilityID)
    {
        return FirebaseDatabase.DefaultInstance.RootReference.Child(kAbilityEffectTable).Child(abilityID);
    }

    /*public void SetUnitReference(string prevUnitID, string newUnitID)
    {
        FirebaseDatabase.DefaultInstance.RootReference.Child(kUnitTable).Child(prevUnitID).SetValueAsync(newUnitID);
    }*/
    //END NOTE


    /*private void CollectUnit(string userID, string unitID)
    {
        UnitOwnershipModel unitOwnership = new UnitOwnershipModel(userID, unitID, DateTime.Now);
        string json = JsonUtility.ToJson(unitOwnership);
        DatabaseReference unitRef = databaseReference.Child(kUnitOwnershipTable).Child(userID).Child(unitID);
            
        unitRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("There was a problem getting a reference to the unit");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string unitName = (string)snapshot.Child("unitName").Value;
                if(unitName != null)
                {
                    Debug.Log("Found unit: " + unitName);
                }
                else
                {
                    Debug.Log("Didnt find unit. Should create it.");
                }
            }
        });
    }*/
}

public class PlayerModel
{
    public string nickname;
    public int experience;

    public PlayerModel(string nickname)
    {
        this.nickname = nickname;
        this.experience = 0;
    }
}

public class UnitModel
{
    public string unitID;
    public string unitName;
    public float health;
    public int basicAbilityID;
    public int ultimateAbilityID;
    public int unitType;

    public UnitModel(string unitName, float health, int basicAbilityID, int ultimateAbilityID, int unitType)
    {
        this.unitName = unitName;
        this.health = health;
        this.basicAbilityID = basicAbilityID;
        this.ultimateAbilityID = ultimateAbilityID;
        this.unitType = unitType;
    }
}

public class UnitOwnershipModel
{
    public string userID;
    public string unitID;
    public int elixir;
    public int level;
    public DateTime collectDate;

    public UnitOwnershipModel(string userID, string unitID, DateTime collectDate)
    {
        this.userID = userID;
        this.unitID = unitID;
        this.collectDate = collectDate;
        this.elixir = 0;
        this.level = 0;
    }
}

public class AbilityModel
{


}