using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class ImprovedWardrobeClothingTracker
    {
        private static readonly object clothingLock = new object();
        private static Dictionary<uint, HashSet<uint>> ownedUpperBody = new Dictionary<uint, HashSet<uint>>();
        private static Dictionary<uint, HashSet<uint>> ownedLowerBody = new Dictionary<uint, HashSet<uint>>();
        private static Dictionary<uint, HashSet<uint>> ownedFeet = new Dictionary<uint, HashSet<uint>>();

        private const string UpperBodyPrefix = "OwnedUpperBody";
        private const string LowerBodyPrefix = "OwnedLowerBody";
        private const string FeetPrefix = "OwnedFeet";

        /// <summary>
        /// Periodically checks the player's current clothing and adds it to the owned lists.
        /// </summary>
        public static void Tick()
        {
            if (!IS_PLAYER_CONTROL_ON(Main.PlayerIndex) || ImprovedWardrobeClothesVanStore.storeActive)
                return;

            lock (clothingLock)
            {
                uint currentTorso = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso);
                uint currentTorsoTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso);

                uint currentLegs = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs);
                uint currentLegsTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs);

                uint currentFeet = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet);
                uint currentFeetTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet);

                AddClothingWithTexture(PedComponents.Torso, currentTorso, currentTorsoTexture);
                AddClothingWithTexture(PedComponents.Legs, currentLegs, currentLegsTexture);
                AddClothingWithTexture(PedComponents.Feet, currentFeet, currentFeetTexture);
            }
        }
        public static bool PlayerOwnsClothing(uint componentType, uint drawableId, uint textureId)
        {
            lock (clothingLock)
            {
                Dictionary<uint, HashSet<uint>> targetDictionary = null;

                switch (componentType)
                {
                    case PedComponents.Torso:
                        targetDictionary = ownedUpperBody;
                        break;
                    case PedComponents.Legs:
                        targetDictionary = ownedLowerBody;
                        break;
                    case PedComponents.Feet:
                        targetDictionary = ownedFeet;
                        break;
                    default:
                        return false;
                }

                return targetDictionary != null &&
                       targetDictionary.ContainsKey(drawableId) &&
                       targetDictionary[drawableId].Contains(textureId);
            }
        }

        /// <summary>
        /// Adds a clothing item and its texture to the player's owned list.
        /// </summary>
        public static void AddClothingWithTexture(uint componentType, uint drawableId, uint textureId)
        {
            lock (clothingLock)
            {
                Dictionary<uint, HashSet<uint>> targetDictionary = null;

                switch (componentType)
                {
                    case PedComponents.Torso:
                        targetDictionary = ownedUpperBody;
                        break;
                    case PedComponents.Legs:
                        targetDictionary = ownedLowerBody;
                        break;
                    case PedComponents.Feet:
                        targetDictionary = ownedFeet;
                        break;
                    default:
                        return;
                }

                if (targetDictionary != null)
                {
                    if (!targetDictionary.ContainsKey(drawableId))
                    {
                        targetDictionary[drawableId] = new HashSet<uint>();
                    }

                    targetDictionary[drawableId].Add(textureId);
                }
            }
        }
        public static void AddVanillaClothingToOwnedForTBoGT()
        {
            if (Main.Episode != (uint)Episode.TBoGT)
                return; 

            foreach (var item in ImprovedWardrobeClothesVanStore.VanillaClothingLimitsTorsoTBoGT)
            {
                for (uint texture = 0; texture <= item.Value; texture++)
                {
                    ImprovedWardrobeClothingTracker.AddClothingWithTexture(PedComponents.Torso, item.Key, texture);
                }
            }

            foreach (var item in ImprovedWardrobeClothesVanStore.VanillaClothingLimitsLegsTBoGT)
            {
                for (uint texture = 0; texture <= item.Value; texture++)
                {
                    ImprovedWardrobeClothingTracker.AddClothingWithTexture(PedComponents.Legs, item.Key, texture);
                }
            }

            foreach (var item in ImprovedWardrobeClothesVanStore.VanillaClothingLimitsFeetTBoGT)
            {
                for (uint texture = 0; texture <= item.Value; texture++)
                {
                    ImprovedWardrobeClothingTracker.AddClothingWithTexture(PedComponents.Feet, item.Key, texture);
                }
            }

            Main.Log("Added all vanilla TBoGT clothing to owned list.");
        }


        /// <summary>
        /// Saves the owned clothing data and textures to the save file.
        /// </summary>
        /// 
        public static void Save()
        {
            if (Main.GameSaved)
            {
                SaveClothingSet(ownedUpperBody, UpperBodyPrefix);
                SaveClothingSet(ownedLowerBody, LowerBodyPrefix);
                SaveClothingSet(ownedFeet, FeetPrefix);
            }
        }

        /// <summary>
        /// Loads the owned clothing data and textures from the LibertyTweaks.save file.
        /// </summary>
        public static void Load()
        {
            lock (clothingLock)
            {
                LoadClothingSet(UpperBodyPrefix, ownedUpperBody);
                LoadClothingSet(LowerBodyPrefix, ownedLowerBody);
                LoadClothingSet(FeetPrefix, ownedFeet);
            }
        }
        private static void SaveClothingSet(Dictionary<uint, HashSet<uint>> clothingSet, string prefix)
        {
            string serializedModels = string.Join(",", clothingSet.Keys);
            Main.GetTheSaveGame().SetValue(prefix, serializedModels);
            Main.Log($"Saved {prefix}: {serializedModels}");

            foreach (var model in clothingSet)
            {
                string textureKey = $"{prefix}Texture{model.Key}";
                string serializedTextures = string.Join(",", model.Value);
                Main.GetTheSaveGame().SetValue(textureKey, serializedTextures);
            }

            Main.GetTheSaveGame().Save();
        }

        private static void LoadClothingSet(string prefix, Dictionary<uint, HashSet<uint>> clothingSet)
        {
            clothingSet.Clear();

            string serializedModels = Main.GetTheSaveGame().GetValue(prefix);

            if (!string.IsNullOrEmpty(serializedModels))
            {
                var models = serializedModels.Split(',')
                                             .Select(item => uint.TryParse(item, out var value) ? value : (uint?)null)
                                             .Where(value => value.HasValue)
                                             .Select(value => value.Value);

                foreach (var model in models)
                {
                    string textureKey = $"{prefix}Texture{model}";
                    string serializedTextures = Main.GetTheSaveGame().GetValue(textureKey);

                    HashSet<uint> textures = new HashSet<uint>();
                    if (!string.IsNullOrEmpty(serializedTextures))
                    {
                        textures = serializedTextures.Split(',')
                                                     .Select(item => uint.TryParse(item, out var value) ? value : (uint?)null)
                                                     .Where(value => value.HasValue)
                                                     .Select(value => value.Value)
                                                     .ToHashSet();
                    }

                    clothingSet[model] = textures;
                }
            }
            else
            {
                Main.Log($"No data found for {prefix}");
            }
        }
    }
}
