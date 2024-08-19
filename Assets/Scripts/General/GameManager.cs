using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

// manages scene transitions, load/save, etc
// created 22/8/23
// last modified 5/9/23

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [field: SerializeField] public UpgradeManager upgrades { get; private set; }
    [SerializeField] private AudioMixer audioMixer;
    [Header("Karmic balance")]
    [SerializeField] private float diffKarmaBalance = 0.2f;
    [field: SerializeField]public SO_ShopSettings shopSettings { get; private set; }
#if UNITY_EDITOR
    [SerializeField] private bool DEBUGWIPESAVEDATA;
#endif
    public float volBGM { get => settingData.volumeBGM; }
    public float volSFX { get => settingData.volumeSFX; }
    public int coinsStash { get => saveData.coins; }
    public int gemsStash { get => saveData.gems; }
    public int distanceBest { get => saveData.distance; }
    public bool tutorialDone { get => (saveData.GetFlag(GlobalVars.SAVEFLAGTUTORIAL)); }
    // pathing difficulty balancer
    private float diffKarma = 0f;
    private SaveData saveData;
    private SettingData settingData;

    private void Awake()
    {
        if (instance)
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        else instance = this;

        // transition from playerprefs to ES3
        if (ES3.KeyExists(GlobalVars.SAVEPROGRESS))
        {
            saveData = ES3.Load<SaveData>(GlobalVars.SAVEPROGRESS);
        }

        if (saveData == null)
        {
            saveData = new SaveData(PlayerPrefs.GetInt(GlobalVars.SAVECOINS, 0), PlayerPrefs.GetInt(GlobalVars.SAVEGEMS, 0), PlayerPrefs.GetInt(GlobalVars.SAVEDISTANCE, 0), PlayerPrefs.GetInt(GlobalVars.SAVETUTORIAL, 0), new string[0]);
            ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
        }

        if (ES3.KeyExists(GlobalVars.SAVESETTINGS))
        {
            settingData = ES3.Load<SettingData>(GlobalVars.SAVESETTINGS);
            Debug.Log("Loaded SettingData from ES3 with SFX: " + settingData.volumeSFX + " BGM: " + settingData.volumeBGM);
        }

#if UNITY_EDITOR
        if (DEBUGWIPESAVEDATA)
        {
            saveData.coins = 10000;
            saveData.gems = 1000;
            saveData.owned = new string[0];
        }
#endif

        upgrades.Initialise();

        if (settingData == null)
        {
            settingData = new SettingData(1f, PlayerPrefs.GetFloat(GlobalVars.SAVEVOLUMEBGM, 1f), PlayerPrefs.GetFloat(GlobalVars.SAVEVOLUMESFX, 1f));
            ES3.Save(GlobalVars.SAVESETTINGS, settingData);
        }

        SetVolumeBGM(settingData.volumeBGM, false);
        SetVolumeSFX(settingData.volumeSFX, false);
    }

    public void SetVolumeBGM(float volume, bool save = true)
    {
        settingData.volumeBGM = volume;
        audioMixer.SetFloat("volumeBGM", CoSephUtils.VolumeToDecibels(settingData.volumeBGM));
        if (save)
        {
            ES3.Save(GlobalVars.SAVESETTINGS, settingData);
            Debug.Log("Saved SAVESETTINGS with volumeBGM: " + settingData.volumeBGM);
        }
    }

    public void SetVolumeSFX(float volume, bool save = true)
    {
        settingData.volumeSFX = volume;
        audioMixer.SetFloat("volumeSFX", CoSephUtils.VolumeToDecibels(settingData.volumeSFX));
        if (save)
        {
            ES3.Save(GlobalVars.SAVESETTINGS, settingData);
            Debug.Log("Saved SAVESETTINGS with volumeSFX: " + settingData.volumeSFX);
        }
    }

    public void SaveSettings()
    {
        ES3.Save(GlobalVars.SAVESETTINGS, settingData);
        ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
    }

    // returns list of all SO_ShopItems already owned
    public List<SO_ShopItem> GetUpgradesOwned()
    {
        List<SO_ShopItem> itemsOwned = new List<SO_ShopItem>();

        for (int i = 0; i < saveData.owned.Length; i++)
        {
            if (saveData.owned[i] != null)
            {
                SO_ShopItem itemAdd = shopSettings.GetShopItem(saveData.owned[i]);
                if (itemAdd != null && !itemsOwned.Contains(itemAdd))
                    itemsOwned.Add(itemAdd);
            }
        }

        return itemsOwned;
    }

    public void SetCoins(int coins)
    {
        saveData.coins = coins;
        ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
    }
    public void SetGems(int gems)
    {
        saveData.gems = gems;
        ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
    }
    // add coins to the player's collection
    public void AddCoins(int coins)
    {
        saveData.coins += coins;
        ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
    }
    // add gems to the player's collection
    public void AddGems(int gems)
    {
        saveData.gems += gems;
        ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
    }

    // add a new distance high score
    public void AddDistance(int distance)
    {
        if (distance > saveData.distance)
        {
            saveData.distance = distance;
            ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
        }
    }

    public bool KarmicChance(float chanceBase = 0.5f)
    {
        float chance = chanceBase + diffKarma;

        if (Random.Range(0f, 1f) < chance)
        {
            diffKarma -= (1f - chanceBase) * diffKarmaBalance;
            return true;
        }
        else
        {
            diffKarma += chanceBase * diffKarmaBalance;
            return false;
        }
    }

    // adjust the current karmic rating by the indicating amount
    // remember positive is good for the player, negative is bad for the player
    public void KarmicAdjust(float chanceAdjustment)
    {
        diffKarma += chanceAdjustment;
    }
    public void TutorialComplete()
    {
        saveData.SetFlag(GlobalVars.SAVEFLAGTUTORIAL);
        ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
    }

    public bool HasItem(SO_ShopItem itemCheck)
    {
        for (int i = 0; i < saveData.owned.Length; i++)
        {
            if (saveData.owned[i] == itemCheck.shopUniqueName) return true;
        }
        return false;
    }

    public bool GetFlag(int flag)
    {
        return saveData.GetFlag(flag);
    }
    public void SetFlag(int flag, bool clear = false)
    {
        if (clear)
            saveData.ClearFlag(flag);
        else
            saveData.SetFlag(flag);
    }

    public bool GetItem(SO_ShopItem itemCheck)
    {
        if (HasItem(itemCheck)) return false;

        bool paid = false;
        if (itemCheck.costType == CostType.Coin)
        {
            if (saveData.coins < itemCheck.costAmount)
            {
                // fail!
            }
            else
            {
                // pay
                saveData.coins -= itemCheck.costAmount;
                paid = true;
            }
        }
        else // Gem
        {
            if (saveData.gems < itemCheck.costAmount)
            {
                // fail
            }
            else
            {
                // pay
                saveData.gems -= itemCheck.costAmount;
                paid = true;
            }
        }

        if (paid)
        {
            List<string> ownedNew = saveData.owned.ToList();
            ownedNew.Add(itemCheck.shopUniqueName);
            saveData.owned = ownedNew.ToArray();
            Debug.Log("Successfully bought " + itemCheck.shopUniqueName);

            ES3.Save(GlobalVars.SAVEPROGRESS, saveData);

            return true;
        }
        return false;
    }

    public bool GetItemPremium(SO_ShopItem itemCheck)
    {
        if (itemCheck.itemType != ItemType.BuyGems) return false;

        bool paid = false;

        // TEMP TODO actual payment system
        paid = true;

        if (paid)
        {
            saveData.gems += itemCheck.itemMagnitude;
            ES3.Save(GlobalVars.SAVEPROGRESS, saveData);
            return true;
        }

        return false;
    }
}
