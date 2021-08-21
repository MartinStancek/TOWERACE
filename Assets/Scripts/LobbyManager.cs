using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LobbyManager : MonoBehaviour
{
    #region Singleton
    private static LobbyManager _instance;
    public static LobbyManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType<LobbyManager>();
            }
            return _instance;

        }
    }
    #endregion

    private Coroutine readyCor = null;
    private GameController gc;

    // Start is called before the first frame update
    void Awake()
    {
        gc = GameController.Instance;

    }

    public void LobbyPlayersReady()
    {
        gc.countDownText.gameObject.SetActive(true);
        gc.countDownText.text = "" + 3;

        readyCor = StartCoroutine(SetEndLobbyCountDonw(2, gc.StartGame, () =>
        {
            gc.countDownText.gameObject.SetActive(false);

        }));
    }
    public void ResetLobbyPlayersReady()
    {
        if (readyCor != null)
        {
            StopCoroutine(readyCor);
        }
        gc.countDownText.gameObject.SetActive(false);
    }

    public int ReadyPlayersCount()
    {
        int count = 0;
        foreach (var p in gc.players)
        {
            if (p.isReady)
            {
                count++;
            }
        }
        return count;
    }

    private IEnumerator SetEndLobbyCountDonw(int secondsRemain, Action finishAction, Action reverseAction)
    {

        yield return new WaitForSeconds(1);
        if (ReadyPlayersCount() < gc.players.Count)
        {
            reverseAction.Invoke();
        }
        else
        {
            Debug.Log("EndLobbyCountDonw: " + secondsRemain);
            if (secondsRemain > 0)
            {
                gc.countDownText.text = "" + secondsRemain;
                yield return SetEndLobbyCountDonw(secondsRemain - 1, finishAction, reverseAction);
            }
            else
            {
                finishAction.Invoke();
            }
        }
    }
}
