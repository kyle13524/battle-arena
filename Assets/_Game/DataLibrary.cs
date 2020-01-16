using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class DataLibrary : MonoBehaviour 
{
	public List<BaseUnit> Units = new List<BaseUnit>();
    public List<Ability> Abilities = new List<Ability>();

	public static DataLibrary Instance { get; private set; }
	void Awake()
	{
		if (Instance == null)
			Instance = this;
		else if (Instance != this)
			Destroy(gameObject);
	}

	public T[] GetUnits<T>() where T : BaseUnit 
	{
        return Units.OfType<T>().ToArray();
	}

    public T[] GetAbilities<T>() where T : Ability
    {
        return Abilities.OfType<T>().ToArray();
    }

	public BaseUnit GetUnitById(string id)
	{
        return Units.FirstOrDefault(u => u.id == id);
	}

    public Ability GetAbilityById(string id)
    {
        return Abilities.FirstOrDefault(u => u.id == id);
    }
}
