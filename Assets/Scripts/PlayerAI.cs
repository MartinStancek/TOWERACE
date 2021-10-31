using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Player))]
public class PlayerAI : MonoBehaviour
{
    private Player player;
    private CarController car;
    private TowerPlacer towerPlacer;
    private CheckPointController checkpointController;

    private bool snapSelected = false;
    private bool snapBuyed = false;
    private bool towerSelected = false;
    private int snapMoves;
    private int targetTowerIndex;

    private float lastTimeAction = 0f;
    public float actionEvery = 0.5f;

    private void Start()
    {
        player = GetComponent<Player>();
        car = GetComponentInChildren<CarController>();
        checkpointController = car.rb.GetComponent<CheckPointController>();
        towerPlacer = GetComponent<TowerPlacer>();

        StartCoroutine(InvokeWithDelay(0.5f, () =>
        {
            player.SetReady(true);
        }));

        GameController.Instance.onRacingResultEnd.AddListener(ResetTowerPlacing);
    }

    private void ResetTowerPlacing()
    {
        snapSelected = false;
        snapBuyed = false;
        towerSelected = false;
        snapMoves = Mathf.RoundToInt(Random.Range(0f, 5f)) + 1;
        targetTowerIndex = Mathf.RoundToInt(Random.Range(-0.5f, 4.5f));

        while (targetTowerIndex > 0 && towerPlacer.towerOptions.data[targetTowerIndex].price + 100 > player.money.Value )
        {
            targetTowerIndex--;
        }
    }

    private float randomState = 0f;

    void FixedUpdate()
    {
        if (GameController.Instance.gameMode.Value == GameMode.RACING)
        {
            var dificulty = (PlayerPrefs.GetFloat("bot_dificulty", MenuManager.botDificultyDefaultValue) - 0.5f) * 0.4f;
            car.OnAcceleration(Mathf.Clamp(1f + dificulty, 0.9f, 2f));

            var targetIndex = mod(checkpointController.lastPassed + 2, GameController.Instance.checkPonts.childCount);
            var targetCheckpoint = GameController.Instance.checkPonts.GetChild(targetIndex);
            var angle = Vector3.SignedAngle(targetCheckpoint.transform.position - car.transform.position, car.transform.forward, Vector3.up);
            if (angle > 3f)
            {
                car.OnSteering(Mathf.Clamp(-1f + Random.Range(-0.2f, 0.2f), -1f, 0f) - dificulty * 0.5f + randomState);
            } 
            else if(angle < -3f)
            {
                car.OnSteering(Mathf.Clamp(1f + Random.Range(-0.2f, 0.2f), 0f, 1f) + dificulty * 0.5f + randomState);
            } 
            else 
            {
                car.OnSteering(randomState);
            }
            if (Time.time - lastTimeAction > actionEvery)
            {
                lastTimeAction = Time.time;
                randomState = Random.Range(-0.6f, 0.6f);
            }
        }

        if (GameController.Instance.gameMode.Value == GameMode.TOWER_PLACING && player.money.Value >= 100)
        {
            if (!snapSelected && Time.time - lastTimeAction > actionEvery)
            {
                var direction = Vector2.zero;
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    direction.x = Mathf.RoundToInt(Random.Range(0f, 1f)) * 2 - 1;
                } 
                else
                {
                    direction.y = Mathf.RoundToInt(Random.Range(0f, 1f)) * 2 - 1;
                }
                towerPlacer.MoveSpot(direction);
                lastTimeAction = Time.time;
                snapMoves--;

                if(snapMoves <= 0)
                {
                    snapSelected = true;
                    towerPlacer.OnSpotClick();
                }
            }

            if (!snapBuyed && Time.time - lastTimeAction > actionEvery)
            {
                lastTimeAction = Time.time;
                snapBuyed = true;
                towerPlacer.Clicked();
            }

            if (!towerSelected && Time.time - lastTimeAction > actionEvery)
            {
                lastTimeAction = Time.time;
                Debug.Log("Bot target index: " + targetTowerIndex + ", current: " + towerPlacer.towerIndex);
                if (targetTowerIndex - towerPlacer.towerIndex < 0)
                {
                    towerPlacer.MoveTowerLeft();
                }
                else if (targetTowerIndex - towerPlacer.towerIndex > 0)
                {
                    towerPlacer.MoveTowerRight();
                } 
                else
                {
                    towerSelected = true;
                    towerPlacer.Clicked();
                    ResetTowerPlacing();
                }
            }
        }

        if(GameController.Instance.gameMode.Value == GameMode.TOWER_PLACING && !player.isReady && 
            (player.money.Value < 100 || GameController.Instance.GetAllFreeTowerSnapes(player).Count == 0))
        {
            player.SetReady(true);
        }


    }

    int mod(int k, int n) { return ((k %= n) < 0) ? k + n : k; }

    private IEnumerator InvokeWithDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();

    }
}
