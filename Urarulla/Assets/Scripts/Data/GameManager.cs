using System.Collections.Generic;
using UnityEngine;

namespace DiMe.Urarulla
{
    public class GameManager : Singleton<GameManager>
    {
        // ScriptableObjects
        public GameDataSO gameData;
        public static GameDataSO Data => Instance.gameData;
        public static GameObject RandomCharacterModel => Instance.gameData.BackGroundCharacterModels.Random();

        public GameSettingsSO gameSettings;
        internal static bool IsMultiplayer => Instance.gameSettings.IsMultiplayer;
        internal static bool CheckIfPlayerSlotsFilled
        {
            get
            {
                if (!IsMultiplayer && Players.Count >= MaxPlayerCount)
                {
                    Debug.Log("Note: This is a single-player game build. There cannot be more than one player at a time.");
                    return true;
                }
                return false;
            }
        }

        // Json
        public Questions questions;
        public static Questions Questions => Instance.questions;

        public Degrees degrees;
        internal static Degrees Degrees => Instance.degrees;

        public CharacteristicInfos characteristicInfos;
        internal static CharacteristicInfo[] CharacteristicInfos => Instance.characteristicInfos.characteristicInfos;
        
        public CharacteristicSkills characteristicSkills;
        internal static CharacteristicSkill[] CharacteristicSkills => Instance.characteristicSkills.characteristicSkills;

        // Events
        /// <summary>
        /// Activated when the MainScene is activated (index of 3).
        /// </summary>
        public static onGameStarted OnGameStarted;
        public delegate void onGameStarted();

        /// <summary>
        /// Activated when all JSON files are done being serialized in the memory.
        /// </summary>
        public static onDataLoaded OnDataLoaded;
        public delegate void onDataLoaded();

        // References
        internal LuckWheel luckWheel;
        private Transform wheel;

        internal MenusManager menusManager;
        internal void SetMenuActive(int index) => menusManager.SetMenu(index);
        internal static bool IsMainSceneActive => Instance.menusManager.CurrentActiveMenu == 3;

        internal const int MaxPlayerCount = 5;
        public List<Player> players = new List<Player>();
        public static List<Player> Players => Instance?.players;
        public Player currentTurnPlayer;
        public static int CurrentTurnPlayerIndex { get; private set; }

        internal static ResponseManager responseManager { get; private set; }
        internal static TextToSpeech textToSpeech { get; private set; }

        private void Awake()
        {
            menusManager = GetComponent<MenusManager>();
            textToSpeech = GetComponentInChildren<TextToSpeech>(true);
            CacheJson();
        }

        private void CacheJson()
        {
            degrees = JsonUtilityExt.GetData<Degrees>(gameData.DegreeFile);
            
            characteristicInfos = JsonUtilityExt.GetData<CharacteristicInfos>(gameData.CharacteristicsInfosFile);
            characteristicSkills = JsonUtilityExt.GetData<CharacteristicSkills>(gameData.CharacteristicsSkillsFile);

            questions = new Questions(
                JsonUtilityExt.GetData<Questions.CharacteristicsQuestions>(gameData.CharacteristicsQuestionsFile).characteristicsQuestions,
                JsonUtilityExt.GetData<Questions.IndustryQuestions>(gameData.IndustryQuestionsFile).industryQuestions,
                JsonUtilityExt.GetData<Questions.TrickyQuestions>(gameData.TrickyQuestionsFile).trickyQuestions
            );

            EditMenu.OnDataLoaded();
        }

        private void Start()
        {
            luckWheel = FindObjectOfType<LuckWheel>();
            wheel = luckWheel.transform.GetChild(0);
        }

        internal static void StartGame()
        {
            Instance.SetMenuActive(3);
            OnGameStarted();
        }

        internal static void AddPlayer(string name)
        {
            if (CheckIfPlayerSlotsFilled)
                return;
            Instance.players.Add(new Player($"{name}"));
            
            Debug.Log("Adding a new player to the game...");

            SetupMenu.CreateNewPlayerSetupStand();
            MainScene.CreateNewPlayerGameStand();
        }

        internal static void RemovePlayer(string name)
        {
            var index = 0;
            Player target = null;
            for (int i = 0; i < Instance.players.Count; i++)
            {
                var player = Instance.players[i];
                if (player.name == name)
                {
                    target = player;
                    index = i;
                    break;
                }
            }
            if (target == null)
                return;
            Instance.players.Remove(target);
            SetupMenu.RemovePlayerSetupStand(index);
        }

        internal void NextTurn()
        {
            if (players.Count < 2)
                return;

            var index = System.Array.IndexOf(players.ToArray(), currentTurnPlayer) + 1;
            if (index >= players.Count - 1)
                index = 0;

            currentTurnPlayer = players[index];
            CurrentTurnPlayerIndex = System.Array.IndexOf(players.ToArray(), currentTurnPlayer);

            // if we do change the turn, check player's scores' sum:
            // if it's more than 20, that means the player has answered enough questions for the game to know about the player

            if (GetCharacteristicsScoreSum(currentTurnPlayer.characteristics) > 20)
            {
                Debug.Log("TODO: move onto the next question section...");

            }
        }

        internal static void AddScore(float amount)
        {
            Instance.players[CurrentTurnPlayerIndex].scoreProgress += amount;
        }

        private int GetCharacteristicsScoreSum(Characteristics characteristics)
        {
            return characteristics.creative + characteristics.physical + characteristics.leader + characteristics.team + characteristics.greed;
        }
    }
}