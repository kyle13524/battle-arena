using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UnitLibrary : MonoBehaviour 
{
	private List<BaseUnit> units = new List<BaseUnit>();

	public static UnitLibrary Instance { get; private set; }
	void Awake()
	{
		if (Instance == null)
			Instance = this;
		else if (Instance != this)
			Destroy(gameObject);
	}

    public void AddUnit(BaseUnit unit)
    {
        units.Add(unit);
    }

	public T[] GetUnits<T>() where T : BaseUnit 
	{
		return units.OfType<T>().ToArray();
	}

	public BaseUnit GetUnitById(string id)
	{
		return units.Where(u => u.id == id).FirstOrDefault();
	}
}
