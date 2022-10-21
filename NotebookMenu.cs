﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;

namespace Creaturebook
{
    public class NotebookMenu : IClickableMenu
    {
        readonly ClickableTextureComponent RightArrow = new(new((int)TopLeftCorner.X + 1282 - 372, (int)TopLeftCorner.Y + 718 - 242, 48, 44), Game1.mouseCursors, new(365, 495, 12, 11), 4f);
        readonly ClickableTextureComponent LeftArrow = new(new((int)TopLeftCorner.X, (int)TopLeftCorner.Y + 718 - 242, 48, 44), Game1.mouseCursors, new(352, 495, 12, 11), 4f);

        readonly ClickableTextureComponent CloseButton = new(new((int)TopLeftCorner.X + 982, (int)TopLeftCorner.Y, 48, 48), Game1.mouseCursors, new(337, 494, 12, 12), 4f);

        readonly ClickableTextureComponent Button_1;
        readonly ClickableTextureComponent Button_2;
        readonly ClickableTextureComponent Button_3;

        readonly ClickableTextureComponent SwitchToNormal_1 = new(new((int)TopLeftCorner.X + 365, (int)TopLeftCorner.Y + 10, 64, 64), Game1.mouseCursors, new(162, 440, 16, 16), 4f);
        readonly ClickableTextureComponent SwitchToNormal_2 = new(new((int)TopLeftCorner.X + 880, (int)TopLeftCorner.Y + 10, 64, 64), Game1.mouseCursors, new(162, 440, 16, 16), 4f);
        readonly ClickableTextureComponent ShowSetView = new(new((int)TopLeftCorner.X + 982, (int)TopLeftCorner.Y + 200, 48, 48), Game1.mouseCursors, new(337, 494, 12, 12), 4f);

        ClickableComponent Sticky_Purple; ClickableComponent Sticky_Yellow;
        ClickableComponent Sticky_Blue; ClickableComponent Sticky_Green;

        internal static string[] PagesWithStickies = new string[4];

        public int currentID = 0;
        public int actualID = 0;
        public int currentChapter = 0;
        public int currentSetPage = 0;
        public static Chapter ChapterYoureIn = ModEntry.Chapters[0];
        public static Creature CurrentCreature = ChapterYoureIn.Creatures[0];
        string fullCreatureID;
        string modID;
        readonly TextBox textBox = new(Game1.content.Load<Texture2D>(PathUtilities.NormalizePath("LooseSprites\\textBox")), null, Game1.smallFont, Color.Black)
        {
            X = (int)TopLeftCorner.X + 1282 - 300,
            Y = (int)TopLeftCorner.Y + 718 - 300,
            Width = (int)TopLeftCorner.X + 1282 - 250
        };
        float Stickyrotation;

        // sets the corner, The lower Y, the lower left-up corner
        static readonly Vector2 TopLeftCorner = Utility.getTopLeftPositionForCenteringOnScreen(960, 520);

        bool willSearch = false;
        bool IsFirstActive = true;
        bool IsSecondActive = false;
        bool IsThirdActive = false;
        bool showSetPaging = ChapterYoureIn.EnableSets;
        bool IsHeaderPage = true;
        bool WasHeaderPage = false;
        bool DontDrawRightArrow = false;
        List<bool> pageOrder = new();

        readonly string unknownLabel = ModEntry.Helper.Translation.Get("CB.UnknownLabel");
        readonly string unknownDesc = ModEntry.Helper.Translation.Get("CB.UnknownDesc");

        string latinName;
        string description;
        string localizedName = CurrentCreature.Name;
        string authorchapterTitle = ModEntry.Helper.Translation.Get("CB.Chapter") + "1: " + ChapterYoureIn.Title + "\n" + ModEntry.Helper.Translation.Get("CB.ChapterAuthorBy") + ChapterYoureIn.Author;

        static readonly Texture2D NotebookTexture = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", "NotebookTexture"));
        static readonly Texture2D ButtonTexture = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", "SearchButton"));
        static readonly Texture2D Stickies = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", "Stickies"));

        static readonly ClickableTextureComponent SearchButton = new(new((int)TopLeftCorner.X + 982, (int)TopLeftCorner.Y + 718 - 220, 68, 64), ButtonTexture, new(0, 0, 17, 16), 4f);

