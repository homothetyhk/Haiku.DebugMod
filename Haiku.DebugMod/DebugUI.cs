using System.Linq;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Modding;
using UnityEngine.UI;

namespace Haiku.DebugMod {
    public sealed class DebugUI : MonoBehaviour {
        public static bool ShowStats = false;
        private static string[] fileNames = new string[10];
        GameObject DebugCanvas;
        GameObject CheatsPanel;
        GameObject ShowStatsGameObject;
        GameObject ShowStatesGameObject;
        GameObject DisplayLoadingSavingGameObject;
        Text CheatsText;
        Text ShowStatsText;
        Text ShowStatesText;
        Text DisplayLoadingSavingText;

        void Start()
        {
            DebugCanvas = CanvasUtil.CreateCanvas(1);
            DebugCanvas.name = "DebugCanvas";
            DebugCanvas.transform.SetParent(gameObject.transform);

            GameObject DebugPanel = CanvasUtil.CreateBasePanel(DebugCanvas, 
                new CanvasUtil.RectData(new Vector2(0, 0), new Vector2(10, -4), new Vector2(0, 0), new Vector2(1, 2)));
            DebugPanel.name = "DebugPanel";

            #region Cheats
            CheatsPanel = CanvasUtil.CreateTextPanel(DebugPanel, "NoHeat", 5, TextAnchor.MiddleLeft, 
                new CanvasUtil.RectData(new Vector2(400, 10), new Vector2(0, 0)),CanvasUtil.GameFont);
            CheatsPanel.name = "CheatsPanel";
            CheatsText = CheatsPanel.GetComponent<Text>();
            #endregion

            ShowStatsGameObject = CanvasUtil.CreateTextPanel(DebugPanel, "", 4, TextAnchor.MiddleLeft,
                new CanvasUtil.RectData(new Vector2(400, 300), new Vector2(0, -90)), CanvasUtil.GameFont);
            ShowStatsGameObject.name = "ShowStatsGameObject";
            ShowStatsGameObject.SetActive(false);
            ShowStatsText = ShowStatsGameObject.GetComponent<Text>();

            #region SaveStates
            ShowStatesGameObject = CanvasUtil.CreateTextPanel(DebugPanel, "", 4, TextAnchor.MiddleLeft,
                new CanvasUtil.RectData(new Vector2(400, 300), new Vector2(0, -175)), CanvasUtil.GameFont);
            ShowStatesGameObject.name = "ShowStatesGameObject";
            ShowStatesGameObject.SetActive(false);
            ShowStatesText = ShowStatesGameObject.GetComponent<Text>();
            
            DisplayLoadingSavingGameObject = CanvasUtil.CreateTextPanel(DebugPanel, "", 10, TextAnchor.MiddleLeft,
                new CanvasUtil.RectData(new Vector2(400, 300), new Vector2(280, -200)), CanvasUtil.GameFont);
            DisplayLoadingSavingGameObject.name = "DisplayLoadingSavingGameObject";
            DisplayLoadingSavingGameObject.SetActive(false);
            DisplayLoadingSavingText = DisplayLoadingSavingGameObject.GetComponent<Text>();
            #endregion
        }

        public static void findFileNames()
        {
            fileNames = SaveStates.SaveData.loadFileName(Settings.debugPath + $"/SaveState/{SaveStates.SaveStatesManager.currentPage}/fileNameList.haiku");
            if (fileNames == null) return;
        }

