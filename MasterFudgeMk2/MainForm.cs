using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.IO.Compression;

using MasterFudgeMk2.AudioBackends;
using MasterFudgeMk2.VideoBackends;
using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Common.XInput;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2
{
    // TODO: slim down & clean up form code, it's getting stupid again!

    public partial class MainForm : Form
    {
        const int maxRecentFiles = 12;

        string programNameVersion;
        EmulatorConfiguration emuConfig;

        Stopwatch stopWatch;
        long startTime, frameCounter, interval;
        double framesPerSecond;

        List<Keys> keysPressed;
        Controller controller;

        FileInfo romFileInfo;
        string romFriendlyName;

        UiStateBoolean emulationIsInitialized;
        bool emulationIsPaused;

        IMachineManager machineManager;
        IVideoBackend renderer;
        IAudioBackend soundOutput;

        List<string> tempFiles;

        public bool LimitFps
        {
            get { return emuConfig.LimitFps; }
            set { emuConfig.LimitFps = value; }
        }

        public bool ForceSquarePixels
        {
            get
            {
                if (renderer != null) renderer.ForceSquarePixels = emuConfig.ForceSquarePixels;
                return emuConfig.ForceSquarePixels;
            }
            set { renderer.ForceSquarePixels = emuConfig.ForceSquarePixels = value; }
        }

        public bool LinearInterpolation
        {
            get
            {
                if (renderer != null) renderer.LinearInterpolation = emuConfig.LinearInterpolation;
                return emuConfig.LinearInterpolation;
            }
            set { renderer.LinearInterpolation = emuConfig.LinearInterpolation = value; }
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

            Assembly assembly = Assembly.GetExecutingAssembly();
            var programName = (assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).FirstOrDefault() as AssemblyProductAttribute).Product;
            var programVersion = new Version((assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).FirstOrDefault() as AssemblyFileVersionAttribute).Version);
            programNameVersion = string.Format("{0} v{1}.{2}", programName, string.Format((programVersion.Major > 0 ? "{0}.{1:D2}" : "{1:D3}"), programVersion.Major, programVersion.Minor), programVersion.Build);

            emuConfig = ConfigFile.Load<EmulatorConfiguration>();

            stopWatch = new Stopwatch();
            stopWatch.Start();
            startTime = frameCounter = 0;
            framesPerSecond = 0.0;

            keysPressed = new List<Keys>();
            KeyDown += MainForm_KeyDown;
            KeyUp += MainForm_KeyUp;
            Resize += MainForm_Resize;
            controller = ControllerManager.GetController(0);

            menuStrip.MenuActivate += (s, e) => { keysPressed.Clear(); };

            emulationIsInitialized = new UiStateBoolean(false);

            PrepareUserInterface();

            InitializeVideoBackend(emuConfig.VideoBackend);
            InitializeAudioBackend(emuConfig.AudioBackend);

            PrepareDataBindings();

            soundOutput.Stop();

            tempFiles = new List<string>();

            Application.Idle += (s, e) => { while (Common.NativeMethods.IsApplicationIdle()) { StepMachine(); } };

            DebugBoot();
        }

        [Conditional("DEBUG")]
        private void DebugBoot()
        {
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

                //LoadMedia(@"D:\ROMs\NES\Super Mario Bros\Super Mario Bros. (JU) (PRG0) [!].nes");
                //LoadMedia(@"D:\ROMs\NES\ROM files\Devil World (Europe).nes");
                //LoadMedia(@"D:\ROMs\NES\nestest.nes");
                //LoadMedia(@"D:\ROMs\NES\ROM files\Mega Man 2 (USA).nes");
                //LoadMedia(@"D:\ROMs\NES\Yo! Noid (U).nes");

                LoadMedia(@"D:\No-Intro\Nintendo - Nintendo Entertainment System (2017-06-18)\Super Mario Bros. (World).zip");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();

                if (soundOutput != null)
                {
                    if (soundOutput is FileWriterBackend)
                    {
                        if (Directory.Exists(@"E:\temp\sms\new\"))
                            (soundOutput as FileWriterBackend).Save(@"E:\temp\sms\new\test.wav");
                    }
                    soundOutput.Dispose();
                }
                if (renderer != null) renderer.Dispose();
            }

            base.Dispose(disposing);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            machineManager?.Configuration.Save();
            machineManager?.Shutdown();

            emuConfig?.Save();

            foreach (var tempFile in tempFiles)
                File.Delete(tempFile);
        }

        private void PrepareDataBindings()
        {
            /* Set up databindings for settings */
            limitFPSToolStripMenuItem.DataBindings.Add(nameof(limitFPSToolStripMenuItem.Checked), emuConfig, nameof(emuConfig.LimitFps), false, DataSourceUpdateMode.OnPropertyChanged);
            forceSquarePixelsToolStripMenuItem.DataBindings.Add(nameof(forceSquarePixelsToolStripMenuItem.Checked), this, nameof(ForceSquarePixels), false, DataSourceUpdateMode.OnPropertyChanged);
            linearInterpolationToolStripMenuItem.DataBindings.Add(nameof(linearInterpolationToolStripMenuItem.Checked), this, nameof(LinearInterpolation), false, DataSourceUpdateMode.OnPropertyChanged);

            /* Databindings for initialized */
            takeScreenshotToolStripMenuItem.DataBindings.Add(nameof(takeScreenshotToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);
            pauseToolStripMenuItem.DataBindings.Add(nameof(pauseToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);
            resetToolStripMenuItem.DataBindings.Add(nameof(resetToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);
            infoToolStripMenuItem.DataBindings.Add(nameof(infoToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);
            configToolStripMenuItem.DataBindings.Add(nameof(configToolStripMenuItem.Enabled), emulationIsInitialized, nameof(emulationIsInitialized.IsTrue), false, DataSourceUpdateMode.OnPropertyChanged);

            /* Databindings for paused */
            pauseToolStripMenuItem.DataBindings.Add(nameof(pauseToolStripMenuItem.Checked), this, nameof(EmulationIsPaused), false, DataSourceUpdateMode.OnPropertyChanged);

            /* Databindings for form size and location */
            DataBindings.Add("Location", emuConfig, nameof(emuConfig.WindowLocation), false, DataSourceUpdateMode.OnPropertyChanged);
            DataBindings.Add("Size", emuConfig, nameof(emuConfig.WindowSize), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private IEnumerable<Type> GetImplementationsFromAssembly(Type type)
        {
            if (!type.IsInterface) throw new Exception("Type is not interface");
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }

        private void PrepareUserInterface()
        {
            /* Set various UI text */
            SetFormText();

            /* Fetch and iterate over video & audio backend */
            foreach (Type videoBackendType in GetImplementationsFromAssembly(typeof(IVideoBackend)).OrderByDescending(x => x.Name.StartsWith("Null")).ThenBy(x => x.Name))
            {
                string videoBackendName = (videoBackendType.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute)?.Description;

                BindableToolStripMenuItem videoBackendMenuItem = new BindableToolStripMenuItem() { Text = (videoBackendName ?? videoBackendType.Name), Tag = videoBackendType, CheckOnClick = true };
                videoBackendMenuItem.Click += (s, e) =>
                {
                    InitializeVideoBackend(emuConfig.VideoBackend = (s as ToolStripMenuItem).Tag as Type);
                };
                videoBackendToolStripMenuItem.DropDownItems.Add(videoBackendMenuItem);
            }
            foreach (Type audioBackendType in GetImplementationsFromAssembly(typeof(IAudioBackend)).OrderByDescending(x => x.Name.StartsWith("Null")).ThenBy(x => x.Name))
            {
                string audioBackendName = (audioBackendType.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute)?.Description;

                BindableToolStripMenuItem audioBackendMenuItem = new BindableToolStripMenuItem() { Text = (audioBackendName ?? audioBackendType.Name), Tag = audioBackendType, CheckOnClick = true };
                audioBackendMenuItem.Click += (s, e) =>
                {
                    InitializeAudioBackend(emuConfig.AudioBackend = (s as ToolStripMenuItem).Tag as Type);
                };
                audioBackendToolStripMenuItem.DropDownItems.Add(audioBackendMenuItem);
            }

            /* Some stuff we'll need... */
            List<string> filters = new List<string>();
            List<string> extensions = new List<string>();

            /* Fetch and iterate over machine types */
            foreach (Type machineType in GetImplementationsFromAssembly(typeof(IMachineManager)).OrderBy(x => x.Name))
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
                emuConfig.RecentFiles = new List<string>(maxRecentFiles);
                for (int i = 0; i < emuConfig.RecentFiles.Count; i++) emuConfig.RecentFiles[i] = string.Empty;
            }
            CleanUpRecentList();
            UpdateRecentFilesMenu();

            /* Set common image format filter, for screenshot saving */
            sfdSaveScreenshot.SetCommonImageFilter("png");
        }

        private void InitializeVideoBackend(Type videoBackendType)
        {
            if (videoBackendType == null)
                emuConfig.VideoBackend = videoBackendType = GetImplementationsFromAssembly(typeof(IVideoBackend)).FirstOrDefault();

            if (renderer != null)
            {
                if (machineManager != null)
                    machineManager.RenderScreen -= renderer.OnRenderScreen;

                renderer.Dispose();
            }
            renderer = (Activator.CreateInstance(videoBackendType, new object[] { scScreen }) as IVideoBackend);

            if (machineManager != null)
            {
                machineManager.RenderScreen += renderer.OnRenderScreen;

                ForceSquarePixels = emuConfig.ForceSquarePixels;
                LinearInterpolation = emuConfig.LinearInterpolation;

                renderer.AspectRatio = machineManager.AspectRatio;
            }

            foreach (ToolStripMenuItem menuItem in videoBackendToolStripMenuItem.DropDownItems)
                menuItem.Checked = ((menuItem.Tag as Type) == videoBackendType);
        }

        private void InitializeAudioBackend(Type audioBackendType)
        {
            if (audioBackendType == null)
                emuConfig.AudioBackend = audioBackendType = GetImplementationsFromAssembly(typeof(IAudioBackend)).FirstOrDefault();

            if (soundOutput != null)
            {
                soundOutput.Stop();

                if (machineManager != null)
                    machineManager.AddSampleData -= soundOutput.OnAddSampleData;

                soundOutput.Dispose();
            }
            soundOutput = (Activator.CreateInstance(audioBackendType, new object[] { 44100, 2 }) as IAudioBackend);

            if (machineManager != null)
            {
                machineManager.AddSampleData += soundOutput.OnAddSampleData;
            }

            soundOutput.Play();

            foreach (ToolStripMenuItem menuItem in audioBackendToolStripMenuItem.DropDownItems)
                menuItem.Checked = ((menuItem.Tag as Type) == audioBackendType);
        }

        private void SetFormText()
        {
            if (!emulationIsInitialized.IsTrue)
            {
                Text = programNameVersion;
                tsslFps.Text = "";
                tsslStatus.Text = "Ready";
            }
            else
            {
                string mediaName = (romFileInfo != null && romFileInfo.Exists ? (romFriendlyName == string.Empty ? romFileInfo.Name : romFriendlyName) : "[No Media]");
                Text = string.Format("{0} - {1} - {2}", programNameVersion, machineManager.FriendlyShortName, mediaName);
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
            emuConfig.RecentFiles = files.Take(maxRecentFiles).ToList();
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

            emuConfig.RecentFiles = files.Take(maxRecentFiles).ToList();
        }

        private void RemoveFileFromRecentList(string filename)
        {
            List<string> files = emuConfig.RecentFiles.Where(x => x != string.Empty).ToList();
            files.Reverse();

            if (files.Contains(filename)) files.Remove(filename);

            files.Reverse();

            /* Pad with dummy values */
            while (files.Count < maxRecentFiles) files.Add(string.Empty);

            emuConfig.RecentFiles = files.Take(maxRecentFiles).ToList();
        }

        private void UpdateRecentFilesMenu()
        {
            /* Recent files menu */
            var oldRecentItems = recentFilesToolStripMenuItem.DropDownItems.Cast<ToolStripItem>().Where(x => x is ToolStripMenuItem && x.Tag is int).ToList();
            foreach (ToolStripItem item in oldRecentItems)
                recentFilesToolStripMenuItem.DropDownItems.Remove(item);

            for (int i = 0; i < emuConfig.RecentFiles.Count; i++)
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
                            RemoveFileFromRecentList(emuConfig.RecentFiles[fileNumber]);
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

            soundOutput?.Stop();

            romFileInfo = null;
            romFriendlyName = string.Empty;

            machineManager = (Activator.CreateInstance(machineType) as IMachineManager);
            machineManager.RenderScreen += renderer.OnRenderScreen;
            machineManager.ScreenViewportChange += renderer.OnScreenViewportChange;
            machineManager.PollInput += MachineManager_OnPollInput;
            machineManager.FrameEnded += MachineManager_FrameEnded;
            machineManager.AddSampleData += soundOutput.OnAddSampleData;

            renderer.AspectRatio = machineManager.AspectRatio;

            machineManager.Startup();

            emulationIsInitialized.IsTrue = (machineManager != null);

            SetFormText();

            interval = (long)TimeSpan.FromSeconds(1.0 / machineManager.RefreshRate).TotalMilliseconds;

            soundOutput?.Play();
        }

        // TODO: rewrite b/c ZIP archive handling is bad

        public void LoadMedia(string filename)
        {
            LoadMedia(new FileInfo(filename));
        }

        public void LoadMedia(FileInfo fileInfo)
        {
            Type machineType = null;
            string mediaName = string.Empty;

            /* Check if the file is a ZIP archive */
            if (fileInfo.Extension == ".zip")
            {
                using (FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (ZipArchive zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                    {
                        var zippedFile = zip.Entries.FirstOrDefault();
                        var tempDestination = Path.Combine(Path.GetTempPath(), zippedFile.Name);
                        using (FileStream tempStream = new FileStream(tempDestination, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (Stream zippedFileStream = zippedFile.Open())
                            {
                                zippedFileStream.CopyTo(tempStream);
                            }
                        }
                        tempFiles.Add(tempDestination);
                        LoadMedia(tempDestination);

                        AddFileToRecentList(fileInfo.FullName);
                        UpdateRecentFilesMenu();
                        return;
                    }
                }
            }

            /* Try to detect machine via No-Intro DATs */
            var datResult = DatHelper.FindGameInDats(fileInfo);
            if (datResult != null)
            {
                /* Media found in DAT, use the corresponding machine type & media name */
                machineType = datResult.Type;
                mediaName = datResult.Game.Name;
            }
            else
            {
                /* Media not found in DAT, query all machines' CanLoadMedia function */
                foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IMachineManager).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).OrderBy(x => x.Name))
                {
                    IMachineManager machine = (Activator.CreateInstance(type) as IMachineManager);
                    if (machine.CanLoadMedia(fileInfo))
                    {
                        machineType = type;
                        mediaName = fileInfo.Name;
                        break;
                    }
                }
            }

            /* If machine still hasn't been identified, throw an exception */
            if (machineType == null)
                throw new Exception(string.Format("Could not identify machine from media file '{0}'", fileInfo.Name));

            /* Initialize machine */
            StartMachine(machineType);

            /* Load media into machine */
            IMedia media = MediaLoader.LoadMedia(machineManager, fileInfo);
            machineManager?.LoadMedia(0, media);
            machineManager?.Reset();

            /* Housekeeping & UI stuff */
            soundOutput?.Reset();

            romFileInfo = fileInfo;
            romFriendlyName = mediaName;

            if (fileInfo.DirectoryName != Path.GetDirectoryName(Path.GetTempPath()))
            {
                AddFileToRecentList(fileInfo.FullName);
                UpdateRecentFilesMenu();
            }

            SetFormText();
            tsslStatus.Text = string.Format("'{0}' loaded", romFriendlyName);
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

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    e.IsInputKey = true;
                    break;
            }
            base.OnPreviewKeyDown(e);
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

        private void MainForm_Resize(object sender, EventArgs e)
        {
            renderer.OnOutputResized(sender, new OutputResizedEventArgs(scScreen.Width, scScreen.Height));
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

        private void MachineManager_FrameEnded(object sender, EventArgs e)
        {
            tsslFps.Text = string.Format(CultureInfo.InvariantCulture, "{0:##.##} FPS", framesPerSecond);
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
            emuConfig.RecentFiles = new List<string>(maxRecentFiles);

            UpdateRecentFilesMenu();
        }

        private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdSaveScreenshot.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap screenshot = renderer.GetRawScreenshot())
                {
                    screenshot.Save(sfdSaveScreenshot.FileName);
                }
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
            sb.AppendFormat(CultureInfo.InvariantCulture, "DAT filename: {0}\n", machineManager.DatFileName);
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
                    //machineManager.Configuration.InputConfig.ConfigSource.Save();

                    UpdateBootWithoutMediaMenu();
                }
            }
            machineManager.Unpause();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var description = (assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).FirstOrDefault() as AssemblyDescriptionAttribute).Description;
            var copyright = (assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).FirstOrDefault() as AssemblyCopyrightAttribute).Copyright;

            StringBuilder aboutBuilder = new StringBuilder();
            aboutBuilder.AppendFormat("{0} - {1}", programNameVersion, description);
            aboutBuilder.AppendLine();
            aboutBuilder.AppendLine();
            aboutBuilder.AppendLine(copyright);
            MessageBox.Show(aboutBuilder.ToString(), "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
