using HUD;
using UnityEngine;
namespace Drown
{
    public class StoreHUD : HudPart
    {
        private RoomCamera camera;
        private RainWorldGame game;
        private DrownMode drown;
        private StoreOverlay? storeOverlay;

        public StoreHUD(HUD.HUD hud, RoomCamera camera, DrownMode drown) : base(hud)
        {
            this.camera = camera;
            this.game = camera.game;
            this.drown = drown;
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            if (RainMeadow.RainMeadow.isArenaMode(out var arena))
            {
                if (Input.GetKeyDown(RainMeadow.RainMeadow.rainMeadowOptions.SpectatorKey.Value))
                {
                    if (storeOverlay == null)
                    {
                        RainMeadow.RainMeadow.Debug("Creating storeOverlay overlay");
                        storeOverlay = new StoreOverlay(game.manager, game, drown, arena);
                        this.drown.isInStore = true;


                    }
                    else
                    {
                        RainMeadow.RainMeadow.Debug("storeOverlay destroy!");
                        this.drown.isInStore = false;
                        storeOverlay.ShutDownProcess();
                        storeOverlay = null;
                    }
                }
            }

            if (storeOverlay != null)
            {
                storeOverlay.GrafUpdate(timeStacker);
            }
        }

        public override void Update()
        {
            base.Update();
            if (storeOverlay != null)
            {
                if (RainMeadow.RainMeadow.isArenaMode(out var _))
                {

                    if (game.arenaOverlay != null || game.pauseMenu != null || game.manager.upcomingProcess != null)
                    {
                        RainMeadow.RainMeadow.Debug("Shutting down storeOverlay overlay due to another process request");
                        this.drown.isInStore = false;
                        storeOverlay.ShutDownProcess();
                        storeOverlay = null;
                        return;
                    }
                }

                storeOverlay.Update();
            }
        }
    }
}