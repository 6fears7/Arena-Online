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
    [BepInPlugin("uo.drown", "Drown", "0.4.3")]
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
                On.Spear.Spear_makeNeedle += Spear_Spear_makeNeedle;
                IL.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
                On.ArenaGameSession.PlayersStillActive += ArenaGameSession_PlayersStillActive;
                On.Player.checkInput += Player_checkInput;
                On.ArenaGameSession.Killing += ArenaGameSession_Killing;
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

        private void ArenaGameSession_Killing(On.ArenaGameSession.orig_Killing orig, ArenaGameSession self, Player player, Creature killedCrit)
        {
            orig(self, player, killedCrit);
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out _))
            {

                foreach (var abs in self.Players)
                {
                    if (abs == killedCrit.killTag && OnlinePhysicalObject.map.TryGetValue(abs, out var onlinePlayer) && onlinePlayer.owner == OnlineManager.mePlayer) //  Me. I killed them.
                    {
                        OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                        if (cs != null)
                        {

                            cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                            if (clientSettings != null)
                            {
                                int arenaPlayer = ArenaHelpers.FindOnlinePlayerNumber(arena, OnlineManager.mePlayer);
                                IconSymbol.IconSymbolData iconSymbolData = CreatureSymbol.SymbolDataFromCreature(killedCrit.abstractCreature);
                                int index = MultiplayerUnlocks.SandboxUnlockForSymbolData(iconSymbolData).Index;
                                if (index >= 0)
                                {
                                    self.arenaSitting.players[arenaPlayer].AddSandboxScore(self.arenaSitting.gameTypeSetup.killScores[index]);
                                }
                                else
                                {
                                    self.arenaSitting.players[arenaPlayer].AddSandboxScore(0);
                                }
                                clientSettings.score += self.arenaSitting.gameTypeSetup.killScores[index];
                            }
                        }
                    }

                }
            }
        }

        private void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var _) && self.IsLocal())
            {
                if (self.controller is null && self.room.world.game.cameras[0]?.hud is HUD.HUD hud
                    && (hud.parts.OfType<StoreHUD>().Any(x => x.active == true)))
                {
                    InputOverride.StopPlayerMovement(self);
                }
            }
        }

        private int ArenaGameSession_PlayersStillActive(On.ArenaGameSession.orig_PlayersStillActive orig, ArenaGameSession self, bool addToAliveTime, bool dontCountSandboxLosers)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
            {
                bool teamWork = !self.GameTypeSetup.spearsHitPlayers;

                var count = 0;
                foreach (var p in arena.arenaSittingOnlineOrder)
                {
                    OnlinePlayer? pl = ArenaHelpers.FindOnlinePlayerByLobbyId(p);
                    if (pl != null)
                    {
                        OnlineManager.lobby.clientSettings.TryGetValue(pl, out var cs);
                        if (cs != null)
                        {

                            cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                            if (clientSettings != null)
                            {

                                if ((teamWork ? clientSettings.teamScore : clientSettings.score) >= drown.respCost && !drown.openedDen) // We can still respawn
                                {
                                    count++;
                                }
                            }
                        }
                    }
                }

                if (teamWork && (self.Players.FindAll(x => x.realizedCreature != null && x.realizedCreature.State.alive).Count > 0))
                {
                    return orig(self, addToAliveTime, dontCountSandboxLosers);
                }

                if (self.Players.FindAll(x => x.realizedCreature != null && x.realizedCreature.State.alive).Count == 0)
                {
                    if (count > 0)
                    {
                        return count;
                    }
                }

            }
            return orig(self, addToAliveTime, dontCountSandboxLosers);
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

        private void Lizard_Violence(On.Lizard.orig_Violence orig, Lizard self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
        {
            orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);

            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
            {
                if (self.State.dead)
                {
                    return;
                }
                var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
                if (game.manager.upcomingProcess != null)
                {
                    return;
                }
                //foreach (var abs in game.GetArenaGameSession.Players)
                //{
                //    if (abs == self.killTag && OnlinePhysicalObject.map.TryGetValue(abs, out var onlinePlayer) && onlinePlayer.owner == OnlineManager.mePlayer) //  Me. I killed them.
                //    {
                //        OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                //        if (cs != null)
                //        {

                //            cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                //            if (clientSettings != null)
                //            {
                //                clientSettings.score++;
                //            }
                //        }
                //    }
                //}


            }
        }

        private void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && DrownMode.isDrownMode(arena, out var drown))
            {
                if (self.State.dead)
                {
                    return;
                }
                var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
                if (game.manager.upcomingProcess != null)
                {
                    return;
                }
                //foreach (var abs in game.GetArenaGameSession.Players)
                //{
                //    if (abs == self.killTag && OnlinePhysicalObject.map.TryGetValue(abs, out var onlinePlayer) && onlinePlayer.owner == OnlineManager.mePlayer) //  Me. I killed them.
                //    {
                //        OnlineManager.lobby.clientSettings.TryGetValue(OnlineManager.mePlayer, out var cs);
                //        if (cs != null)
                //        {

                //            cs.TryGetData<ArenaDrownClientSettings>(out var clientSettings);
                //            if (clientSettings != null)
                //            {
                //                clientSettings.score++;
                //            }
                //        }
                //    }
                //}

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