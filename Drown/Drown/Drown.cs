using Menu;
using Menu.Remix;
using RainMeadow;
using RainMeadow.UI.Components;
using System.Linq;
using UnityEngine;
using Menu.Remix.MixedUI;
using System.Collections.Generic;
namespace Drown
{
    public class DrownMode : ExternalArenaGameMode
    {

        public static ArenaSetup.GameTypeID Drown = new ArenaSetup.GameTypeID("Drown", register: true);
        public static bool isDrownMode(ArenaOnlineGameMode arena, out DrownMode mode)
        {
            mode = null;
            if (arena.currentGameMode == Drown.value)
            {
                mode = (arena.registeredGameModes.FirstOrDefault(x => x.Key == Drown.value).Value as DrownMode);
                return true;
            }
            return false;
        }

        public bool isInStore = false;

        public static int currentPoints;
        public static bool iOpenedDen = false;

        public int spearCost;
        public int spearExplCost;
        public int bombCost;
        public int respCost;
        public int denCost;
        public int maxCreatures;
        public int creatureCleanupWaves;

        private int _timerDuration;
        public bool openedDen = false;
        public int waveStart = 20;
        public int currentWaveTimer = 20;
        public int currentWave = 0;
        public int lastCleanupWave = 0;
        public bool waveNeedsUpdate = true;

        public override bool IsExitsOpen(ArenaOnlineGameMode arena, On.ArenaBehaviors.ExitManager.orig_ExitsOpen orig, ArenaBehaviors.ExitManager self)
        {
            return openedDen;

        }


        public override bool SpawnBatflies(FliesWorldAI self, int spawnRoom)
        {
            return false;
        }

        public override void ArenaSessionCtor(ArenaOnlineGameMode arena, On.ArenaGameSession.orig_ctor orig, ArenaGameSession self, RainWorldGame game)
        {
            openedDen = false;
            DrownMode.iOpenedDen = false;
            currentWave = 1;
            currentPoints = 5;
            lastCleanupWave = 0;

            if (OnlineManager.lobby.isOwner)
            {
                spearCost = DrownMod.drownOptions.PointsForSpear.Value;
                spearExplCost = DrownMod.drownOptions.PointsForExplSpear.Value;
                bombCost = DrownMod.drownOptions.PointsForBomb.Value;
                respCost = DrownMod.drownOptions.PointsForRespawn.Value;
                denCost = DrownMod.drownOptions.PointsForDenOpen.Value;
                maxCreatures = DrownMod.drownOptions.MaxCreatureCount.Value;
                creatureCleanupWaves = DrownMod.drownOptions.CreatureCleanup.Value;
            }

            foreach (var player in self.arenaSitting.players)
            {
                player.score = currentPoints;
            }

        }

        public override void InitAsCustomGameType(ArenaSetup.GameTypeSetup self)
        {

            self.foodScore = 1;
            self.survivalScore = 0;
            self.spearHitScore = 1;
            self.repeatSingleLevelForever = false;
            self.denEntryRule = ArenaSetup.GameTypeSetup.DenEntryRule.Standard;
            self.rainWhenOnePlayerLeft = false;
            self.levelItems = true;
            self.fliesSpawn = true;
            self.saveCreatures = false;

        }

        public override string TimerText()
        {
            var waveTimer = ArenaPrepTimer.FormatTime(currentWaveTimer);
            return $": Current points: {currentPoints}. Current Wave: {currentWave}. Next wave: {waveTimer}";
        }

        public override int SetTimer(ArenaOnlineGameMode arena)
        {
            return arena.setupTime = 1;
        }

        public override void ResetGameTimer()
        {
            _timerDuration = 1;

        }

        public override int TimerDuration
        {
            get { return _timerDuration; }
            set { _timerDuration = value; }
        }

        public override int TimerDirection(ArenaOnlineGameMode arena, int timer)
        {
            if (!openedDen)
            {

                currentWaveTimer--;
                if (currentWaveTimer == 0)
                {
                    currentWaveTimer = waveStart;
                    waveNeedsUpdate = true;
                }

                return ++arena.setupTime;
            }
            else
            {
                return arena.setupTime;
            }
        }

        public override void LandSpear(ArenaOnlineGameMode arena, ArenaGameSession self, Player player, Creature target, ArenaSitting.ArenaPlayer aPlayer)
        {

        }

