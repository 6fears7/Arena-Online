using RainMeadow;
using System.Linq;

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
                mode = (arena.registeredGameModes.FirstOrDefault(x => x.Value == Drown.value).Key as DrownMode);
                return true;
            }
            return false;
        }

        public bool isInStore = false;

        public static int currentPoints;
        public static bool iOpenedDen = false;

        public static int spearCost;
        public static int spearExplCost;
        public static int bombCost;
        public static int respCost;
        public static int denCost;
        public static int maxCreatures;
        public static int creatureCleanupWaves;

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
            return true;
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

            if (OnlineManager.lobby.isOwner)
            {
                foreach (var player in OnlineManager.players)
                {
                    if (!player.isMe) player.InvokeOnceRPC(DrownModeRPCs.SyncRemix, spearCost, spearExplCost, bombCost, respCost, denCost, maxCreatures, creatureCleanupWaves);

                }
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