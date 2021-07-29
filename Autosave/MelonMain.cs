using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using MelonLoader;
using System.Diagnostics;
using System.IO;

namespace Autosave
{
    public class MelonMain : BloonsTD6Mod
    {
        public ModSettingBool openBackupDir = new ModSettingBool(true);
        public ModSettingBool openSaveDir = new ModSettingBool(true);
        public ModSettingString autosavePath = new ModSettingString("");
        public ModSettingInt timeBetweenBackup = new ModSettingInt(5);
        public ModSettingInt maxSavedBackups = new ModSettingInt(10);

        public const string currentVersion = "1.0.0";
        public override bool CheatMod => false;
        public static MelonMain instance;
        BackupCreator backup;
        bool initialized;

        public override void OnApplicationStart()
        {
            instance = this;
            MelonLogger.Msg("Mod has finished loading");
        }

        public override void OnUpdate()
        {
            if (Game.instance)
                return;

            if (InGame.instance is null)
                return;
        }
        
        public override void OnMainMenu()
        {
            if (initialized)
                return;

            InitSettings();

            backup = new BackupCreator(autosavePath, maxSavedBackups);

            ScheduleAutosave();
            initialized = true;
        }

        public override void OnMatchEnd()
        {
            backup.CreateBackup();
        }

        void InitSettings()
        {
            openBackupDir.IsButton = true;
            openBackupDir.SetName("Open Backup Directory");
            openBackupDir.OnInitialized.Add((option) => InitOpenBackupButton(option));

            timeBetweenBackup.SetName("Minutes Between Each Backup");
            maxSavedBackups.SetName("Max Saved Backups");
            maxSavedBackups.OnValueChanged.Add((newMax) => backup.SetMaxBackups(newMax));

            if (string.IsNullOrEmpty(autosavePath))
            {
                Directory.CreateDirectory(this.GetModDirectory());
                autosavePath.SetValue(this.GetModDirectory());
            }
            autosavePath.SetName("Backup Directory");
            autosavePath.OnValueChanged.Add((newPath) =>
            {
                Directory.CreateDirectory(newPath);
                backup.MoveBackupDir(newPath);
            });
        }

        void ScheduleAutosave()
        {
            TaskScheduler.ScheduleTask(() => TriggerAutosave(),
                ScheduleType.WaitForSeconds, timeBetweenBackup);
        }

        void TriggerAutosave()
        {
            backup.CreateBackup();
            ScheduleAutosave();
        }

        void InitOpenBackupButton(SharedOption option)
        {
            var button = (ButtonOption)option;
            button.ButtonText.text = "Open";
            button.Button.onClick.AddListener(() => 
                Process.Start((string)autosavePath.GetValue()));
        }
    }
}