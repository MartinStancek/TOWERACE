using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public Transform playersScoreParent;

    public TMP_Text countDonwScoreText;
    public TMP_Text roundText;
    public TMP_Text winnerText;

    public int timeInSeconds = 3;

    public GameObject resultsPanel;
    public Transform towerCountDownParent;

    public int[] previousMoney;
    private int[] previousStars;
    private GameController gc;

    private int round;

    public float pointsDelay = 0.8f;


    void Awake()
    {
        gc = GameController.Instance;
        gc.onEndRace.AddListener(EndRaceScoreHandle);
        gc.onStartRace.AddListener(RoundReinicialization);
        gc.onStartGame.AddListener(StartGameInit);
        resultsPanel.SetActive(false);
    }

    private void EndRacingResult()
    {
        gc.EndRacingResult();
        resultsPanel.SetActive(false);
    }

    private void EndRaceScoreHandle()
    {
        resultsPanel.SetActive(true);
        roundText.text = "" + (round++);
        var delay = pointsDelay;

        for (var i = 0; i < gc.players.Count; i++)
        {
            var p = gc.players[i];
            var playerPanel = playersScoreParent.GetChild(p.playerIndex);
            var playerPosition = gc.playersFinished.IndexOf(p.playerIndex);
            var starsPanel = playerPanel.Find("Stars");

            var starIncome = Mathf.Clamp(3 - playerPosition, 0, 3);
            var newStarCount = Mathf.Clamp(p.stars + starIncome, 0, starsPanel.childCount);
            delay = pointsDelay;
            for (var j = 0; j < starsPanel.childCount; j++)
            {
                if (j < p.stars)
                {
                    var color = p.playerColor;
                    color.a = 0.6f;
                    starsPanel.GetChild(j).GetComponent<Image>().color = color;
                }
                else if (j < newStarCount)
                {
                    StartCoroutine(SetColorWithDelay(delay, starsPanel.GetChild(j).GetComponent<Image>(), p.playerColor));
                    delay += pointsDelay;
                }
                else
                {
                    //starsPanel.GetChild(j).GetComponent<Image>().sprite = starImages.black;
                }
            }
            p.stars = newStarCount;

            playerPanel.Find("Income").GetComponent<TMP_Text>().text = "+" + (p.money.Value - previousMoney[i]) + " $";
        }
        delay = pointsDelay;
        for(var i = 0; i< 3; i++)
        {
            StartCoroutine(PlaySoudWithDelay(delay));
            delay += pointsDelay;

        }

        countDonwScoreText.text = "" + timeInSeconds;

        var maxstars = gc.players.Max(e => e.stars);
        if (maxstars >= 10)
        {
            var winners = gc.players.Where(e => e.stars == maxstars).Select(e => playersScoreParent.GetChild(e.playerIndex).Find("Name").GetComponent<TMP_Text>().text).ToArray();
            winnerText.text = "THE WINNER IS: " + String.Join(" AND ", winners);
            winnerText.gameObject.SetActive(true);
            towerCountDownParent.gameObject.SetActive(false);
        }
        else
        {
            towerCountDownParent.gameObject.SetActive(true);

            StartCoroutine(SetEndRacingResultCountDonw(timeInSeconds - 1, EndRacingResult));
        }
    }

    private IEnumerator PlaySoudWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.PlaySound(SoundManager.SoundType.POINT_ADDED);
    }

    private IEnumerator SetColorWithDelay(float delay, Image target, Color color)
    {
        yield return new WaitForSeconds(delay);
        target.color = color;
        LeanTween.scale(target.gameObject, Vector3.one * 1.4f, 0.1f)
    .setOnComplete(() =>
    {
        LeanTween.scale(target.gameObject, Vector3.one, 0.1f);
    }
    );
    }


    private void StartGameInit()
    {
        previousMoney = new int[gc.players.Count];
        previousStars = new int[gc.players.Count];
        RoundReinicialization();

        for (var i = 0; i < 4; i++) 
        {
            playersScoreParent.GetChild(i).gameObject.SetActive(i < gc.players.Count);
        }

        foreach(var p in gc.players)
        {
            playersScoreParent.GetChild(p.playerIndex).Find("Name").GetComponent<TMP_Text>().color = p.playerColor;
            if (p.GetComponent<PlayerAI>() != null)
            {
                playersScoreParent.GetChild(p.playerIndex).Find("Name").GetComponent<TMP_Text>().text += " (BOT)";
            }


        }

        round = 1;
    }

    private void RoundReinicialization()
    {
        Debug.Log("Player count:" + gc.players.Count);
        for (var i = 0; i < gc.players.Count; i++)
        {
            previousMoney[i] = gc.players[i].money.Value;
            previousStars[i] = gc.players[i].stars;
        }
    }

    private IEnumerator SetEndRacingResultCountDonw(int secondsRemain, Action finishAction)
    {

        yield return new WaitForSeconds(1);
        Debug.Log("EndRaceResultCountDonw: " + secondsRemain);
        if (secondsRemain > 0)
        {
            countDonwScoreText.text = "" + secondsRemain;
            yield return SetEndRacingResultCountDonw(secondsRemain - 1, finishAction);
        }
        else
        {
            finishAction.Invoke();
        }
    }
}
