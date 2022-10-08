﻿using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace Creaturebook
{
    internal class DiscoveredCreaturesFromAChapter
    {
        public string previousPrefix;
        public string b;
        public int c;
        public int d;

        public bool AllowsInput()
        {
            return true;
        }
        public bool CanHaveMultipleValues(string input = null)
        {
            return false;
        }

        public bool IsReady()
        {
            return Context.IsWorldReady;
        }

        public bool UpdateContext()
        {
            if (IsReady())
            {
                foreach (var data in Game1.player.modData.Pairs)
                {
                    foreach (var item in ModEntry.Chapters)
                    {
                        if (item.FromContentPack.Manifest.UniqueID + "_" + item.CreatureNamePrefix == previousPrefix && data.Value != "null")
                        {
                            if (data.Key.Contains(previousPrefix))
                            {
                                d++;
                            }
                        }
                    }
                }
                return c != d;
            }
            else           
                return false;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public IEnumerable<string> GetValues(string input)
        {
            b = input;
            if (IsReady() && input != "" && input != null)
            {
                foreach (var data in Game1.player.modData.Pairs)
                {
                    foreach (var chapter in ModEntry.Chapters)
                    {
                        if (chapter.CreatureNamePrefix == input && data.Value != "null")
                        {
                            if (data.Key.Contains(previousPrefix))
                            {
                                c++;
                            }
                        }
                    }
                }
                yield return Convert.ToString(c);
            }
            else
            {
                yield return null;
            }
        }
    }
    internal class IsCreatureDiscovered 
    {
        public string previousKey; 
        public bool a_Boolean;
        public bool b_Boolean;

        public bool AllowsInput()
        {
            return true;
        }
        public bool CanHaveMultipleValues(string input = null)
        {
            return false;
        }

        public bool IsReady()
        {
            return Context.IsWorldReady;
        }

        public bool UpdateContext()
        {
            if (IsReady())
            {
                foreach (var item in Game1.player.modData.Pairs)
                {
                    if (item.Key.StartsWith(ModEntry.MyModID) && item.Key != ModEntry.MyModID + "_IsNotebookObtained")
                    {
                        foreach (var chapter in ModEntry.Chapters)
                        {
                            for (int i = 0; i < chapter.Creatures.Count; i++)
                            {
                                if (chapter.FromContentPack.Manifest.UniqueID + "_" + chapter.CreatureNamePrefix + "_" + chapter.Creatures[i].ID == previousKey && item.Value != "null")
                                {
                                    b_Boolean = true;
                                    break;
                                }
                            }                        }
                    }
                }
                return a_Boolean != b_Boolean;
            }
            else
                return false;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public IEnumerable<string> GetValues(string input)
        {
            if (IsReady())
            {
                foreach (var item in Game1.player.modData.Pairs)
                {
                    if (item.Key.StartsWith(ModEntry.MyModID) && item.Key != ModEntry.MyModID + "_IsNotebookObtained")
                    {
                        foreach (var chapter in ModEntry.Chapters)
                        {
                            for (int i = 0; i < chapter.Creatures.Count; i++)
                            {
                                if (chapter.FromContentPack.Manifest.UniqueID + "_" + chapter.CreatureNamePrefix + "_" + chapter.Creatures[i].ID == input && item.Value != null)
                                {
                                    a_Boolean = true;
                                    previousKey = input;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
                yield return Convert.ToString(a_Boolean);
            }
            else
            {
                yield return null;
            }
        }
    }
}
