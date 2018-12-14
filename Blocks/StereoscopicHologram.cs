using BrilliantSkies.Common.CarriedObjects;
using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EndlessShapes.Blocks
{
    public class StereoscopicHologram : SyncroniseBlock
    {
        private enum MeshListType
        {
            FTD_Mesh_List,
            Unity_Mesh_List,
            PLY_Mesh_List
        }

        private enum MaterialListType
        {
            FTD_Material_List,
            Image_Material_List
        }



        private MeshListType SelectMeshListType = MeshListType.FTD_Mesh_List;

        private MaterialListType SelectMaterialListType = MaterialListType.FTD_Material_List;

        private GameObject ParentObject;

        private CarriedObjectReference GO;

        private Vector3 Position;

        private Vector3 Angle;

        private Vector3 Scale = new Vector3(2, 2, 2);

        private List<Mesh> MainMeshList;

        private List<Material> MainMaterialList;

        private List<string> MeshNameList;

        private List<string> MaterialNameList;

        private Mesh SelectMesh;

        private Material SelectMaterial;

        private Mesh CopyMesh;

        private Material CopyMaterial;

        private Color preColor;

        private Vector2 Scroll1;

        private Vector2 Scroll2;

        private string SearchText1 = string.Empty;

        private string SearchText2 = string.Empty;

        private string MeshExtraLine = string.Empty;

        private string MaterialExtraLine = string.Empty;

        private bool VertexColorIsOn;

        private bool BlockIsOn = true;

        private bool MeshIsOn = true;

        private int[] SelectIDMemory = new int[2];

        private int[] ListCountMemory = new int[] { 0, 0 };



        private void StartObject()
        {
            if (ParentObject == null)
            {
                base.Mesh = new Mesh();
                MeshDefinition MeshD = Configured.i.Meshes.Find(new Guid("1e73382b-e366-41ec-931e-e17196340657"));
                MaterialDefinition MaterialD = Configured.i.Materials.Find(new Guid("2dbba7ea-61a9-4d76-bcb6-12b927f0ea08"));

                ParentObject = new GameObject();
                ParentObject.transform.position = GameWorldPosition;
                ParentObject.transform.rotation = GameWorldRotation;
                ParentObject.AddComponent<MeshFilter>().sharedMesh = UnityEngine.Object.Instantiate(MeshD.Mesh);
                ParentObject.AddComponent<MeshRenderer>().sharedMaterial = UnityEngine.Object.Instantiate(MaterialD.Material);
                CarryThisWithUs(ParentObject);

                GO = CarryEmptyWithUs();
                GO.ObjectItself.transform.parent = ParentObject.transform;
                GO.ObjectItself.AddComponent<MeshFilter>();
                GO.ObjectItself.AddComponent<MeshRenderer>();
            }
        }

        private void UpdateTransform()
        {
            GO.ObjectItself.transform.localPosition = Position;
            GO.ObjectItself.transform.localRotation = Quaternion.Euler(Angle);
        }

        private void UpdateMainMeshList()
        {
            switch (SelectMeshListType)
            {
                case MeshListType.FTD_Mesh_List:
                    MainMeshList = Configured.i.Meshes.Components.Select(D => D.Mesh).ToList();
                    MeshNameList = Configured.i.Meshes.Components.Select(D => D.ComponentId.Name).ToList();
                    break;
                case MeshListType.Unity_Mesh_List:
                    MainMeshList = ((int[])Enum.GetValues(typeof(PrimitiveType))).Select(D => PrimitiveMesh.Create(D)).ToList();
                    MeshNameList = Enum.GetNames(typeof(PrimitiveType)).ToList();
                    break;
                case MeshListType.PLY_Mesh_List:
                    MainMeshList = new List<Mesh>();

                    foreach (PLYDataStorageBlock PDSB in PLYDataStorageBlock.PLYDataStorageBlockList)
                    {
                        if (PDSB.MainConstruct == MainConstruct) MainMeshList.AddRange(PDSB.MeshList);
                    }

                    MeshNameList = MainMeshList.Select(D => D.name).ToList();
                    break;
            }
        }

        private void UpdateMainMaterialList()
        {
            switch (SelectMaterialListType)
            {
                case MaterialListType.FTD_Material_List:
                    MainMaterialList = Configured.i.Materials.Components.Select(D => D.Material).ToList();
                    MaterialNameList = Configured.i.Materials.Components.Select(D => D.ComponentId.Name).ToList();
                    break;
                case MaterialListType.Image_Material_List:
                    MainMaterialList = new List<Material>();

                    foreach (ImageDataStorageBlock IDSB in ImageDataStorageBlock.ImageDataStorageBlockList)
                    {
                        if (IDSB.MainConstruct == MainConstruct) MainMaterialList.AddRange(IDSB.MaterialList);
                    }

                    MaterialNameList = MainMaterialList.Select(D => D.name).ToList();
                    break;
            }
        }

        private void SetSelectMeshID(int ID)
        {
            if (0 <= ID && ID < MainMeshList.Count) SelectMesh = MainMeshList[ID];
        }

        private void SetSelectMaterialID(int ID)
        {
            if (0 <= ID && ID < MainMaterialList.Count) SelectMaterial = MainMaterialList[ID];
        }

        private int GetSelectMeshID()
        {
            return MainMeshList.IndexOf(SelectMesh);
        }

        private int GetSelectMaterialID()
        {
            return MainMaterialList.IndexOf(SelectMaterial);
        }

        private void UpdateMesh(bool ListUpdate = true)
        {
            if (ListUpdate) UpdateMainMeshList();
            UnityEngine.Object.Destroy(CopyMesh);
            MeshExtraLine = string.Empty;

            if (GetSelectMeshID() != -1)
            {
                int GSMI = GetSelectMeshID();
                if (GSMI >= 0) MeshExtraLine = MeshNameList[GSMI];
                CopyMesh = UnityEngine.Object.Instantiate(SelectMesh);
                CopyMesh.MarkDynamic();
                MeshRemodeling.ChangeScale(CopyMesh, Scale);
                MeshRemodeling.ChangeColor(CopyMesh, GetColor(true, true), VertexColorIsOn);
                MeshRemodeling.Recalculate(CopyMesh);
            }

            GO.ObjectItself.GetComponent<MeshFilter>().sharedMesh = CopyMesh;
            ListCountMemory[0] = MainMeshList.Count;
        }

        private void UpdateMaterial(bool ListUpdate = true)
        {
            if (ListUpdate) UpdateMainMaterialList();
            UnityEngine.Object.Destroy(CopyMaterial);
            MaterialExtraLine = string.Empty;

            if (GetSelectMaterialID() != -1)
            {
                int GSMI = GetSelectMaterialID();
                if (GSMI >= 0) MaterialExtraLine = MaterialNameList[GSMI];
                CopyMaterial = UnityEngine.Object.Instantiate(SelectMaterial);
            }

            GO.ObjectItself.GetComponent<MeshRenderer>().sharedMaterial = CopyMaterial;
            ListCountMemory[1] = MainMaterialList.Count;
        }

        private void UpdateOnOff()
        {
            ParentObject.GetComponent<MeshRenderer>().enabled = BlockIsOn;
            GO.ObjectItself.GetComponent<MeshRenderer>().enabled = MeshIsOn;
        }

        private void UpdateColor()
        {
            Color NewColor = GetColor(true, true);

            if (preColor != NewColor)
            {
                MeshRemodeling.ChangeColor(ParentObject.GetComponent<MeshFilter>().sharedMesh, NewColor, VertexColorIsOn);
                if (GO.ObjectItself.GetComponent<MeshFilter>().sharedMesh != null)
                    MeshRemodeling.ChangeColor(GO.ObjectItself.GetComponent<MeshFilter>().sharedMesh, NewColor, VertexColorIsOn);
                preColor = NewColor;
            }
        }

        private void UpdateSEI()
        {
            StartObject();
            UpdateTransform();
            UpdateMainMeshList();
            UpdateMainMaterialList();
            SetSelectMeshID(SelectIDMemory[0]);
            SetSelectMaterialID(SelectIDMemory[1]);
            UpdateMesh(false);
            UpdateMaterial(false);
            UpdateColor();
            UpdateOnOff();
        }



        public override void SetExtraInfo(ExtraInfoArrayReadPackage v)
        {
            base.SetExtraInfo(v);

            if (v.FindDelimiterAndSpoolToIt(DelimiterType.BaseTier))
            {
                Position = new Vector3(v.GetNextFloat(), v.GetNextFloat(), v.GetNextFloat());
                Angle = new Vector3(v.GetNextFloat(), v.GetNextFloat(), v.GetNextFloat());
                Scale = new Vector3(v.GetNextFloat(), v.GetNextFloat(), v.GetNextFloat());
                SelectIDMemory[0] = v.GetNextInt();
                SelectIDMemory[1] = v.GetNextInt();
                SelectMeshListType = (MeshListType)v.GetNextInt();
                SelectMaterialListType = (MaterialListType)v.GetNextInt();
                VertexColorIsOn = v.GetNextBool();
                BlockIsOn = v.GetNextBool();
                MeshIsOn = v.GetNextBool();
            }
        }

        public override void GetExtraInfo(ExtraInfoArrayWritePackage v)
        {
            base.GetExtraInfo(v);

            v.AddDelimiterOpen(DelimiterType.BaseTier);
            v.WriteNextFloat(Position.x);
            v.WriteNextFloat(Position.y);
            v.WriteNextFloat(Position.z);
            v.WriteNextFloat(Angle.x);
            v.WriteNextFloat(Angle.y);
            v.WriteNextFloat(Angle.z);
            v.WriteNextFloat(Scale.x);
            v.WriteNextFloat(Scale.y);
            v.WriteNextFloat(Scale.z);
            v.WriteNextInt(GetSelectMeshID());
            v.WriteNextInt(GetSelectMaterialID());
            v.WriteNextInt((int)SelectMeshListType);
            v.WriteNextInt((int)SelectMaterialListType);
            v.WriteNextBool(VertexColorIsOn);
            v.WriteNextBool(BlockIsOn);
            v.WriteNextBool(MeshIsOn);
            v.AddDelimiterClose(DelimiterType.BaseTier);
        }



        public override void FinalOptionalInitialisationStage()
        {
            base.FinalOptionalInitialisationStage();
            UpdateSEI();
        }

        public override void StuffChangedSyncIt()
        {
            base.StuffChangedSyncIt();
            UpdateSEI();
        }

        public override void CheckStatus(IStatusUpdate updater)
        {
            base.CheckStatus(updater);
            UpdateMainMeshList();
            UpdateMainMaterialList();
            if (ListCountMemory[0] != MainMeshList.Count) UpdateMesh(false);
            if (ListCountMemory[1] != MainMaterialList.Count) UpdateMaterial(false);
            UpdateColor();
        }



        public override InteractionReturn Secondary()
        {
            UpdateColor();

            InteractionReturn interactionReturn = new InteractionReturn
            {
                SpecialNameField = "StereoscopicHologram"
            };

            interactionReturn.AddExtraLine(MeshExtraLine);
            interactionReturn.AddExtraLine(MaterialExtraLine);
            interactionReturn.AddExtraLine("Press <<Q>> to settings");
            return interactionReturn;
        }

        public override void Secondary(Transform T)
        {
            new GenericBlockGUI().ActivateGui(this, GuiActivateType.Standard);
        }

        public override bool ExtraGUI()
        {
            Gui_MeshDataList();
            Gui_MaterialDataList();
            Vector3 NSV = Vector3.zero;
            Vector3 NPV = Vector3.zero;
            Vector3 NAV = Vector3.zero;

            Rect Panel1 = new Rect(500f, 100f, 780f, 550f);
            GUILayout.BeginArea(Panel1, "StereoscopicHologram", GUI.skin.window);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            bool Flag0 = GUILayout.Toggle(BlockIsOn, "Block On/Off", GUILayout.Width(160f));
            bool Flag1 = GUILayout.Toggle(MeshIsOn, "Mesh On/Off", GUILayout.Width(160f));
            bool Frag2 = GUILayout.Toggle(VertexColorIsOn, "Vertex Color On/Off");
            GUILayout.EndHorizontal();

            ModGUI.TextWidth = 80;
            ModGUI.DigitSlider();
            GUILayout.Space(10);
            NSV.x = ModGUI.HorizontalSlider("Scale X", Scale.x, -100, 100, true);
            NSV.y = ModGUI.HorizontalSlider("Scale Y", Scale.y, -100, 100, true);
            NSV.z = ModGUI.HorizontalSlider("Scale Z", Scale.z, -100, 100, true);
            NPV.x = ModGUI.HorizontalSlider("Position X", Position.x, -100, 100, true);
            NPV.y = ModGUI.HorizontalSlider("Position Y", Position.y, -100, 100, true);
            NPV.z = ModGUI.HorizontalSlider("Position Z", Position.z, -100, 100, true);
            NAV.x = Mathf.DeltaAngle(0, ModGUI.HorizontalSlider("Angle X", Angle.x, -180, 180, true));
            NAV.y = Mathf.DeltaAngle(0, ModGUI.HorizontalSlider("Angle Y", Angle.y, -180, 180, true));
            NAV.z = Mathf.DeltaAngle(0, ModGUI.HorizontalSlider("Angle Z", Angle.z, -180, 180, true));
            bool result = (GuiCommon.DisplayCloseButton((int)Panel1.width)) ? true : false;
            GUILayout.EndArea();

            if (BlockIsOn != Flag0 || MeshIsOn != Flag1)
            {
                BlockIsOn = Flag0;
                MeshIsOn = Flag1;
                UpdateOnOff();
                SyncroniseDataUpLoad();
            }

            if (Scale != NSV || VertexColorIsOn != Frag2)
            {
                Scale = NSV;
                VertexColorIsOn = Frag2;
                UpdateMesh();
                SyncroniseDataUpLoad();
            }

            if (Position != NPV || Angle != NAV)
            {
                Position = NPV;
                Angle = NAV;
                UpdateTransform();
                SyncroniseDataUpLoad();
            }

            return result;
        }

        private void Gui_MeshDataList()
        {
            Rect Panel = new Rect(0f, 0f, 250f, 800f);
            GUILayout.BeginArea(Panel, SelectMeshListType.ToString(), GUI.skin.window);

            if (GUILayout.Button("Change Mesh List", GUILayout.Height(40)))
            {
                ++SelectMeshListType;
                if ((int)SelectMeshListType >= Enum.GetNames(typeof(MeshListType)).Count()) SelectMeshListType = 0;
                UpdateMesh();
                SyncroniseDataUpLoad();
            }
            
            if (Gui_1(MeshNameList, MainMeshList, ref SelectMesh, ref SearchText1, ref Scroll1))
            {
                UpdateMesh();
                SyncroniseDataUpLoad();
            }
        }

        private void Gui_MaterialDataList()
        {
            Rect Panel = new Rect(250f, 0f, 250f, 800f);
            GUILayout.BeginArea(Panel, SelectMaterialListType.ToString(), GUI.skin.window);

            if (GUILayout.Button("Change Material List", GUILayout.Height(40)))
            {
                ++SelectMaterialListType;
                if ((int)SelectMaterialListType >= Enum.GetNames(typeof(MaterialListType)).Count()) SelectMaterialListType = 0;
                UpdateMaterial();
                SyncroniseDataUpLoad();
            }
            
            if (Gui_1(MaterialNameList, MainMaterialList, ref SelectMaterial, ref SearchText2, ref Scroll2))
            {
                UpdateMaterial();
                SyncroniseDataUpLoad();
            }
        }

        private bool Gui_1<T>(List<string> NameList, List<T> ObjectList, ref T SelectObject, ref string Search, ref Vector2 Scroll) where T : UnityEngine.Object
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(60));
            Search = GUILayout.TextField(Search);
            GUILayout.EndHorizontal();

            Scroll = GUILayout.BeginScrollView(Scroll, false, true);
            bool B = false;

            for (int Index = 0; Index < NameList.Count; ++Index)
            {
                string Name = NameList[Index];

                if (ModGUI.NameSearch(Name, Search))
                {
                    bool Flag = SelectObject != ObjectList[Index];
                    string Text = string.Format("ID : {0}\n{1}", Index, Name);

                    if (GUILayout.Button(new GUIContent(Text), (Flag) ? "button" : "buttongreen", GUILayout.ExpandHeight(false)) && Flag)
                    {
                        SelectObject = ObjectList[Index];
                        B = true;
                        GUISoundManager.GetSingleton().PlayBeep();
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            return B;
        }
    }
}