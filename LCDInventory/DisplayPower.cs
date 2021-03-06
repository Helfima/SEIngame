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

namespace IngameScript
{
    partial class Program
    {
        public class DisplayPower
        {
            protected DisplayLcd DisplayLcd;

            private MyDefinitionId PowerDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");

            private bool enable = false;
            public DisplayPower(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }
            public void Load(MyIni MyIni)
            {
                enable = MyIni.Get("Power", "on").ToBoolean(false);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Power", "on", enable);
            }
            public Vector2 Draw(Drawing drawing, Vector2 position)
            {
                if (!enable) return position;
                BlockSystem<IMyTerminalBlock> producers = BlockSystem<IMyTerminalBlock>.SearchBlocks(DisplayLcd.program, block => block.Components.Has<MyResourceSourceComponent>());
                BlockSystem<IMyTerminalBlock> consummers = BlockSystem<IMyTerminalBlock>.SearchBlocks(DisplayLcd.program, block => block.Components.Has<MyResourceSinkComponent>());
                Dictionary<string, Power> outputs = new Dictionary<string, Power>();
                Power batteries_store = new Power() { Type = "Batteries" };
                outputs.Add("all", new Power() { Type = "All" });
                float current_input = 0f;
                float max_input = 0f;
                float width = 30f;
                StyleGauge style = new StyleGauge()
                {
                    Orientation = SpriteOrientation.Horizontal,
                    Fullscreen = true,
                    Width = width,
                    Height = width,
                    Padding = new StylePadding(0),
                    Round = false,
                    RotationOrScale = 0.5f
                };

                MySprite text = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Color = Color.DimGray,
                    Position = position + new Vector2(0, 0),
                    RotationOrScale = .6f,
                    FontId = drawing.Font,
                    Alignment = TextAlignment.LEFT

                };

                producers.ForEach(delegate (IMyTerminalBlock block)
                {
                    string type = block.BlockDefinition.SubtypeName;
                    if (block is IMyBatteryBlock)
                    {
                        IMyBatteryBlock battery = (IMyBatteryBlock)block;
                        batteries_store.AddCurrent(battery.CurrentStoredPower);
                        batteries_store.AddMax(battery.MaxStoredPower);
                    }
                    else
                    {
                        MyResourceSourceComponent resourceSource;
                        block.Components.TryGet<MyResourceSourceComponent>(out resourceSource);
                        if (resourceSource != null)
                        {
                            ListReader<MyDefinitionId> myDefinitionIds = resourceSource.ResourceTypes;
                            if (myDefinitionIds.Contains(PowerDefinitionId))
                            {
                                Power global_output = outputs["all"];
                                global_output.AddCurrent(resourceSource.CurrentOutputByType(PowerDefinitionId));
                                global_output.AddMax(resourceSource.MaxOutputByType(PowerDefinitionId));

                                if (!outputs.ContainsKey(type)) outputs.Add(type, new Power() { Type = type });
                                Power current_output = outputs[type];
                                current_output.AddCurrent(resourceSource.CurrentOutputByType(PowerDefinitionId));
                                current_output.AddMax(resourceSource.MaxOutputByType(PowerDefinitionId));
                            }
                        }
                    }
                });

                drawing.DrawGauge(position, outputs["all"].Current, outputs["all"].Max, style);

                foreach (KeyValuePair<string, Power> kvp in outputs)
                {
                    string title = kvp.Key;
                    
                    position += new Vector2(0, 40);
                    if (kvp.Key.Equals("all"))
                    {
                        text.Data = $"Global Generator\n Out: {Math.Round(kvp.Value.Current, 2)}MW / {Math.Round(kvp.Value.Max, 2)}MW";
                    }
                    else
                    {
                        text.Data = $"{title} (n={kvp.Value.Count})\n";
                        text.Data += $" Out: {Math.Round(kvp.Value.Current, 2)}MW / {Math.Round(kvp.Value.Max, 2)}MW";
                        text.Data += $" (Moy={kvp.Value.Moyen}MW)";
                    }
                    text.Position = position;
                    drawing.AddSprite(text);
                }

                position += new Vector2(0, 40);
                drawing.DrawGauge(position, batteries_store.Current, batteries_store.Max, style, true);
                position += new Vector2(0, 40);
                text.Data = $"Battery Store (n={batteries_store.Count})\n Store: {Math.Round(batteries_store.Current, 2)}MW / {Math.Round(batteries_store.Max, 2)}MW";
                text.Position = position;
                drawing.AddSprite(text);

                consummers.ForEach(delegate (IMyTerminalBlock block)
                {
                    if (block is IMyBatteryBlock)
                    {
                    }
                    else
                    {
                        MyResourceSinkComponent resourceSink;
                        block.Components.TryGet<MyResourceSinkComponent>(out resourceSink);
                        if (resourceSink != null)
                        {
                            ListReader<MyDefinitionId> myDefinitionIds = resourceSink.AcceptedResources;
                            if (myDefinitionIds.Contains(PowerDefinitionId))
                            {
                                max_input += resourceSink.RequiredInputByType(PowerDefinitionId);
                                current_input += resourceSink.CurrentInputByType(PowerDefinitionId);
                            }
                        }
                    }
                });
                position += new Vector2(0, 40);
                drawing.DrawGauge(position, current_input, max_input, style, true);
                position += new Vector2(0, 40);
                text.Data = $"Power In: {Math.Round(current_input, 2)}MW / {Math.Round(max_input, 2)}MW";
                text.Position = position;
                drawing.AddSprite(text);

                return position + new Vector2(0, 60);
            }
        }

        public class Power
        {
            public string Type;
            public float Current = 0f;
            public float Max = 0f;
            public int Count = 0;

            public void AddCurrent(float value)
            {
                Current += value;
                Count += 1;
            }
            public void AddMax(float value)
            {
                Max += value;
            }
            public double Moyen
            {
                get
                {
                    return Math.Round(Current / Count, 2);
                }
            }
        }
    }
}
