using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using static ItemSearchPlugin.ClassExtensions;

namespace ItemSearchPlugin.Filters
{
    internal class RaceSexSearchFilter : SearchFilter
    {
        private int selectedOption;
        private int lastIndex;
        private readonly List<(string text, uint raceId, CharacterSex sex)> options;
        private readonly List<EquipRaceCategory> equipRaceCategories;

        public RaceSexSearchFilter(ItemSearchPluginConfig pluginConfig, IDataManager data) : base(pluginConfig)
        {
            equipRaceCategories = data.GetExcelSheet<EquipRaceCategory>().ToList();

            options = [(Loc.Localize("NotSelected", "Not Selected"), 0, CharacterSex.Female)];

            foreach (var race in data.GetExcelSheet<Race>().ToList())
            {
                if (race.RSEMBody.RowId > 0 && race.RSEFBody.RowId > 0)
                {
                    string male = string.Format(Loc.Localize("RaceSexMale", "Male {0}"), race.Masculine);
                    string female = string.Format(Loc.Localize("RaceSexFemale", "Female {0}"), race.Feminine);
                    options.Add((male, race.RowId, CharacterSex.Male));
                    options.Add((female, race.RowId, CharacterSex.Female));
                }
                else if (race.RSEMBody.RowId > 0)
                {
                    options.Add((race.Masculine.ToString(), race.RowId, CharacterSex.Male));
                }
                else if (race.RSEFBody.RowId > 0)
                {
                    options.Add((race.Feminine.ToString(), race.RowId, CharacterSex.Female));
                }
            }
        }

        public override string Name => "性别/种族";

        public override string NameLocalizationKey => "RaceSexSearchFilter";

        public override bool IsSet => selectedOption > 0;

        public override bool HasChanged
        {
            get
            {
                if (lastIndex == selectedOption) return false;
                lastIndex = selectedOption;
                return true;
            }
        }

        public override bool CheckFilter(Item item)
        {
            try
            {
                var (_, raceId, sex) = options[selectedOption];
                var erc = equipRaceCategories[item.EquipRestriction];
                return erc.AllowsRaceSex(raceId, sex);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.ToString());
                return true;
            }
        }

        public override void DrawEditor()
        {
            ImGui.BeginChild($"{NameLocalizationKey}Child", new Vector2(-1, 23 * ImGui.GetIO().FontGlobalScale), false,
                usingTags ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None);
            if (ClientState.LocalContentId != 0 && !usingTags)
            {
                ImGui.SetNextItemWidth(-80 * ImGui.GetIO().FontGlobalScale);
            }
            else
            {
                ImGui.SetNextItemWidth(-1);
            }

            ImGui.Combo("##RaceSexSearchFilter", ref selectedOption, options.Select(a => a.text).ToArray(),
                options.Count);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                selectedOption = nonTagSelection;
                usingTags = false;
                Modified = true;
            }

            if (ClientState.LocalContentId != 0 && !usingTags)
            {
                ImGui.SameLine();

                if (ImGui.SmallButton($"当前"))
                {
                    if (ClientState.LocalPlayer != null)
                    {
                        var race = ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Race];
                        var sex = ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Gender] == 0
                            ? CharacterSex.Male
                            : CharacterSex.Female;

                        for (var i = 0; i < options.Count; i++)
                        {
                            if (options[i].sex == sex && options[i].raceId == race)
                            {
                                selectedOption = i;
                                break;
                            }
                        }
                    }
                }
            }

            ImGui.EndChild();
        }


        private bool usingTags;

        private int nonTagSelection;

        public override void ClearTags()
        {
            if (usingTags)
            {
                selectedOption = nonTagSelection;
                usingTags = false;
                Modified = true;
            }
        }

        public override bool IsFromTag => usingTags;

        public override bool ParseTag(string tag)
        {
            var t = tag.ToLower().Trim();
            var selfTag = false;
            if (t == "self")
            {
                if (ClientState.LocalPlayer != null)
                {
                    var race = ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Race];
                    var sex = ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Gender] == 0
                        ? CharacterSex.Male
                        : CharacterSex.Female;

                    for (var i = 0; i < options.Count; i++)
                    {
                        if (options[i].sex == sex && options[i].raceId == race)
                        {
                            t = options[i].text.ToLower();
                            selfTag = true;
                            break;
                        }
                    }
                }
            }

            t = t.Replace(" ", "").Replace("'", "");

            for (var i = 1; i < options.Count; i++)
            {
                if (t == options[i].text.ToLower().Replace(" ", "").Replace("'", ""))
                {
                    if (!usingTags)
                    {
                        nonTagSelection = selectedOption;
                    }

                    usingTags = true;
                    selectedOption = i;
                    return !selfTag;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return options[selectedOption].text;
        }
    }
}