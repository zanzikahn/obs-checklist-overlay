using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace OBSChecklistEditor
{
    public class ConfigManager
    {
        private readonly string _configPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public ConfigManager(string configPath)
        {
            _configPath = configPath;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
        }

        public ChecklistConfig LoadConfig()
        {
            try
            {
                Logger.Log($"Loading config from: {_configPath}");
                
                if (!File.Exists(_configPath))
                {
                    Logger.Log("Config file not found, creating default");
                    return CreateDefaultConfig();
                }

                string json = File.ReadAllText(_configPath);
                var loaded = JsonSerializer.Deserialize<ChecklistConfig>(json, _jsonOptions);
                
                if (loaded != null)
                {
                    Logger.Log($"Config loaded successfully. Lists: {loaded.lists.Count}");
                    foreach (var kvp in loaded.lists)
                    {
                        Logger.Log($"  - {kvp.Key}: {kvp.Value.name} ({kvp.Value.items.Count} items)");
                    }
                }
                
                return loaded ?? CreateDefaultConfig();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading config from {_configPath}", ex);
                return CreateDefaultConfig();
            }
        }

        public bool SaveConfig(ChecklistConfig config)
        {
            try
            {
                Logger.Log($"Saving config to: {_configPath}");
                string json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(_configPath, json);
                Logger.Log("Config saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error saving config to {_configPath}", ex);
                return false;
            }
        }

        private ChecklistConfig CreateDefaultConfig()
        {
            return new ChecklistConfig
            {
                settings = new Settings
                {
                    sequentialMode = false,
                    autoScrollSpeed = 30,
                    itemHeight = 80,
                    activeListId = "list1",
                    activeListIds = new List<string> { "list1" },
                    overlayOpacity = 1.0,
                    autoScrollEnabled = false,
                    scrollViewportHeight = 600,
                    pauseTimeBottom = 3000,
                    reverseScroll = false,
                    alternateLists = false
                },
                theme = new Theme
                {
                    backgroundColor = "rgba(0, 0, 0, 0.7)",
                    textColor = "#ffffff",
                    progressBarColor = "#4CAF50",
                    progressBarBackground = "rgba(255, 255, 255, 0.2)",
                    checkboxColor = "#4CAF50",
                    fontFamily = "Arial, sans-serif",
                    fontSize = "18px",
                    borderRadius = "5px"
                },
                lists = new Dictionary<string, ChecklistData>
                {
                    ["list1"] = new ChecklistData
                    {
                        name = "Main Checklist",
                        items = new List<TaskItem>()
                    }
                }
            };
        }

        public string GetConfigPath()
        {
            return _configPath;
        }
    }
}