        void Update()
        {
            Hooks.timer += 0.02f;

            #region Cheats
            string ActiveCheats = "";
            if (MiniCheats.IgnoreHeat)
            {
                ActiveCheats += "No Heat ";
            }
            if (MiniCheats.Invuln)
            {
                ActiveCheats += "Invuln ";
            }
            if (MiniCheats.CameraFollow)
            {
                ActiveCheats += $"CamFollow: {CameraBehavior.instance.cameraObject.orthographicSize} ";
            }
            CheatsText.text = ActiveCheats;

            ShowStatsGameObject.SetActive(ShowStats);

            if (ShowStats)
            {
                
                var gm = GameManager.instance;
                if (!gm) return;

                var tileCount = gm.mapTiles.Count(t => t.explored);
                var chipCount = gm.chip.Count(c => c.collected);
                var bossCount = gm.bosses.Count(b => b.defeated);
                var cellCount = gm.powerCells.Count(c => c.collected);
                var disruptCount = gm.disruptors.Count(d => d.destroyed);
                var slotCount = gm.chipSlot.Count(s => s.collected);
                var stationCount = gm.trainStations.Count(s => s.unlockedStation);

                int abilityCount = 0;
                if (gm.canBomb)
                {
                    abilityCount++;
                }
                if (gm.canRoll)
                {
                    abilityCount++;
                }
                if (gm.canWallJump)
                {
                    abilityCount++;
                }
                if (gm.canDoubleJump)
                {
                    abilityCount++;
                }
                if (gm.canGrapple)
                {
                    abilityCount++;
                }
                if (gm.canTeleport)
                {
                    abilityCount++;
                }
                if (gm.waterRes)
                {
                    abilityCount++;
                }
                if (gm.fireRes)
                {
                    abilityCount++;
                }
                if (gm.lightBulb)
                {
                    abilityCount++;
                }

                var completePercent = 85f * (tileCount + chipCount + cellCount + disruptCount + slotCount + stationCount + abilityCount + gm.maxHealth + gm.coolingPoints) /
                                      (gm.mapTiles.Length + gm.chip.Length + gm.powerCells.Length + gm.disruptors.Length +
                                       gm.chipSlot.Length + gm.trainStations.Length + 9 + 8 + 3);
                completePercent += bossCount;

                string stats = $"Map Tiles {tileCount}/{gm.mapTiles.Length}" + "\n" + 
                    $"Disruptors {disruptCount}/{gm.disruptors.Length}" + "\n" +
                    $"Chips {chipCount}/{gm.chip.Length}" + "\n" + 
                    $"Chip Slots {slotCount}/{gm.chipSlot.Length}" + "\n" +
                    $"Power Cells {cellCount}/{gm.powerCells.Length}" + "\n" + 
                    $"Bosses {bossCount}/{gm.bosses.Length}" + "\n" + 
                    $"Stations {stationCount}/{gm.trainStations.Length}" + "\n" + 
                    $"Coolant {gm.coolingPoints}/3" + "\n" +
                    $"Health {gm.maxHealth}/8" + "\n" + 
                    $"Abilities {abilityCount}/9" + "\n" + 
                    $"Completion {completePercent:0.00}%";
                var player = PlayerScript.instance;
                if (player)
                {
                    stats += "\n" + $"Invuln {player.isInvunerableTimer:0.00}s";
                }

                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.IsValid())
                {
                    stats += "\n" + $"Scene# {activeScene.buildIndex} : {activeScene.name}";
                }
                stats += "\n" + $"Player Position: {PlayerScript.instance.transform.position.x} : {PlayerScript.instance.transform.position.y}";
                ShowStatsText.GetComponent<Text>().text = stats;
            }

            if (Event.current.type.Equals(EventType.Repaint))
            {
                HitboxRendering.Render();
            }
            #endregion

            #region SaveStateUI
            bool displayLoadingSaving = SaveStates.SaveStatesManager.isSaving || SaveStates.SaveStatesManager.isLoading;
            DisplayLoadingSavingGameObject.SetActive(displayLoadingSaving);
            if (SaveStates.SaveStatesManager.isSaving)
            {
                if (SaveStates.SaveStatesManager.saveSlot == -1)
                {
                    DisplayLoadingSavingText.text = "Saved to Quick Slot";
                }
                else
                {
                    DisplayLoadingSavingText.text = $"Saved to Slot {SaveStates.SaveStatesManager.saveSlot}";
                }
            }
            if (SaveStates.SaveStatesManager.isLoading)
            {
                if (SaveStates.SaveStatesManager.saveSlot == -1)
                {
                    DisplayLoadingSavingText.text = "Loaded Quick Slot";
                }
                else
                {
                    DisplayLoadingSavingText.text = $"Loaded Slot {SaveStates.SaveStatesManager.loadSlot}";
                }
            }

            ShowStatesGameObject.SetActive(SaveStates.SaveStatesManager.showFiles);
            if (SaveStates.SaveStatesManager.showFiles)
            {
                string states = "Current Page: " + SaveStates.SaveStatesManager.currentPage.ToString();
                for (int i = 1; i < fileNames.Length; i++) 
                {
                    states += "\n" + $"{i}: " + fileNames[i];
                }
                states += "\n" + $"{0}: " + fileNames[0];
                ShowStatesText.text = states;
            }
            #endregion
        }
    }
}
