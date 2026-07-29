using SteamMarketDeepSearch.Enums;
using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamMarketDeepSearch.Services
{
    public static class CatalogGenerator
    {
        public static JsonSerializerOptions opt = new()
        {
            WriteIndented = true,
            Converters =
        {
            new JsonStringEnumConverter()
        }
        };

        public static void Generate()
        {
            List<MarketSearchTarget> targets = [];

            foreach (WeaponType weapon in Enum.GetValues<WeaponType>())
            {
                if (weapon == WeaponType.Unknown)
                    continue;

                targets.Add(new MarketSearchTarget
                {
                    DisplayName = FormatName(weapon),
                    WeaponType = weapon,
                    Category = GetCategory(weapon),
                    MarketQuery = string.Empty
                });
            }


            string json = JsonSerializer.Serialize(
                targets,
                opt
            );

            string path =
                @"C:\Users\Brydżu\source\repos\SteamMarketDeepSearch\SteamMarketDeepSearch\SteamMarketDeepSearch\Data\WeaponCatalog.json";

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            File.WriteAllText(path, json);
        }


        private static string FormatName(WeaponType weapon)
        {
            return weapon.ToString()
                .Replace("_", "-");
        }


        private static ItemCategory GetCategory(WeaponType weapon)
        {
            return weapon switch
            {
                WeaponType.Zeus_x27
                    => ItemCategory.Special,

                WeaponType.Glock_18
                or WeaponType.USP_S
                or WeaponType.P2000
                or WeaponType.P250
                or WeaponType.Five_SeveN
                or WeaponType.Tec_9
                or WeaponType.CZ75_Auto
                or WeaponType.DesertEagle
                or WeaponType.R8Revolver
                or WeaponType.DualBerettas
                    => ItemCategory.Pistol,

                WeaponType.MAC10
                or WeaponType.MP9
                or WeaponType.MP5SD
                or WeaponType.MP7
                or WeaponType.UMP45
                or WeaponType.P90
                or WeaponType.PPBizon
                    => ItemCategory.SMG,

                WeaponType.AK_47
                or WeaponType.M4A4
                or WeaponType.M4A1_S
                or WeaponType.FAMAS
                or WeaponType.Galil_AR
                or WeaponType.AUG
                or WeaponType.SG_553
                    => ItemCategory.Rifle,

                WeaponType.AWP
                or WeaponType.SSG_08
                or WeaponType.SCAR_20
                or WeaponType.G3SG1
                    => ItemCategory.SniperRifle,

                WeaponType.XM1014
                or WeaponType.MAG_7
                or WeaponType.Nova
                or WeaponType.Sawed_Off
                    => ItemCategory.Shotgun,

                WeaponType.M249
                or WeaponType.Negev
                    => ItemCategory.MachineGun,

                WeaponType.Bayonet
                or WeaponType.Butterfly_Knife
                or WeaponType.Karambit
                or WeaponType.M9_Bayonet
                or WeaponType.Navaja_Knife
                or WeaponType.Talon_Knife
                or WeaponType.Falchion_Knife
                or WeaponType.Flip_Knife
                or WeaponType.Gut_Knife
                or WeaponType.Nomad_Knife
                or WeaponType.Paracord_Knife
                or WeaponType.Bowie_Knife
                or WeaponType.Stiletto_Knife
                or WeaponType.Skeleton_Knife
                or WeaponType.Huntsman_Knife
                or WeaponType.Shadow_Daggers
                or WeaponType.Survival_Knife
                or WeaponType.Classic_Knife
                or WeaponType.Kukri_Knife
                or WeaponType.Ursus_Knife
                    => ItemCategory.Knife,

                WeaponType.Bloodhound_Gloves
                or WeaponType.Driver_Gloves
                or WeaponType.Sport_Gloves
                or WeaponType.Specialist_Gloves
                or WeaponType.Hand_Wraps
                or WeaponType.Moto_Gloves
                or WeaponType.Broken_Fang_Gloves
                or WeaponType.Hydra_Gloves
                    => ItemCategory.Gloves,

                _ => ItemCategory.Unknown
            };
        }
    }
};