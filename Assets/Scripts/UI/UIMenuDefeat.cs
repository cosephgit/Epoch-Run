using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// defeat menu when the player quits or loses

public class UIMenuDefeat : MonoBehaviour
{
    [SerializeField] private UIMenus menuHub;
    [SerializeField] private UIBaseAccumulator defeatDistance;
    [SerializeField] private UIBase defeatHighScore;
    [SerializeField] private int defeatPopCount = 4;
    [SerializeField] private float defeatPopMagnitude = 4f;
    [SerializeField] private float defeatPopDelay = 0.5f;
    [SerializeField] private GameObject buttonContinueAd;
    [SerializeField] private GameObject buttonContinueGem;
    [SerializeField] private TextMeshProUGUI defeatGemPrice;
    [SerializeField] private GameObject placeholderAdWall;
    [SerializeField] private AudioClip recoverySound;

    public void Initialise()
    {
        gameObject.SetActive(false);
    }

    public void Open(int distance, int coins, int gems)
    {
        int revivecost = GameManager.instance.shopSettings.reviveGemCost;

        gameObject.SetActive(true);

        GameManager.instance.SetCoins(coins);
        GameManager.instance.SetGems(gems);

        if (gems < revivecost)
        {
            buttonContinueAd.gameObject.SetActive(true);
            buttonContinueGem.gameObject.SetActive(false);
        }
        else
        {
            defeatGemPrice.text = revivecost.ToString();
            buttonContinueAd.gameObject.SetActive(false);
            buttonContinueGem.gameObject.SetActive(true);
        }

        if (distance > GameManager.instance.distanceBest)
        {
            defeatDistance.SetValue(distance);
            StartCoroutine(NewHighScorePops());
            defeatHighScore.gameObject.SetActive(true);
        }
        else
        {
            defeatDistance.SetValue(distance, true);
            defeatHighScore.gameObject.SetActive(false);
        }
        GameManager.instance.AddDistance(distance);
        GameManager.instance.SaveSettings();
    }

    private IEnumerator NewHighScorePops()
    {
        for (int i = 0; i < defeatPopCount; i++)
        {
            defeatHighScore.AddShake(defeatPopMagnitude);
            UIPopManager.instance.ShowPops(defeatHighScore.transform.position, defeatPopMagnitude, Color.magenta);
            yield return new WaitForSeconds(defeatPopDelay);
        }
    }

    public void ButtonContinueAd()
    {
        menuHub.SoundButton();
        placeholderAdWall.gameObject.SetActive(true);
    }
    public void ButtonContinueGems()
    {
        if (GameManager.instance.shopSettings.reviveGemCost > PlayerPawn.instance.pawnPurse.gems)
        {
            ButtonContinueAd();
        }
        else
        {
            PlayerPawn.instance.pawnPurse.AddGems(-GameManager.instance.shopSettings.reviveGemCost);

            AudioManager.instance.SoundPlayEven(recoverySound, Vector2.zero);
            Continue();
        }
    }
    public void ButtonContinuePostAd()
    {
        menuHub.SoundButton();
        placeholderAdWall.gameObject.SetActive(false);
        Continue();
    }
    private void Continue()
    {
        Debug.Log("EXTRA LIFE! GO!");
        TerrainManager.instance.PlayerRecover();
    }
    public void ButtonRestart()
    {
        menuHub.ButtonRestartConfirm();
    }
    public void ButtonQuit()
    {
        menuHub.ButtonDefeatConfirm();
    }
}