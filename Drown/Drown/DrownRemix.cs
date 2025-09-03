using Menu.Remix.MixedUI;
using System;
using UnityEngine;
public class DrownOptions : OptionInterface
{

    public readonly Configurable<int> MaxCreatureCount;
    public readonly Configurable<int> PointsForSpear;
    public readonly Configurable<int> PointsForExplSpear;
    public readonly Configurable<int> PointsForBomb;
    public readonly Configurable<int> PointsForElectricSpear;
    public readonly Configurable<int> PointsForBoomerang;

    public readonly Configurable<int> PointsForRespawn;
    public readonly Configurable<int> PointsForDenOpen;
    public readonly Configurable<int> CreatureCleanup;


    public readonly Configurable<KeyCode> StoreItem1;
    public readonly Configurable<KeyCode> StoreItem2;
    public readonly Configurable<KeyCode> StoreItem3;
    public readonly Configurable<KeyCode> StoreItem4;
    public readonly Configurable<KeyCode> StoreItem5;
    public readonly Configurable<KeyCode> StoreItem6;
    public readonly Configurable<KeyCode> StoreItem7;

    public readonly Configurable<KeyCode> OpenStore;



    private UIelement[] OnlineArenaSettings;


    public DrownOptions(global::Drown.DrownMod instance)
    {

        MaxCreatureCount = config.Bind("DrownMaxCreatures", 10);
        PointsForSpear = config.Bind("DrownPointsForSpear", 1);
        PointsForExplSpear = config.Bind("DrownPointsForExplSpear", 10);
        PointsForBomb = config.Bind("DrownPointsForBomb", 10);
        PointsForElectricSpear = config.Bind("PointsForElectricSpear", 12);

        PointsForBoomerang = config.Bind("PointsForBoomerang", 15);

        PointsForRespawn = config.Bind("DrownPointsForRespawn", 25);
        PointsForDenOpen = config.Bind("DrownPointsForDenOpen", 100);
        CreatureCleanup = config.Bind("DrownCreatureCleanup", 3);


        StoreItem1 = config.Bind("DrownStoreItem1", KeyCode.Alpha1);
        StoreItem2 = config.Bind("DrownStoreItem2", KeyCode.Alpha2);
        StoreItem3 = config.Bind("DrownStoreItem3", KeyCode.Alpha3);
        StoreItem4 = config.Bind("DrownStoreItem4", KeyCode.Alpha4);
        StoreItem5 = config.Bind("DrownStoreItem5", KeyCode.Alpha5);
        StoreItem6 = config.Bind("DrownStoreItem6", KeyCode.Alpha6);
        StoreItem7 = config.Bind("DrownStoreItem7", KeyCode.Alpha7);
        OpenStore = config.Bind("DrownStoreAccess", KeyCode.Tab);
    }

    public override void Initialize()
    {
        try
        {
            OpTab drownTab = new OpTab(this, "DROWN");
            Tabs = new OpTab[1] { drownTab };
            OnlineArenaSettings = new UIelement[]
            {
                new OpLabel(10f, 550f, "DROWN", bigText: true),
                new OpLabel(10f, 505, "Max creatures in level", bigText: false),
                new OpTextBox(MaxCreatureCount, new Vector2(10, 480), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },

                new OpLabel(10f, 460, "Points required to buy a spear", bigText: false),
                new OpTextBox(PointsForSpear, new Vector2(10, 435), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },
                new OpLabel(10f, 410, "Points required to buy an explosive spear", bigText: false),
                new OpTextBox(PointsForExplSpear, new Vector2(10, 385), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },
                new OpLabel(10f, 365, "Points required to buy a scav bomb", bigText: false),
                new OpTextBox(PointsForBomb, new Vector2(10, 340), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },


                new OpLabel(10f, 315, "[MSC]: Points required to buy an electric spear", bigText: false),
                new OpTextBox(PointsForElectricSpear, new Vector2(10, 290), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },

                new OpLabel(10f, 265, "[Watcher]: Points required to buy a boomerang", bigText: false),
                new OpTextBox(PointsForBoomerang, new Vector2(10, 240), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },

                new OpLabel(10f, 215, "Points required to buy a respawn", bigText: false),
                new OpTextBox(PointsForRespawn, new Vector2(10, 190), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },
                new OpLabel(10f, 165, "Points required to open dens", bigText: false),
                new OpTextBox(PointsForDenOpen, new Vector2(10, 140), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },

                new OpLabel(10f, 115, "How many waves before creature cleanup", bigText: false),
                new OpTextBox(CreatureCleanup, new Vector2(10, 90), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },

                new OpLabel(260, 500, "Hot key used to buy spear (store needs to be open)", bigText: false),
                new OpKeyBinder(StoreItem1, new Vector2(260, 470), new Vector2(150f, 30f)),

                new OpLabel(260, 445, "Hot key used to buy explosive spear", bigText: false),
                new OpKeyBinder(StoreItem2, new Vector2(260, 415), new Vector2(150f, 30f)),

                new OpLabel(260, 390, "Hot key used to buy scav bomb", bigText: false),
                new OpKeyBinder(StoreItem3, new Vector2(260, 360), new Vector2(150f, 30f)),
                
                new OpLabel(260, 340, "[MSC]: Hot key used to buy Electric Spear", bigText: false),
                new OpKeyBinder(StoreItem4, new Vector2(260, 310), new Vector2(150f, 30f)),

                new OpLabel(260, 290, "[Watcher]: Hot key used to buy boomerang", bigText: false),
                new OpKeyBinder(StoreItem5, new Vector2(260, 260), new Vector2(150f, 30f)),


                new OpLabel(260, 240, "Hot key used to buy respawn", bigText: false),
                new OpKeyBinder(StoreItem6, new Vector2(260, 210), new Vector2(150f, 30f)),

                new OpLabel(260, 190, "Hot key used to open den", bigText: false),
                new OpKeyBinder(StoreItem7, new Vector2(260, 160), new Vector2(150f, 30f)),

                new OpLabel(260, 140, "Key used to access store", bigText: false),
                new OpKeyBinder(OpenStore, new Vector2(260, 110), new Vector2(150f, 30f)),


        };
            drownTab.AddItems(OnlineArenaSettings);

        }
        catch (Exception ex)
        {
            RainMeadow.RainMeadow.Error("Error opening Drown Options Menu" + ex);
        }
    }

    public override void Update()
    {
    }
}
