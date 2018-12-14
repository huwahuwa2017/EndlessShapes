using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EndlessShapes.Blocks
{
    public class PLYDataStorageBlock : SyncroniseBlock
    {
        public static List<PLYDataStorageBlock> PLYDataStorageBlockList = new List<PLYDataStorageBlock>();

        public List<Mesh> MeshList = new List<Mesh>();

        private List<string> PLYDataList = new List<string>();

        private string PLYNameDelimiter = "<Delimiter 75930352>";

        private string PLYDataDelimiter = "<Delimiter 27604961>";

        private string PLYName = string.Empty;

        private string PLYData = string.Empty;

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
            PLYDataList = new List<string>(NewText.Split(new string[] { PLYDataDelimiter }, StringSplitOptions.None));
            List<string> NameList = (PLYDataList[0] == string.Empty) ? new List<string>() : new List<string>(PLYDataList[0].Split(new string[] { PLYNameDelimiter }, StringSplitOptions.None));
            PLYDataList.RemoveAt(0);

            for (int i = 0; i < PLYDataList.Count; ++i)
            {
                MeshList.Add(PLYMeshConstruct.MeshConstruct(PLYDataList[i], NameList[i]));
            }

            return string.Empty;
        }

        public override string GetText()
        {
            if (PLYDataList.Count == 0) return string.Empty;
            return String.Join(PLYNameDelimiter, MeshList.Select(M => M.name)) + PLYDataDelimiter + String.Join(PLYDataDelimiter, PLYDataList);
        }



        public override void StateChanged(IBlockStateChange change)
        {
            if (change.InitiatedOrInitiatedInUnrepairedState_OnlyCalledOnce)
            {
                BlockWithText = true;
                PLYDataStorageBlockList.Add(this);
            }
            else if (change.IsPerminentlyRemovedOrConstructDestroyed)
            {
                PLYDataStorageBlockList.Remove(this);
            }

            base.StateChanged(change);
        }



        public override InteractionReturn Secondary()
        {
            InteractionReturn interactionReturn = new InteractionReturn
            {
                SpecialNameField = "PLYDataStorageBlock",
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
            GUILayout.BeginArea(Panel1, "PLYDataStorageBlock", GUI.skin.window);

            GUILayout.BeginVertical(GUILayout.Height(490));
            GUILayout.Label("State");
            GUILayout.TextArea(StatsText, GUILayout.Height(130));

            if (SelectID != -1)
            {
                AddDataMenu = false;
                PLYData = string.Empty;
                StatsText = SetStatsText(PLYDataList[SelectID]);

                if (GUILayout.Button("Delete", GUILayout.ExpandHeight(false)))
                {
                    PLYDataList.RemoveAt(SelectID);
                    UnityEngine.Object.Destroy(MeshList[SelectID]);
                    MeshList.RemoveAt(SelectID--);
                    StatsText = string.Empty;
                }
            }

            if (AddDataMenu)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Set Name : ", GUILayout.MaxWidth(100));
                PLYName = GUILayout.TextField(PLYName);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("File Path : ", GUILayout.MaxWidth(100));
                SetFilePath = GUILayout.TextField(SetFilePath);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Load", GUILayout.ExpandHeight(false)))
                {
                    PLYData = new WWW(SetFilePath).text;
                    StatsText = SetStatsText(PLYData);
                }

                if (PLYData != string.Empty && GUILayout.Button("Add To List", GUILayout.ExpandHeight(false)))
                {
                    PLYDataList.Add(PLYData);
                    MeshList.Add(PLYMeshConstruct.MeshConstruct(PLYData, PLYName));
                    PLYData = string.Empty;
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

            Gui_PLYDataList();
            return result;
        }

        private void Gui_PLYDataList()
        {
            Rect Panel = new Rect(0f, 0f, 250f, 800f);
            GUILayout.BeginArea(Panel, new GUIContent("PLY Data List"), GUI.skin.window);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(60));
            SearchText = GUILayout.TextField(SearchText);
            GUILayout.EndHorizontal();

            ListScroll = GUILayout.BeginScrollView(ListScroll, false, true);

            for (int Index = 0; Index < PLYDataList.Count; ++Index)
            {
                string Name = MeshList[Index].name;

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
        
        private string SetStatsText(string PLYData)
        {
            PLYMeshConstruct.GetPLYState(PLYData);
            int VertexCount = PLYMeshConstruct.VertexCount;
            string Text = string.Format("Vertex Count : {0}\nPolygon Count : {1}\nUV : {2}\nVertex Color : {3}", VertexCount, PLYMeshConstruct.PolygonCount, PLYMeshConstruct.ReadUV, PLYMeshConstruct.ReadColor);
            if (VertexCount > 65535) Text = Text + "\n\nWarning : Vertex Count Maximum 65535";
            return Text;
        }
    }
}