        public override void HUD_InitMultiplayerHud(ArenaOnlineGameMode arena, HUD.HUD self, ArenaGameSession session)
        {
            base.HUD_InitMultiplayerHud(arena, self, session);
            self.AddPart(new StoreHUD(self, session.game.cameras[0], this));
        }

        public override bool HoldFireWhileTimerIsActive(ArenaOnlineGameMode arena)
        {
            return arena.countdownInitiatedHoldFire = false;
        }
        public override string AddCustomIcon(ArenaOnlineGameMode arena, PlayerSpecificOnlineHud hud)
        {
            if (isInStore && hud.clientSettings.owner == OnlineManager.mePlayer)
            {
                return "spearSymbol";

            }
            else
            {
                return base.AddCustomIcon(arena, hud);
            }
        }

        public override DialogNotify AddGameModeInfo(Menu.Menu menu)
        {
            return new DialogNotify(menu.LongTranslate("Killing enemies grants points. Points buy weapons and ultimately...your escape"), new UnityEngine.Vector2(500f, 400f), menu.manager, () => { menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed); });
        }

        public override string AddGameSettingsTab()
        {
            return "Drown";
        }
        public OpTextBox? maxCreaturesTextBox;
        public OpTextBox? maxCTextBox;
        public OpTextBox? pointsForSpearTextBox;
        public OpTextBox? pointsForExplSpearTextBox;
        public OpTextBox? pointsForBombTextBox;
        public OpTextBox? pointsForRespawnTextBox;
        public OpTextBox? pointsForDenOpenTextBox;
        public OpTextBox? creatureCleanupsTextBox;

        public override void ArenaExternalGameModeSettingsInterface_ctor(ArenaOnlineGameMode arena, OnlineArenaExternalGameModeSettingsInterface extComp, Menu.Menu menu, MenuObject owner, MenuTabWrapper tabWrapper, Vector2 pos, float settingsWidth = 300)
        {
            if (isDrownMode(arena, out var drown))
            {


                var maxCLLabel = new ProperlyAlignedMenuLabel(menu, extComp, "Max creatures in level", new Vector2(10f, 400), new Vector2(0, 20), false);
                maxCTextBox = new(new Configurable<int>(DrownMod.drownOptions.MaxCreatureCount.Value), new Vector2(10, maxCLLabel.pos.y - 25), 160f)
                {
                    accept = OpTextBox.Accept.Int
                };
                UIelementWrapper maxCTextBoxWrapper = new UIelementWrapper(tabWrapper, maxCTextBox);

                var pointsForSpearLabel = new ProperlyAlignedMenuLabel(menu, extComp, "Points required to buy a spear", new Vector2(10f, maxCTextBox.pos.y - 15), new Vector2(0, 20), false);
                pointsForSpearTextBox = new(new Configurable<int>(DrownMod.drownOptions.PointsForSpear.Value), new Vector2(10, pointsForSpearLabel.pos.y - 25), 160f)
                {
                    accept = OpTextBox.Accept.Int
                };
                UIelementWrapper pointsForSpearTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForSpearTextBox);

                var pointsForExplSpearLabel = new ProperlyAlignedMenuLabel(menu, extComp, "Points required to buy an explosive spear", new Vector2(10f, pointsForSpearTextBox.pos.y - 15), new Vector2(0, 20), false);
                pointsForExplSpearTextBox = new(new Configurable<int>(DrownMod.drownOptions.PointsForExplSpear.Value), new Vector2(10, pointsForExplSpearLabel.pos.y - 25), 160f)
                {
                    accept = OpTextBox.Accept.Int
                };
                UIelementWrapper pointsForExplSpearTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForExplSpearTextBox);

