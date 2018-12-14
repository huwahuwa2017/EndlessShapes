using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EndlessShapes.Blocks
{
    public class ImageDataStorageBlock : SyncroniseBlock
    {
        public static List<ImageDataStorageBlock> ImageDataStorageBlockList = new List<ImageDataStorageBlock>();

        public List<Material> MaterialList = new List<Material>();

        private List<string> ImageDataList = new List<string>();

        private string ImageNameDelimiter = "<Delimiter 07834184>";

        private string ImageDataDelimiter = "<Delimiter 67353950>";

        private string ImageName = string.Empty;

        private string ImageData = string.Empty;

        private string SetFilePath = "file:///";

        private string StatsText = string.Empty;

        private string SearchText = string.Empty;

        private Vector2 ListScroll;

        private int SelectID = -1;

        private bool AddDataMenu;

        

        public override void SetExtraInfo(ExtraInfoArrayReadPackage v)
        {
            base.SetExtraInfo(v);

            if (v.FindDelimiterAndSpoolToIt(DelimiterType.BaseTier))
            {
                UniqueId = v.GetNextInt();
            }
        }

        public override void GetExtraInfo(ExtraInfoArrayWritePackage v)
        {
            base.GetExtraInfo(v);
            v.AddDelimiterOpen(DelimiterType.BaseTier);
            v.WriteNextInt(UniqueId);
            v.AddDelimiterClose(DelimiterType.BaseTier);
        }

        public override string SetText(string NewText, bool sync = true)
        {
            ImageDataList = new List<string>(NewText.Split(new string[] { ImageDataDelimiter }, StringSplitOptions.None));
            List<string> NameList = (ImageDataList[0] == string.Empty) ? new List<string>() : new List<string>(ImageDataList[0].Split(new string[] { ImageNameDelimiter }, StringSplitOptions.None));
            ImageDataList.RemoveAt(0);

            for (int i = 0; i < ImageDataList.Count; ++i)
            {
                MaterialList.Add(PNGTextureConstruct.MaterialConstruct(ImageDataList[i], NameList[i]));
            }

            return string.Empty;
        }

        public override string GetText()
        {
            if (ImageDataList.Count == 0) return string.Empty;
            return String.Join(ImageNameDelimiter, MaterialList.Select(M => M.name)) + ImageDataDelimiter + String.Join(ImageDataDelimiter, ImageDataList);
        }



        public override void StateChanged(IBlockStateChange change)
        {
            if (change.InitiatedOrInitiatedInUnrepairedState_OnlyCalledOnce)
            {
                BlockWithText = true;
                ImageDataStorageBlockList.Add(this);
            }
            else if (change.IsPerminentlyRemovedOrConstructDestroyed)
            {
                ImageDataStorageBlockList.Remove(this);
            }

            base.StateChanged(change);
        }



        public override InteractionReturn Secondary()
        {
            InteractionReturn interactionReturn = new InteractionReturn
            {
                SpecialNameField = "ImageDataStorageBlock",
                SpecialBasicDescriptionField = "ID : " + UniqueId
            };
            interactionReturn.AddExtraLine("Press <<Q>> to settings");
            return interactionReturn;
        }

        public override void Secondary(Transform T)
        {
            new GenericBlockGUI().ActivateGui(this, GuiActivateType.Standard);
        }

        public override bool ExtraGUI()
        {
            Rect Panel1 = new Rect(250f, 100f, 1030f, 600f);
            GUILayout.BeginArea(Panel1, "ImageDataStorageBlock", GUI.skin.window);

            GUILayout.BeginVertical(GUILayout.Height(490));
            GUILayout.Label("State");
            GUILayout.TextArea(StatsText, GUILayout.Height(130));

            if (SelectID != -1)
            {
                AddDataMenu = false;
                ImageData = string.Empty;
                StatsText = SetStatsText(ImageDataList[SelectID]);

                if (GUILayout.Button("Delete", GUILayout.ExpandHeight(false)))
                {
                    ImageDataList.RemoveAt(SelectID);
                    UnityEngine.Object.Destroy(MaterialList[SelectID]);
                    MaterialList.RemoveAt(SelectID--);
                    StatsText = string.Empty;
                }
            }

            if (AddDataMenu)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Set Name : ", GUILayout.MaxWidth(100));
                ImageName = GUILayout.TextField(ImageName);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("File Path : ", GUILayout.MaxWidth(100));
                SetFilePath = GUILayout.TextField(SetFilePath);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Load", GUILayout.ExpandHeight(false)))
                {
                    ImageData = Convert.ToBase64String(ImageConversion.EncodeToPNG(new WWW(SetFilePath).texture));
                    StatsText = SetStatsText(ImageData);
                }

                if (ImageData != string.Empty && GUILayout.Button("Add To List", GUILayout.ExpandHeight(false)))
                {
                    ImageDataList.Add(ImageData);
                    MaterialList.Add(PNGTextureConstruct.MaterialConstruct(ImageData, ImageName));
                    ImageData = string.Empty;
                    StatsText = string.Empty;
                    AddDataMenu = false;
                    SyncroniseDataUpLoad();
                }
            }
            
            GUILayout.EndVertical();

            if (!AddDataMenu && GUILayout.Button("Add New Data", GUILayout.Height(40)))
            {
                SelectID = -1;
                StatsText = string.Empty;
                AddDataMenu = true;
            }

            bool result = GuiCommon.DisplayCloseButton((int)Panel1.width);
            GUILayout.EndArea();

            Gui_ImageDataList();
            return result;
        }

        private void Gui_ImageDataList()
        {
            Rect Panel = new Rect(0f, 0f, 250f, 800f);
            GUILayout.BeginArea(Panel, new GUIContent("Image Data List"), GUI.skin.window);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(60));
            SearchText = GUILayout.TextField(SearchText);
            GUILayout.EndHorizontal();
            
            ListScroll = GUILayout.BeginScrollView(ListScroll, false, true);

            for (int Index = 0; Index < ImageDataList.Count; ++Index)
            {
                string Name = MaterialList[Index].name;

                if (ModGUI.NameSearch(Name, SearchText))
                {
                    bool flag2 = Index == SelectID;

                    if (GUILayout.Button(string.Format("ID : {0}\n{1}", Index, Name), (!flag2) ? "button" : "buttongreen", GUILayout.ExpandHeight(false)) && !flag2)
                    {
                        SelectID = Index;
                        GUISoundManager.GetSingleton().PlayBeep();
                    }
                }
            }

            GUILayout.EndScrollView();
            
            GUILayout.EndArea();
        }

        private string SetStatsText(string ImageData)
        {
            PNGTextureConstruct.TextureStats(ImageData);
            return string.Format("Width : {0}\nHeight : {1}\nBit Depth : {2}\nColor Type : {3}", PNGTextureConstruct.Width, PNGTextureConstruct.Height, PNGTextureConstruct.BitDepth, PNGTextureConstruct.ColorType);
        }
    }
}