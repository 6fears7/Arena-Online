using ArenaMode = RainMeadow.ArenaOnlineGameMode;
using System.Collections.Generic;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;
using System.Linq;
using RainMeadow.UI.Components.Patched;
using RainMeadow.Arena.ArenaOnlineGameModes.TeamBattle;
using System.Globalization;
using HarmonyLib;
using System;
using Drown;

namespace RainMeadow.UI.Components
{
    public class DrownInterface : RectangularMenuObject
    {
        public FSprite divider;
        public MenuTabWrapper tabWrapper;
        public EventfulScrollButton? prevButton, nextButton;
        private int currentOffset;

        public ArenaMode arenaMode;
        public DrownMode DROWN;
        public bool OwnerSettingsDisabled => !(OnlineManager.lobby?.isOwner == true);


        public OpTextBox? maxCreaturesTextBox;
        public OpTextBox? maxCTextBox;
        public OpTextBox? pointsForSpearTextBox;
        public OpTextBox? pointsForExplSpearTextBox;
        public OpTextBox? pointsForBombTextBox;
        public OpTextBox? pointsForRespawnTextBox;
        public OpTextBox? pointsForDenOpenTextBox;
        public OpTextBox? creatureCleanupsTextBox;




        public DrownInterface(ArenaMode arena, DrownMode drown, Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base(menu, owner, pos, size)
        {
            tabWrapper = new(menu, this);
            DROWN = drown;
            var maxCLLabel = new ProperlyAlignedMenuLabel(menu, owner, "Max creatures in level", new Vector2(10f, 400), new Vector2(0, 20), false);
            maxCTextBox = new(new Configurable<int>(drown.maxCreatures), new Vector2(10, maxCLLabel.pos.y - 25), 160f)
            {
                accept = OpTextBox.Accept.Int,
                greyedOut = OwnerSettingsDisabled

            };
            maxCTextBox.OnValueUpdate += (config, value, oldValue) =>
            {
                DROWN.maxCreatures = maxCTextBox.valueInt;
                DrownMod.drownOptions.MaxCreatureCount.Value = maxCTextBox.valueInt;
            };
            UIelementWrapper maxCTextBoxWrapper = new UIelementWrapper(tabWrapper, maxCTextBox);

            var pointsForSpearLabel = new ProperlyAlignedMenuLabel(menu, owner, "Points required to buy a spear", new Vector2(10f, maxCTextBox.pos.y - 15), new Vector2(0, 20), false);
            pointsForSpearTextBox = new(new Configurable<int>(DrownMod.drownOptions.PointsForSpear.Value), new Vector2(10, pointsForSpearLabel.pos.y - 25), 160f)
            {
                accept = OpTextBox.Accept.Int,
                greyedOut = OwnerSettingsDisabled
            };
            pointsForSpearTextBox.OnValueUpdate += (config, value, oldValue) =>
            {
                DROWN.spearCost = pointsForSpearTextBox.valueInt;
            };
            UIelementWrapper pointsForSpearTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForSpearTextBox);

            var pointsForExplSpearLabel = new ProperlyAlignedMenuLabel(menu, owner, "Points required to buy an explosive spear", new Vector2(10f, pointsForSpearTextBox.pos.y - 15), new Vector2(0, 20), false);
            pointsForExplSpearTextBox = new(new Configurable<int>(drown.spearExplCost), new Vector2(10, pointsForExplSpearLabel.pos.y - 25), 160f)
            {
                accept = OpTextBox.Accept.Int,
                greyedOut = OwnerSettingsDisabled
            };
            pointsForExplSpearTextBox.OnValueUpdate += (config, value, oldValue) =>
            {
                DROWN.spearExplCost = pointsForExplSpearTextBox.valueInt;
                DrownMod.drownOptions.PointsForExplSpear.Value = pointsForExplSpearTextBox.valueInt;


            };
            UIelementWrapper pointsForExplSpearTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForExplSpearTextBox);

            var pointsForBombLabel = new ProperlyAlignedMenuLabel(menu, owner, "Points required to buy a scav bomb", new Vector2(10f, pointsForExplSpearTextBox.pos.y - 15), new Vector2(0, 20), false);
            pointsForBombTextBox = new(new Configurable<int>(drown.bombCost), new Vector2(10, pointsForBombLabel.pos.y - 25), 160f)
            {
                accept = OpTextBox.Accept.Int,
                greyedOut = OwnerSettingsDisabled
            };
            pointsForBombTextBox.OnValueUpdate += (config, value, oldValue) =>
            {
                DROWN.bombCost = pointsForBombTextBox.valueInt;
            };
            UIelementWrapper pointsForBombTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForBombTextBox);

