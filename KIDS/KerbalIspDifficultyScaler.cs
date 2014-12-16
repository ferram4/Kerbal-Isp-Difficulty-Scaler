/*  This file is part of Kerbal Isp Difficulty Scaler.

    Kerbal Isp Difficulty Scaler is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kerbal Isp Difficulty Scaler is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Kerbal Isp Difficulty Scaler.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalIspDifficultyScaler
{

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class IspAdjustmentGUIController : UnityEngine.MonoBehaviour
    {
        private bool windowOpen = false;
        private Rect windowPos = new Rect(100, 100, 800, 465);
        private List<IspPreset> IspPresetList = new List<IspPreset>();
        private IspPreset selectedPreset = new IspPreset();

        private int selectedIndex = 0;
        private int _selectedIndex = 0;

        private string[] presetNames;
        private Vector2 scroll = new Vector2(0, 0);

        private string workingPresetName = "";
        private string workingPresetDescription = "";
        private string workingIspAtmMult = "";
        private string workingIspVacMult = "";

        private bool workingExtendToZero = false;
        private bool workingThrustCorrection = false;

        private float workingEngineRatingIspCutoff = 0;
        private bool workingEngineIspCutoffActive = false;

        private float workingEngineRatingThrustCutoff = 0;
        private bool workingEngineThrustCutoffActive = false;

        private bool incompatibilityDetected = false;
        private List<string> incompatibilities = new List<string>();

        //private IButton KIDSbutton;
        private ApplicationLauncherButton KIDSButtonStock = null;

        private static KSP.IO.PluginConfiguration config;

        public void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible())
                return;

            foreach (AssemblyLoader.LoadedAssembly loaded in AssemblyLoader.loadedAssemblies)
            {
                if (loaded.assembly.GetName().Name == "ArcturusThrustCorrector")
                    incompatibilities.Add("ArcturusThrustCorrector");

                if (incompatibilities.Count > 0)
                    incompatibilityDetected = true;
            }

            LoadPresets();
            if (IspPresetList.Count == 0)
                IspPresetList.Add(selectedPreset);

            selectedIndex = IspPresetList.IndexOf(selectedPreset);

            SelectPreset(selectedIndex);

            UpdatePresetNames();
            /*KIDSbutton = ToolbarManager.Instance.add("KerbalIspDifficultyScaler", "KIDSPresetButton");
            KIDSbutton.TexturePath = "KerbalIspDifficultyScaler/Textures/icon_button";
            KIDSbutton.ToolTip = "KIDS Preset Modifier";
            KIDSbutton.OnClick += (e) => windowOpen = !windowOpen;*/
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
        }

        void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready)
            {
                KIDSButtonStock = ApplicationLauncher.Instance.AddModApplication(
                    onAppLaunchToggleOn,
                    onAppLaunchToggleOff,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    (Texture)GameDatabase.Instance.GetTexture("KerbalIspDifficultyScaler/Textures/icon_button_stock", false));

            }
        }

        void onAppLaunchToggleOn()
        {
            windowOpen = true;
        }

        void onAppLaunchToggleOff()
        {
            windowOpen = false;
        }

        void DummyVoid() { }

        public void Start()
        {
            RenderingManager.AddToPostDrawQueue(0, new Callback(drawGUI));
            print("Isp Scaler GUI Loaded");
        }

        public void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = buttonStyle.focused.textColor = Color.white;
            buttonStyle.hover.textColor = buttonStyle.active.textColor = buttonStyle.onActive.textColor = Color.yellow;
            buttonStyle.onNormal.textColor = buttonStyle.onFocused.textColor = buttonStyle.onHover.textColor = Color.green;
            buttonStyle.padding = new RectOffset(4, 4, 4, 4);

            if (incompatibilityDetected)
            {
                windowPos = GUILayout.Window(250, new Rect(Screen.width / 2 - 250, Screen.height / 2 - 250, 500, 250), WarningWindow, "WARNING!", GUILayout.Width(500), GUILayout.Height(200), GUILayout.ExpandWidth(false));

            }
            else
            {
                //windowOpen = GUI.Toggle(new Rect(Screen.width - 80, 15, 65, 25), windowOpen, "KIDS", buttonStyle);

                if (windowOpen)
                    windowPos = GUILayout.Window(250, windowPos, MainWindow, "Kerbal Isp Difficulty Scaler v1.4.2", GUILayout.Width(800), GUILayout.Height(465), GUILayout.ExpandWidth(false));
            }
        }

        private void WarningWindow(int windowID)
        {            
            GUILayout.BeginVertical();
            GUILayout.Label("Kerbal Isp Difficulty Scaler has detected installed plugins that interfere with it.  It is advised that the following plugins are uninstalled to improve your gameplay experience.");
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(150));
            foreach (string s in incompatibilities)
            {
                if (s == "ArcturusThrustCorrector")
                {
                    GUILayout.Label(s);
                    GUILayout.Space(5);
                    GUILayout.Label("This plugin will cause engine thrust to be multiplied by the Isp scaling factors selected, causing engines to have absurdly low thrusts with realistic settings selected.  KIDS also includes the same features as Arcturus Thrust Controller, making it redundant.");
                }
                GUILayout.Space(5);
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(200);
            if (GUILayout.Button("Continue", GUILayout.Width(100)))
            {
                incompatibilities = null;
                incompatibilityDetected = false;
                scroll = new Vector2(0, 0);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        private void MainWindow(int windowID)
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = boxStyle.focused.textColor = Color.white;
            boxStyle.hover.textColor = boxStyle.active.textColor = Color.yellow;
            boxStyle.onNormal.textColor = boxStyle.onFocused.textColor = boxStyle.onHover.textColor = boxStyle.onActive.textColor = Color.green;
            boxStyle.padding = new RectOffset(4, 4, 4, 4);
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = buttonStyle.focused.textColor = Color.white;
            buttonStyle.hover.textColor = buttonStyle.active.textColor = buttonStyle.onActive.textColor = Color.yellow;
            buttonStyle.onNormal.textColor = buttonStyle.onFocused.textColor = buttonStyle.onHover.textColor = Color.green;
            buttonStyle.padding = new RectOffset(4, 4, 4, 4);

            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.alignment = TextAnchor.UpperCenter;

            GUIStyle textAreaStyle = new GUIStyle(GUI.skin.textArea);
            textAreaStyle.wordWrap = true;

            GUIStyle tooltipStyle = new GUIStyle(GUI.skin.label);
            tooltipStyle.normal.textColor = tooltipStyle.focused.textColor = tooltipStyle.hover.textColor = tooltipStyle.active.textColor = tooltipStyle.onNormal.textColor = tooltipStyle.onFocused.textColor = tooltipStyle.onHover.textColor = tooltipStyle.onActive.textColor = Color.white;

            GUILayout.BeginHorizontal();
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(300));

            selectedIndex = GUILayout.SelectionGrid(selectedIndex, presetNames, 1, buttonStyle);
            GUILayout.Space(20);
            if (GUILayout.Button(" + Add Preset + ", buttonStyle))
                AddPreset();

            if (selectedIndex != _selectedIndex)
            {
                UpdateSelectedPreset();
                _selectedIndex = selectedIndex;
                SelectPreset(selectedIndex);
                UpdatePresetNames();
                SavePresets();
            }
            GUILayout.EndScrollView();

            GUILayout.BeginVertical(GUILayout.Width(500), GUILayout.ExpandWidth(false));

            workingPresetName = GUILayout.TextField(workingPresetName, textFieldStyle);

            workingPresetDescription = GUILayout.TextArea(workingPresetDescription, textAreaStyle, GUILayout.MinHeight(150), GUILayout.Width(500));
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(300));
            GUILayout.Label("Atm Isp Multiplier: ");
            GUILayout.Label("Vac Isp Multiplier: ");
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(200));
            workingIspAtmMult = GUILayout.TextField(workingIspAtmMult, textFieldStyle);
            workingIspAtmMult = Regex.Replace(workingIspAtmMult, @"[^\d+-.]", "");

            workingIspVacMult = GUILayout.TextField(workingIspVacMult, textFieldStyle);
            workingIspVacMult = Regex.Replace(workingIspVacMult, @"[^\d+-.]", "");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            workingExtendToZero = GUILayout.Toggle(workingExtendToZero, "Extend Curve to Zero Isp");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            workingThrustCorrection = GUILayout.Toggle(workingThrustCorrection, "Thrust Varies with Isp");
            GUILayout.EndHorizontal();

            if (workingThrustCorrection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                GUILayout.BeginVertical();

                GUILayout.Label(new GUIContent("Vac- / Atm- Engine Rating Settings"));
                GUILayout.BeginHorizontal();
                workingEngineIspCutoffActive = GUILayout.Toggle(workingEngineIspCutoffActive, new GUIContent("Isp Cutoff (Before Scaling):", "Click to toggle Isp-based classification of vac- / atm-rated engines"), GUILayout.Width(250));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                if (workingEngineIspCutoffActive)
                {
                    GUILayout.Label("0 s", GUILayout.Width(50));
                    GUILayout.Label(new GUIContent("<  atm-rated  <", "These engines will produce their rated thrust at sea level"), GUILayout.Width(100));
                    GUILayout.Label(new GUIContent(workingEngineRatingIspCutoff + " s"), GUILayout.Width(45));
                    GUILayout.Label(new GUIContent("<  vac-rated  <", "These engines will produce their rated thrust in vacuum"), GUILayout.Width(100));
                    GUILayout.Label("800 s", GUILayout.Width(50));
                }
                else
                {
                    GUILayout.Space(150);
                    GUILayout.Label("N/A");
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                //GUILayout.BeginArea(new Rect(), new GUIContent("", "Engines with Isps below the selected value will produce their stated thrust at sea level; engines with Isps above the selected value will produce their max thrust in vacuum; both types will have greater thrust in vacuum than in an atmosphere"));
                if (workingEngineIspCutoffActive)
                {
                    workingEngineRatingIspCutoff = GUILayout.HorizontalSlider(workingEngineRatingIspCutoff, 100, 800, GUILayout.Width(335));
                    workingEngineRatingIspCutoff = Mathf.RoundToInt(workingEngineRatingIspCutoff);
                }
                else
                    GUILayout.HorizontalSlider(450, 100, 800, GUILayout.Width(250));

                //GUILayout.EndArea();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                workingEngineThrustCutoffActive = GUILayout.Toggle(workingEngineThrustCutoffActive, new GUIContent("Thrust Cutoff", "Click to toggle thrust-based classification of vac- / atm-rated engines"), GUILayout.Width(250));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                if (workingEngineThrustCutoffActive)
                {
                    GUILayout.Label("0 kN", GUILayout.Width(50));
                    GUILayout.Label(new GUIContent("<  vac-rated  <", "These engines will produce their rated thrust in vacuum"), GUILayout.Width(100));
                    GUILayout.Label(new GUIContent(workingEngineRatingThrustCutoff + " kN"), GUILayout.Width(45));
                    GUILayout.Label(new GUIContent("<  atm-rated  <", "These engines will produce their rated thrust at sea level"), GUILayout.Width(100));
                    GUILayout.Label("2000 kN", GUILayout.Width(50));
                }
                else
                {
                    GUILayout.Space(150);
                    GUILayout.Label("N/A");
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                //GUILayout.BeginArea(new Rect(), new GUIContent("", "Engines with max thrusts below the selected value will produce their stated thrust in vacuum; engines with max thrusts above the selected value will produce their max thrust at sea level; both types will have greater thrust in vacuum than in an atmosphere"));
                if (workingEngineThrustCutoffActive)
                {
                    workingEngineRatingThrustCutoff = GUILayout.HorizontalSlider(workingEngineRatingThrustCutoff, 0, 2000, GUILayout.Width(335));
                    workingEngineRatingThrustCutoff = Mathf.RoundToInt(workingEngineRatingThrustCutoff);
                }
                else
                    GUILayout.HorizontalSlider(1000, 0, 2000, GUILayout.Width(250));
                //GUILayout.EndArea();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

            }


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete This Preset", buttonStyle))
            {
                RemovePreset(selectedIndex);
                SavePresets();
            }
/*            if (GUILayout.Button("Update and Save This Preset", buttonStyle))
            {
                UpdateSelectedPreset();
                IspPresetList[selectedIndex] = selectedPreset;
                UpdatePresetNames();
                SavePresets();
            }*/

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (GUI.tooltip != "")
            {
                GUI.Label(new Rect(Input.mousePosition.x - windowPos.x + 15, (Screen.height - Input.mousePosition.y) - windowPos.y + 15, 250, 100), GUI.tooltip, tooltipStyle);
            }

            GUI.DragWindow();
        }

        private void UpdateSelectedPreset()
        {
            float ispAtm, ispVac;
            ispAtm = Convert.ToSingle(workingIspAtmMult);
            ispVac = Convert.ToSingle(workingIspVacMult);

            if (ispAtm < 0.01f)
            {
                ispAtm = 0.01f;
                workingIspAtmMult = "0.01";
            }
            if (ispVac < 0.01f)
            {
                ispVac = 0.01f;
                workingIspVacMult = "0.01";
            }
            //These reduce the amount of memory needed to toggle cutoffs
            if (!workingEngineIspCutoffActive)
                workingEngineRatingIspCutoff = -1;
            if (!workingEngineThrustCutoffActive)
                workingEngineRatingThrustCutoff = -1;
            selectedPreset.UpdatePreset(workingPresetName, workingPresetDescription, ispAtm, ispVac, workingExtendToZero, workingThrustCorrection, workingEngineRatingIspCutoff, workingEngineRatingThrustCutoff);
        }

        private void AddPreset()
        {
            IspPresetList.Add(new IspPreset());
            UpdatePresetNames();
        }

        private void RemovePreset(int index)
        {
            IspPresetList.Remove(IspPresetList[index]);
            UpdatePresetNames();
            selectedIndex--;
        }

        private void UpdatePresetNames()
        {
            presetNames = new string[IspPresetList.Count];

            for (int i = 0; i < presetNames.Length; i++)
                presetNames[i] = IspPresetList[i].presetName;
        }

        private void SelectPreset(int index)
        {
            selectedPreset = IspPresetList[index];

            workingEngineRatingIspCutoff = selectedPreset.ratingIspCutoff;
            workingEngineRatingThrustCutoff = selectedPreset.ratingThrustCutoff;

            if (workingEngineRatingIspCutoff == -1)
            {
                workingEngineIspCutoffActive = false;
                workingEngineRatingIspCutoff = 0;
            }
            else
                workingEngineIspCutoffActive = true;

            if (workingEngineRatingThrustCutoff == -1)
            {
                workingEngineThrustCutoffActive = false;
                workingEngineRatingThrustCutoff = 0;
            }
            else
                workingEngineThrustCutoffActive = true;

            workingExtendToZero = selectedPreset.extendToZero;
            workingThrustCorrection = selectedPreset.thrustCorrection;

            workingIspAtmMult = selectedPreset.atmIspMult.ToString();
            workingIspVacMult = selectedPreset.vacIspMult.ToString();

            workingPresetDescription = selectedPreset.presetDescription;
            workingPresetName = selectedPreset.presetName;
        }

        private void LoadPresets()
        {
            config = KSP.IO.PluginConfiguration.CreateForType<IspAdjustmentGUIController>();
            config.load();
            int presetNum = 0;
            do
            {
                string tmpName;
                tmpName = config.GetValue("IspPreset" + presetNum + "Name", "NothingHere");
                //IspPreset tmp = config.GetValue("IspPreset" + presetNum, new IspPreset("NothingHere", "", 1, 1, false));
                if (tmpName == "NothingHere")
                    break;

                IspPreset tmp = new IspPreset();
                tmp.presetName = tmpName;
                tmp.presetDescription = config.GetValue<string>("IspPreset" + presetNum + "Description");
                tmp.vacIspMult = Convert.ToSingle(config.GetValue<string>("IspPreset" + presetNum + "Vac"));
                tmp.atmIspMult = Convert.ToSingle(config.GetValue<string>("IspPreset" + presetNum + "Atm"));

                tmp.extendToZero = config.GetValue<bool>("IspPreset" + presetNum + "Extend");
                tmp.thrustCorrection = config.GetValue<bool>("IspPreset" + presetNum + "ThrustCorrection");

                tmp.ratingIspCutoff = Convert.ToSingle(config.GetValue<string>("IspPreset" + presetNum + "RatingIspCutoff", "385"));
                tmp.ratingThrustCutoff = Convert.ToSingle(config.GetValue<string>("IspPreset" + presetNum + "RatingThrustCutoff", "300"));


                presetNum++;
                IspPresetList.Add(tmp);

            } while (true);

            string gameName = HighLogic.CurrentGame.Title;

            int selectedPresetNum = Convert.ToInt32(config.GetValue("SelectedPreset" + gameName, "0"));

            selectedPreset = IspPresetList[selectedPresetNum];

            print("Presets Loaded...");
        }

        private void SavePresets()
        {
            int presetNum = 0;
            string selectedPresetNum = "0";
            foreach (IspPreset p in IspPresetList)
            {
                //config.SetValue("IspPreset" + presetNum, p);
                
                config.SetValue("IspPreset" + presetNum + "Name", p.presetName);
                config.SetValue("IspPreset" + presetNum + "Description", p.presetDescription);
                config.SetValue("IspPreset" + presetNum + "Vac", p.vacIspMult.ToString());
                config.SetValue("IspPreset" + presetNum + "Atm", p.atmIspMult.ToString());

                config.SetValue("IspPreset" + presetNum + "Extend", p.extendToZero);
                config.SetValue("IspPreset" + presetNum + "ThrustCorrection", p.thrustCorrection);

                config.SetValue("IspPreset" + presetNum + "RatingIspCutoff", p.ratingIspCutoff.ToString());
                config.SetValue("IspPreset" + presetNum + "RatingThrustCutoff", p.ratingThrustCutoff.ToString());


                if (p == selectedPreset)
                    selectedPresetNum = presetNum.ToString();
                presetNum++;
            }

            string gameName = HighLogic.CurrentGame.Title;
            
            config.SetValue("SelectedPreset" + gameName, selectedPresetNum);

            config.save();
            print("Presets Saved...");
        }

        private void OnDestroy()
        {
            if (!CompatibilityChecker.IsAllCompatible())
                return;

            UpdateSelectedPreset();
            SavePresets();
            //KIDSbutton.Destroy();
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            if (KIDSButtonStock != null)
                ApplicationLauncher.Instance.RemoveModApplication(KIDSButtonStock);

        }
    }

    public class IspPreset
    {
        public string presetName;
        public string presetDescription;

        public float atmIspMult;
        public float vacIspMult;

        public float ratingIspCutoff;
        public float ratingThrustCutoff;


        public bool extendToZero;
        public bool thrustCorrection;

        public void UpdatePreset(string name, string description, float atm, float vac, bool extend, bool thrust, float ratingIsp, float ratingThrust)
        {
            presetName = name;
            presetDescription = description;

            atmIspMult = atm;
            vacIspMult = vac;

            extendToZero = extend;
            thrustCorrection = thrust;

            ratingIspCutoff = ratingIsp;
            ratingThrustCutoff = ratingThrust;
        }

        public IspPreset()
        {
            UpdatePreset("New Preset", "Describe this preset here!", 1, 1, false, false, -1, -1);
        }

        public IspPreset(string name, string description, float atm, float vac, bool extend, bool thrust, float ratingIsp, float ratingThrust)
        {
            UpdatePreset(name, description, atm, vac, extend, thrust, ratingIsp, ratingThrust);
        }
    }


    public class EngineData
    {
        public float mDot = 0;
        public bool doNotCorrect = false;

        public EngineData(ModuleEngines e, float ispCutoff, float thrustCutoff)
        {
            //This makes sure that jet engines are handled differently
            foreach (Propellant p in e.propellants)
            {
                if (p.name == "IntakeAir")
                {
                    doNotCorrect = true;
                    break;
                }
            }

            if (doNotCorrect)
                return;

            //Both cutoffs enabled, both must be satisfied for atm rating
            if (ispCutoff != -1 && thrustCutoff != -1)
            {
                if ((e.atmosphereCurve.Evaluate(0) <= ispCutoff && e.maxThrust >= thrustCutoff))
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(1) * e.g);
                }
                else
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(0) * e.g);
                }
            }
            //No cutoffs; all engines vacuum rated
            else if(ispCutoff == -1 && thrustCutoff == -1)
            {

                mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(0) * e.g);

            }
            //No Isp Cutoff, but there is a thrust cutoff
            else if (ispCutoff == -1)
            {
                if (e.maxThrust >= thrustCutoff)
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(1) * e.g);
                }
                else
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(0) * e.g);
                }
            }
            //No thrust cutoff, but there is an Isp cutoff
            else
            {
                if (e.atmosphereCurve.Evaluate(0) <= ispCutoff)
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(1) * e.g);
                }
                else
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(0) * e.g);
                }
            }
        }

        public EngineData(ModuleEnginesFX e, float ispCutoff, float thrustCutoff)
        {
            //This makes sure that jet engines are handled differently
            foreach (Propellant p in e.propellants)
                if (p.name == "IntakeAir")
                {
                    doNotCorrect = true;
                    break;
                }

            if (doNotCorrect)
                return;

            //Both cutoffs enabled, both must be satisfied for atm rating
            if (ispCutoff != -1 && thrustCutoff != -1)
            {
                if ((e.atmosphereCurve.Evaluate(0) <= ispCutoff && e.maxThrust >= thrustCutoff))
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(1) * e.g);
                }
                else
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(0) * e.g);
                }
            }
            //No cutoffs; all engines vacuum rated
            else if (ispCutoff == -1 && thrustCutoff == -1)
            {

                mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(0) * e.g);

            }
            //No Isp Cutoff, but there is a thrust cutoff
            else if (ispCutoff == -1)
            {
                if (e.maxThrust >= thrustCutoff)
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(1) * e.g);
                }
                else
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(0) * e.g);
                }
            }
            //No thrust cutoff, but there is an Isp cutoff
            else
            {
                if (e.atmosphereCurve.Evaluate(0) <= ispCutoff)
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(1) * e.g);
                }
                else
                {
                    mDot = e.maxThrust / (e.atmosphereCurve.Evaluate(0) * e.g);
                }
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class IspEditorAdjuster : UnityEngine.MonoBehaviour
    {
        Dictionary<PartModule, EngineData> enginesUpdated = new Dictionary<PartModule, EngineData>();
        List<PartModule> otherModulesUpdated = new List<PartModule>();

        float ispMultiplierVac = 1;
        float ispMultiplierAtm = 1;

        float ispCutoff = -1;
        float thrustCutoff = -1;

        bool extendToZeroIsp = false;
        bool thrustCorrection = false;

        public void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible())
            {
                this.enabled = false;
                return;
            }
            KerbalIspDifficultyScalerUtils.GetIspMultiplier(out ispMultiplierVac, out ispMultiplierAtm, out extendToZeroIsp, out thrustCorrection, out ispCutoff, out thrustCutoff);
            KerbalIspDifficultyScalerUtils.ModularFuelsIntegration(ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp);
        }

        
        public void FixedUpdate()
        {
            List<PartModule> removeOtherList = new List<PartModule>();
            List<PartModule> removeEngineList = new List<PartModule>();
            if (EditorLogic.RootPart)
            {
                KerbalIspDifficultyScalerUtils.ModifyEngines(EditorLogic.SortedShipList, enginesUpdated, otherModulesUpdated, ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp, ispCutoff, thrustCutoff);
            }
            foreach (KeyValuePair<PartModule, EngineData> engine in enginesUpdated)
            {
                if (!engine.Key)
                    removeEngineList.Add(engine.Key);
            }


            foreach (PartModule m in otherModulesUpdated)
            {
                if (!m)
                    removeOtherList.Add(m);
            }

            foreach (PartModule m in removeEngineList)
                enginesUpdated.Remove(m);
            foreach (PartModule m in removeOtherList)
                otherModulesUpdated.Remove(m);

            
        }

    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class IspFlightAdjuster : UnityEngine.MonoBehaviour
    {
        List<Vessel> vesselsLoadedList = new List<Vessel>();
        Dictionary<PartModule, EngineData> enginesUpdated = new Dictionary<PartModule, EngineData>();
        List<PartModule> otherModulesUpdated = new List<PartModule>();
        float ispMultiplierVac = 1;
        float ispMultiplierAtm = 1;

        float ispCutoff = -1;
        float thrustCutoff = -1;

        bool extendToZeroIsp = false;
        bool thrustCorrection = false;

        public void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible())
            {
                this.enabled = false;
                return;
            }

            KerbalIspDifficultyScalerUtils.GetIspMultiplier(out ispMultiplierVac, out ispMultiplierAtm, out extendToZeroIsp, out thrustCorrection, out ispCutoff, out thrustCutoff);
            KerbalIspDifficultyScalerUtils.ModularFuelsIntegration(ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp);
        }

        public void FixedUpdate()
        {
            if (FlightGlobals.ready)
            {
                List<Vessel> removeVesselList = new List<Vessel>();
                List<PartModule> removeEngineList = new List<PartModule>();
                List<PartModule> removeOtherList = new List<PartModule>();

                foreach (Vessel v in FlightGlobals.Vessels)
                {
                    if(!vesselsLoadedList.Contains(v))
                        KerbalIspDifficultyScalerUtils.ModifyEngines(v.Parts, enginesUpdated, otherModulesUpdated, ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp, ispCutoff, thrustCutoff);
                    if (v.loaded && !vesselsLoadedList.Contains(v))
                    {
                        vesselsLoadedList.Add(v);
                    }
                    else if (!v.loaded && vesselsLoadedList.Contains(v))
                    {
                        removeVesselList.Add(v);
                    }
                }

                foreach (KeyValuePair<PartModule, EngineData> engine in enginesUpdated)
                {
                    if (removeVesselList.Contains(engine.Key.vessel) || !engine.Key)
                        removeEngineList.Add(engine.Key);

                    else if (thrustCorrection && !engine.Value.doNotCorrect)
                    {
                        if (engine.Key is ModuleEngines)
                        {
                            ModuleEngines e = engine.Key as ModuleEngines;
                            float currentIsp = e.atmosphereCurve.Evaluate((float)engine.Key.vessel.staticPressure);

                            e.maxThrust = engine.Value.mDot * e.g * currentIsp;
                        }
                        else if (engine.Key is ModuleEnginesFX)
                        {
                            ModuleEnginesFX e = engine.Key as ModuleEnginesFX;
                            float currentIsp = e.atmosphereCurve.Evaluate((float)engine.Key.vessel.staticPressure);

                            e.maxThrust = engine.Value.mDot * e.g * currentIsp;
                        }
                    }
                }


                foreach (PartModule m in otherModulesUpdated)
                {
                    if (removeVesselList.Contains(m.vessel) || !m)
                        removeOtherList.Add(m);
                }

                foreach (Vessel v in removeVesselList)
                    vesselsLoadedList.Remove(v);
                foreach (PartModule m in removeEngineList)
                    enginesUpdated.Remove(m);
                foreach (PartModule m in removeOtherList)
                    otherModulesUpdated.Remove(m);
            }
        }
    }
        

    public static class KerbalIspDifficultyScalerUtils
    {
        public static void ModifyEngines(List<Part> PartList, Dictionary<PartModule, EngineData> enginesUpdated, List<PartModule> otherModulesUpdated, float ispMultiplierVac, float ispMultiplierAtm, bool extendToZeroIsp, float ispCutoff, float thrustCutoff)
        {
            foreach (Part p in PartList)
            {
                if (p.Modules.Contains("ModuleEngineConfigs"))
                    continue;
                        
                foreach (PartModule m in p.Modules)
                    if (m.GetType().ToString() == "HydraEngineController")
                    {
                        //This one has two engine configs, primaryEngine and secondaryEngine
                        if (!otherModulesUpdated.Contains(m))
                        {
                            Type engineType = m.GetType();
                            ConfigNode engine = (ConfigNode)engineType.GetField("primaryEngine").GetValue(m);

                            ModifyEngineConfigNode(engine, ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp);
                            engineType.GetField("primaryEngine").SetValue(m, engine);

                            engine = (ConfigNode)engineType.GetField("secondaryEngine").GetValue(m);

                            ModifyEngineConfigNode(engine, ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp);
                            engineType.GetField("secondaryEngine").SetValue(m, engine);

                            MethodInfo switchEngine = engineType.GetMethod("SwitchEngine");
                            switchEngine.Invoke(null, null);    //We do this to update the engine state
                            switchEngine.Invoke(null, null);    //And again to swithc back to the original engine

                            otherModulesUpdated.Add(m);
                        }
                    }
                    else if (m is ModuleEngines)
                    {
                        ModuleEngines e = m as ModuleEngines;
                        if (!enginesUpdated.ContainsKey(e))
                        {
                            e.atmosphereCurve = ModifyCurveKeys(e.atmosphereCurve, ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp);
                            EngineData data = new EngineData(e, ispCutoff * ispMultiplierVac, thrustCutoff);

                            enginesUpdated.Add(m, data);
                        }
                    }
                    else if (m is ModuleEnginesFX)
                    {
                        ModuleEnginesFX e = m as ModuleEnginesFX;
                        if (!enginesUpdated.ContainsKey(e))
                        {
                            e.atmosphereCurve = ModifyCurveKeys(e.atmosphereCurve, ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp);
                            EngineData data = new EngineData(e, ispCutoff * ispMultiplierVac, thrustCutoff);

                            enginesUpdated.Add(m, data);
                        }
                    }
                    else if (m is ModuleRCS)
                    {
                        ModuleRCS r = m as ModuleRCS;
                        if (!otherModulesUpdated.Contains(m))
                        {
                            r.atmosphereCurve = ModifyCurveKeys(r.atmosphereCurve, ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp);
                            otherModulesUpdated.Add(m);
                        }
                    }
            }
        }

        public static void ModularFuelsIntegration(float ispMultiplierVac, float ispMultiplierAtm, bool extendToZeroIsp)
        {
            string RF_MFS_assemblyName = "";
            foreach (AssemblyLoader.LoadedAssembly loaded in AssemblyLoader.loadedAssemblies)
            {
                if (loaded.assembly.GetName().Name == "RealFuels")
                {
                    RF_MFS_assemblyName = loaded.assembly.FullName;
                }
            }

            if (RF_MFS_assemblyName == "")
                return;
            
            Type ModuleEngineConfigsType = Type.GetType("RealFuels.ModuleEngineConfigs, " + RF_MFS_assemblyName);
            if (ModuleEngineConfigsType == null)
                return;

            ModuleEngineConfigsType.GetField("ispSLMult").SetValue(null, ispMultiplierAtm);
            ModuleEngineConfigsType.GetField("ispVMult").SetValue(null, ispMultiplierVac);
            ModuleEngineConfigsType.GetField("correctThrust").SetValue(null, extendToZeroIsp);


            Debug.Log("MFS / RF Integration; IspSLMult: " + (float)ModuleEngineConfigsType.GetField("ispSLMult").GetValue(null) +
                " IspVMult: " + (float)ModuleEngineConfigsType.GetField("ispVMult").GetValue(null) +
                " correctThrust: " + (bool)ModuleEngineConfigsType.GetField("correctThrust").GetValue(null));
        }

        public static void ModifyEngineConfigNode(ConfigNode engine, float ispMultiplierVac, float ispMultiplierAtm, bool extendToZeroIsp)
        {
            ConfigNode atmoCurve = engine.GetNode("atmosphereCurve");

            FloatCurve engineCurve = ModifyCurveKeys(atmoCurve, ispMultiplierVac, ispMultiplierAtm, extendToZeroIsp);
            atmoCurve = new ConfigNode("atmosphereCurve");
            engineCurve.Save(atmoCurve);
            engine.RemoveNode("atmosphereCurve");
            engine.AddNode(atmoCurve);
        }

        public static FloatCurve ModifyCurveKeys(ConfigNode tempNode, float vacMult, float atmMult, bool extendToZero)
        {
            FloatCurve initialCurve = new FloatCurve();

            initialCurve.Load(tempNode);

            string[] keyStrings = tempNode.GetValues("key");

            float maxTime, ispAtMaxTime, secondTime, ispAtSecondTime, maxPressure;
            maxTime = ispAtMaxTime = secondTime = ispAtSecondTime = maxPressure = 0;
            FloatCurve newAtmosphereCurve = new FloatCurve();

            maxTime = initialCurve.maxTime;

            for (int i = 0; i < keyStrings.Length; i++)
            {
                string[] splitKey = keyStrings[i].Split(' ');

                float scalar = vacMult + Convert.ToSingle(splitKey[0]) * (atmMult - vacMult);
                if (!extendToZero)
                    scalar = Mathf.Clamp(scalar, Mathf.Min(atmMult, vacMult), Mathf.Max(atmMult, vacMult));

                if (Convert.ToSingle(splitKey[0]) != 0)
                    newAtmosphereCurve.Add(Convert.ToSingle(splitKey[0]), Convert.ToSingle(splitKey[1]) * scalar, Convert.ToSingle(splitKey[2]) * scalar, Convert.ToSingle(splitKey[3]) * scalar);
                else
                    newAtmosphereCurve.Add(Convert.ToSingle(splitKey[0]), Convert.ToSingle(splitKey[1]) * scalar, 0, 0);

                if (i == keyStrings.Length - 2)
                {
                    secondTime = Convert.ToSingle(splitKey[0]);
                    ispAtSecondTime = Convert.ToSingle(splitKey[1]) * scalar;
                }

            }

            ispAtMaxTime = newAtmosphereCurve.Evaluate(maxTime);

            if (extendToZero && (ispAtSecondTime - ispAtMaxTime) >= 0.0001f)
            {
                maxPressure = maxTime + (0.01f - ispAtMaxTime) / (ispAtSecondTime - ispAtMaxTime) * (secondTime - maxTime);
                newAtmosphereCurve.Add(maxPressure, 0.01f, 0, 0);
            }

            return newAtmosphereCurve;
        }

        public static FloatCurve ModifyCurveKeys(FloatCurve initialCurve, float vacMult, float atmMult, bool extendToZero)
        {
            ConfigNode tempNode = new ConfigNode();

            initialCurve.Save(tempNode);

            string[] keyStrings = tempNode.GetValues("key");

            float maxTime, ispAtMaxTime, secondTime, ispAtSecondTime, maxPressure;
            maxTime = ispAtMaxTime = secondTime = ispAtSecondTime = maxPressure = 0;
            FloatCurve newAtmosphereCurve = new FloatCurve();

            maxTime = initialCurve.maxTime;

            for (int i = 0; i < keyStrings.Length; i++)
            {
                string[] splitKey = keyStrings[i].Split(' ');

                float scalar = vacMult + Convert.ToSingle(splitKey[0]) * (atmMult - vacMult);
                if (!extendToZero)
                    scalar = Mathf.Clamp(scalar, Mathf.Min(atmMult, vacMult), Mathf.Max(atmMult, vacMult));

                if (Convert.ToSingle(splitKey[0]) != 0)
                    newAtmosphereCurve.Add(Convert.ToSingle(splitKey[0]), Convert.ToSingle(splitKey[1]) * scalar, Convert.ToSingle(splitKey[2]) * scalar, Convert.ToSingle(splitKey[3]) * scalar);
                else
                    newAtmosphereCurve.Add(Convert.ToSingle(splitKey[0]), Convert.ToSingle(splitKey[1]) * scalar, 0, 0);

                if (i == keyStrings.Length - 2)
                {
                    secondTime = Convert.ToSingle(splitKey[0]);
                    ispAtSecondTime = Convert.ToSingle(splitKey[1]) * scalar;
                }
            }

            ispAtMaxTime = newAtmosphereCurve.Evaluate(maxTime);

            if (extendToZero && (ispAtSecondTime - ispAtMaxTime) >= 0.0001f)
            {
                maxPressure = maxTime + (0.01f - ispAtMaxTime) / (ispAtSecondTime - ispAtMaxTime) * (secondTime - maxTime);
                newAtmosphereCurve.Add(maxPressure, 0.01f, 0, 0);
            }

            return newAtmosphereCurve;
        }

        public static void GetIspMultiplier(out float ispMultiplierVac, out float ispMultiplierAtm, out bool extendToZeroIsp, out bool thrustCorrection, out float ispCutoff, out float thrustCutoff)
        {
            var config = KSP.IO.PluginConfiguration.CreateForType<IspAdjustmentGUIController>();
            config.load();

            string gameName = HighLogic.CurrentGame.Title;

            int selectedPresetNum = Convert.ToInt32(config.GetValue("SelectedPreset" + gameName, "-1"));

            if (selectedPresetNum >= 0)
            {
                ispMultiplierVac = Convert.ToSingle(config.GetValue<string>("IspPreset" + selectedPresetNum + "Vac"));
                ispMultiplierAtm = Convert.ToSingle(config.GetValue<string>("IspPreset" + selectedPresetNum + "Atm"));
                extendToZeroIsp = config.GetValue<bool>("IspPreset" + selectedPresetNum + "Extend");
                thrustCorrection = config.GetValue<bool>("IspPreset" + selectedPresetNum + "ThrustCorrection");

                ispCutoff = Convert.ToSingle(config.GetValue<string>("IspPreset" + selectedPresetNum + "RatingIspCutoff", "385"));
                thrustCutoff = Convert.ToSingle(config.GetValue<string>("IspPreset" + selectedPresetNum + "RatingThrustCutoff", "300"));

            }
            else
            {
                ispMultiplierAtm = ispMultiplierVac = 1;
                extendToZeroIsp = thrustCorrection = false;
                ispCutoff = -1;
                thrustCutoff = -1;
            }

            MonoBehaviour.print("Isp Vac Multiplier = " + ispMultiplierVac + "\n\rIsp Atm Multiplier = " + ispMultiplierAtm + "\n\rExtend to Zero Isp = " + extendToZeroIsp + "\n\rThrust Correction = " + thrustCorrection);

        }
    }
}
