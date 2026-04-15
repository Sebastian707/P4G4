using System.Collections.Generic;
using UnityEngine;
public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField]
    private Weapon.WeaponCatagory currentCategory;
    [SerializeField]
    private int currentWeaponIndex;
    [SerializeField]
    private GameObject prevWeapon;
    [SerializeField]
    private GameObject currWeapon;
    [SerializeField]
    private List<GameObject> allWeapons = new List<GameObject>();
    [SerializeField]
    private List<GameObject> projectileWeapons = new List<GameObject>();
    [SerializeField]
    private List<GameObject> hitScanWeapons = new List<GameObject>();
    [SerializeField]
    private List<GameObject> meleeWeapons = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Weapon[] weapons = GetComponentsInChildren<Weapon>(true);
        foreach (Weapon weapon in weapons)
        {
            if (weapon.isActiveAndEnabled)
            {
                currWeapon = weapon.gameObject;
                currentCategory = weapon.WeaponsCatagory;
            }
            allWeapons.Add(weapon.gameObject);
            if (weapon.WeaponsCatagory == Weapon.WeaponCatagory.HITSCAN)
            {
                hitScanWeapons.Add(weapon.gameObject);
                continue;
            }
            if (weapon.WeaponsCatagory == Weapon.WeaponCatagory.PROJECTILE)
            {
                projectileWeapons.Add(weapon.gameObject);
            }
        }
    }


    void OnWeapon1()
    {
        SetWeapon(Weapon.WeaponCatagory.PROJECTILE);
    }
    void OnWeapon2()
    {
        SetWeapon(Weapon.WeaponCatagory.HITSCAN);


    }
    void OnWeapon3()
    {
        SetWeapon(Weapon.WeaponCatagory.MELEE);


    }
    void OnWeapon4()
    {


    }
    List<GameObject> GetWeaponsFromCatagory(Weapon.WeaponCatagory weaponCatagory)
    {
        switch (weaponCatagory)
        {
            case Weapon.WeaponCatagory.MELEE:
                return meleeWeapons;
            case Weapon.WeaponCatagory.HITSCAN:
                return hitScanWeapons;
            case Weapon.WeaponCatagory.PROJECTILE:
                return projectileWeapons;
            default:
                return null;
        }
    }
    void SetWeapon(Weapon.WeaponCatagory weaponCategory)
    {
        if (currentCategory == weaponCategory) currentWeaponIndex++;
        currentCategory = weaponCategory;
        var weaponsInCatagory = GetWeaponsFromCatagory(currentCategory);
        if (weaponsInCatagory.Count <= currentWeaponIndex)
        {
            currentWeaponIndex = 0;
        }
        if (weaponsInCatagory.Count > 0)
        {
            prevWeapon = currWeapon;
            prevWeapon.SetActive(false);
            currWeapon = weaponsInCatagory[currentWeaponIndex];
            currWeapon.SetActive(true);
        } else
        {
            Debug.Log("no weapons in catagory");
        }



    }
}
