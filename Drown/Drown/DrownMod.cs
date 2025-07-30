using BepInEx;
using MonoMod.Cil;
using RainMeadow;
using System;
using System.Linq;
using System.Security.Permissions;
using Mono.Cecil.Cil;
using Unity.IO.LowLevel.Unsafe;
using RainMeadow.UI;
using IL.Watcher;
using MonoMod.RuntimeDetour;
using System.Reflection;

//#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace Drown
{
    [BepInPlugin("uo.drown", "Drown", "0.4.0")]
    public partial class DrownMod : BaseUnityPlugin
    {
        public static DrownOptions drownOptions;
        public static DrownMod instance;
        private bool init;
        private bool fullyInit;
        private bool addedMod = false;
        private bool showedUserStoreMessage = false;
        public void OnEnable()
        {
            instance = this;
            drownOptions = new DrownOptions(this);


            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        }


        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (init) return;
            init = true;

            try
            {
                MachineConnector.SetRegisteredOI("uo_drown", drownOptions);

                On.Menu.Menu.ctor += Menu_ctor;
                On.HUD.TextPrompt.AddMessage_string_int_int_bool_bool += TextPrompt_AddMessage_string_int_int_bool_bool;
                On.Creature.Violence += Creature_Violence;
                On.Lizard.Violence += Lizard_Violence;
                On.ArenaGameSession.ShouldSessionEnd += ArenaGameSession_ShouldSessionEnd;
                On.Spear.Spear_makeNeedle += Spear_Spear_makeNeedle;
                IL.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
                new Hook(typeof(Lobby).GetMethod("ActivateImpl", BindingFlags.NonPublic | BindingFlags.Instance), (Action<Lobby> orig, Lobby self) =>
                {
                    orig(self);
                    OnlineManager.lobby.AddData(new DrownData());
                });
                new Hook(typeof(ArenaOnlineGameMode).GetMethod("AddClientData", BindingFlags.Public | BindingFlags.Instance), (Action<ArenaOnlineGameMode> orig, ArenaOnlineGameMode self) =>
                {
                    orig(self);
                    self.clientSettings.AddData(new ArenaDrownClientSettings());
                });

                fullyInit = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                fullyInit = false;
            }
        }

        private void Menu_ctor(On.Menu.Menu.orig_ctor orig, Menu.Menu self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && arena != null && self is ArenaOnlineLobbyMenu)
            {
                if (!arena.registeredGameModes.ContainsKey(DrownMode.Drown.value))
                {
                    arena.registeredGameModes.Add(DrownMode.Drown.value, new DrownMode());
                }
            }
            orig(self, manager, ID);

        }


        private void Player_ClassMechanicsSaint(ILContext il)
        {

            try
            {
                var c = new ILCursor(il);
                ILLabel skip = il.DefineLabel();
                c.GotoNext(
                     i => i.MatchLdloc(18),
                     i => i.MatchIsinst<Creature>(),
                     i => i.MatchCallvirt<Creature>("Die")
                     );
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 18);
                c.EmitDelegate((Player self, PhysicalObject po) =>
                {
                    if (self.IsLocal() && RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
                    {

                        OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                        if (cs != null)
                        {

                            cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                            if (clientSettings != null)
                            {
                                clientSettings.score++;
                            }
                        }
                    }
                });

            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

        }

        private void Spear_Spear_makeNeedle(On.Spear.orig_Spear_makeNeedle orig, Spear self, int type, bool active)
        {
            orig(self, type, active);
            if (!self.IsLocal())
            {
                return;
            }
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
            {
                OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                if (cs != null)
                {

                    cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                    if (clientSettings != null)
                    {
                        clientSettings.score = clientSettings.score - 1;
                    }
                }
            }

        }

        private bool ArenaGameSession_ShouldSessionEnd(On.ArenaGameSession.orig_ShouldSessionEnd orig, ArenaGameSession self)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
            {
                OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                if (cs != null)
                {

                    cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                    if (clientSettings != null)
                    {

                        //if (self.GameTypeSetup.spearsHitPlayers) // Competitive
                        //{
                        if (clientSettings.score >= drown.respCost && !drown.openedDen) // We can still respawn
                        {
                            return false;
                        }
                    }
                }

                //var alivePlayers = self.Players.Where(player => player.state.alive);

            }
            //    return alivePlayers.Any(player => self.exitManager.playersInDens.Any(denPlayer => denPlayer.creature.abstractCreature == player)) || !self.Players.Any(player => player.state.alive); // Dens are opened and we have no money. Did anyone beat us there or is everyone dead?

            //}

            //if (!self.GameTypeSetup.spearsHitPlayers) // Cooperative
            //{

            //    if (drown.currentPoints >= drown.respCost && !drown.openedDen) // We can still respawn
            //    {
            //        return false;
            //    }
            //    //    var alivePlayers = self.Players.Where(player => player.state.alive);
            //    //    return alivePlayers.All(player => self.exitManager.playersInDens.Any(denPlayer => denPlayer.creature.abstractCreature == player)) || !self.Players.Any(player => player.state.alive);
            //    //}
            //}
            return orig(self);
        }

        private void Lizard_Violence(On.Lizard.orig_Violence orig, Lizard self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
        {
            orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
            if (self.State.dead)
            {
                return;
            }
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
            {

                var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
                if (game.manager.upcomingProcess != null)
                {
                    return;
                }
                foreach (var abs in game.GetArenaGameSession.Players)
                {
                    if (abs == self.killTag && OnlinePhysicalObject.map.TryGetValue(abs, out var onlinePlayer) && onlinePlayer.owner == OnlineManager.mePlayer) //  Me. I killed them.
                    {
                        OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                        if (cs != null)
                        {

                            cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                            if (clientSettings != null)
                            {
                                clientSettings.score++;
                            }
                        }
                    }
                }


            }
        }

        private void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

            if (self.State.dead)
            {
                return;
            }
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
            {
                var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
                if (game.manager.upcomingProcess != null)
                {
                    return;
                }
                foreach (var abs in game.GetArenaGameSession.Players)
                {
                    if (abs == self.killTag && OnlinePhysicalObject.map.TryGetValue(abs, out var onlinePlayer) && onlinePlayer.owner == OnlineManager.mePlayer) //  Me. I killed them.
                    {
                        drown.currentPoints++;
                        OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                        if (cs != null)
                        {

                            cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                            if (clientSettings != null)
                            {
                                clientSettings.score = drown.currentPoints;
                            }
                        }
                    }
                }


            }


        }

        private void TextPrompt_AddMessage_string_int_int_bool_bool(On.HUD.TextPrompt.orig_AddMessage_string_int_int_bool_bool orig, HUD.TextPrompt self, string text, int wait, int time, bool darken, bool hideHud)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && arena.externalArenaGameMode == arena.registeredGameModes.FirstOrDefault(kvp => kvp.Key == DrownMode.Drown.value).Value)
            {
                text = text + $" - Press {drownOptions.OpenStore.Value} to access the store";
                orig(self, text, wait, time, darken, hideHud);
            }
            else
            {
                orig(self, text, wait, time, darken, hideHud);
            }

        }



    }
}