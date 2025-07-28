using Menu;
using Menu.Remix;
using RainMeadow;
using RainMeadow.UI.Components;
using System.Linq;
using UnityEngine;
using Menu.Remix.MixedUI;
using System.Collections.Generic;
using RainMeadow.Arena.ArenaOnlineGameModes.TeamBattle;
using ArenaMode = RainMeadow.ArenaOnlineGameMode;
using RainMeadow.UI;

namespace Drown
{
    public partial class DrownMode : ExternalArenaGameMode
    {

        public static ArenaSetup.GameTypeID Drown = new ArenaSetup.GameTypeID("Drown", register: true);
        public override ArenaSetup.GameTypeID GetGameModeId
        {
            get
            {
                return Drown;
            }
            set { GetGameModeId = value; }

        }

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
        public int currentPoints;

        public static bool iOpenedDen = false;

        public int spearCost;
        public int spearExplCost;
        public int bombCost;
        public int respCost;
        public int denCost;
        public int maxCreatures;
        public int creatureCleanupWaves;
        public int teamSharedScore;

        private int _timerDuration;
        public bool openedDen = false;
        public int waveStart = 20;
        public int currentWaveTimer = 20;
        public int currentWave = 0;
        public int lastCleanupWave = 0;
        public bool waveNeedsUpdate = true;
        public DrownInterface? drownInterface;
        public TabContainer.Tab? myTab;

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

        public override string AddIcon(ArenaOnlineGameMode arena, PlayerSpecificOnlineHud owner, SlugcatCustomization customization, OnlinePlayer player)
        {
            if (player != null)
            {
                OnlineManager.lobby.clientSettings.TryGetValue(player, out var cs);
                if (cs != null)
                {

                    cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                    if (clientSettings != null && clientSettings.isInStore)
                    {
                        return "spearSymbol";
                    }
                }
            }

            return base.AddIcon(arena, owner, customization, player);
        }


        public override Dialog AddGameModeInfo(ArenaOnlineGameMode arena, Menu.Menu menu)
        {
            return base.AddGameModeInfo(arena, menu);
        }

        public override void OnUIEnabled(ArenaOnlineLobbyMenu menu)
        {
            base.OnUIEnabled(menu);
            myTab = menu.arenaMainLobbyPage.tabContainer.AddTab(menu.Translate("Drown Settings"));
            myTab.AddObjects(drownInterface = new DrownInterface((ArenaMode)OnlineManager.lobby.gameMode, this, myTab.menu, myTab, new(0, 0), menu.arenaMainLobbyPage.tabContainer.size));
        }
        public override void OnUIDisabled(ArenaOnlineLobbyMenu menu)
        {
            base.OnUIDisabled(menu);
            drownInterface?.OnShutdown();
            if (myTab != null) menu.arenaMainLobbyPage.tabContainer.RemoveTab(myTab);
            myTab = null;
        }

        public override void ArenaSessionEnded(ArenaMode arena, On.ArenaSitting.orig_SessionEnded orig, ArenaSitting self, ArenaGameSession session, List<ArenaSitting.ArenaPlayer> list)
        {
            foreach (var player in self.players)
            {
                var onlinePlayer = ArenaHelpers.FindOnlinePlayerByFakePlayerNumber(arena, player.playerNumber);
                if (onlinePlayer != null)
                {
                    OnlineManager.lobby.clientSettings.TryGetValue(onlinePlayer, out var cs);
                    if (cs != null)
                    {
                        if (cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings))
                        {
                            player.score = clientSettings.score;
                        }
                    }
                }
            }

            base.ArenaSessionEnded(arena, orig, self, session, list);

        }

        public override void ArenaSessionUpdate(ArenaOnlineGameMode arena, ArenaGameSession session)
        {
            if (isDrownMode(arena, out var drown))
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

                    if (!session.GameTypeSetup.spearsHitPlayers) // team work makes the dream work
                    {
                        var greatestPlayer = session.arenaSitting.players.OrderByDescending(p => p.score).FirstOrDefault();
                        drown.teamSharedScore = greatestPlayer.score;
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