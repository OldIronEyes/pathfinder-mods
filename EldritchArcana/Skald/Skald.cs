// Copyright (c) 2019 Vijay Challa
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Validation;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace EldritchArcana
{
    static class SkaldClass
    {
        static LibraryScriptableObject library => Main.library;

        internal static BlueprintCharacterClass skald;
        internal static BlueprintCharacterClass[] skaldArray;

        internal static void Load()
        {
            if (SkaldClass.skald != null) return;

            var bard = Helpers.GetClass("772c83a25e2268e448e841dcd548235f");
            var barbarian = Helpers.GetClass("f7d7eb166b3dd594fb330d085df41853");

            var skald = SkaldClass.skald = Helpers.Create<BlueprintCharacterClass>();
            skaldArray = new BlueprintCharacterClass[] { skald };
            skald.name = "SkaldClass";
            library.AddAsset(skald, "4df69f3bc6674169ade41646c65796a7");
            skald.LocalizedName = Helpers.CreateString("Skald.Name", "Skald");
            skald.LocalizedDescription = Helpers.CreateString("Skald.Description", "");
            skald.m_Icon = barbarian.Icon;
            skald.SkillPoints = 4;
            skald.HitDie = DiceType.D8;
            skald.BaseAttackBonus = bard.BaseAttackBonus;
            skald.FortitudeSave = barbarian.FortitudeSave;
            skald.ReflexSave = barbarian.ReflexSave;
            skald.WillSave = bard.WillSave;

            // TODO: Skald will likely have problems with Mystic Theurge for the same reasons as Elmindra's Oracle
            var spellbook = Helpers.Create<BlueprintSpellbook>();
            spellbook.name = "SkaldSpellbook";
            library.AddAsset(spellbook, "364446a69082426a806939cbe94f1f17");
            spellbook.Name = skald.LocalizedName;
            spellbook.SpellsPerDay = bard.Spellbook.SpellsPerDay;
            spellbook.SpellsKnown = bard.Spellbook.SpellsKnown;
            spellbook.SpellList = bard.Spellbook.SpellList;
            spellbook.Spontaneous = true;
            spellbook.IsArcane = true;
            spellbook.AllSpellsKnown = false;
            spellbook.CanCopyScrolls = false;
            spellbook.CastingAttribute = StatType.Charisma;
            spellbook.CharacterClass = skald;
            spellbook.CantripsType = CantripsType.Cantrips;
            skald.Spellbook = spellbook;

            // Due to how skills were combined, I took Skalds to be more focused on a bard's knowledges than 
            // the stuff they'd do as general skill users (thievery, stealth, etc.)
            skald.ClassSkills = new StatType[]{
                StatType.SkillMobility,
                StatType.SkillKnowledgeArcana,
                StatType.SkillKnowledgeWorld,
                StatType.SkillAthletics,
                StatType.SkillLoreNature,
                StatType.SkillLoreReligion,
                StatType.SkillPerception,
                StatType.SkillPersuasion
            };

            skald.IsArcaneCaster = true;
            skald.IsArcaneCaster = false;

            skald.StartingGold = bard.StartingGold;
            skald.PrimaryColor = barbarian.PrimaryColor;
            skald.SecondaryColor = barbarian.SecondaryColor;

            skald.RecommendedAttributes = new StatType[] {
                StatType.Strength,
                StatType.Constitution,
                StatType.Charisma,
            };
            skald.NotRecommendedAttributes = new StatType[]{
                StatType.Intelligence
            };

            skald.StartingItems = barbarian.StartingItems;

            var progression = Helpers.CreateProgression("SkaldProgression",
                skald.name,
                skald.Description,
                "16d1751a69f5422d899c47076f005237",
                skald.Icon,
                FeatureGroup.None);
            progression.Classes = skaldArray;
            var entries = new List<LevelEntry>();

            var cantrips = library.CopyAndAdd<BlueprintFeature>(
                "4f422e8490ec7d94592a8069cce47f98",
                "SkaldCantripFeature",
                "7ae5b7faaeb54d29a3e1146eb48fdae5");
            cantrips.SetDescription("Skalds learn a number of cantrips, or 0-level spells. These spells are cast like any other spell, but they do not consume any slots and may be used again.");
            cantrips.SetComponents(cantrips.ComponentsArray.Select(c =>
            {
                var bind = c as BindAbilitiesToClass;
                if (bind == null) return c;
                bind = UnityEngine.Object.Instantiate(bind);
                bind.CharacterClass = skald;
                bind.Stat = StatType.Charisma;
                return bind;
            }));

            var proficiencies = library.CopyAndAdd<BlueprintFeature>(
                "acc15a2d19f13864e8cce3ba133a1979",
                "SkaldProficiencies",
                "78d35a0ec8cf4225af5e89af9eb23500");
            proficiencies.SetName("Skald Proficiencies");
            proficiencies.SetDescription("A Skald is proficient with all simple and martial weapons, light and medium armor, and shields (except tower shields).");

            var spellFailure = library.CopyAndAdd<BlueprintFeature>(
                "8498cdb82f5eb6d4f91e72b1a18944ff", //Magus medium armor feature
                "SkaldSpellFailure",
                "3df490f2fcbf4ed98acb82f4379f2aa5");

            var detectMagic = library.Get<BlueprintFeature>("ee0b69e90bac14446a4cf9a050f87f2e");

            //var ragingSong = 

            var bardicKnowledge = library.Get<BlueprintFeature>("65cff8410a336654486c98fd3bacd8c5");

            var wellVersed = library.Get<BlueprintFeature>("8f4060852a4c8604290037365155662f");

            //var ragePowers = 

            //var uncannyDodge = 

            //var loreMaster =

            //var damageReduction =

            //var masterSkald = 

            entries.Add(Helpers.LevelEntry(1,
                proficiencies,
                cantrips,
                spellFailure,
                detectMagic,
                bardicKnowledge
                //,raging song
            ));
            entries.Add(Helpers.LevelEntry(2, wellVersed));
            entries.Add(Helpers.LevelEntry(3, ragePowers));
        }
    }
}