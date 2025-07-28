using RainMeadow;

namespace Drown
{
    public static class DrownModeRPCs
    {
        //[RPCMethod]
        //public static void Arena_IncrementPlayerScore(RPCEvent rpcEvent, int score, ushort userWhoScored)
        //{
        //    if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
        //    {
        //        var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
        //        if (game.manager.upcomingProcess != null)
        //        {
        //            return;
        //        }
        //        if (!game.GetArenaGameSession.GameTypeSetup.spearsHitPlayers) // team work makes the dream work
        //        {
        //            drown.currentPoints++;
        //        }
        //        var oe = ArenaHelpers.FindOnlinePlayerByLobbyId(userWhoScored);
        //        var playerWhoScored = ArenaHelpers.FindOnlinePlayerNumber(arena, oe);
        //        game.GetArenaGameSession.arenaSitting.players[playerWhoScored].score = score;
        //    }
        //}

        [RPCMethod]
        public static void Arena_OpenDen(bool denOpen)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
            {
                drown.openedDen = denOpen;
                var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
                if (game.manager.upcomingProcess != null)
                {
                    return;
                }

                if (!game.GetArenaGameSession.GameTypeSetup.spearsHitPlayers)
                {
                    game.cameras[0].hud.PlaySound(SoundID.UI_Multiplayer_Player_Revive);
                }
            }
        }
    }
}

//    }
//}