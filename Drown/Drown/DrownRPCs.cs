﻿using RainMeadow;

namespace Drown
{
    public static class DrownModeRPCs
    {
        //[RPCMethod]
        //public static void Arena_IncrementPlayerShareScore(RPCEvent rpcEvent)
        //{
        //    if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
        //    {
        //        var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
        //        if (game.manager.upcomingProcess != null)
        //        {
        //            return;
        //        }
        //        foreach (var player in game.GetArenaGameSession.arenaSitting.players)
        //        {
        //            player.score = drown.teamSharedScore;
        //        }

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
                    OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                    if (cs != null)
                    {

                        cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                        if (clientSettings != null)
                        {
                            clientSettings.iOpenedDen = denOpen;
                        }
                    }
                }
            }
        }
    }
}

//    }
//}