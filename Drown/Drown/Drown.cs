using RainMeadow;
using System.Text.RegularExpressions;
using Menu;
using System.Linq;

namespace Drown
{
    public class DrownMode : ExternalArenaGameMode
    {

        public static ArenaSetup.GameTypeID Drown = new ArenaSetup.GameTypeID("Drown", register: true);


        public bool isInStore = false;

        public static int currentPoints;
        public static bool openedDen = false;

        private int _timerDuration;
        private int waveStart = 1200;
        private int currentWaveTimer = 1200;


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
            currentPoints = 5;
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
            return $": Current points: {currentPoints}. Next wave: {waveTimer}";
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
                return ++timer;
            }
            else
            {
                return timer;
            }
        }

        public override void LandSpear(ArenaOnlineGameMode arena, ArenaGameSession self, Player player, Creature target, ArenaSitting.ArenaPlayer aPlayer)
        {
            currentPoints++;
            aPlayer.score = currentPoints;

            for (int i = 0; i < arena.arenaSittingOnlineOrder.Count; i++)
            {
                var currentPlayer = ArenaHelpers.FindOnlinePlayerByFakePlayerNumber(arena, i);
                if (!currentPlayer.isMe)
                {
                    currentPlayer.InvokeOnceRPC(DrownModeRPCs.Arena_IncrementPlayerScore, currentPoints, OnlineManager.mePlayer.inLobbyId);

                }
            }

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
            if (isInStore)
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
            if (session.GameTypeSetup.spearsHitPlayers)
            {
                for (int i = 0; i < session.Players.Count; i++)
                {
                    if (!session.sessionEnded)
                    {
                        if (session.exitManager.IsPlayerInDen(session.Players[i]))
                        {
                            session.EndSession();
                        }
                    }
                }
            }

            currentWaveTimer--;
            if (currentWaveTimer == 0)
            {
                currentWaveTimer = waveStart;
            }
            if (currentWaveTimer % waveStart == 0)
            {
                session.SpawnCreatures();
            }

        }


    }
}
