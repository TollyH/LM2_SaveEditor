# Luigi's Mansion 2 Save Editor

A save editor for the 3DS game *Luigi's Mansion 2* (also known as *Luigi's Mansion: Dark Moon* in North American releases).

## Table of Contents

- [Luigi's Mansion 2 Save Editor](#luigis-mansion-2-save-editor)
  - [Table of Contents](#table-of-contents)
  - [Save File Documentation](#save-file-documentation)
    - [General Information](#general-information)
    - [Save File Format](#save-file-format)
    - [Mission Indices](#mission-indices)
      - [Gloomy Manor](#gloomy-manor)
      - [Haunted Towers](#haunted-towers)
      - [Old Clockworks](#old-clockworks)
      - [Secret Mine](#secret-mine)
      - [Treacherous Mansion](#treacherous-mansion)
      - [King Boo's Illusion](#king-boos-illusion)
    - [Ghost Indices](#ghost-indices)
      - [Basic (Evershade Valley) Ghosts](#basic-evershade-valley-ghosts)
      - [(Tower) Ghosts](#tower-ghosts)
    - [Gem Indices](#gem-indices)
      - [Gem Indices for Each Mansion](#gem-indices-for-each-mansion)
    - [Tower Mode Indices](#tower-mode-indices)
    - [CRC Checksums](#crc-checksums)

## Save File Documentation

This section of the README documents all of my findings related to how the game's save file works. You do not need to read this section to utilise the save editor, it is only for those curious as to the inner workings of the save file format. At some points I reference address offsets in the game's executable in case you wish to find relevant code/data yourself. These addresses require an ELF dump of the European version's executable to utilise. [ctr-elf](https://github.com/archshift/ctr-elf) is a good tool to get one of these (you will need an original ROM file). Reverse engineering tools compatible with ARM (such as [Ghidra](https://ghidra-sre.org/) and [IDA Pro](https://hex-rays.com/ida-pro/)) are recommended, along with the [Citra](https://citra-emu.org/) emulator to allow for easy GDB debugging and save file access.

### General Information

As with many games, Luigi's Mansion 2 save files use a proprietary binary format designed specifically for the game.

Each profile is stored in a separate file with the slot number being determined solely by the filename. This means that renaming a file is all you need to do to move a profile to another slot, the file contents itself does not differ at all. The files are called `profile1.sav`, `profile2.sav`, and `profile3.sav` in the order you would expect.

As far as I can tell, the save file format does not differ *at all* between regions. This means, for example, you could copy a save file from the European version to the North American version and it would work identically, no modifications required. Even the so-called "Version CRC" does not change.

The save file is split into two sections which are loaded separately by the game. The first section is 0x1A (26) bytes long and stores the information that is shown when hovering over the profile on the title screen. Changing it does not affect anything in-game. The second section is 0xF1D (3869) bytes long and stores all the information needed once loaded into a save profile. With the sole exception of total playtime, there is no data in the title screen section that cannot be determined using data already present in the in-game section.

If the title screen section of the save file is modified without changing the relevant parameters in the in-game section of the save file, it will be overwritten with the correct values according to the in-game section when the game is next saved.

All numeric values in the save file are stored using the little endian byte format, therefore in memory the single number `0xAABBCCDD` would be represented with the bytes `0xDD, 0xCC, 0xBB, 0xAA` in that order.

### Save File Format

The following is a table of the locations of all the fields within the save file, as well as any additional notes I may have on each field. The in-game section byte offsets have both the overall file offset, as well as the offset relative to the start of the in-game section in brackets.

| Byte Offset | Field Name | Data Type | Notes |
|-----------------------------------|--------------------------|-----------|------------------------------------------------------------------------------------|
| **Title Screen Section** ||||
| 0x0000 - 0x0003 | Data CRC | UInt32 | See CRC Checksums section below. |
| 0x0004 - 0x0007 | Version CRC | UInt32 | Always `0x7B, 0x0C, 0x27, 0x49`. |
| 0x0008 | Furthest Cleared Mansion | Byte | 0-indexed, follows the same order as in-game, `0xFF` if no mission has been cleared. |
| 0x0009 | Furthest Cleared Mission | Byte | Uses the rightmost digit of the mission's index, see the mission indices section below. |
| 0x000A | Highest Tower Floor | Byte | |
| 0x000B - 0x000E | Total Treasure Acquired | Int32 | |
| 0x000F | Boos Captured | Byte | |
| 0x0010 | Dark Moon Pieces | Byte | Even if no missions are complete, player will always start with 1 piece. |
| 0x0011 | E. Gadd Medals | Byte | Referred to as "stars" internally. |
| 0x0012 - 0x0019 | Total Playtime | Int64 | Measured in seconds. Only value that cannot be determined with the in-game section. |
| **In-Game Section** |
| 0x001A - 0x001D (0x0000 - 0x0003) | Data CRC | UInt32 | See CRC Checksums section below. |
| 0x001E - 0x0021 (0x0004 - 0x0007) | Version CRC | UInt32 | Always `0xAD, 0x03, 0x32, 0xD4`. |
| 0x0022 - 0x0821 (0x0008 - 0x0807) | Discovered NIS | Unknown | NIS most likely stands for "Non-Interactable Sequences" (i.e. real-time cutscenes). Has nearly no discernable effect on the game, and I haven't been able to determine its format, so it is best to leave it unmodified. Possibly stores a list of cutscenes that have been seen for the few circumstances where cutscenes play differently if you've seen them before. The only one I'm aware of that does this is Luigi attaching the Poltergust to the vault nozzle, however, so why this needs over 2KB (over half the entire save file!) I do not know. |
| 0x0822 - 0x085D (0x0808 - 0x0843) | Mission Completion | Boolean\[60\] | `0` = mission not complete, `1` = mission complete. See section below on mission indices. |
| 0x085E - 0x0899 (0x0844 - 0x087F) | Mission Locked | Boolean\[60\] | `0` = mission unlocked, `1` = mission locked. See section below on mission indices. Even though it never happens legitimately, you can unlock more than one mission after the last completed one. You can also unlock missions whilst still having prior ones locked and they will be made visible, though they will be inaccessible. |
| 0x089A - 0x08D5 (0x0880 - 0x08BB) | Mission Grade | Byte\[60\] | `0` = Bronze, `1` = Silver, `2` = Gold. See section below on mission indices. |
| 0x08D6 - 0x0911 (0x08BC - 0x08F7) | Mission Prev Grade | Byte\[60\] | Possibly previous grade? Value doesn't seem to affect anything, keeping it the same as Mission Grade is a safe bet. See section below on mission indices. |
| 0x0912 - 0x094D (0x08F8 - 0x0933) | Mission Boo Captured | Boolean\[60\] | `0` = Boo not captured, `1` = Boo captured. See section below on mission indices. Setting this to `1` on levels that don't have a Boo will not increase the Boo counter in the vault, but it will increase the Boos shown toward the bonus level for each mansion. E-4 "Ambush Manoeuvre" (index `43`) counts for 10 Boos instead of the usual 1 in the vault when set to `1`. It still counts as 1 toward the Treacherous Mansion bonus mission, however. |
| 0x094E - 0x0989 (0x0934 - 0x096F) | Mission Boo Notify State | Byte\[60\] | Seems to always be between `0` and `2`. Changing it doesn't seem to do anything. |
| 0x098A - 0x09C5 (0x0970 - 0x09AB) | Mission Notify State | Byte\[60\] | Seems to always be between `0` and `2`. Changing it doesn't seem to do anything. |
| 0x09C6 - 0x0AB5 (0x09AC - 0x0A9B) | Mission Clear Time | Single\[60\] | Stores the **shortest** completion time. Measured in seconds. See section below on mission indices. Despite being a float, the value stored is always a whole number. |
| 0x0AB6 - 0x0B2D (0x0A9C - 0x0B13) | Mission Ghosts Captured | UInt16\[60\] | Stores the **most** ghosts captured. See section below on mission indices. |
| 0x0B2E - 0x0BA5 (0x0B14 - 0x0B8B) | Mission Damage Taken | UInt16\[60\] | Stores the **least** damage taken. See section below on mission indices. |
| 0x0BA6 - 0x0C1D (0x0B8C - 0x0C03) | Mission Treasure Collected | UInt16\[60\] | Stores the **most** treasure collected. See section below on mission indices. |
| 0x0C1E - 0x0C3A (0x0C04 - 0x0C20) | Num Basic Ghost Collected | Byte\[29\] | Caps at `99` (anything higher will be set back down). See the below section on ghost indices. *Also 99 is nowhere near enough, this really could have done with being a 16-bit integer, which considering ghost weight did, I don't know why this didn't. And for that matter why did they leave half the byte unused capping at 99 not 255? There's more than enough space on the UI for 255 and beyond. Ugh.* |
| 0x0C3B - 0x0C74 (0x0C21 - 0x0C5A) | Max Basic Ghost Weight | UInt16\[29\] | See the below section on ghost indices. |
| 0x0C75 - 0x0C91 (0x0C5B - 0x0C77) | Basic Ghost Notify State | Byte\[29\] | `2` to mark as new on the UI, `0` otherwise. See the below section on ghost indices. |
| 0x0C92 - 0x0CAE (0x0C78 - 0x0C94) | Basic Ghost Notify Because Higher Weight | Byte\[29\] | No effect I could find. |
| 0x0CAF (0x0C95) | Any Optional Boo Captured | Boolean | If this is `0` then E. Gadd will call Luigi after he catches a Boo explaining Boos to him. It will then be set to `1` and this will no longer occur. |
| 0x0CB0 (0x0C96) | Just Collected Chaser | Boolean | I'm pretty sure "Chaser" is the internal name for the Polterpup, though changing this doesn't seem to do anything. |
| 0x0CB1 - 0x0D0A (0x0C97 - 0x0CF0) | Ghost Weight Requirement | Unknown\[45\] | Not sure what this is for, I couldn't spot any differences after modifying. As will be explained in the section on ghost indices, this is related to the tower ghosts. |
| 0x0D0B - 0x0D37 (0x0CF1 - 0x0D1D) | Ghost Collectable State | Byte\[45\] | `2` = caught at least once, `0` = uncaught. See the below section on ghost indices. Not sure why tower ghosts have this field when Evershade ghosts just use the number caught to determine if they've been caught before. |
| 0x0D38 - 0x0D64 (0x0D1E - 0x0D4A) | Num Ghost Collected | Byte\[45\] | Same restrictions (and complaints) as "Num Basic Ghost Collected". See the below section on ghost indices. |
| 0x0D65 - 0x0DBE (0x0D4B - 0x0DA4) | Max Ghost Weight | UInt16\[45\] | See the below section on ghost indices. |
| 0x0DBF - 0x0DEB (0x0DA5 - 0x0DD1) | Ghost Notify State | Byte\[45\] | `2` to mark as new on the UI, `0` otherwise. See the below section on ghost indices. |
| 0x0DEC - 0x0E18 (0x0DD2 - 0x0DFE) | Ghost Notify Because Higher Weight | Byte\[45\] | No effect I could find. |
| 0x0E19 - 0x0E66 (0x0DFF - 0x0E4C) | Gem Collected | Boolean\[78\] | `0` = gem not collected, `1` = gem collected. See the below section on gem indices. |
| 0x0E67 - 0x0EB4 (0x0E4D - 0x0E9A) | Gem Notify State | Byte\[78\] | `2` to mark as new on the UI, `0` otherwise. See the below section on gem indices. |
| 0x0EB5 (0x0E9B) | Has Poltergust | Boolean | Completely redundant as Luigi will always be given the Poltergust on levels where he should have it. Doesn't even affect A-1, as whether Luigi is given the Poltergust in A-1 is determined by whether you've completed the level before, not by this field. This does, however, remove the Poltergust from Luigi in the bunker if set to `0` (though it will reappear in animations where Luigi would normally have it). |
| 0x0EB6 (0x0E9C) | Seen Initial Dual Scream Animation | Boolean | If this is `0`, Luigi will play his initial "aha" animation from A-1 when answering an E. Gadd call. Afterwards this will be set to `1` and the animation will play as normal for future calls. |
| 0x0EB7 (0x0E9D) | Has Mario Been Revealed in the Story | Boolean | If `0`, Luigi will call out his usual voice clips when pressing a D-Pad button. If `1`, Luigi will instead call for Mario. In normal gameplay, this is set to `1` once Mario has been revealed at the start of E-3 (***not*** after finishing the game; a common misconception). |
| 0x0EB8 (0x0E9E) | Last Mansion Played | Byte | Determines which mansion will be initially selected on the central screen in the bunker. 0-indexed, follows the same order as in-game. |
| 0x0EB9 - 0x0EBC (0x0E9F - 0x0EA2) | Total Treasure Acquired | Int32 | |
| 0x0EBD - 0x0EC0 (0x0EA3 - 0x0EA6) | Treasure to Notify During Unloading | Int32 | Gets set to however much treasure you got in the last level you completed. Doesn't actually seem to affect anything though. |
| 0x0EC1 - 0x0EC4 (0x0EA7 - 0x0EAA) | Total Ghost Weight Acquired | Int32 | |
| 0x0EC5 (0x0EAB) | Darklight Upgrade Level | Byte | Starts at `1`, goes up to `3`. This value is prioritised over the total treasure collected, therefore setting to something lower after already being max upgraded will make it impossible to upgrade again without re-editing the save. |
| 0x0EC6 (0x0EAC) | Darklight Notify State | Byte | No effect I could find. |
| 0x0EC7 (0x0EAD) | Poltergust Upgrade Level | Byte | Starts at `1`, goes up to `3`. Does not include Super Poltergust. This value is prioritised over the total treasure collected, therefore setting to something lower after already being max upgraded will make it impossible to upgrade again without re-editing the save. |
| 0x0EC8 (0x0EAE) | Poltergust Notify State | Byte | No effect I could find. |
| 0x0EC9 (0x0EAF) | Has Super Poltergust | Boolean | `0` = not unlocked, `1` = unlocked. This value is prioritised over the total treasure collected, therefore setting to `0` after already being max upgraded will make it impossible to upgrade again without re-editing the save. |
| 0x0ECA (0x0EB0) | Super Poltergust Notify State | Byte | No effect I could find. |
| 0x0ECB (0x0EB1) | Has Seen Revive Bone PIP | Boolean | Most likely stores whether or not you've seen the Polterpup revive cutscene or not. I'm unsure what PIP stands for. |
| 0x0ECC - 0x0F2B (0x0EB2 - 0x0F11) | Best Tower Clear Time | UInt16\[48\] | Each possible configuration of tower has its own best time. See section on tower mode indices. Despite not being timed overall, endless towers are given (unused) spaces in this array. |
| 0x0F2C - 0x0F2F (0x0F12 - 0x0F15) | Endless Mode Highest Floor Reached | Byte\[4\] | Stored for each gameplay mode individually in this order: Hunter, Rush, Polterpup, Surprise. |
| 0x0F30 (0x0F16) | Any Mode Highest Floor Reached | Byte | Likely stores the highest floor reached in any tower configuration. |
| 0x0F31 - 0x0F34 (0x0F17 - 0x0F1A) | Endless Floors Unlocked | Boolean\[4\] | Whether or not endless mode has been unlocked for each tower gameplay mode. `0` = not unlocked, `1` = unlocked. In normal gameplay, this requires you to beat 25 floor mode on each respective gameplay mode on any difficulty setting. Stored for each gameplay mode individually in this order: Hunter, Rush, Polterpup, Surprise. |
| 0x0F35 (0x0F1B) | Random Tower Unlocked | Boolean | Whether or not the surprise tower gameplay mode is unlocked. `0` = not unlocked, `1` = unlocked. In normal gameplay, this is unlocked after beating Hunter, Rush, and Polterpup modes on any difficulty and floor setting. |
| 0x0F36 (0x0F1C) | Tower Notify State | Byte | No effect I could find. |

The names and locations of these fields (though not their type or purpose) is fairly easy to locate due to two functions within the game's executable that list them.

The first one, used for the title screen data, is as follows (decompiled C code generated by Ghidra):

```c
// .text:0027fa94h (file offset: 0x0028fa94)
void FUN_0037fa94(int param_1,undefined *param_2,undefined4 param_3,int param_4)
{
  (*(code *)param_2)("mDataCRC",param_1,0,4,param_3);
  (*(code *)param_2)("mVersionCRC",param_1 + 4,4,4,param_3);
  (*(code *)param_2)("mFurthestClearedMansionIndex",param_1 + 8,8,1,param_3);
  (*(code *)param_2)("mFurthestClearedMissionIndex",param_1 + 9,9,1,param_3);
  (*(code *)param_2)("mTowerHighestFloorClimbed",param_1 + 10,10,1,param_3);
  (*(code *)param_2)("mTotalTreasureAcquired",param_1 + 0xc,0xb,4,param_3);
  (*(code *)param_2)("mTotalBoosCaptured",param_1 + 0x10,0xf,1,param_3);
  (*(code *)param_2)("mTotalDarkmoonPiecesAcquired",param_1 + 0x11,0x10,1,param_3);
  (*(code *)param_2)("mStars",param_1 + 0x12,0x11,1,param_3);
  (*(code *)param_2)("mTimePlayedInSeconds",param_1 + 0x18,0x12,8,param_3);
  if ((0 < param_4) && (param_4 < 0x1a)) {
    software_bkpt(0xa6);
    return;
  }
  return;
}
```

The second one, used for in-game save data, is as follows:

```c
void FUN_0037fe78(int param_1,undefined *param_2,undefined4 param_3,int param_4)
{
  bool bVar1;
  bool bVar2;
  
  (*(code *)param_2)("mDataCRC",param_1,0,4,param_3);
  (*(code *)param_2)("mVersionCRC",param_1 + 4,4,4,param_3);
  (*(code *)param_2)("mDiscoveredNIS",param_1 + 8,8,0x800,param_3);
  (*(code *)param_2)("mMansionMissionCompletion",param_1 + 0x808,0x808,0x3c,param_3);
  (*(code *)param_2)("mMansionMissionLocked",param_1 + 0x844,0x844,0x3c,param_3);
  (*(code *)param_2)("mMansionMissionGrade",param_1 + 0x880,0x880,0x3c,param_3);
  (*(code *)param_2)("mMansionMissionPrevGrade",param_1 + 0x8bc,0x8bc,0x3c,param_3);
  (*(code *)param_2)("mMansionMissionBooCaptured",param_1 + 0x8f8,0x8f8,0x3c,param_3);
  (*(code *)param_2)("mMansionMissionBooNotifyState",param_1 + 0x934,0x934,0x3c,param_3);
  (*(code *)param_2)("mMansionMissionNotifyState",param_1 + 0x970,0x970,0x3c,param_3);
  (*(code *)param_2)("mMansionMissionClearTime",param_1 + 0x9ac,0x9ac,0xf0,param_3);
  (*(code *)param_2)("mMansionMissionGhostsCaptured",param_1 + 0xa9c,0xa9c,0x78,param_3);
  (*(code *)param_2)("mMansionMissionDamageTaken",param_1 + 0xb14,0xb14,0x78,param_3);
  (*(code *)param_2)("mMansionMissionTreasureCollected",param_1 + 0xb8c,0xb8c,0x78,param_3);
  (*(code *)param_2)("mNumBasicGhostCollected",param_1 + 0xc04,0xc04,0x1d,param_3);
  (*(code *)param_2)("mMaxBasicGhostWeight",param_1 + 0xc22,0xc21,0x3a,param_3);
  (*(code *)param_2)("mBasicGhostNotifyState",param_1 + 0xc5c,0xc5b,0x1d,param_3);
  (*(code *)param_2)("mBasicGhostNotifyBecauseHigherWeight",param_1 + 0xc79,0xc78,0x1d,param_3);
  (*(code *)param_2)("mAnyOptionalBooCaptured",param_1 + 0xc96,0xc95,1,param_3);
  (*(code *)param_2)("mJustCollectedChaser",param_1 + 0xc97,0xc96,1,param_3);
  (*(code *)param_2)("mGhostWeightRequirement",param_1 + 0xc98,0xc97,0x5a,param_3);
  (*(code *)param_2)("mGhostCollectableState",param_1 + 0xcf2,0xcf1,0x2d,param_3);
  (*(code *)param_2)("mNumGhostCollected",param_1 + 0xd1f,0xd1e,0x2d,param_3);
  (*(code *)param_2)("mMaxGhostWeight",param_1 + 0xd4c,0xd4b,0x5a,param_3);
  (*(code *)param_2)("mGhostNotifyState",param_1 + 0xda6,0xda5,0x2d,param_3);
  (*(code *)param_2)("mGhostNotifyBecauseHigherWeight",param_1 + 0xdd3,0xdd2,0x2d,param_3);
  (*(code *)param_2)("mGemCollected",param_1 + 0xe00,0xdff,0x4e,param_3);
  (*(code *)param_2)("mGemNotifyState",param_1 + 0xe4e,0xe4d,0x4e,param_3);
  (*(code *)param_2)("mHasPoltergust",param_1 + 0xe9c,0xe9b,1,param_3);
  (*(code *)param_2)("mSeenInitialDSHorrorAnim",param_1 + 0xe9d,0xe9c,1,param_3);
  (*(code *)param_2)("mHasMarioBeenRevealedInTheStory",param_1 + 0xe9e,0xe9d,1,param_3);
  (*(code *)param_2)("mLastMansionPlayed",param_1 + 0xe9f,0xe9e,1,param_3);
  (*(code *)param_2)("mTotalTreasureAcquired",param_1 + 0xea0,0xe9f,4,param_3);
  (*(code *)param_2)("mTreasureToNotifyDuringUnloading",param_1 + 0xea4,0xea3,4,param_3);
  (*(code *)param_2)("mTotalGhostWeightAcquired",param_1 + 0xea8,0xea7,4,param_3);
  (*(code *)param_2)("mDarklightUpgradeLevel",param_1 + 0xeac,0xeab,1,param_3);
  (*(code *)param_2)("mDarklightUpgradeNotifyState",param_1 + 0xead,0xeac,1,param_3);
  (*(code *)param_2)("mPoltergustUpgradeLevel",param_1 + 0xeae,0xead,1,param_3);
  (*(code *)param_2)("mPoltergustUpgradeNotifyState",param_1 + 0xeaf,0xeae,1,param_3);
  (*(code *)param_2)("mHasSuperPoltergust",param_1 + 0xeb0,0xeaf,1,param_3);
  (*(code *)param_2)("mSuperPoltergustNotifyState",param_1 + 0xeb1,0xeb0,1,param_3);
  (*(code *)param_2)("mHasSeenReviveBonePIP",param_1 + 0xeb2,0xeb1,1,param_3);
  (*(code *)param_2)("mBestTerrorTowerClearTime",param_1 + 0xeb4,0xeb2,0x60,param_3);
  (*(code *)param_2)("mEndlessModeHighestFloorReached",param_1 + 0xf14,0xf12,4,param_3);
  (*(code *)param_2)("mAnyModeHighestFloorReached",param_1 + 0xf18,0xf16,1,param_3);
  (*(code *)param_2)("mEndlessFloorsUnlocked",param_1 + 0xf19,0xf17,4,param_3);
  (*(code *)param_2)("mRandomTowerUnlocked",param_1 + 0xf1d,0xf1b,1,param_3);
  (*(code *)param_2)("mTerrorTowerNotifyState",param_1 + 0xf1e,0xf1c,1,param_3);
  bVar1 = param_4 < 0;
  bVar2 = param_4 == 0;
  if (0 < param_4) {
    bVar1 = 0xf1d - param_4 < 0;
    bVar2 = param_4 == 0xf1d;
  }
  if (!bVar2 && bVar1 == (0 < param_4 && SBORROW4(0xf1d,param_4))) {
    software_bkpt(0xa6);
    return;
  }
  return;
}
```

I'm not 100% sure on what these functions are actually for, though they appear to be a part of invoking many things related to save data, from clearing and copying to allocated memory, to calculating checksums. I'm also not sure why the names of the fields themselves were included, though I am glad they were as it saved a lot of work figuring out the boundaries of data in the save file.

### Mission Indices

The following is a list of every mission in the game along with it's associated index in the mission related save file arrays. Each of these arrays is 60 elements long (10 elements for each mansion, including King Boo's Illusion), despite there only being 34 missions in the game and each mansion having at most 7 missions. Modifying the values of unused missions in any of the fields doesn't do anything.

#### Gloomy Manor

- **A-1**: 0
- **A-2**: 1
- **A-3**: 2
- **A-4**: 3
- **A-5**: 4
- **A-Boss**: 5
- **A-Bonus**: 7

#### Haunted Towers

- **B-1**: 10
- **B-2**: 11
- **B-3**: 12
- **B-4**: 13
- **B-5**: 14
- **B-Boss**: 15
- **B-Bonus**: 18

#### Old Clockworks

- **C-1**: 20
- **C-2**: 21
- **C-3**: 22
- **C-4**: 23
- **C-5**: 24
- **C-Boss**: 25
- **C-Bonus**: 27

#### Secret Mine

- **D-1**: 30
- **D-2**: 31
- **D-3**: 32
- **D-Boss**: 35
- **D-Bonus**: 38

#### Treacherous Mansion

- **E-1**: 40
- **E-2**: 41
- **E-3**: 42
- **E-4**: 43
- **E-5**: 44
- **E-Boss**: 46
- **E-Bonus**: 49

#### King Boo's Illusion

- **King Boo**: 50

### Ghost Indices

The following are lists of every ghost in the game along with their associated index in the ghost save file arrays. In the save file field names, "basic ghosts" refer to the Evershade Valley ghosts, and "ghosts" (with no adjective) refer to the tower ghosts. A few indices are left unused, however changing the values in these locations doesn't do anything. These values are still 0-indexed, the 0 index is just left unused.

#### Basic (Evershade Valley) Ghosts

- **Greenie**: 12
- **Slammer**: 1
- **Hider**: 9
- **Sneaker**: 24
- **Creeper**: 6

---

- **Sister Melinda**: 26
- **Sister Belinda**: 27
- **Sister Herlinda**: 28
- **Gobber**: 7
- **Boffin**: 15

---

- **Strong Greenie**: 13
- **Strong Slammer**: 2
- **Strong Hider**: 10
- **Strong Sneaker**: 25
- **Strong Gobber**: 8

---

- **Grouchy Possessor**: 19
- **Harsh Possessor**: 20
- **Overset Possessor**: 21
- **Scornful Possessor**: 22
- **Tough Possessor**: 23

---

- **Boffin Elder**: 16
- **Strong Boffin**: 17
- **Gold Greenie**: 14

---

- **Polterpup (story)**: 3

*The following ghosts only appear in the tower and are in the tower section of the vault's ghost container, however information pertaining to them is stored in the Evershade Valley ghost arrays*

- **Polterpups (tower)**: 4
- **Big Polterpups**: 5
- **The Brain**: 18

#### (Tower) Ghosts

- **Bomb Brothers**: 40
- **Scarab Nabber**: 41
- **Terrible Teleporter**: 42
- **Primordial Goo**: 43
- **Creeper Creator**: 44

---

- **Fright Knight**: 0
- **Snug Thug**: 1
- **Sleek Sneaker**: 2
- **Tether Jacket**: 3
- **Spectral Sloth**: 4

---

- **Blue Pimpernel**: 5
- **Sunflower**: 6
- **Pink Zinnia**: 7
- **Violet**: 8
- **Daisy**: 9

---

- **Melon-choly**: 10
- **Aweberry**: 11
- **Scorn**: 12
- **Fright Egg**: 13
- **Terrorange**: 14

---

- **Spooky Spook**: 15
- **Scars**: 16
- **Skoul**: 17
- **Jack-goo'-lantern**: 18
- **Blimp Reaper**: 19

---

- **Dreadonfly**: 20
- **Shadybird**: 21
- **Terrorfly**: 22
- **Blobberfly**: 23
- **Grumble Bee**: 24

---

- **Horrorca**: 25
- **Clown Fishy**: 26
- **Shriek Shark**: 27
- **Pondguin**: 28
- **Snapper**: 29

---

- **Bad-minton**: 30
- **American Footbrawl**: 31
- **Tennis Menace**: 32
- **Goolf**: 33
- **Ball Hog**: 34

---

- **Maligator**: 35
- **Banegal**: 36
- **Zebrawl**: 37
- **Leoprank**: 38
- **Full Moo**: 39

### Gem Indices

The gem related save file arrays store every mansions' 13 gems in the same order that the mansions appear in-game. Within each mansion, however, the gems are stored in a different order to how they appear in the vault.

In the order that they appear in the vault, left-to-right, top-to-bottom, the indices of each gem is in the following, consistent order: `7, 11, 0, 3, 2, 6, 10, 8, 4, 1, 9, 12, 5`.

Here is an image showing this order: ![Numbered order of gems in the vault](https://github.com/TollyH/LM2_SaveEditor/blob/main/README_Images/Gem%20Indices.png)

As each mansion's gems are stored one after the other with no space in-between, simply add 13 for each mansion you wish to go ahead in the array, for example +0 for Gloomy Manor, +13 for Haunted Towers, +26 for Old Clockworks, etc.

Note that King Boo's Illusion is also given 13 spaces (the same as the other mansions) in the arrays despite not having any gems. Modifying any of these values does nothing.

#### Gem Indices for Each Mansion

This is a list of each mansions gem indices with the relevant offset already added should you need it:

- **Gloomy Manor**: `7, 11, 0, 3, 2, 6, 10, 8, 4, 1, 9, 12, 5`
- **Haunted Towers**: `20, 24, 13, 16, 15, 19, 23, 21, 17, 14, 22, 25, 18`
- **Old Clockworks**: `33, 37, 26, 29, 28, 32, 36, 34, 30, 27, 35, 38, 31`
- **Secret Mine**: `46, 50, 39, 42, 41, 45, 49, 47, 43, 40, 48, 51, 44`
- **Treacherous Mansion**: `59, 63, 52, 55, 54, 58, 62, 60, 56, 53, 61, 64, 57`
- **King Boo's Illusion** (Unused): `72, 76, 65, 68, 67, 71, 75, 73, 69, 66, 74, 77, 70`

### Tower Mode Indices

This is a list of all the possible tower configurations and their respective index in the best tower clear time array.

- **Hunter/5/Normal**: 0
- **Hunter/5/Hard**: 1
- **Hunter/5/Expert**: 2

---

- **Hunter/10/Normal**: 3
- **Hunter/10/Hard**: 4
- **Hunter/10/Expert**: 5

---

- **Hunter/25/Normal**: 6
- **Hunter/25/Hard**: 7
- **Hunter/25/Expert**: 8

---

- **Hunter/Endless/Normal** (Unused): 9
- **Hunter/Endless/Hard** (Unused): 10
- **Hunter/Endless/Expert** (Unused): 11

---

- **Rush/5/Normal**: 12
- **Rush/5/Hard**: 13
- **Rush/5/Expert**: 14

---

- **Rush/10/Normal**: 15
- **Rush/10/Hard**: 16
- **Rush/10/Expert**: 17

---

- **Rush/25/Normal**: 18
- **Rush/25/Hard**: 19
- **Rush/25/Expert**: 20

---

- **Rush/Endless/Normal** (Unused): 21
- **Rush/Endless/Hard** (Unused): 22
- **Rush/Endless/Expert** (Unused): 23

---

- **Polterpup/5/Normal**: 24
- **Polterpup/5/Hard**: 25
- **Polterpup/5/Expert**: 26

---

- **Polterpup/10/Normal**: 27
- **Polterpup/10/Hard**: 28
- **Polterpup/10/Expert**: 29

---

- **Polterpup/25/Normal**: 30
- **Polterpup/25/Hard**: 31
- **Polterpup/25/Expert**: 32

---

- **Polterpup/Endless/Normal** (Unused): 33
- **Polterpup/Endless/Hard** (Unused): 34
- **Polterpup/Endless/Expert** (Unused): 35

---

- **Surprise/5/Normal**: 36
- **Surprise/5/Hard**: 37
- **Surprise/5/Expert**: 38

---

- **Surprise/10/Normal**: 39
- **Surprise/10/Hard**: 40
- **Surprise/10/Expert**: 41

---

- **Surprise/25/Normal**: 42
- **Surprise/25/Hard**: 43
- **Surprise/25/Expert**: 44

---

- **Surprise/Endless/Normal** (Unused): 45
- **Surprise/Endless/Hard** (Unused): 46
- **Surprise/Endless/Expert** (Unused): 47

### CRC Checksums

The save file contains **four** checksums, two for each section. Each checksum is 32-bits (4 bytes), and like every other numerical value, is stored in little endian order.

Each section has one "Data CRC" and one "Version CRC". The Version CRC is constant across all save files: `0x7B, 0x0C, 0x27, 0x49` for the title screen section and `0xAD, 0x03, 0x32, 0xD4` for the in-game section. The Data CRC is more interesting and is the part of the reverse engineering that by far took the longest to figure out. It is computed using all of the data stored within the relevant section of the save file (including the Version CRCs) and is used to detect corruption/modification.

If any of these four checksums fail to match the expected values, the game will refuse to load the save file. If it is the title screen section that is corrupt, then the profile will be labelled "Corrupted" on the title screen before even selecting it. If it is the in-game section that is corrupted, the message saying as such will not be displayed until you attempt to load the corrupted save file.

The game does not fully compute the Data CRC on-the-fly, instead it utilises a 1024-byte long lookup table stored within the game executable in order to calculate the checksum of the save file.

The game's CRC generation algorithm looks something like this (decompiled C code generated by Ghidra, generic names replaced with more descriptive ones by myself):

```c
// .text:002b4b98h (file offset: 0x002c4b98)
void GenerateCRC(uint *destination, uint *source, uint byteCount)
{
  uint toProcess;
  uint crcPart;
  
  for (; ((uint)source & 3) != 0; source = (uint *)((int)source + 1)) {
    if (byteCount == 0) goto no_more_bytes;
    byteCount = byteCount - 1;
    *destination = *(uint *)(&CRCTable + ((uint)*(byte *)source ^ *destination & 0xff) * 4) ^
                   *destination >> 8;
  }
  for (; 3 < byteCount; byteCount = byteCount - 4) {
    toProcess = *source;
    crcPart = *(uint *)(&CRCTable + (toProcess & 0xff ^ *destination & 0xff) * 4) ^
              *destination >> 8;
    *destination = crcPart;
    crcPart = *(uint *)(&CRCTable + (toProcess >> 8 & 0xff ^ crcPart & 0xff) * 4) ^ crcPart >> 8;
    *destination = crcPart;
    crcPart = *(uint *)(&CRCTable + (toProcess >> 0x10 & 0xff ^ crcPart & 0xff) * 4) ^ crcPart >> 8;
    *destination = crcPart;
    *destination = *(uint *)(&CRCTable + (toProcess >> 0x18 ^ crcPart & 0xff) * 4) ^ crcPart >> 8;
    source = source + 1;
  }
no_more_bytes:
  for (; byteCount != 0; byteCount = byteCount - 1) {
    *destination = *(uint *)(&CRCTable + ((uint)*(byte *)source ^ *destination & 0xff) * 4) ^
                   *destination >> 8;
    source = (uint *)((int)source + 1);
  }
  return;
}
```

However this algorithm is longer than it needs to be, and the entire algorithm can be shortened whilst retaining identical behaviour. This is my re-implementation in C#:

```csharp
public static uint CalculateChecksum(Span<byte> source, uint initial = 0)
{
    uint crc = initial;
    foreach (byte toProcess in source)
    {
        int lookupIndex = (int)((toProcess ^ (crc & 0xff)) * 4);
        crc = LookupCRCTable(lookupIndex) ^ (crc >> 8);
    }
    return crc;
}

private static uint LookupCRCTable(int lookupIndex)
{
    return BinaryPrimitives.ReadUInt32LittleEndian(crcTable.AsSpan()[lookupIndex..(lookupIndex + 4)]);
}
```

(The first two `for` loops have been completely omitted, as simply running the final one alone will produce the same result).

The full lookup table can be found in [SaveTools/CRC.cs](https://github.com/TollyH/LM2_SaveEditor/blob/main/SaveTools/CRC.cs) if you're curious, though it is just one long list of bytes. If you want to find it in an original dumped binary, it can be found at `.data:00022904h` (file offset `0x0082b904`).
