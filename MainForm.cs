using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace o_manager
{
    public partial class MainForm : Form
    {
        string osuPath;
        string otdSettingsPath;
        string otdExePath;
        string currentWindowsUser;

        string configsRoot;
        string backupRoot;
        string indexFile;

        ConfigIndex configIndex;

        public MainForm()
        {
            InitializeComponent();

            configsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
            backupRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            indexFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configs.json");

            Directory.CreateDirectory(configsRoot);
            Directory.CreateDirectory(backupRoot);

            currentWindowsUser = Environment.UserName;

            Log("Detecting osu! stable...");
            osuPath = DetectOsuStable();

            Log("Detecting OpenTabletDriver settings...");
            otdSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenTabletDriver");
            if (!Directory.Exists(otdSettingsPath))
                otdSettingsPath = null;

            Log("Searching for OpenTabletDriver executable...");
            otdExePath = DetectOtdExe();

            Log($"osu! path: {(osuPath ?? "NOT FOUND")}");
            Log($"OTD settings path: {(otdSettingsPath ?? "NOT FOUND")}");
            Log($"OTD executable path: {(otdExePath ?? "NOT FOUND")}");

            LoadConfigIndex();
            RefreshDropdown();
        }

        private void Log(string msg)
        {
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {msg}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        // ================== osu! stable detection ==================
        private string DetectOsuStable()
        {
            string path = null;

            Process[] processes = Process.GetProcessesByName("osu!");
            foreach (var proc in processes)
            {
                try
                {
                    string exePath = proc.MainModule.FileName;
                    string dir = Path.GetDirectoryName(exePath);
                    if (IsOsuStableDirectory(dir))
                        return dir;
                }
                catch { }
            }

            try
            {
                string key = "HKEY_CURRENT_USER\\Software\\ppy\\osu!";
                string installPath = (string)Registry.GetValue(key, "InstallPath", null);
                if (!string.IsNullOrEmpty(installPath) && IsOsuStableDirectory(installPath))
                    return installPath;
            }
            catch { }

            string local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "osu!");
            if (Directory.Exists(local) && IsOsuStableDirectory(local))
                return local;

            return null;
        }

        private bool IsOsuStableDirectory(string dir)
        {
            string userCfg = $"osu!.{currentWindowsUser}.cfg";
            return File.Exists(Path.Combine(dir, "osu!.db")) &&
                   File.Exists(Path.Combine(dir, userCfg));
        }

        // ================== OpenTabletDriver executable detection ==================
        private string DetectOtdExe()
        {
            string[] exeNames = {
                "OpenTabletDriver.Console.exe",
                "OpenTabletDriver.Daemon.exe",
                "OpenTabletDriver.UX.Wpf.exe"
            };

            List<string> roots = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), // settings folder
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            // Include all other fixed drives
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                    roots.Add(drive.RootDirectory.FullName);
            }

            foreach (var root in roots)
            {
                string found = SearchSubfolders(root, exeNames, 3);
                if (!string.IsNullOrEmpty(found))
                    return found;
            }

            return null;
        }

        private string SearchSubfolders(string root, string[] exeNames, int maxDepth, int currentDepth = 0)
        {
            if (currentDepth > maxDepth) return null;

            try
            {
                foreach (var file in Directory.GetFiles(root))
                {
                    if (exeNames.Contains(Path.GetFileName(file), StringComparer.OrdinalIgnoreCase))
                        return file;
                }

                foreach (var dir in Directory.GetDirectories(root))
                {
                    string found = SearchSubfolders(dir, exeNames, maxDepth, currentDepth + 1);
                    if (!string.IsNullOrEmpty(found))
                        return found;
                }
            }
            catch { }

            return null;
        }

        // ================== Config Handling ==================
        private void LoadConfigIndex()
        {
            if (File.Exists(indexFile))
            {
                string json = File.ReadAllText(indexFile);
                configIndex = JsonConvert.DeserializeObject<ConfigIndex>(json);
            }
            else
            {
                configIndex = new ConfigIndex { Configs = new List<string>() };
                SaveConfigIndex();
            }
        }

        private void SaveConfigIndex()
        {
            string json = JsonConvert.SerializeObject(configIndex, Formatting.Indented);
            File.WriteAllText(indexFile, json);
        }

        private void RefreshDropdown()
        {
            cmbConfigs.Items.Clear();
            foreach (var c in configIndex.Configs)
                cmbConfigs.Items.Add(c);

            if (cmbConfigs.Items.Count > 0)
                cmbConfigs.SelectedIndex = 0;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            string configName = Microsoft.VisualBasic.Interaction.InputBox("Enter a name for this config:", "Export Config", "MyConfig");
            if (string.IsNullOrWhiteSpace(configName)) return;

            string configDir = Path.Combine(configsRoot, configName);

            if (Directory.Exists(configDir))
            {
                var result = MessageBox.Show($"Config '{configName}' already exists. Overwrite?", "Confirm Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;

                Directory.Delete(configDir, true);
            }

            Directory.CreateDirectory(Path.Combine(configDir, "osu"));
            Directory.CreateDirectory(Path.Combine(configDir, "otd"));

            CopyOsuFiles(osuPath, Path.Combine(configDir, "osu"));
            CopyOtdFiles(otdSettingsPath, Path.Combine(configDir, "otd"));

            if (!configIndex.Configs.Contains(configName))
                configIndex.Configs.Add(configName);

            SaveConfigIndex();
            RefreshDropdown();

            Log($"Exported config '{configName}'.");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (cmbConfigs.SelectedItem == null) { Log("Select a config first."); return; }

            string configName = cmbConfigs.SelectedItem.ToString();
            string configDir = Path.Combine(configsRoot, configName);
            if (!Directory.Exists(configDir)) { Log("Config not found."); return; }

            string backupTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupDir = Path.Combine(backupRoot, backupTime);
            Directory.CreateDirectory(Path.Combine(backupDir, "osu"));
            Directory.CreateDirectory(Path.Combine(backupDir, "otd"));

            BackupOsu(backupDir);
            BackupOtd(backupDir);

            // Update osu CFG filename to current user
            string oldCfg = Path.Combine(configDir, "osu", $"osu!.{currentWindowsUser}.cfg");
            string[] existingCfgs = Directory.GetFiles(Path.Combine(configDir, "osu"), "osu!.*.cfg");
            if (existingCfgs.Length > 0)
            {
                string srcCfg = existingCfgs[0];
                string dstCfg = oldCfg;
                SafeFileCopy(srcCfg, dstCfg);
            }

            CopyOsuFiles(Path.Combine(configDir, "osu"), osuPath);
            CopyOtdFiles(Path.Combine(configDir, "otd"), otdSettingsPath);

            RestartOTD();

            Log($"Loaded config '{configName}'. Backup saved.");
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            var backups = Directory.GetDirectories(backupRoot);
            if (backups.Length == 0) { Log("No backups available."); return; }

            Array.Sort(backups);
            string latestBackup = backups[^1];

            CopyOsuFiles(Path.Combine(latestBackup, "osu"), osuPath);
            CopyOtdFiles(Path.Combine(latestBackup, "otd"), otdSettingsPath);

            RestartOTD();

            Log($"Restored backup from {Path.GetFileName(latestBackup)}.");
        }

        // ================== Helpers ==================
        private void CopyOsuFiles(string srcDir, string dstDir)
        {
            if (string.IsNullOrEmpty(srcDir) || string.IsNullOrEmpty(dstDir)) return;
            Directory.CreateDirectory(dstDir);

            string userCfg = $"osu!.{currentWindowsUser}.cfg";
            string[] osuFiles = { "osu!.db", userCfg, "collection.db" };

            foreach (var f in osuFiles)
            {
                string src = Path.Combine(srcDir, f);
                if (File.Exists(src))
                {
                    string dst = Path.Combine(dstDir, f);
                    SafeFileCopy(src, dst);
                }
            }
        }

        private void CopyOtdFiles(string srcDir, string dstDir)
        {
            if (string.IsNullOrEmpty(srcDir) || string.IsNullOrEmpty(dstDir)) return;
            Directory.CreateDirectory(dstDir);

            string[] otdFiles = { "settings.json", "tablet-data.txt" };
            foreach (var f in otdFiles)
            {
                string src = Path.Combine(srcDir, f);
                if (File.Exists(src))
                {
                    string dst = Path.Combine(dstDir, f);
                    SafeFileCopy(src, dst);
                }
            }
        }

        private void BackupOsu(string backupDir)
        {
            if (osuPath != null)
            {
                string backupOsu = Path.Combine(backupDir, "osu");
                CopyOsuFiles(osuPath, backupOsu);
            }
        }

        private void BackupOtd(string backupDir)
        {
            if (otdSettingsPath != null)
            {
                string backupOtd = Path.Combine(backupDir, "otd");
                CopyOtdFiles(otdSettingsPath, backupOtd);
            }
        }

        private void RestartOTD()
        {
            if (string.IsNullOrEmpty(otdExePath)) return;

            string daemonExe = Path.Combine(Path.GetDirectoryName(otdExePath), "OpenTabletDriver.Daemon.exe");

            foreach (var proc in Process.GetProcessesByName("OpenTabletDriver.Console")
                .Concat(Process.GetProcessesByName("OpenTabletDriver.UX.Wpf"))
                .Concat(Process.GetProcessesByName("OpenTabletDriver.Daemon")))
            {
                try { proc.Kill(); proc.WaitForExit(); } catch { }
            }

            if (!File.Exists(daemonExe))
            {
                Log("OpenTabletDriver Daemon not found.");
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = daemonExe,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(daemonExe)
                };
                Process.Start(psi);
                Log("OpenTabletDriver restarted successfully (daemon).");
            }
            catch (Exception ex)
            {
                Log($"Failed to restart OpenTabletDriver: {ex.Message}");
            }
        }

        // Safe file copy to prevent locks
        private void SafeFileCopy(string src, string dst)
        {
            try
            {
                using (var sourceStream = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var destStream = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    sourceStream.CopyTo(destStream);
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to copy '{Path.GetFileName(src)}': {ex.Message}");
            }
        }

        // Designer required empty handlers
        private void txtLog_TextChanged(object sender, EventArgs e) { }
        private void cmbConfigs_SelectedIndexChanged(object sender, EventArgs e) { }
    }

    public class ConfigIndex
    {
        public List<string> Configs { get; set; }
    }
}
