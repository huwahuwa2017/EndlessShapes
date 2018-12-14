using BrilliantSkies.Ui.Tips;
using System;
using UnityEngine;

namespace EndlessShapes
{
    public class ModGUI
    {
        public static int Digit;

        public static int TextWidth = 100;

        private static int UpdateFrame;



        public static void DigitSlider()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Digit", GUILayout.Width(80));

            GUILayout.BeginVertical(GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            Digit = Mathf.Clamp((int)GUILayout.HorizontalSlider(Digit, -4, 2), -4, 2);
            GUILayout.EndVertical();

            Digit = int.Parse(GUILayout.TextField(Digit.ToString(), GUILayout.Width(100)));
            string TextPow = Mathf.Pow(10, Digit).ToString();
            GUILayout.Box(TextPow, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, rect.height), new GUIContent(string.Empty, null, "Digit"), "empty");

            if (GUI.tooltip == "Digit" || Input.GetKey("left shift"))
            {
                TipDisplayer TI = TipDisplayer.Instance;
                TI.TooltipDelay = 0;
                TI.TooltipFadeTime = 0;
                TI.ForceTip(new ToolTip(TextPow), TextPow);
                int TFC = Time.frameCount;
                
                if (UpdateFrame != TFC)
                {
                    float axis = Input.GetAxis("Mouse ScrollWheel");

                    if (axis > 0)
                    {
                        ++Digit;
                        UpdateFrame = TFC;
                    }
                    else if (axis < 0)
                    {
                        --Digit;
                        UpdateFrame = TFC;
                    }
                }
            }
        }

        public static float HorizontalSlider(string Name, float value, float min, float max, bool SignInversion)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Name, GUILayout.Width(TextWidth));

            GUILayout.BeginVertical(GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            value = GUILayout.HorizontalSlider(value, min, max);
            GUILayout.EndVertical();

            value = float.Parse(GUILayout.TextField(value.ToString(), GUILayout.Width(200)));

            if (SignInversion)
            {
                if (GUILayout.Button("+/-", GUILayout.Width(60), GUILayout.ExpandHeight(false))) value = -value;
            }
            else
            {
                GUILayout.Label("empty", GUILayout.Width(60));
            }

            if (GUILayout.Button("set 0", GUILayout.Width(60), GUILayout.ExpandHeight(false))) value = 0;
            GUILayout.EndHorizontal();

            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, rect.height), new GUIContent(string.Empty, null, Name), "empty");

            if (GUI.tooltip == Name && !Input.GetKey("left shift"))
            {
                float axis = Input.GetAxis("Mouse ScrollWheel");

                if (axis > 0)
                {
                    value += Mathf.Pow(10, Digit);
                }
                else if (axis < 0)
                {
                    value -= Mathf.Pow(10, Digit);
                }
            }

            return (float)Math.Round(value, 4);
        }

        public static bool NameSearch(string Name, string SearchText)
        {
            if (SearchText != string.Empty)
            {
                string[] STA = SearchText.Split(' ');

                foreach (string s in STA)
                {
                    if (!Name.ToLower().Contains(s.ToLower())) return false;
                }
            }

            return true;
        }
    }
}