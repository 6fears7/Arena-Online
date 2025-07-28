using System;
using UnityEngine;
using RainMeadow;
namespace Drown
{
    public class ArenaDrownClientSettings : OnlineEntity.EntityData
    {
        public int score;
        public bool isInStore;

        public ArenaDrownClientSettings() { }

        public override EntityDataState MakeState(OnlineEntity entity, OnlineResource inResource)
        {
            return new State(this);
        }

        public class State : EntityDataState
        {
            [OnlineField]
            public int score;
            [OnlineField]
            public bool isInStore;
            public State() { }

            public State(ArenaDrownClientSettings onlineEntity) : base()
            {
                if (RainMeadow.RainMeadow.isArenaMode(out var arena) && arena != null &&  DrownMode.isDrownMode(arena, out var drown) && drown != null)
                {
                    score = drown.currentPoints;
                    isInStore = onlineEntity.isInStore;
                }
            }

            public override void ReadTo(OnlineEntity.EntityData entityData, OnlineEntity onlineEntity)
            {
                var avatarSettings = (ArenaDrownClientSettings)entityData;
                avatarSettings.score = score;
                avatarSettings.isInStore = isInStore;

            }

            public override Type GetDataType() => typeof(ArenaDrownClientSettings);
        }
    }
}