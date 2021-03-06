﻿using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;
using System.Text.RegularExpressions;

namespace IngameScript
{
    partial class Program
    {
        public class KProperty
        {
            protected MyIni MyIni = new MyIni();
            protected Program program;

            public string color_default = "128,128,128,255";

            public float elevator_position_max;
            public float elevator_position_min;
            public float elevator_velocity_max;
            public float elevator_velocity_medium;
            public float elevator_velocity_min;

            public float locker_velocity;
            public float locker_position_min;
            public float locker_position_max;

            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                elevator_position_max = MyIni.Get("Elevator", "position_max").ToSingle(8.6f);
                elevator_position_min = MyIni.Get("Elevator", "position_min").ToSingle(1.1f);
                elevator_velocity_max = MyIni.Get("Elevator", "velocity_max").ToSingle(1f);
                elevator_velocity_medium = MyIni.Get("Elevator", "velocity_medium").ToSingle(0.5f);
                elevator_velocity_min = MyIni.Get("Elevator", "velocity_min").ToSingle(0.1f);

                locker_velocity = MyIni.Get("Locker", "velocity").ToSingle(0.4f);
                locker_position_min = MyIni.Get("Locker", "position_min").ToSingle(4.9f);
                locker_position_max = MyIni.Get("Locker", "position_max").ToSingle(8f);

                if (program.Me.CustomData.Equals(""))
                {
                    Save();
                }
            }

            public string Get(string section, string key, string default_value = "")
            {
                return MyIni.Get(section, key).ToString(default_value);
            }

            public int GetInt(string section, string key, int default_value = 0)
            {
                return MyIni.Get(section, key).ToInt32(default_value);
            }

            public Color GetColor(string section, string key, string default_value = null)
            {
                if (default_value == null) default_value = color_default;
                string colorValue = MyIni.Get(section, key).ToString(default_value);
                Color color = Color.Gray;
                // Find matches.
                //program.drawingSurface.WriteText($"{section}/{key}={colorValue}", true);
                if (!colorValue.Equals(""))
                {
                    string[] colorSplit = colorValue.Split(',');
                    color = new Color(int.Parse(colorSplit[0]), int.Parse(colorSplit[1]), int.Parse(colorSplit[2]), int.Parse(colorSplit[3]));
                }
                return color;
            }

            public void Save()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                MyIni.Set("Elevator", "position_max", elevator_position_max);
                MyIni.Set("Elevator", "position_min", elevator_position_min);
                MyIni.Set("Elevator", "velocity_max", elevator_velocity_max);
                MyIni.Set("Elevator", "velocity_medium", elevator_velocity_medium);
                MyIni.Set("Elevator", "velocity_min", elevator_velocity_min);

                MyIni.Set("Locker", "velocity", locker_velocity);
                MyIni.Set("Locker", "position_min", locker_position_min);
                MyIni.Set("Locker", "position_max", locker_position_max);

                program.Me.CustomData = MyIni.ToString();
            }
        }
    }
}
