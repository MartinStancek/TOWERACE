using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
[System.Serializable]
public class StarImages
{
    public Sprite black;
    public Sprite yellow;
    public Sprite blackYellow;
}
public class ScoreManager : MonoBehaviour
{
    public Transform playersScoreParent;

    public StarImages starImages;

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

        for (var i = 0; i < gc.players.Count; i++)
        {
            var p = gc.players[i];
            var playerPanel = playersScoreParent.GetChild(p.playerIndex);
            var playerPosition = gc.playersFinished.IndexOf(p.playerIndex);
            var starsPanel = playerPanel.Find("Stars");

            var starIncome = Mathf.Clamp(3 - playerPosition, 0, 3);
            var newStarCount = Mathf.Clamp(p.stars + starIncome, 0, starsPanel.childCount);

            for (var j = 0; j < starsPanel.childCount; j++)
            {
                if (j < p.stars)
                {
                    starsPanel.GetChild(j).GetComponent<Image>().sprite = starImages.yellow;
                }
                else if (j < newStarCount)
                {
                    starsPanel.GetChild(j).GetComponent<Image>().sprite = starImages.blackYellow;
                } 
                else
                {
                    starsPanel.GetChild(j).GetComponent<Image>().sprite = starImages.black;
                }
            }
            p.stars = newStarCount;

            playerPanel.Find("Income").GetComponent<TMP_Text>().text = "+" + (p.money - previousMoney[i]) + "$";
        }

        countDonwScoreText.text = "" + timeInSeconds;

        var maxstars = gc.players.Max(e => e.stars);
        if (maxstars >= 10)
        {
            var winners = gc.players.Where(e => e.stars == maxstars).Select(e => "player" + (e.playerIndex + 1)).ToArray();
            winnerText.text = "The Winner is: " + String.Join(" and ", winners);
            winnerText.gameObject.SetActive(true);
            towerCountDownParent.gameObject.SetActive(false);
        }
        else
        {
            towerCountDownParent.gameObject.SetActive(true);

            StartCoroutine(SetEndRacingResultCountDonw(timeInSeconds - 1, EndRacingResult));
        }
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
            playersScoreParent.GetChild(p.playerIndex).GetComponent<Image>().color = p.playerColor;

        }

        round = 1;
    }

    private void RoundReinicialization()
    {
        for (var i = 0; i < gc.players.Count; i++)
        {
            previousMoney[i] = gc.players[i].money;
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
