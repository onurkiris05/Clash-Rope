using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VP.Nest;
using VP.Nest.Analytics;
using VP.Nest.CameraControl;
using VP.Nest.UI;
using VP.Nest.UI.InGame;

namespace _Main.Scripts.Managers
{
    public class StateManager : MonoBehaviour
    {
        public static StateManager Instance;

        [Header("General Components")] [SerializeField]
        CameraManager cameraManager;

        [SerializeField] RopeManager ropeManager;
        [SerializeField] PlayerManager playerManager;
        [SerializeField] SoldierManager soldierManager;
        [SerializeField] BattleManager battleManager;
        [SerializeField] InGameUI inGameUI;
        [SerializeField] EnemyManager enemyManager;
        [SerializeField] GateManager gateManager;
        [SerializeField] EconomyManager economyManager;

        [Header("General Settings")] [SerializeField]
        int turnPerStage = 5;

        public bool GameActive { get; set; }
        public static bool IsNewLevel { get; set; } = true;
        public static bool IsLoaded { get; private set; }

        public bool IsSaved
        {
            get => Convert.ToBoolean(PlayerPrefs.GetInt("IsSaved" + PlayerPrefKeys.CurrentLevel));
            set => PlayerPrefs.SetInt("IsSaved" + PlayerPrefKeys.CurrentLevel, Convert.ToInt32(value));
        }

        bool isHookedUp;
        bool isEndGame;
        int currentTurnIndex = 1;

        void Awake()
        {
            if (PlayerPrefs.GetInt("CurrentLevelIndex") >= 3 && !IsLoaded)
            {
                IsLoaded = true;
                SceneManager.LoadScene(PlayerPrefs.GetInt("CurrentLevelIndex"));
            }

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            GameActive = true;
        }

        public void ProcessHookedUpRope()
        {
            if (!isHookedUp)
            {
                inGameUI.SetTapFtue(false);
                soldierManager.TransferSoldiers(ropeManager.GetRopePath());
                inGameUI.ToggleRollbackButton();
                isHookedUp = true;
            }
        }

        public void ProcessTurnEnd()
        {
            if (!isEndGame)
            {
                playerManager.ProcessRestartRope();
                inGameUI.PrepareTurnEndUI();
                isEndGame = true;
            }
        }

        public void ProcessLevelEnd()
        {
            if (!isEndGame)
            {
                playerManager.ProcessRestartRope();
                UIManager.Instance.SuccessGame();
                enemyManager.Set();
                enemyManager.DiscardRemainingEnemies();
                isEndGame = true;
            }
        }

        public void ProcessNewTurn()
        {
            StartCoroutine(NewTurn());
        }

        IEnumerator NewTurn()
        {
            IsSaved = true;
            PlayerPrefs.SetInt("IsSaved", 1);

            AnalyticsManager.CustomEvent("new_turn",PlayerPrefKeys.CurrentLevel);

            if (currentTurnIndex < turnPerStage)
            {
                enemyManager.DiscardRemainingEnemies();
                soldierManager.CreateSoldiers();
                battleManager.Set();
                economyManager.Set();
                enemyManager.Set();
                cameraManager.AssignCamera("Main");

                yield return new WaitForSeconds(3f);

                inGameUI.ToggleRollbackButton();
                isEndGame = false;
                isHookedUp = false;
                GameActive = true;
                currentTurnIndex++;
                yield return null;
            }
            else
            {
                gateManager.ChangeGates();
                enemyManager.DiscardRemainingEnemies();
                soldierManager.CreateSoldiers();
                battleManager.Set();
                economyManager.Set();
                enemyManager.Set();
                cameraManager.AssignCamera("Main");

                yield return new WaitForSeconds(3f);

                inGameUI.ToggleRollbackButton();
                isEndGame = false;
                isHookedUp = false;
                GameActive = true;
                currentTurnIndex = 1;
            }
        }
    }
}