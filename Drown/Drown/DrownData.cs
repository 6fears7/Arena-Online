using Drown;
using System;
using System.Linq;

namespace RainMeadow
{
    internal class DrownData : OnlineResource.ResourceData
    {

        public int currentWaveTimer;
        public int currentWave;
        public bool densOpened;
        public override ResourceDataState MakeState(OnlineResource resource)
        {
            return new State(this, resource);
        }

        internal class State : ResourceDataState
        {
            [OnlineField]
            int currentWaveTimer;

            [OnlineField]
            int currentWave;
            [OnlineField]
            bool densOpened;
            public State() { }
            public State(DrownData drownLobbyData, OnlineResource onlineResource)
            {
                
                ArenaOnlineGameMode arena = (onlineResource as Lobby).gameMode as ArenaOnlineGameMode;
                var cachedDrown = DrownMode.isDrownMode(arena, out var drownData);

                if (cachedDrown)
                {
                    currentWaveTimer = (drownData as Drown.DrownMode).currentWaveTimer;
                    currentWave = (drownData as Drown.DrownMode).currentWave;
                    densOpened = (drownData as Drown.DrownMode).openedDen;

                }

            }

            public override Type GetDataType() => typeof(DrownData);

            public override void ReadTo(OnlineResource.ResourceData data, OnlineResource resource)
            {
                var lobby = (resource as Lobby);
                var cachedDrown = DrownMode.isDrownMode((lobby.gameMode as ArenaOnlineGameMode), out var drownData);

                if (cachedDrown && drownData != null && (drownData as DrownMode != null))
                {
                    (drownData as Drown.DrownMode).currentWaveTimer = currentWaveTimer;
                    (drownData as Drown.DrownMode).currentWave = currentWave;
                    (drownData as Drown.DrownMode).openedDen = densOpened;

                }
            }
        }
    }
}