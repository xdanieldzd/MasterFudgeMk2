using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Globalization;

using MasterFudgeMk2.Common.AudioBackend;
using MasterFudgeMk2.Common.VideoBackend;
using MasterFudgeMk2.Common.XInput;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2
{
    public partial class MainForm : Form
    {
        const int maxRecentFiles = 12;

        EmulatorConfiguration emuConfig;

        Stopwatch stopWatch;
        long startTime, frameCounter, interval;
        double framesPerSecond;

        List<Keys> keysPressed;
        Controller controller;
        FileInfo romFileInfo;

        Common.UiStateBoolean emulationIsInitialized;
        bool emulationIsPaused;
        IMachineManager machineManager;
        Rectangle outputViewport;
        Bitmap outputBitmap;
        ISoundOutput soundOutput;

        public bool LimitFps
        {
            get { return emuConfig.LimitFps; }
            set { emuConfig.LimitFps = value; }
        }

        public bool MuteSound
        {
            get { return (emuConfig.MuteSound = (soundOutput?.Volume == 0.0f)); }
            set { if (soundOutput != null) soundOutput.Volume = ((emuConfig.MuteSound = value) ? 0.0f : 1.0f); }
        }

        public bool KeepAspectRatio
        {
            get { return (emuConfig.KeepAspectRatio = scScreen.KeepAspectRatio); }
            set { emuConfig.KeepAspectRatio = scScreen.KeepAspectRatio = value; }
        }

        public bool EmulationIsPaused
        {
            get { return emulationIsPaused; }
            set
            {
                if ((emulationIsPaused = value) == true) machineManager?.Pause();
                else machineManager?.Unpause();
                SetFormText();
            }
        }

        public MainForm()
        {
            InitializeComponent();

            emuConfig = new EmulatorConfiguration();

            stopWatch = new Stopwatch();
            stopWatch.Start();
            startTime = frameCounter = 0;
            framesPerSecond = 0.0;

            keysPressed = new List<Keys>();
            KeyDown += MainForm_KeyDown;
            KeyUp += MainForm_KeyUp;
            controller = ControllerManager.GetController(0);

            menuStrip.MenuActivate += (s, e) => { keysPressed.Clear(); };

            emulationIsInitialized = new Common.UiStateBoolean(false);

            soundOutput = new NAudioOutput(44100, 1);
            soundOutput = new WavFileSoundOutput(44100, 1);

            soundOutput.Stop();

            Application.Idle += (s, e) => { while (Common.NativeMethods.IsApplicationIdle()) { StepMachine(); } };

            PrepareUserInterface();

            // DEBUG SHORTCUTS HO
            if (Environment.MachineName == "NANAMI-X")
            {
                //StartMachine(typeof(Machines.Sega.MasterSystem.Manager));
                //StartMachine(typeof(Machines.Coleco.ColecoVision.Manager));

                //LoadMedia(@"D:\ROMs\SG1000\Bank Panic (Japan).sg");
                //LoadMedia(@"D:\ROMs\SG1000\Othello (Japan).sg");
                //LoadMedia(@"D:\ROMs\SG1000\Castle, The (Japan).sg");
                //LoadMedia(@"D:\ROMs\SG1000\Girl's Garden (Japan).sg");
                //LoadMedia(@"D:\ROMs\SG1000\Hang-On II (Japan).sg");
                //LoadMedia(@"D:\ROMs\SG1000\Bomb Jack (Japan).sg");

                //LoadMedia(@"D:\ROMs\SMS\F16_Fighting_Falcon_(UE)_[!].sms");
                //LoadMedia(@"D:\ROMs\SMS\Dr._Robotnik's_Mean_Bean_Machine_(UE)_[!].sms");
                //LoadMedia(@"D:\ROMs\SMS\Sonic_the_Hedgehog_(UE)_[!].sms");
                //LoadMedia(@"D:\ROMs\SMS\VDPTEST.sms");
                //LoadMedia(@"D:\ROMs\SMS\zexdoc.sms");
                //LoadMedia(@"D:\ROMs\SMS\zexdoc_sdsc.sms");
                //LoadMedia(@"D:\ROMs\SMS\Choplifter_(UE)_[!].sms");
                //LoadMedia(@"D:\ROMs\SMS\SMS Sound Test 1.1.sms");
                //LoadMedia(@"D:\ROMs\SMS\Alex_Kidd_in_Miracle_World_(UE)_[!].sms");

                //LoadMedia(@"D:\ROMs\GG\Fantasy_Zone_Gear_(JUE).gg");
                //LoadMedia(@"D:\ROMs\GG\Gunstar_Heroes_(J).gg");
                //LoadMedia(@"D:\ROMs\GG\Sonic_the_Hedgehog_(JUE).gg");
                //LoadMedia(@"D:\ROMs\GG\Coca Cola Kid (Japan).gg");
                //LoadMedia(@"D:\ROMs\GG\Puyo_Puyo_2_(J)_[!].gg");
                //LoadMedia(@"D:\ROMs\GG\Puzlow_Kids_(Puyo_Puyo)_(J).gg");
                //LoadMedia(@"D:\ROMs\GG\GG_Shinobi_(E)_[!].gg");

                //LoadMedia(@"D:\ROMs\ColecoVision\Popeye (1983) (Parker Bros).col");
                //LoadMedia(@"D:\ROMs\ColecoVision\Burgertime (1982-84) (Data East).col");
                //LoadMedia(@"D:\ROMs\ColecoVision\Boulder Dash (1984) (Micro Fun).col");
                //LoadMedia(@"D:\ROMs\ColecoVision\Frogger (1982-83) (Parker Bros).col");
                //LoadMedia(@"D:\ROMs\ColecoVision\Kevtris by Kevin Horton (1996) (PD).col");
                //LoadMedia(@"D:\ROMs\ColecoVision\Smurf - Rescue in Gargamel's Castle (1982).col");
                //LoadMedia(@"D:\ROMs\ColecoVision\Smurf - Paint 'n Play Workshop (1983).col");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();

                if (outputBitmap != null) outputBitmap.Dispose();
                if (soundOutput != null)
                {
                    if (soundOutput is WavFileSoundOutput)
                    {
                        (soundOutput as WavFileSoundOutput).Save(@"E:\temp\sms\new\test.wav");
                    }
                    soundOutput.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            machineManager?.Shutdown();
        }

        private void PrepareUserInterface()
        {
            /* Set various UI text */
            SetFormText();

            /* Set up databindings for settings */
            limitFPSToolStripMenuItem.DataBindings.Add(nameof(limitFPSToolStripMenuItem.Checked), emuConfig, nameof(emuConfig.LimitFps), false, DataSourceUpdateMode.OnPropertyChanged);
            muteSoundToolStripMenuItem.DataBindings.Add(nameof(muteSoundToolStripMenuItem.Checked), this, nameof(MuteSound), false, DataSourceUpdateMode.OnPropertyChanged);
            keepAspectRatioToolStripMenuItem.DataBindings.Add(nameof(keepAspectRatioToolStripMenuItem.Checked), emuConfig, nameof(emuConfig.KeepAspectRatio), false, DataSourceUpdateMode.OnPropertyChanged);
            autoResizeWindowToolStripMenuItem.DataBindings.Add(nameof(autoResizeWindowToolStripMenuItem.Checked), emuConfig, nameof(emuConfig.AutoResize), false, DataSourceUpdateMode.OnPropertyChanged);

            /* Databindings for initialized */
            takeScreenshotToolStripMenuItem.DataBindings.Add(nameof(takeScreenshotToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);
            pauseToolStripMenuItem.DataBindings.Add(nameof(pauseToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);
            resetToolStripMenuItem.DataBindings.Add(nameof(resetToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);
            infoToolStripMenuItem.DataBindings.Add(nameof(infoToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);
            configToolStripMenuItem.DataBindings.Add(nameof(configToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);

            /* Databindings for paused */
            pauseToolStripMenuItem.DataBindings.Add(nameof(pauseToolStripMenuItem.Checked), this, nameof(EmulationIsPaused), false, DataSourceUpdateMode.OnPropertyChanged);

            /* Some stuff we'll need... */
            List<string> filters = new List<string>();
            List<string> extensions = new List<string>();

            /* Fetch and iterate over machine types */
            foreach (Type machineType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IMachineManager).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).OrderBy(x => x.Name))
            {
                IMachineManager machine = (Activator.CreateInstance(machineType) as IMachineManager);

                /* Set up filters for file dialog */
                string machineFilter = machine.FileFilter;
                filters.Add(machineFilter);
                extensions.Add(machineFilter.Substring(machineFilter.LastIndexOf('*')));

                /* Set up "boot without media" menu */
                if (machine.SupportsBootingWithoutMedia)
                {
                    ToolStripMenuItem machineBootMenuItem = new ToolStripMenuItem() { Text = machine.FriendlyName, Tag = machineType };
                    machineBootMenuItem.Click += (s, e) => { StartMachine((s as ToolStripMenuItem).Tag as Type); };
                    bootWithoutMediaToolStripMenuItem.DropDownItems.Add(machineBootMenuItem);
                }

                machine.Shutdown();
            }

            filters.Insert(0, string.Format("All Supported ROMs ({0})|{0}", string.Join(";", extensions)));
            filters.Add("All Files (*.*)|*.*");
            ofdOpenRom.Filter = string.Join("|", filters);

            /* Update "boot without media" menu */
            UpdateBootWithoutMediaMenu();

            /* Set up "recent files" menu */
            if (emuConfig.RecentFiles == null)
            {
                emuConfig.RecentFiles = new string[maxRecentFiles];
                for (int i = 0; i < emuConfig.RecentFiles.Length; i++) emuConfig.RecentFiles[i] = string.Empty;
            }
            CleanUpRecentList();
            UpdateRecentFilesMenu();
        }

        private void SetFormText()
        {
            if (!emulationIsInitialized.IsTrue)
            {
                Text = Application.ProductName;
                tsslFps.Text = "";
                tsslStatus.Text = "Ready";
            }
            else
            {
                string mediaName = (romFileInfo != null && romFileInfo.Exists ? romFileInfo.Name : "[No Media]");
                Text = string.Format("{0} - {1} - {2}", Application.ProductName, machineManager.FriendlyShortName, mediaName);
                tsslStatus.Text = (emulationIsPaused ? "Paused" : "Running");
            }
        }

        public void UpdateBootWithoutMediaMenu()
        {
            foreach (ToolStripMenuItem machineBootMenuItem in bootWithoutMediaToolStripMenuItem.DropDownItems)
            {
                if (!(machineBootMenuItem.Tag is Type)) continue;

                Type machineType = (machineBootMenuItem.Tag as Type);
                if (!(typeof(IMachineManager).IsAssignableFrom(machineType) && !machineType.IsInterface)) continue;

                IMachineManager machine = (Activator.CreateInstance(machineType) as IMachineManager);
                machineBootMenuItem.Enabled = machine.CanCurrentlyBootWithoutMedia;
                machine.Shutdown();
            }
        }

        private void CleanUpRecentList()
        {
            List<string> files = emuConfig.RecentFiles.Where(x => x != string.Empty).ToList();
            while (files.Count < maxRecentFiles) files.Add(string.Empty);
            emuConfig.RecentFiles = files.Take(maxRecentFiles).ToArray();
        }

        private void AddFileToRecentList(string filename)
        {
            List<string> files = emuConfig.RecentFiles.Where(x => x != string.Empty).ToList();
            files.Reverse();

            /* Remove if already exists, so that adding it will make it the most recent entry */
            if (files.Contains(filename)) files.Remove(filename);

            files.Add(filename);
            files.Reverse();

            /* Pad with dummy values */
            while (files.Count < maxRecentFiles) files.Add(string.Empty);

            emuConfig.RecentFiles = files.Take(maxRecentFiles).ToArray();
        }

        private void UpdateRecentFilesMenu()
        {
            /* Recent files menu */
            var oldRecentItems = recentFilesToolStripMenuItem.DropDownItems.Cast<ToolStripItem>().Where(x => x is ToolStripMenuItem && x.Tag is int).ToList();
            foreach (ToolStripItem item in oldRecentItems)
                recentFilesToolStripMenuItem.DropDownItems.Remove(item);

            for (int i = 0; i < emuConfig.RecentFiles.Length; i++)
            {
                string recentFile = emuConfig.RecentFiles[i];

                if (recentFile == string.Empty)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem("-");
                    menuItem.ShortcutKeys = Keys.Control | (Keys.F1 + i);
                    menuItem.Enabled = false;
                    menuItem.Tag = -1;
                    recentFilesToolStripMenuItem.DropDownItems.Add(menuItem);
                }
                else
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(Path.GetFileName(recentFile));
                    menuItem.ShortcutKeys = Keys.Control | (Keys.F1 + i);
                    menuItem.Tag = i;
                    menuItem.Click += ((s, ev) =>
                    {
                        int fileNumber = (int)(s as ToolStripMenuItem).Tag;
                        string filePath = emuConfig.RecentFiles[fileNumber];
                        if (!File.Exists(filePath))
                        {
                            MessageBox.Show("Selected file does not exist anymore; it will be removed from the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            emuConfig.RecentFiles[fileNumber] = string.Empty;
                            CleanUpRecentList();
                            UpdateRecentFilesMenu();
                        }
                        else
                            LoadMedia(filePath);
                    });
                    recentFilesToolStripMenuItem.DropDownItems.Add(menuItem);
                }
            }
        }

        public void StartMachine(Type machineType)
        {
            if (machineType == null) return;

            outputViewport = Rectangle.Empty;
            soundOutput?.Stop();

            romFileInfo = null;

            machineManager = (Activator.CreateInstance(machineType) as IMachineManager);
            machineManager.ScreenResize += MachineManager_OnScreenResize;
            machineManager.RenderScreen += MachineManager_OnRenderScreen;
            machineManager.ScreenViewportChange += MachineManager_OnScreenViewportChange;
            machineManager.PollInput += MachineManager_OnPollInput;

            machineManager.AddSampleData += MachineManager_OnAddSampleData;

            machineManager.Startup();

            emulationIsInitialized.IsTrue = (machineManager != null);

            SetFormText();

            interval = (long)TimeSpan.FromSeconds(1.0 / machineManager.RefreshRate).TotalMilliseconds;

            soundOutput?.Play();
        }

        public void LoadMedia(string filename)
        {
            LoadMedia(new FileInfo(filename));
        }

        public void LoadMedia(FileInfo fileInfo)
        {
            StartMachine(MachineLoader.DetectMachine(fileInfo));

            IMedia media = MediaLoader.LoadMedia(machineManager, fileInfo);
            machineManager?.LoadMedia(media);
            machineManager?.Reset();

            soundOutput?.Reset();

            romFileInfo = fileInfo;

            AddFileToRecentList(fileInfo.FullName);
            UpdateRecentFilesMenu();

            SetFormText();
            tsslStatus.Text = "ROM loaded";
        }

        private void StepMachine()
        {
            if (machineManager == null || EmulationIsPaused) return;

            startTime = stopWatch.ElapsedMilliseconds;
            machineManager.RunFrame();

            while (LimitFps && (stopWatch.ElapsedMilliseconds - startTime) < interval)
                Thread.Sleep(1);

            frameCounter++;
            double timeDifference = (stopWatch.ElapsedMilliseconds - startTime);
            if (timeDifference >= 1.0)
            {
                framesPerSecond = (frameCounter / (timeDifference / 1000));
                frameCounter = 0;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!keysPressed.Contains(e.KeyCode))
                keysPressed.Add(e.KeyCode);
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (keysPressed.Contains(e.KeyCode))
                keysPressed.Remove(e.KeyCode);
        }

        private void MachineManager_OnScreenResize(object sender, ScreenResizeEventArgs e)
        {
            outputBitmap?.Dispose();
            outputBitmap = new Bitmap(e.Width, e.Height, PixelFormat.Format32bppArgb);

            if (outputViewport == Rectangle.Empty) outputViewport = new Rectangle(0, 0, e.Width, e.Height);

            scScreen.OutputBitmap = outputBitmap;
            scScreen.Viewport = outputViewport;
        }

        private void MachineManager_OnRenderScreen(object sender, RenderScreenEventArgs e)
        {
            BitmapData bmpData = outputBitmap.LockBits(new Rectangle(0, 0, e.Width, e.Height), ImageLockMode.WriteOnly, outputBitmap.PixelFormat);

            byte[] pixelData = new byte[bmpData.Stride * bmpData.Height];
            Buffer.BlockCopy(e.FrameData, 0, pixelData, 0, pixelData.Length);
            Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);

            outputBitmap.UnlockBits(bmpData);

            scScreen.Invalidate();
            scScreen.Update();

            tsslFps.Text = string.Format(CultureInfo.InvariantCulture, "{0:##.##} FPS", framesPerSecond);
        }

        private void MachineManager_OnScreenViewportChange(object sender, ScreenViewportChangeEventArgs e)
        {
            outputViewport = new Rectangle(e.X, e.Y, e.Width, e.Height);
            scScreen.Viewport = outputViewport;

            if (emuConfig.AutoResize)
            {
                FormWindowState oldWindowState = WindowState;
                WindowState = FormWindowState.Normal;

                SuspendLayout();
                {
                    Size formSize = ClientSize;
                    Size oldScreenSize = scScreen.ClientSize;

                    formSize.Width = ((formSize.Width - oldScreenSize.Width) + (e.Width * 2));
                    formSize.Height = ((formSize.Height - oldScreenSize.Height) + (e.Height * 2));

                    ClientSize = formSize;
                    oldScreenSize = scScreen.ClientSize;

                    WindowState = oldWindowState;
                }
                ResumeLayout();
            }
        }

        private void MachineManager_OnPollInput(object sender, PollInputEventArgs e)
        {
            controller.Update();

            List<Enum> pressed = new List<Enum>();
            pressed.AddRange(Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(x => (x != Keys.None) && keysPressed.Contains(x)).Cast<Enum>());
            pressed.AddRange(Enum.GetValues(typeof(Buttons)).Cast<Buttons>().Where(x => (x != Buttons.None) && (controller?.Buttons & x) == x).Cast<Enum>());
            if (!pressed.Contains(Buttons.DPadLeft) && controller?.LeftThumbstick?.X < -XInputGamepad.LeftThumbDeadzone) pressed.Add(Buttons.DPadLeft);
            if (!pressed.Contains(Buttons.DPadDown) && controller?.LeftThumbstick?.Y < -XInputGamepad.LeftThumbDeadzone) pressed.Add(Buttons.DPadDown);
            if (!pressed.Contains(Buttons.DPadRight) && controller?.LeftThumbstick?.X > XInputGamepad.LeftThumbDeadzone) pressed.Add(Buttons.DPadRight);
            if (!pressed.Contains(Buttons.DPadUp) && controller?.LeftThumbstick?.Y > XInputGamepad.LeftThumbDeadzone) pressed.Add(Buttons.DPadUp);
            e.Pressed = pressed;

            if (emuConfig.DebugMode)
            {
                if (controller.IsConnected)
                {
                    if (controller.IsLeftShoulderPressed())
                        controller.Vibrate(controller.LeftTrigger, controller.RightTrigger, TimeSpan.FromSeconds(1));

                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Connected? {0}, Index: {1}\n", controller.IsConnected, controller.UserIndex);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "L-Thumb: {0}  R-Thumb: {1}  L-Trig: {2}  R-Trig: {3}\n", controller.LeftThumbstick, controller.RightThumbstick, controller.LeftTrigger, controller.RightTrigger);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "Buttons: {0}\n", controller.Buttons);
                    scScreen.Text = sb.ToString();
                }
                else
                {
                    scScreen.Text = "no controller";
                }
            }
        }

        private void MachineManager_OnAddSampleData(object sender, AddSampleDataEventArgs e)
        {
            soundOutput.AddSampleData(e.Samples);
        }

        private void openROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            machineManager?.Pause();
            soundOutput?.Stop();

            if (ofdOpenRom.ShowDialog() == DialogResult.OK)
            {
                LoadMedia(ofdOpenRom.FileName);
            }
            else
            {
                machineManager?.Unpause();
                if (machineManager != null) soundOutput?.Play();
            }
        }

        private void clearListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < emuConfig.RecentFiles.Length; i++)
                emuConfig.RecentFiles[i] = string.Empty;

            UpdateRecentFilesMenu();
        }

        private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Bitmap screenshot = new Bitmap(outputViewport.Width, outputViewport.Height))
            {
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.DrawImage(outputBitmap, new Rectangle(0, 0, screenshot.Width, screenshot.Height), outputViewport, GraphicsUnit.Pixel);
                }
                screenshot?.Save(@"E:\temp\sms\new\temp.png");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            machineManager?.Reset();
            soundOutput?.Reset();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (machineManager == null) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(CultureInfo.InvariantCulture, "Type: {0}\n", machineManager.GetType().FullName);
            sb.AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Friendly name (long): {0}\n", machineManager.FriendlyName);
            sb.AppendFormat(CultureInfo.InvariantCulture, "Friendly name (short): {0}\n", machineManager.FriendlyShortName);
            sb.AppendFormat(CultureInfo.InvariantCulture, "File filter: {0}\n", machineManager.FileFilter);
            sb.AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Supports boot without media? {0}\n", machineManager.SupportsBootingWithoutMedia);
            sb.AppendFormat(CultureInfo.InvariantCulture, "Can currently boot without media? {0}\n", machineManager.CanCurrentlyBootWithoutMedia);
            sb.AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Refresh rate: {0:##.####} Hz\n", machineManager.RefreshRate);
            sb.AppendLine();
            foreach (Tuple<string, Type, double> chip in machineManager.DebugChipInformation)
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1} at {2:##.####} MHz\n", chip.Item1, chip.Item2.FullName, (chip.Item3 / 1000) / 1000);

            MessageBox.Show(sb.ToString(), "System Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (machineManager == null) return;

            machineManager.Pause();
            using (ConfigForm configForm = new ConfigForm(machineManager))
            {
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    machineManager.Configuration = configForm.Configuration;
                    machineManager.Configuration.InputConfig.ConfigSource.Save();

                    UpdateBootWithoutMediaMenu();
                }
            }
            machineManager.Unpause();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Format("{0} by {1}", Application.ProductName, Application.CompanyName), "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