                var pointsForBombLabel = new ProperlyAlignedMenuLabel(menu, extComp, "Points required to buy a scav bomb", new Vector2(10f, pointsForExplSpearTextBox.pos.y - 15), new Vector2(0, 20), false);
                pointsForBombTextBox = new(new Configurable<int>(DrownMod.drownOptions.PointsForBomb.Value), new Vector2(10, pointsForBombLabel.pos.y - 25), 160f)
                {
                    accept = OpTextBox.Accept.Int
                };
                UIelementWrapper pointsForBombTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForBombTextBox);

                var pointsForRespawnLabel = new ProperlyAlignedMenuLabel(menu, extComp, "Points required to buy a respawn", new Vector2(10f, pointsForBombTextBox.pos.y - 15), new Vector2(0, 20), false);
                pointsForRespawnTextBox = new(new Configurable<int>(DrownMod.drownOptions.PointsForRespawn.Value), new Vector2(10, pointsForRespawnLabel.pos.y - 25), 160f)
                {
                    accept = OpTextBox.Accept.Int
                };
                UIelementWrapper pointsForRespawnTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForRespawnTextBox);

                var pointsForDenOpenLabel = new ProperlyAlignedMenuLabel(menu, extComp, "Points required to open dens", new Vector2(10f, pointsForRespawnTextBox.pos.y - 15), new Vector2(0, 20), false);
                pointsForDenOpenTextBox = new(new Configurable<int>(DrownMod.drownOptions.PointsForDenOpen.Value), new Vector2(10, pointsForDenOpenLabel.pos.y - 25), 160f)
                {
                    accept = OpTextBox.Accept.Int
                };
                UIelementWrapper pointsForDenOpenTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForDenOpenTextBox);

                var creatureCleanupsLabel = new ProperlyAlignedMenuLabel(menu, extComp, "How many waves before creature cleanup", new Vector2(10f, pointsForDenOpenTextBox.pos.y - 15), new Vector2(0, 20), false);
                creatureCleanupsTextBox = new(new Configurable<int>(DrownMod.drownOptions.CreatureCleanup.Value), new Vector2(10, creatureCleanupsLabel.pos.y - 25), 160f)
                {
                    accept = OpTextBox.Accept.Int
                };
                UIelementWrapper creatureCleanupsTextBoxWrapper = new UIelementWrapper(tabWrapper, creatureCleanupsTextBox);

                var storeItem1Label = new ProperlyAlignedMenuLabel(menu, extComp, "Hot key used to buy spear (store needs to be open)", new Vector2(260, maxCLLabel.pos.y), new Vector2(0, 20), false);
                var storeItem1KeyBinder = new OpKeyBinder(DrownMod.drownOptions.StoreItem1, new Vector2(260, storeItem1Label.pos.y - 30), new Vector2(150f, 30f));
                UIelementWrapper storeItem1KeyBinderWrapper = new UIelementWrapper(tabWrapper, storeItem1KeyBinder);

                var storeItem2Label = new ProperlyAlignedMenuLabel(menu, extComp, "Hot key used to buy explosive spear", new Vector2(260, storeItem1KeyBinder.pos.y - 15), new Vector2(0, 20), false);
                var storeItem2KeyBinder = new OpKeyBinder(DrownMod.drownOptions.StoreItem2, new Vector2(260, storeItem2Label.pos.y - 30), new Vector2(150f, 30f));
                UIelementWrapper storeItem2KeyBinderWrapper = new UIelementWrapper(tabWrapper, storeItem2KeyBinder);

                var storeItem3Label = new ProperlyAlignedMenuLabel(menu, extComp, "Hot key used to buy scav bomb", new Vector2(260, storeItem2KeyBinder.pos.y - 15), new Vector2(0, 20), false);
                var storeItem3KeyBinder = new OpKeyBinder(DrownMod.drownOptions.StoreItem3, new Vector2(260, storeItem3Label.pos.y - 30), new Vector2(150f, 30f));
                UIelementWrapper storeItem3KeyBinderWrapper = new UIelementWrapper(tabWrapper, storeItem3KeyBinder);

                var storeItem4Label = new ProperlyAlignedMenuLabel(menu, extComp, "Hot key used to buy respawn", new Vector2(260, storeItem3KeyBinder.pos.y - 15), new Vector2(0, 20), false);
                var storeItem4KeyBinder = new OpKeyBinder(DrownMod.drownOptions.StoreItem4, new Vector2(260, storeItem4Label.pos.y - 30), new Vector2(150f, 30f));
                UIelementWrapper storeItem4KeyBinderWrapper = new UIelementWrapper(tabWrapper, storeItem4KeyBinder);

                var storeItem5Label = new ProperlyAlignedMenuLabel(menu, extComp, "Hot key used to open den", new Vector2(260, storeItem4KeyBinder.pos.y - 15), new Vector2(0, 20), false);
                var storeItem5KeyBinder = new OpKeyBinder(DrownMod.drownOptions.StoreItem5, new Vector2(260, storeItem5Label.pos.y - 30), new Vector2(150f, 30f));
                UIelementWrapper storeItem5KeyBinderWrapper = new UIelementWrapper(tabWrapper, storeItem5KeyBinder);

                var openStoreLabel = new ProperlyAlignedMenuLabel(menu, extComp, "Key used to access store", new Vector2(260, storeItem5KeyBinder.pos.y - 15), new Vector2(0, 20), false);
                var openStoreKeyBinder = new OpKeyBinder(DrownMod.drownOptions.OpenStore, new Vector2(260, openStoreLabel.pos.y - 30), new Vector2(150f, 30f));
                extComp.SafeAddSubobjects(tabWrapper,  maxCLLabel, maxCTextBoxWrapper,
                    pointsForSpearLabel, pointsForSpearTextBoxWrapper, pointsForExplSpearLabel, pointsForExplSpearTextBoxWrapper,
                    pointsForBombLabel, pointsForBombTextBoxWrapper, pointsForRespawnLabel, pointsForRespawnTextBoxWrapper,
                    pointsForDenOpenLabel, pointsForDenOpenTextBoxWrapper, creatureCleanupsLabel, creatureCleanupsTextBoxWrapper,
                    storeItem1Label, storeItem1KeyBinderWrapper, storeItem2Label, storeItem2KeyBinderWrapper,
                    storeItem3Label, storeItem3KeyBinderWrapper, storeItem4Label, storeItem4KeyBinderWrapper,
                    storeItem5Label, storeItem5KeyBinderWrapper, openStoreLabel);
            }
        }


        public override void ArenaSessionUpdate(ArenaOnlineGameMode arena, ArenaGameSession session)
        {

            if (!session.sessionEnded)
            {
                for (int i = 0; i < session.Players.Count; i++)
                {
                    if (session.GameTypeSetup.spearsHitPlayers)
                    {
                        if (session.exitManager.IsPlayerInDen(session.Players[i]))
                        {
                            session.EndSession();
                        }
                    }

                    if (!OnlinePhysicalObject.map.TryGetValue(session.Players[i], out var onlineP) || session.Players[i].state.dead)
                    {
                        session.Players.Remove(session.Players[i]);
                    }

                }

            }

            if (!openedDen)
            {
                if (currentWaveTimer % waveStart == 0 && session.playersSpawned && waveNeedsUpdate)
                {
                    var notSlugcatCount = 0;
                    for (int i = 0; i < session.room.abstractRoom.creatures.Count; i++)
                    {
                        if (session.room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Slugcat && session.room.abstractRoom.creatures[i].state.alive)
                        {
                            notSlugcatCount++;
                        }

                    }
                    if (notSlugcatCount < maxCreatures)
                    {
                        session.SpawnCreatures();
                    }
                    currentWave++;
                }
                if (currentWave % creatureCleanupWaves == 0 && currentWave > lastCleanupWave)
                {
                    lastCleanupWave = currentWave;

                    CreatureCleanup(arena, session);
                }
                waveNeedsUpdate = false;
            }

        }

        private void CreatureCleanup(ArenaOnlineGameMode arena, ArenaGameSession session)
        {
            if (RoomSession.map.TryGetValue(session.room.abstractRoom, out var roomSession))
            {
                var entities = session.room.abstractRoom.entities;
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    if (entities[i] is AbstractPhysicalObject apo && apo is AbstractCreature ac && ac.state.dead && ac.realizedCreature.grabbedBy.Count <= 0 && OnlinePhysicalObject.map.TryGetValue(apo, out var oe))
                    {
                        for (int num = ac.stuckObjects.Count - 1; num >= 0; num--)
                        {
                            if (ac.stuckObjects[num] is AbstractPhysicalObject.AbstractSpearStick && ac.stuckObjects[num].A.type == AbstractPhysicalObject.AbstractObjectType.Spear && ac.stuckObjects[num].A.realizedObject != null)
                            {
                                (ac.stuckObjects[num].A.realizedObject as Spear).ChangeMode(Weapon.Mode.Free);
                            }
                        }
                        ac.LoseAllStuckObjects();
                        if (!oe.isMine)
                        {
                            oe.beingMoved = true;

                            if (oe.apo.realizedObject is Creature c && c.inShortcut)
                            {
                                if (c.RemoveFromShortcuts()) c.inShortcut = false;
                            }

                            entities.Remove(oe.apo);

                            session.room.abstractRoom.creatures.Remove(oe.apo as AbstractCreature);

                            session.room.RemoveObject(oe.apo.realizedObject);
                            session.room.CleanOutObjectNotInThisRoom(oe.apo.realizedObject);
                            oe.beingMoved = false;
                        }
                        else
                        {
                            oe.ExitResource(roomSession);
                            oe.ExitResource(roomSession.worldSession);
                            oe.apo.realizedObject.RemoveFromRoom();

                        }


                    }
                }
            }
        }

    }
}