        Texture2D CreatureTexture = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ChapterYoureIn.FromContentPack.Manifest.UniqueID + "." + ChapterYoureIn.CreatureNamePrefix + "_0_Image1"));
        Texture2D CreatureTexture_2;
        Texture2D CreatureTexture_3;

        readonly string[] menuTexts;
        readonly Texture2D[] MenuTextures;
        public NotebookMenu()
        {
            MenuTextures = new Texture2D[] { NotebookTexture, CreatureTexture, ButtonTexture, CreatureTexture_2, CreatureTexture_3, Stickies };
            if (ModEntry.modConfig.EnableStickies)
            {
                PagesWithStickies = ModEntry.Helper.Data.ReadJsonFile<string[]>(PathUtilities.NormalizeAssetName("localData/stickies.json"));
                if (PagesWithStickies is null)
                {
                    PagesWithStickies = new string[4];
                    Sticky_Yellow = new(new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84), "");
                    Sticky_Green = new(new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84), "");
                    Sticky_Blue = new(new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84), "");
                    Sticky_Purple = new(new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 380, 240, 84), "");
                }
                else
                {
                    Sticky_Yellow = PagesWithStickies[0] is null ? new(new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84), "") : new(new((int)TopLeftCorner.X - 150, (int)TopLeftCorner.Y + 200, 240, 84), "");
                    Sticky_Green = PagesWithStickies[1] is null ? new(new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84), "") : new(new((int)TopLeftCorner.X - 150, (int)TopLeftCorner.Y + 260, 240, 84), "");
                    Sticky_Blue = PagesWithStickies[2] is null ? new(new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84), "") : new(new((int)TopLeftCorner.X - 150, (int)TopLeftCorner.Y + 320, 240, 84), "");
                    Sticky_Purple = PagesWithStickies[3] is null ? new(new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 380, 240, 84), "") : new(new((int)TopLeftCorner.X - 150, (int)TopLeftCorner.Y + 380, 240, 84), "");
                }
            }
            if (CurrentCreature.HasScientificName)
                latinName = ModEntry.Helper.Translation.Get("CB.LatinName") + CurrentCreature.ScientificName;
            else
                latinName = null;
            if (CurrentCreature.HasFunFact)
                description = CurrentCreature.Desc;
            if (CurrentCreature.HasExtraImages)
            {
                Button_2 = new(new((int)TopLeftCorner.X + 50, (int)TopLeftCorner.Y + 50, 50, 50), Game1.mouseCursors, new(528, 128, 8, 8), 4f);
                if (File.Exists(PathUtilities.NormalizeAssetName(CurrentCreature.Directory + "\\book-image_3.png")))
                    Button_3 = new(new((int)TopLeftCorner.X + 50, (int)TopLeftCorner.Y + 100, 50, 50), Game1.mouseCursors, new(520, 128, 8, 8), 4f);
                else
                    Button_3 = null;
            }
            else
            {
                Button_1 = null;
                Button_2 = null;
                Button_3 = null;
            }

            menuTexts = new string[] { latinName, description, localizedName, unknownLabel, unknownDesc, authorchapterTitle };

            textBox.OnEnterPressed += TextBoxEnter;
            Game1.keyboardDispatcher.Subscriber = textBox;
            UpdateNotebookPage();
            if (ChapterYoureIn.EnableSets)
                CalculateSetPages(ChapterYoureIn);
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            if (PagesWithStickies[2] is not null && PagesWithStickies[2] != modID + "." + fullCreatureID && ModEntry.modConfig.EnableStickies)
                b.Draw(MenuTextures[5], new(Sticky_Blue.bounds.X, Sticky_Blue.bounds.Y), new(0, 42, 60, 21), Color.White, Stickyrotation, Vector2.Zero, 3f, SpriteEffects.None, layerDepth: 0.5f);

            b.Draw(MenuTextures[0], TopLeftCorner, null, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth: 0.5f);

            if (PagesWithStickies[2] == modID + "." + fullCreatureID || PagesWithStickies[2] is null && ModEntry.modConfig.EnableStickies)
                b.Draw(MenuTextures[5], new(Sticky_Blue.bounds.X, Sticky_Blue.bounds.Y), new(0, 42, 60, 21), Color.White, Stickyrotation, Vector2.Zero, 3f, SpriteEffects.None, layerDepth: 0.5f);

            Sticky_Yellow.visible = true;
            Sticky_Green.visible = true;
            Sticky_Blue.visible = true;
            Sticky_Purple.visible = true;

            CloseButton.draw(b);
            if (!IsHeaderPage)
            {
                if (!showSetPaging)
                {
                    SearchButton.draw(b);
                    ShowSetView.draw(b);
                    if (!Game1.player.modData[ModEntry.MyModID + "_" + modID + "." + fullCreatureID].Equals("null"))
                    {
                        string Date = Game1.player.modData[ModEntry.MyModID + "_" + modID + "." + fullCreatureID];
                        int count = Convert.ToInt32(Date);
                        SDate convertedDate = SDate.FromDaysSinceStart(count);
                        string translatedDate = convertedDate.ToLocaleString();
                        string dateDiscovered = ModEntry.Helper.Translation.Get("CB.dateDiscovered") + translatedDate;
                        if (Button_2 is not null)
                        {
                            Button_1.draw(b);
                            Button_2.draw(b);
                            if (Button_3 != null)
                                Button_3.draw(b);
                        }
                        if (IsFirstActive)
                            b.Draw(MenuTextures[1], TopLeftCorner, null, Color.White, 0f, new(CurrentCreature.ImageOffsets[0].X, CurrentCreature.ImageOffsets[0].Y), CurrentCreature.ImageScales[0], SpriteEffects.None, layerDepth: 0.5f);

                        else if (IsSecondActive)
                            b.Draw(MenuTextures[2], TopLeftCorner, null, Color.White, 0f, new(CurrentCreature.ImageOffsets[1].X, CurrentCreature.ImageOffsets[1].Y), CurrentCreature.ImageScales[1], SpriteEffects.None, layerDepth: 0.5f);

                        else if (IsThirdActive)
                            b.Draw(MenuTextures[3], TopLeftCorner, null, Color.White, 0f, new(CurrentCreature.ImageOffsets[2].X, CurrentCreature.ImageOffsets[2].Y), CurrentCreature.ImageScales[2], SpriteEffects.None, layerDepth: 0.5f);

                        b.DrawString(Game1.smallFont, menuTexts[2], new(TopLeftCorner.X + 15, TopLeftCorner.Y + 310), Color.Black);

                        if (ModEntry.modConfig.ShowScientificNames && CurrentCreature.HasScientificName)
                            b.DrawString(Game1.smallFont, menuTexts[0], new(TopLeftCorner.X + 15, TopLeftCorner.Y + 350), Color.Black);

                        if (ModEntry.modConfig.ShowDiscoveryDates)
                            b.DrawString(Game1.smallFont, dateDiscovered, new(TopLeftCorner.X + 15, TopLeftCorner.Y + 390), Color.Black);

                        if (CurrentCreature.HasFunFact)
                            SpriteText.drawString(b, menuTexts[1], (int)TopLeftCorner.X + 910 - 371, (int)TopLeftCorner.Y + 254 - 230, width: 420, height: 490);
                    }
                    else
                    {
                        b.Draw(CreatureTexture, TopLeftCorner, null, Color.Black * 0.8f, 0f, new Vector2(CurrentCreature.ImageOffsets[0].X, CurrentCreature.ImageOffsets[0].Y), CurrentCreature.ImageScales[0], SpriteEffects.None, layerDepth: 0.5f);
                        b.DrawString(Game1.smallFont, menuTexts[3], new Vector2(TopLeftCorner.X + 15, TopLeftCorner.Y + 310), Color.Black);
                        b.DrawString(Game1.smallFont, menuTexts[4], new Vector2(TopLeftCorner.X + 15, TopLeftCorner.Y + 350), Color.Black);
                    }
                }
                else
                {
                    if (currentSetPage < pageOrder.Count - 1 && currentSetPage != 0)
                    {
                        if ((!pageOrder[currentSetPage] && currentSetPage == pageOrder.Count - 1) || (!pageOrder[currentSetPage] && !pageOrder[currentSetPage + 1]))
                            DrawCreatureIcons(b, false, currentSetPage);

                        else if ((!pageOrder[currentSetPage] && pageOrder[currentSetPage + 1]) || pageOrder[currentSetPage])
                            DrawCreatureIcons(b, true, currentSetPage);
                    }
                    else if (currentSetPage == 0)
                    {
                        if ((!pageOrder[currentSetPage] && currentSetPage == pageOrder.Count - 1) || (!pageOrder[currentSetPage] && !pageOrder[currentSetPage + 1]))
                            DrawCreatureIcons(b, false, 0);
                        else if ((!pageOrder[currentSetPage] && pageOrder[currentSetPage + 1]) || pageOrder[currentSetPage])
                            DrawCreatureIcons(b, true, 0);
                    }
                    else if (currentSetPage == pageOrder.Count - 1 && currentChapter != ModEntry.Chapters.Count - 1)
                    {
                        if (!pageOrder[currentSetPage] || !pageOrder[currentSetPage])
                            DrawCreatureIcons(b, false, currentSetPage);
                    }
                }
            }
            else
            {
                /*if (ModEntry.Chapters[currentChapter].EnableSets)
                    b.DrawString(Game1.smallFont, menuTexts[7], new Vector2(TopLeftCorner.X + 910 - 371, TopLeftCorner.Y + 100), Color.Black);
                
                else
                    b.DrawString(Game1.smallFont, menuTexts[6], new Vector2(TopLeftCorner.X + 910 - 371, TopLeftCorner.Y + 100), Color.Black);*/

                SpriteText.drawString(b, menuTexts[5], (int)TopLeftCorner.X + 30, (int)TopLeftCorner.Y + 54, width: 420, height: 490, scroll_text_alignment: SpriteText.ScrollTextAlignment.Center);
            }
            if (actualID == 0 && IsHeaderPage && currentID == 0)
            {
                LeftArrow.visible = false;
                RightArrow.draw(b);
            }
            else if (actualID == ChapterYoureIn.Creatures.Count - 1 || (actualID == ChapterYoureIn.Creatures.Count && currentChapter == 0) || DontDrawRightArrow)
            {
                RightArrow.visible = false;
                LeftArrow.draw(b);
            }
            else
            {
                LeftArrow.visible = true;
                RightArrow.visible = true;
                LeftArrow.draw(b);
                RightArrow.draw(b);
            }

            if (willSearch)
                textBox.Draw(b);
            drawMouse(b);
        }
        public void TextBoxEnter(TextBox sender)
        {
            if (sender.Text.Length >= 1 && willSearch)
            {
                int result = -1;
                if (int.TryParse(sender.Text, out result))
                {
                    if (-1 < Convert.ToInt32(sender.Text) && Convert.ToInt32(sender.Text) < ChapterYoureIn.Creatures.Count)
                    {
                        currentID = Convert.ToInt32(sender.Text);
                        willSearch = false;
                        textBox.Text = "";

                        if (currentChapter is 0 && currentID != 0)
                            actualID = currentID;

                        else if (currentChapter is 0 && currentID == 0)
                            actualID = 0;

                        else
                        {
                            actualID = 0;
                            for (int i = 0; i < currentChapter - 1; i++)
                                actualID += ChapterYoureIn.Creatures.Count;
                            actualID += currentID + ModEntry.Chapters.Count - 1;
                        }
                        UpdateNotebookPage();
                    }
                }
                else if (!int.TryParse(sender.Text, out result))
                {
                    foreach (var item in ChapterYoureIn.Creatures)
                    {
                        if (!string.IsNullOrEmpty(item.ScientificName))
                        {
                            if (item.ScientificName.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                currentID = item.ID;
                                willSearch = false;
                                textBox.Text = "";

                                if (currentChapter is 0 && currentID != 0)
                                    actualID = currentID;

                                else if (currentChapter is 0 && currentID == 0)
                                    actualID = 0;

                                else if (currentChapter > 0 && currentID > 0)
                                {
                                    actualID = 0;
                                    for (int a = 0; a < currentChapter - 1; a++)
                                        actualID += ChapterYoureIn.Creatures.Count;
                                    actualID += currentID + ModEntry.Chapters.Count - 1;
                                }
                                UpdateNotebookPage();
                                break;
                            }
                        }
                        if (item.Name.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            currentID = item.ID;
                            willSearch = false;
                            textBox.Text = "";

                            if (currentChapter is 0 && currentID != 0)
                                actualID = currentID;

                            else if (currentChapter is 0 && currentID == 0)
                                actualID = 0;

                            else if (currentChapter > 0 && currentID > 0)
                            {
                                actualID = 0;
                                for (int a = 0; a < currentChapter - 1; a++)
                                    actualID += ChapterYoureIn.Creatures.Count;
                                actualID += currentID + ModEntry.Chapters.Count - 1;
                            }
                            UpdateNotebookPage();
                            break;
                        }
                    }
                }
            }
        }
        public override void receiveKeyPress(Keys key) { }
        //Leave this here and empty so M and E buttons don't yeet your menu
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Sticky_Blue.containsPoint(x, y))
                UpdateStickies(2);
            if (SwitchToNormal_1.containsPoint(x, y) && showSetPaging && !IsHeaderPage && !WasHeaderPage)
            {
                showSetPaging = false;
                if (!currentID.Equals(ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet[0]))
                {
                    currentID = ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet[0];
                    actualID = 0;
                    if (currentChapter > 0)
                        for (int i = 0; i < currentChapter; i++)
                            actualID += ModEntry.Chapters[i].Creatures.Count;
                    actualID += currentID;
                }
            }
            if (SwitchToNormal_2.containsPoint(x, y) && showSetPaging && !IsHeaderPage && !WasHeaderPage)
            {
                showSetPaging = false;
                if (currentID != ChapterYoureIn.Sets[currentSetPage + 1].CreaturesBelongingToThisSet[0])
                {
                    currentID += ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet.Length;
                    actualID += ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet.Length;
                }
                UpdateNotebookPage();
            }
            if (ShowSetView.containsPoint(x, y) && !showSetPaging)
            {
                showSetPaging = true;
            }
            if (CloseButton.containsPoint(x, y))
            {
                ModEntry.Helper.Data.WriteJsonFile(PathUtilities.NormalizeAssetName("localData/stickies.json"), PagesWithStickies);
                Game1.activeClickableMenu.exitThisMenu();
            }
            willSearch = SearchButton.containsPoint(x, y) && !willSearch && textBox.Text == "";

            if (Button_2 != null || Button_3 != null)
            {
                IsFirstActive = Button_1.containsPoint(x, y);
                IsSecondActive = Button_2.containsPoint(x, y);
                IsThirdActive = Button_3.containsPoint(x, y);
            }
            if (LeftArrow.containsPoint(x, y) && LeftArrow.visible && !willSearch)
            {
                if (IsHeaderPage)
                    IsHeaderPage = false;
                else if (actualID > 0 && !showSetPaging)
                {
                    actualID--;
                    if (currentID == 0 && currentChapter != 0)
                    {
                        currentChapter--;
                        currentID = ChapterYoureIn.Creatures.Count - 1;
                        WasHeaderPage = true;
                        IsHeaderPage = true;
                        ChapterYoureIn = ModEntry.Chapters[currentChapter];
                        CalculateSetPages(ChapterYoureIn);
                    }
                    else if (currentID > 0)
                        currentID--;
                }
                else if (currentID == 0)
                    IsHeaderPage = true;

                else if (showSetPaging)
                {
                    if (DontDrawRightArrow)
                    {
                        DontDrawRightArrow = false;
                    }
                    if (currentSetPage > 0)
                    {
                        actualID -= ChapterYoureIn.Sets[currentSetPage - 1].CreaturesBelongingToThisSet.Length - 1;
                        currentID -= ChapterYoureIn.Sets[currentSetPage - 1].CreaturesBelongingToThisSet.Length - 1;
                        if (currentSetPage is not 1 && !pageOrder[currentSetPage - 2] && !pageOrder[currentSetPage - 1])
                        {
                            actualID -= ChapterYoureIn.Sets[currentSetPage - 2].CreaturesBelongingToThisSet.Length - 1;
                            currentID -= ChapterYoureIn.Sets[currentSetPage - 2].CreaturesBelongingToThisSet.Length - 1;
                            currentSetPage--; currentSetPage--;
                        }
                        else if ((currentSetPage != 1 && pageOrder[currentSetPage - 1]) || (pageOrder[currentSetPage] && !pageOrder[currentSetPage - 1]))
                        {
                            currentSetPage--;
                        }
                    }
                    if (currentSetPage is 0 && currentChapter != 0)
                    {
                        currentChapter--;
                        currentID = ChapterYoureIn.Creatures.Count - 1;
                        WasHeaderPage = true;
                        IsHeaderPage = true;
                        ChapterYoureIn = ModEntry.Chapters[currentChapter];
                        CalculateSetPages(ChapterYoureIn);
                        currentSetPage = pageOrder.Count - 1;
                    }
                }
                for (int i = 0; i < 4; i++)
                    FindPageAndCheckSides(PagesWithStickies[i], false, i);
            }
            else if (RightArrow.containsPoint(x, y) && RightArrow.visible && !willSearch)
            {
                if (IsHeaderPage)
                    IsHeaderPage = false;
                else if (actualID + 1 != ChapterYoureIn.Creatures.Count && !showSetPaging)
                {
                    actualID++;
                    if (currentID + 1 == ChapterYoureIn.Creatures.Count && currentChapter < ModEntry.Chapters.Count - 1)
                    {
                        currentChapter++;
                        currentID = 0;
                        IsHeaderPage = true;
                        currentSetPage = 0;
                        ChapterYoureIn = ModEntry.Chapters[currentChapter];

                        CalculateSetPages(ChapterYoureIn);
                    }
                    else
                        currentID++;
                }
                else if (showSetPaging)
                {
                    if (currentSetPage < pageOrder.Count - 1)
                    {
                        if (currentSetPage != pageOrder.Count - 1 && !pageOrder[currentSetPage + 1])
                        {
                            actualID += ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet.Length - 1;
                            currentID += ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet.Length - 1;
                            actualID += ChapterYoureIn.Sets[currentSetPage + 1].CreaturesBelongingToThisSet.Length - 1;
                            currentID += ChapterYoureIn.Sets[currentSetPage + 1].CreaturesBelongingToThisSet.Length - 1;
                            currentSetPage++; currentSetPage++;
                        }
                        else if ((currentSetPage != pageOrder.Count - 1 && pageOrder[currentSetPage + 1]) || (pageOrder[currentSetPage] && !pageOrder[currentSetPage + 1]))
                        {
                            actualID += ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet.Length;
                            currentID += ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet.Length;
                            currentSetPage++;
                        }
                        else if (currentSetPage == pageOrder.Count - 1 && currentChapter != ModEntry.Chapters.Count - 1)
                        {
                            actualID += ChapterYoureIn.Sets[currentSetPage].CreaturesBelongingToThisSet.Length - 1;
                            currentChapter++;
                            currentID = 0;
                            IsHeaderPage = true;
                            currentSetPage = 0;
                            ChapterYoureIn = ModEntry.Chapters[currentChapter];
                            CalculateSetPages(ChapterYoureIn);
                        }
                        else if (currentSetPage == pageOrder.Count - 1 && currentChapter == ModEntry.Chapters.Count - 1)
                        {
                            DontDrawRightArrow = true;
                        }
                    }
                }
                for (int i = 0; i < 4; i++)
                    FindPageAndCheckSides(PagesWithStickies[i], false, i);
            }
            textBox.Update();
            Game1.keyboardDispatcher.Subscriber = textBox;
            Game1.playSound("shwip");
            UpdateNotebookPage();
        }
        public void UpdateNotebookPage()
        {
            CurrentCreature = ChapterYoureIn.Creatures[currentID];
            fullCreatureID = ChapterYoureIn.CreatureNamePrefix + "_" + currentID.ToString();
            modID = ChapterYoureIn.FromContentPack.Manifest.UniqueID;

            menuTexts[2] = CurrentCreature.Name;
            menuTexts[1] = CurrentCreature.Desc;

            CreatureTexture = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ChapterYoureIn.FromContentPack.Manifest.UniqueID + "." + ChapterYoureIn.CreatureNamePrefix + "_" + currentID.ToString() + "_Image1"));

            MenuTextures[1] = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ChapterYoureIn.FromContentPack.Manifest.UniqueID + "." + ChapterYoureIn.CreatureNamePrefix + "_" + currentID.ToString() + "_Image1"));
            menuTexts[5] = ModEntry.Helper.Translation.Get("CB.Chapter") + Convert.ToString(currentChapter + 1) + ": " + ChapterYoureIn.Title + "\n" + ModEntry.Helper.Translation.Get("CB.ChapterAuthorBy") + ChapterYoureIn.Author;

            showSetPaging = ChapterYoureIn.EnableSets;

            if (CurrentCreature.HasScientificName)
                menuTexts[0] = ModEntry.Helper.Translation.Get("CB.LatinName") + CurrentCreature.ScientificName;
            else
                latinName = "";

            if (WasHeaderPage)
            {
                menuTexts[5] = authorchapterTitle = ModEntry.Helper.Translation.Get("CB.Chapter") + Convert.ToString(currentChapter + 2) + ": " + ChapterYoureIn.Title + "\n" + ModEntry.Helper.Translation.Get("CB.ChapterAuthorBy") + ChapterYoureIn.Author;
                WasHeaderPage = false;
            }
            if (CurrentCreature.HasExtraImages)
            {
                MenuTextures[2] = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ChapterYoureIn.FromContentPack.Manifest.UniqueID + "." + ChapterYoureIn.CreatureNamePrefix + "_" + CurrentCreature.ID + "_Image2"));
                if (File.Exists(PathUtilities.NormalizeAssetName(CurrentCreature.Directory + "\\book-image_3.png")))
                    MenuTextures[3] = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ChapterYoureIn.FromContentPack.Manifest.UniqueID + "." + ChapterYoureIn.CreatureNamePrefix + "_" + CurrentCreature.ID + "_Image3"));
                else
                    MenuTextures[3] = null;
            }
            else
                MenuTextures[2] = null;
        }
        public void CalculateSetPages(Chapter chapter)
        {
            pageOrder = new();
            foreach (Set set in chapter.Sets)
                pageOrder.Add(set.NeedsSecondPage);
        }
        public void DrawCreatureIcons(SpriteBatch b, bool needsSecond, int i)
        {
            Texture2D creature;
            for (int l = 0; l < ChapterYoureIn.Sets[i].OffsetsInMenu.Length; l++)
            {
                creature = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ChapterYoureIn.FromContentPack.Manifest.UniqueID + "." + ChapterYoureIn.CreatureNamePrefix + "_" + ChapterYoureIn.Sets[i].CreaturesBelongingToThisSet[l].ToString() + "_Image1"));
                bool color = !Game1.player.modData[ModEntry.MyModID + "_" + modID + "." + ChapterYoureIn.CreatureNamePrefix + "_" + ChapterYoureIn.Sets[i].CreaturesBelongingToThisSet[l]].Equals("null");
                if (color)
                    b.Draw(creature, new((int)TopLeftCorner.X + ChapterYoureIn.Sets[i].OffsetsInMenu[l].X, (int)TopLeftCorner.Y + ChapterYoureIn.Sets[i].OffsetsInMenu[l].Y), null, Color.White, 0f, Vector2.Zero, ChapterYoureIn.Sets[i].ScalesInMenu[l], SpriteEffects.None, layerDepth: 0.5f);
                else
                    b.Draw(creature, new((int)TopLeftCorner.X + ChapterYoureIn.Sets[i].OffsetsInMenu[l].X, (int)TopLeftCorner.Y + ChapterYoureIn.Sets[i].OffsetsInMenu[l].Y), null, Color.Black * 0.8f, 0f, Vector2.Zero, ChapterYoureIn.Sets[i].ScalesInMenu[l], SpriteEffects.None, layerDepth: 0.5f);
                SwitchToNormal_1.draw(b);
                SpriteText.drawString(b, ChapterYoureIn.FromContentPack.Translation.Get(ChapterYoureIn.Sets[i].DisplayNameKey), (int)TopLeftCorner.X + 20, (int)TopLeftCorner.Y + 10);
            }
            if (!needsSecond && i < pageOrder.Count - 1)
            {
                Texture2D creature_2;
                for (int l = 0; l < ChapterYoureIn.Sets[i + 1].OffsetsInMenu.Length; l++)
                {
                    creature_2 = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ChapterYoureIn.FromContentPack.Manifest.UniqueID + "." + ChapterYoureIn.CreatureNamePrefix + "_" + ChapterYoureIn.Sets[i + 1].CreaturesBelongingToThisSet[l].ToString() + "_Image1"));
                    if (Game1.player.modData[ModEntry.MyModID + "_" + modID + "." + ChapterYoureIn.CreatureNamePrefix + "_" + ChapterYoureIn.Sets[i + 1].CreaturesBelongingToThisSet[l]] is not "null")
                        b.Draw(creature_2, new((int)TopLeftCorner.X + 530 + ChapterYoureIn.Sets[i + 1].OffsetsInMenu[l].X, (int)TopLeftCorner.Y + ChapterYoureIn.Sets[i + 1].OffsetsInMenu[l].Y), null, Color.White, 0f, Vector2.Zero, ChapterYoureIn.Sets[i + 1].ScalesInMenu[l], SpriteEffects.None, layerDepth: 0.5f);
                    else
                        b.Draw(creature_2, new((int)TopLeftCorner.X + 530 + ChapterYoureIn.Sets[i + 1].OffsetsInMenu[l].X, (int)TopLeftCorner.Y + ChapterYoureIn.Sets[i + 1].OffsetsInMenu[l].Y), null, Color.Black * 0.8f, 0f, Vector2.Zero, ChapterYoureIn.Sets[i + 1].ScalesInMenu[l], SpriteEffects.None, layerDepth: 0.5f);
                    SwitchToNormal_2.draw(b);
                    SpriteText.drawString(b, ChapterYoureIn.FromContentPack.Translation.Get(ChapterYoureIn.Sets[i + 1].DisplayNameKey), (int)TopLeftCorner.X + 530, (int)TopLeftCorner.Y + 10);
                }
            }
        }
        private void UpdateStickies(int WhichSticky)
        {
            if (string.IsNullOrEmpty(PagesWithStickies[WhichSticky]) && !IsHeaderPage && !WasHeaderPage)
            {
                Sticky_Yellow.bounds = WhichSticky is 0 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84) : Sticky_Yellow.bounds;
                Sticky_Green.bounds = WhichSticky is 1 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84) : Sticky_Green.bounds;
                Sticky_Blue.bounds = WhichSticky is 2 ? new((int)TopLeftCorner.X, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Blue.bounds;
                Sticky_Purple.bounds = WhichSticky is 3 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Purple.bounds;
                PagesWithStickies[WhichSticky] = modID + "." + fullCreatureID;
            }
            else if (PagesWithStickies[WhichSticky].Equals(modID + "." + fullCreatureID) && !IsHeaderPage && !WasHeaderPage)
            {
                Sticky_Yellow.bounds = WhichSticky is 0 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84) : Sticky_Yellow.bounds;
                Sticky_Yellow.bounds = WhichSticky is 1 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84) : Sticky_Green.bounds;
                Sticky_Blue.bounds = WhichSticky is 2 ? new((int)TopLeftCorner.X, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Blue.bounds;
                Sticky_Yellow.bounds = WhichSticky is 3 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Purple.bounds;
                PagesWithStickies[WhichSticky] = null;
            }
            else if (!string.IsNullOrEmpty(PagesWithStickies[WhichSticky]) && PagesWithStickies[WhichSticky].Equals(modID + "." + fullCreatureID))
                FindPageAndCheckSides(PagesWithStickies[WhichSticky], true, 5);
        }
        private void FindPageAndCheckSides(string CreatureID, bool WillTravel, int SentFromWhichSticky)
        {
            if (string.IsNullOrEmpty(CreatureID))
                return;
            for (int i = 0; i < ModEntry.Chapters.Count; i++)
            {
                for (int c = 0; c < ChapterYoureIn.Creatures.Count; c++)
                {
                    if (CreatureID.Contains(ChapterYoureIn.FromContentPack.Manifest.UniqueID) && CreatureID.Contains(ChapterYoureIn.CreatureNamePrefix) && CreatureID.Contains(c.ToString()))
                    {
                        if (WillTravel)
                        {
                            currentID = c;
                            currentChapter = i;
                            actualID = 0;

                            for (int a = 0; a < currentChapter - 1; a++)
                                actualID += ChapterYoureIn.Creatures.Count;

                            actualID += currentID + ModEntry.Chapters.Count - 1;

                            IsHeaderPage = false;
                            WasHeaderPage = false;
                            UpdateNotebookPage();
                            return;
                        }
                        else
                        {
                            if (i < currentChapter || (i == currentChapter && c < currentID) || CreatureID == modID + "." + fullCreatureID)
                                Stickyrotation = (float)Math.PI;

                            else if (i > currentChapter || (i == currentChapter && c > currentID) && SentFromWhichSticky is not 5)
                            {
                                Stickyrotation = 0f;
                                Sticky_Yellow.bounds = SentFromWhichSticky is 0 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84) : Sticky_Yellow.bounds;
                                Sticky_Green.bounds = SentFromWhichSticky is 1 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84) : Sticky_Green.bounds;
                                Sticky_Blue.bounds = SentFromWhichSticky is 2 ? new(NotebookTexture.Width + 50, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Blue.bounds;
                                Sticky_Purple.bounds = SentFromWhichSticky is 3 ? new((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Purple.bounds;
                            }
                        }
                    }
                }
            }
        }
    }
}
