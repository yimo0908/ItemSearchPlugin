using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin.Filters
{
    class PatchSearchFilter(ItemSearchPluginConfig config) : SearchFilter(config)
    {
        private bool altered;
        private List<Patch> selectedPatches = [];

        private class Patch
        {
            public int Id; // Auto-incrementing ID
            public int Index; // Original ID field
            public string? Name;
            public string? ShortName;
            public bool Expansion;
        }

        // List of patches under shared keys (by expansion name and shorthand)
        private static readonly List<Patch> ExpansionsPatches =
        [
            // Final Fantasy XIV (1.0)
            new Patch { Id = 0, Index = 0, Name = "选择一个版本", ShortName = "999" },

            // Final Fantasy XIV (1.0)
            new Patch { Id = 1, Index = 10, Name = "最终幻想14", ShortName = "1.0", Expansion = false },

            // A Realm Reborn (ARR)
            new Patch { Id = 2, Index = 20, Name = "重生之境", ShortName = "ARR", Expansion = true },
            new Patch { Id = 3, Index = 21, Name = "觉醒之境", ShortName = "2.1" },
            new Patch { Id = 4, Index = 22, Name = "混沌的漩涡", ShortName = "2.2" },
            new Patch { Id = 5, Index = 23, Name = "艾欧泽亚的守护者", ShortName = "2.3" },
            new Patch { Id = 6, Index = 24, Name = "寒冰的幻想", ShortName = "2.4" },
            new Patch { Id = 7, Index = 25, Name = "希望的灯火", ShortName = "2.5" },

            // Heavensward (HW)
            new Patch { Id = 8, Index = 30, Name = "苍穹之禁城", ShortName = "HW", Expansion = true },
            new Patch { Id = 9, Index = 31, Name = "苍穹之禁城", ShortName = "3.0" },
            new Patch { Id = 10, Index = 32, Name = "光与暗的分界", ShortName = "3.1" },
            new Patch { Id = 11, Index = 33, Name = "命运的齿轮", ShortName = "3.2" },
            new Patch { Id = 12, Index = 34, Name = "绝命怒嚎", ShortName = "3.3" },
            new Patch { Id = 13, Index = 35, Name = "灵魂继承者", ShortName = "3.4" },
            new Patch { Id = 14, Index = 36, Name = "命运的止境", ShortName = "3.5" },

            // Stormblood (SB)
            new Patch { Id = 15, Index = 40, Name = "红莲之狂潮", ShortName = "SB", Expansion = true },
            new Patch { Id = 16, Index = 41, Name = "红莲之狂潮", ShortName = "4.0" },
            new Patch { Id = 17, Index = 42, Name = "英雄归来", ShortName = "4.1" },
            new Patch { Id = 18, Index = 43, Name = "曙光微明", ShortName = "4.2" },
            new Patch { Id = 19, Index = 44, Name = "月下芳华", ShortName = "4.3" },
            new Patch { Id = 20, Index = 45, Name = "狂乱前奏", ShortName = "4.4" },
            new Patch { Id = 21, Index = 46, Name = "英雄挽歌", ShortName = "4.5" },

            // Shadowbringers (ShB)
            new Patch { Id = 22, Index = 50, Name = "暗影之逆焰", ShortName = "ShB", Expansion = true },
            new Patch { Id = 23, Index = 51, Name = "暗影之逆焰", ShortName = "5.0" },
            new Patch { Id = 24, Index = 52, Name = "纯白誓约、漆黑密约", ShortName = "5.1" },
            new Patch { Id = 25, Index = 53, Name = "追忆的凶星", ShortName = "5.2" },
            new Patch { Id = 26, Index = 54, Name = "水晶的残光", ShortName = "5.3" },
            new Patch { Id = 27, Index = 55, Name = "另一个未来", ShortName = "5.4" },
            new Patch { Id = 28, Index = 56, Name = "死斗至黎明", ShortName = "5.5" },

            // Endwalker (EW)
            new Patch { Id = 29, Index = 60, Name = "晓月之终途", ShortName = "EW", Expansion = true },
            new Patch { Id = 30, Index = 61, Name = "晓月之终途", ShortName = "6.0" },
            new Patch { Id = 31, Index = 62, Name = "崭新的冒险", ShortName = "6.1" },
            new Patch { Id = 32, Index = 63, Name = "禁断的记忆", ShortName = "6.2" },
            new Patch { Id = 33, Index = 64, Name = "天上欢庆，地下轰鸣", ShortName = "6.3" },
            new Patch { Id = 34, Index = 65, Name = "负罪的王座", ShortName = "6.4" },
            new Patch { Id = 35, Index = 66, Name = "光明的零点", ShortName = "6.5" },

            // Dawntrail (DT)
            new Patch { Id = 36, Index = 70, Name = "金曦之遗辉", ShortName = "DT", Expansion = true },
            new Patch { Id = 37, Index = 71, Name = "金曦之遗辉", ShortName = "7.0" },
            new Patch { Id = 38, Index = 72, Name = "与未知邂逅", ShortName = "7.1" },
            new Patch { Id = 39, Index = 73, Name = "永久探求者", ShortName = "7.2" },
            new Patch { Id = 40, Index = 74, Name = "明日的路标", ShortName = "7.3" }
        ];

        public override string Name => "版本";
        public override string NameLocalizationKey => "PatchSearchFilter";

        public override bool IsSet => selectedPatches.Count >= 1 && selectedPatches.FirstOrDefault()?.Index != 0;

        public override bool HasChanged
        {
            get
            {
                if (altered)
                {
                    altered = false;
                    return true;
                }

                return false;
            }
        }

        public override bool CheckFilter(Item item)
        {
            foreach (Patch selectedPatch in selectedPatches)
            {
                string stringTruePatch = ClassExtensions.GetPatch(item.RowId);
                string stringPatch = stringTruePatch.Length > 3 ? stringTruePatch.Substring(0, 3) : stringTruePatch;

                if (stringPatch == selectedPatch.ShortName && selectedPatches.Contains(selectedPatch)) return true;
            }

            return false;
        }

        public override void DrawEditor()
        {
            var btnSize = new Vector2(24 * ImGui.GetIO().FontGlobalScale);

            Patch? doRemove = null;
            List<Patch> tempPatchList = [];
            var i = 0;

            foreach (var patch in selectedPatches)
            {
                if (ImGui.Button($"-###PatchSearchFilterRemove{i++}", btnSize))
                {
                    doRemove = patch;
                }

                var selectedParam = patch.Id;
                ImGui.SetNextItemWidth(200);

                ImGui.SameLine();

                if (ImGui.Combo(
                        $"###PatchSearchFilterSelectStat{i++}",
                        ref selectedParam,
                        ExpansionsPatches.Select(p => p.Id == 0
                            ? Loc.Localize("PatchSearchFilterSelectStat", "Select a patch...")
                            : $"{p.Name} ({p.ShortName})".ToString()).ToArray(),
                        ExpansionsPatches.Count()))
                {
                    Patch tempPatch = ExpansionsPatches.First(p => p.Id == selectedParam);
                    if (tempPatch.Expansion)
                    {
                        foreach (Patch extensionPatch in ExpansionsPatches.Where(p =>
                                     p.Index.ToString().Substring(0, 1) == tempPatch.Index.ToString().Substring(0, 1)))
                        {
                            Patch newPatch = new Patch
                            {
                                Id = extensionPatch.Id,
                                Index = extensionPatch.Index,
                                Name = extensionPatch.Name,
                                ShortName = extensionPatch.ShortName,
                                Expansion = extensionPatch.Expansion
                            };
                            if (selectedPatches.Contains(newPatch)) continue;
                            tempPatchList.Add(newPatch);
                        }
                    }
                    else
                    {
                        if (selectedPatches.Contains(tempPatch)) continue;
                        patch.Id = tempPatch.Id;
                        patch.Index = tempPatch.Index;
                        patch.Name = tempPatch.Name;
                        patch.ShortName = tempPatch.ShortName;
                        patch.Expansion = tempPatch.Expansion;
                    }

                    altered = true;
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    doRemove = patch;
                }
            }

            if (tempPatchList.Count > 0)
            {
                foreach (Patch patch in tempPatchList)
                {
                    selectedPatches.Add(patch);
                }

                altered = true;
            }

            if (doRemove != null)
            {
                selectedPatches.Remove(doRemove);
                altered = true;
            }

            if (selectedPatches.RemoveAll(p => p.Expansion) > 0) selectedPatches.RemoveAll(p => p.Id == 0);
            selectedPatches = selectedPatches.DistinctBy(p => p.Name).OrderByDescending(p => p.ShortName).ToList();

            if (ImGui.Button("+###PatchSearchFilterPlus", btnSize))
            {
                selectedPatches.Add(new Patch { Id = 0 });
                altered = true;
            }

            if (selectedPatches.Count > 1)
            {
                ImGui.SameLine();
            }
        }
    }
}