using System.Collections.Immutable;

namespace LM2.SaveTools
{
    public static class Utils
    {
        public static readonly ImmutableDictionary<int, string> MissionIndices = new Dictionary<int, string>()
        {
            { 0, "A-1" },
            { 1, "A-2" },
            { 2, "A-3" },
            { 3, "A-4" },
            { 4, "A-5" },
            { 5, "A-Boss" },
            { 7, "A-Bonus" },

            { 10, "B-1" },
            { 11, "B-2" },
            { 12, "B-3" },
            { 13, "B-4" },
            { 14, "B-5" },
            { 15, "B-Boss" },
            { 18, "B-Bonus" },

            { 20, "C-1" },
            { 21, "C-2" },
            { 22, "C-3" },
            { 23, "C-4" },
            { 24, "C-5" },
            { 25, "C-Boss" },
            { 27, "C-Bonus" },

            { 30, "D-1" },
            { 31, "D-2" },
            { 32, "D-3" },
            { 35, "D-Boss" },
            { 38, "D-Bonus" },

            { 40, "E-1" },
            { 41, "E-2" },
            { 42, "E-3" },
            { 43, "E-4" },
            { 44, "E-5" },
            { 46, "E-Boss" },
            { 49, "E-Bonus" },

            { 50, "King Boo" }
        }.ToImmutableDictionary();

        public static readonly ImmutableDictionary<int, string> EvershadeGhostIndices = new Dictionary<int, string>()
        {
            { 12, "Greenie" },
            { 1, "Slammer" },
            { 9, "Hider" },
            { 24, "Sneaker" },
            { 6, "Creeper" },

            { 26, "Sister Melinda" },
            { 27, "Sister Belinda" },
            { 28, "Sister Herlinda" },
            { 7, "Gobber" },
            { 15, "Boffin" },

            { 13, "Strong Greenie" },
            { 2, "Strong Slammer" },
            { 10, "Strong Hider" },
            { 25, "Strong Sneaker" },
            { 8, "Strong Gobber" },

            { 19, "Grouchy Possessor" },
            { 20, "Harsh Possessor" },
            { 21, "Overset Possessor" },
            { 22, "Scornful Possessor" },
            { 23, "Tough Possessor" },

            { 16, "Boffin Elder" },
            { 17, "Strong Boffin" },
            { 14, "Gold Greenie" },

            { 3, "Polterpup (Story)" },

            { 4, "Polterpups (Tower)" },
            { 5, "Big Polterpups (Tower)" },
            { 18, "The Brain (Tower)" }
        }.ToImmutableDictionary();

        public static readonly ImmutableDictionary<int, string> TowerGhostIndices = new Dictionary<int, string>()
        {
            { 40, "Bomb Brothers" },
            { 41, "Scarab Nabber" },
            { 42, "Terrible Teleporter" },
            { 43, "Primordial Goo" },
            { 44, "Creeper Creator" },

            { 0, "Fright Knight" },
            { 1, "Snug Thug" },
            { 2, "Sleek Sneaker" },
            { 3, "Tether Jacket" },
            { 4, "Spectral Sloth" },

            { 5, "Blue Pimpernel" },
            { 6, "Sunflower" },
            { 7, "Pink Zinnia" },
            { 8, "Violet" },
            { 9, "Daisy" },

            { 10, "Melon-choly" },
            { 11, "Aweberry" },
            { 12, "Scorn" },
            { 13, "Fright Egg" },
            { 14, "Terrorange" },

            { 15, "Spooky Spook" },
            { 16, "Scars" },
            { 17, "Skoul" },
            { 18, "Jack-goo'-lantern" },
            { 19, "Blimp Reaper" },

            { 20, "Dreadonfly" },
            { 21, "Shadybird" },
            { 22, "Terrorfly" },
            { 23, "Blobberfly" },
            { 24, "Grumble Bee" },

            { 25, "Horrorca" },
            { 26, "Clown Fishy" },
            { 27, "Shriek Shark" },
            { 28, "Pondguin" },
            { 29, "Snapper" },

            { 30, "Bad-minton" },
            { 31, "American Footbrawl" },
            { 32, "Tennis Menace" },
            { 33, "Goolf" },
            { 34, "Ball Hog" },

            { 35, "Maligator" },
            { 36, "Banegal" },
            { 37, "Zebrawl" },
            { 38, "Leoprank" },
            { 39, "Full Moo" }
        }.ToImmutableDictionary();

        public static readonly ImmutableArray<int> GemIndexOrder = new int[13] { 7, 11, 0, 3, 2, 6, 10, 8, 4, 1, 9, 12, 5 }.ToImmutableArray();

        /// <param name="gemNumber">The 0-based index of the gem based on the order shown in the vault</param>
        public static int GetGemIndex(int gemNumber, Mansion mansion)
        {
            return ((int)mansion * 13) + GemIndexOrder[gemNumber];
        }

        public static int GetTowerModeIndex(TowerMode mode, TowerFloor floor, TowerDifficulty difficulty)
        {
            return ((int)mode * 12) + ((int)floor * 3) + (int)difficulty;
        }
    }
}