            var pointsForRespawnLabel = new ProperlyAlignedMenuLabel(menu, owner, "Points required to buy a respawn", new Vector2(10f, pointsForBombTextBox.pos.y - 15), new Vector2(0, 20), false);
            pointsForRespawnTextBox = new(new Configurable<int>(drown.respCost), new Vector2(10, pointsForRespawnLabel.pos.y - 25), 160f)
            {
                accept = OpTextBox.Accept.Int,
                greyedOut = OwnerSettingsDisabled
            };
            pointsForRespawnTextBox.OnValueUpdate += (config, value, oldValue) =>
            {
                DROWN.respCost = pointsForRespawnTextBox.valueInt;
            };
            UIelementWrapper pointsForRespawnTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForRespawnTextBox);

            var pointsForDenOpenLabel = new ProperlyAlignedMenuLabel(menu, owner, "Points required to open dens", new Vector2(10f, pointsForRespawnTextBox.pos.y - 15), new Vector2(0, 20), false);
            pointsForDenOpenTextBox = new(new Configurable<int>(drown.denCost), new Vector2(10, pointsForDenOpenLabel.pos.y - 25), 160f)
            {
                accept = OpTextBox.Accept.Int,
                greyedOut = OwnerSettingsDisabled
            };
            pointsForDenOpenTextBox.OnValueUpdate += (config, value, oldValue) =>
            {
                DROWN.denCost = pointsForDenOpenTextBox.valueInt;
            };
            UIelementWrapper pointsForDenOpenTextBoxWrapper = new UIelementWrapper(tabWrapper, pointsForDenOpenTextBox);

            var creatureCleanupsLabel = new ProperlyAlignedMenuLabel(menu, owner, "How many waves before creature cleanup", new Vector2(10f, pointsForDenOpenTextBox.pos.y - 15), new Vector2(0, 20), false);
            creatureCleanupsTextBox = new(new Configurable<int>(drown.creatureCleanupWaves), new Vector2(10, creatureCleanupsLabel.pos.y - 25), 160f)
            {
                accept = OpTextBox.Accept.Int,
                greyedOut = OwnerSettingsDisabled
            };
            creatureCleanupsTextBox.OnValueUpdate += (config, value, oldValue) =>
            {
                DROWN.creatureCleanupWaves = creatureCleanupsTextBox.valueInt;
            };
            UIelementWrapper creatureCleanupsTextBoxWrapper = new UIelementWrapper(tabWrapper, creatureCleanupsTextBox);


            this.SafeAddSubobjects(tabWrapper, maxCLLabel, maxCTextBoxWrapper,
                pointsForSpearLabel, pointsForSpearTextBoxWrapper, pointsForExplSpearLabel, pointsForExplSpearTextBoxWrapper,
                pointsForBombLabel, pointsForBombTextBoxWrapper, pointsForRespawnLabel, pointsForRespawnTextBoxWrapper,
                pointsForDenOpenLabel, pointsForDenOpenTextBoxWrapper, creatureCleanupsLabel, creatureCleanupsTextBoxWrapper);
        }
        public void PopulatePage(int offset)
        {
            ClearInterface();
            tabWrapper._tab.myContainer.MoveToFront();
        }
        public void ClearInterface()
        {
            //UnloadAnyConfig(teamColorPickers)

        }
        public void UnloadAnyConfig(params UIelement[]? elements)
        {
            if (elements == null) return;
            foreach (UIelement element in elements)
            {
                if (tabWrapper.wrappers.ContainsKey(element))
                {
                    tabWrapper.ClearMenuObject(tabWrapper.wrappers[element]);
                    tabWrapper.wrappers.Remove(element);
                }
                element.Unload();
            }
        }


        public void OnShutdown()
        {
            if (!(OnlineManager.lobby?.isOwner == true)) return;

            DrownMod.drownOptions.MaxCreatureCount.Value = DROWN.maxCreatures;
            DrownMod.drownOptions.PointsForSpear.Value = DROWN.spearCost;
            DrownMod.drownOptions.PointsForExplSpear.Value = DROWN.spearExplCost;
            DrownMod.drownOptions.PointsForBomb.Value = DROWN.bombCost;
            DrownMod.drownOptions.PointsForRespawn.Value = DROWN.respCost;
            DrownMod.drownOptions.PointsForDenOpen.Value = DROWN.denCost;
            DrownMod.drownOptions.CreatureCleanup.Value = DROWN.creatureCleanupWaves;

        }
        public void CreatePageButtons()
        {
        }
        //public void DeletePageButtons()
        //{
        //    this.ClearMenuObject(ref prevButton);
        //    this.ClearMenuObject(ref nextButton);
        //}
        public override void RemoveSprites()
        {
            base.RemoveSprites();
        }
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

        }
        public override void Update()
        {
            base.Update();


        }

    }
}