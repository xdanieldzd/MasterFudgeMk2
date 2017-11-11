using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;

using MasterFudgeMk2.AudioBackends;
using MasterFudgeMk2.InputBackends;
using MasterFudgeMk2.VideoBackends;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2
{
    public partial class NewMainForm : Form
    {
        #region Constants

        const int maxRecentFiles = 12;

        #endregion

        #region Misc. Variables

        string programNameVersion;
        EmulatorConfigurationNew emulatorConfig;

        Dictionary<Type, string> machineNames;

        IVideoBackend activeVideoBackend;
        IAudioBackend activeAudioBackend;
        IInputBackend activeInputBackend;
        IMachineManager activeMachine;

        long startTime, frameCounter, interval;
        double framesPerSecond;
        Stopwatch stopWatch;

        #endregion

        #region Constructor & Disposal

        public NewMainForm()
        {
            /* Initialize standard Winforms stuff */
            InitializeComponent();

            /* Initialize emulator UI stuff */
            InitializeBasicProperties();

            /* Gather emulated machines' names */
            machineNames = GetMachineNames();

            /* Create various submenus */
            CreateBootSystemMenu(bootSystemToolStripMenuItem);
            CreateBackendsMenu(videoBackendToolStripMenuItem, typeof(IVideoBackend));
            CreateBackendsMenu(audioBackendToolStripMenuItem, typeof(IAudioBackend));
            CreateBackendsMenu(inputBackendToolStripMenuItem, typeof(IInputBackend));
            CreateRecentFilesMenu(recentFilesToolStripMenuItem);

            /* Initialize backends */
            SwitchVideoBackend(emulatorConfig.VideoBackend);
            SwitchAudioBackend(emulatorConfig.AudioBackend);
            SwitchInputBackend(emulatorConfig.InputBackend);

            /* Initialize Application.Idle event as main loop */
            InitializeIdleEvent();

            // TODO: a LOT more, databinding, more menu shit, etc etc
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();

                if (activeInputBackend != null) activeInputBackend.Dispose();
                if (activeAudioBackend != null) activeAudioBackend.Dispose();
                if (activeVideoBackend != null) activeVideoBackend.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Basic Application and UI Setup, Data Gathering, etc.

        private void InitializeBasicProperties()
        {
            /* Create program name/version string */
            var assembly = Assembly.GetExecutingAssembly();
            var programName = (assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).FirstOrDefault() as AssemblyProductAttribute).Product;
            var programVersion = new Version((assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).FirstOrDefault() as AssemblyFileVersionAttribute).Version);
            programNameVersion = string.Format("{0} v{1}.{2}",
                programName,
                string.Format((programVersion.Major > 0 ? "{0}.{1:D1}" : "{1:D3}"), programVersion.Major, programVersion.Minor),
                programVersion.Build);

            /* Read emulator configuration */
            emulatorConfig = new EmulatorConfigurationNew();
        }

        private Dictionary<Type, string> GetMachineNames()
        {
            var machineInfos = new Dictionary<Type, string>();

            foreach (var machineType in typeof(IMachineManager).GetImplementationsFromAssembly())
            {
                /* Create machine instance */
                var machine = (Activator.CreateInstance(machineType) as IMachineManager);
                /* Store machine type & friendly name */
                machineInfos.Add(machineType, machine.FriendlyName);
            }

            /* Sort machines by name */
            return machineInfos.OrderBy(x => x.Value).ToDictionary(k => k.Key, v => v.Value);
        }

        private void InitializeIdleEvent()
        {
            Application.Idle += (s, e) =>
            {
                /* If machine exists and we're idle, run emulation */
                while (activeMachine != null && Common.NativeMethods.IsApplicationIdle())
                {
                    startTime = stopWatch.ElapsedMilliseconds;
                    activeMachine.RunFrame();

                    while (emulatorConfig.LimitFps && (stopWatch.ElapsedMilliseconds - startTime) < interval)
                        Thread.Sleep(1);

                    frameCounter++;
                    double timeDifference = (stopWatch.ElapsedMilliseconds - startTime);
                    if (timeDifference >= 1.0)
                    {
                        framesPerSecond = (frameCounter / (timeDifference / 1000));
                        frameCounter = 0;
                    }
                }
            };
        }

        #endregion

        #region Menu Creation

        private void CreateBootSystemMenu(ToolStripMenuItem rootMenuItem)
        {
            /* Clear menu */
            rootMenuItem.DropDownItems.Clear();

            foreach (var info in machineNames)
            {
                /* Create menu item for booting machine */
                var systemMenuItem = new ToolStripMenuItem() { Text = info.Value, Tag = info.Key };
                systemMenuItem.Click += (s, e) =>
                {
                    var menuItem = (s as ToolStripMenuItem);
                    var machineType = (menuItem.Tag as Type);

                    /* Initialize and boot the machine */
                    InitializeMachineManager(machineType);
                    InitializeFrameLimiter(activeMachine.RefreshRate);

                    /* Create menu for loading media into machine */
                    CreateLoadMediaMenu(loadMediaToolStripMenuItem, machineType);

                    /* Enable disabled menus */
                    takeScreenshotToolStripMenuItem.Enabled = emulationToolStripMenuItem.Enabled = true;
                };
                rootMenuItem.DropDownItems.Add(systemMenuItem);
            }
        }

        private void CreateLoadMediaMenu(ToolStripMenuItem rootMenuItem, Type machineType)
        {
            /* Sanity checks */
            if (rootMenuItem == null)
                throw new ArgumentNullException(nameof(rootMenuItem));

            if (!typeof(IMachineManager).IsAssignableFrom(machineType))
                throw new ArgumentException(string.Format("Given type {0} is not machine manager", machineType.AssemblyQualifiedName));

            if (activeMachine == null)
                throw new Exception(string.Format("Machine is null; expected {0}", machineType.AssemblyQualifiedName));

            if (activeMachine.GetType() != machineType)
                throw new Exception(string.Format("Machine type mismatch; expected {0}, got {1}", machineType.AssemblyQualifiedName, activeMachine.GetType().AssemblyQualifiedName));

            /* Clear menu */
            rootMenuItem.DropDownItems.Clear();

            var machineInfo = machineNames[machineType];
            for (int i = 0; i < activeMachine.MediaSlots.Length; i++)
            {
                var slotMenuItem = new ToolStripMenuItem() { Text = activeMachine.MediaSlots[i], Tag = i };
                slotMenuItem.Click += (s, e) =>
                {
                    var menuItem = (s as ToolStripMenuItem);
                    var slotNumber = (int)menuItem.Tag;

                    using (OpenFileDialog openFileDialog = new OpenFileDialog()
                    {
                        Filter = activeMachine.FileFilter,
                        Title = string.Format("Load into {0} {1}", activeMachine.FriendlyShortName, activeMachine.MediaSlots[slotNumber]),
                        InitialDirectory = activeMachine.Configuration.LastDirectory
                    })
                    {
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            FileInfo mediaFileInfo = new FileInfo(openFileDialog.FileName);
                            IMedia media = MediaLoader.LoadMedia(activeMachine, mediaFileInfo);

                            if (activeMachine.CanLoadMedia(mediaFileInfo) ||
                                (MessageBox.Show("Media appears to be invalid for this machine. Load anyway?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes))
                            {
                                activeMachine.Configuration.LastDirectory = Path.GetDirectoryName(openFileDialog.FileName);

                                InitializeMediaSlot(slotNumber, media);

                                AddToRecentFilesList(machineType, slotNumber, mediaFileInfo.FullName);
                                CreateRecentFilesMenu(recentFilesToolStripMenuItem);
                            }
                        }
                    }
                };
                rootMenuItem.DropDownItems.Add(slotMenuItem);
            }
            rootMenuItem.Enabled = true;
        }

        private void CreateBackendsMenu(ToolStripMenuItem rootMenuItem, Type backendInterfaceType)
        {
            /* Sanity checks */
            if (rootMenuItem == null)
                throw new ArgumentNullException(nameof(rootMenuItem));

            if (!backendInterfaceType.IsInterface)
                throw new ArgumentException(string.Format("Given type {0} is not interface", backendInterfaceType.AssemblyQualifiedName));

            /* Clear menu */
            rootMenuItem.DropDownItems.Clear();

            foreach (var backendType in backendInterfaceType.GetImplementationsFromAssembly().OrderByDescending(x => x.Name.StartsWith("Null")).ThenBy(x => x.Name))
            {
                string backendName = ((backendType.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute)?.Description ?? backendType.Name);

                ToolStripMenuItem backendMenuItem = new ToolStripMenuItem() { Text = backendName, Tag = backendType };
                backendMenuItem.Click += (s, e) => { SwitchBackend((s as ToolStripMenuItem).Tag as Type); };
                rootMenuItem.DropDownItems.Add(backendMenuItem);
            }
        }

        private void CreateRecentFilesMenu(ToolStripMenuItem rootMenuItem)
        {
            /* Sanity checks */
            if (rootMenuItem == null)
                throw new ArgumentNullException(nameof(rootMenuItem));

            /* Clear menu */
            rootMenuItem.DropDownItems.Clear();

            /* Add "clear list" option */
            ToolStripMenuItem clearListMenuItem = new ToolStripMenuItem() { Text = "&Clear List" };
            clearListMenuItem.Click += (s, e) =>
            {
                emulatorConfig.RecentFiles = new List<string>();
                CreateRecentFilesMenu((s as ToolStripMenuItem).OwnerItem as ToolStripMenuItem);
            };
            rootMenuItem.DropDownItems.Add(clearListMenuItem);
            rootMenuItem.DropDownItems.Add(new ToolStripSeparator());

            for (int i = 0; i < maxRecentFiles; i++)
            {
                if (i < emulatorConfig.RecentFiles.Count && emulatorConfig.RecentFiles[i] != string.Empty)
                {
                    /* Valid recent file entry; extract information */
                    var match = Regex.Match(emulatorConfig.RecentFiles[i], @"^(?<machine>.*)\/(?<slot>.*)\/(?<file>.*)");

                    var machineType = Assembly.GetExecutingAssembly().GetType(match.Groups["machine"].Value);
                    var slotNumber = int.Parse(match.Groups["slot"].Value);
                    var mediaFileName = match.Groups["file"].Value;

                    ToolStripMenuItem recentFileMenuItem = new ToolStripMenuItem()
                    {
                        Text = mediaFileName,
                        Tag = new Tuple<Type, int, string>(machineType, slotNumber, mediaFileName),
                        ShortcutKeys = (Keys.Control | (Keys.F1 + i))
                    };
                    recentFileMenuItem.Click += (s, e) =>
                    {
                        var menuItem = (s as ToolStripMenuItem);
                        var recentFileInfo = (menuItem.Tag as Tuple<Type, int, string>);
                        var fileInfoInstance = new FileInfo(recentFileInfo.Item3);

                        if (fileInfoInstance.Exists)
                        {
                            /* Initialize machine and load media */
                            InitializeMachineManager(recentFileInfo.Item1);
                            InitializeFrameLimiter(activeMachine.RefreshRate);
                            InitializeMediaSlot(recentFileInfo.Item2, MediaLoader.LoadMedia(activeMachine, fileInfoInstance));

                            /* Handle recent files updates */
                            AddToRecentFilesList(recentFileInfo.Item1, recentFileInfo.Item2, recentFileInfo.Item3);
                            CreateRecentFilesMenu(menuItem.OwnerItem as ToolStripMenuItem);

                            /* Create load media menu */
                            CreateLoadMediaMenu(loadMediaToolStripMenuItem, recentFileInfo.Item1);

                            /* Enable disabled menus */
                            takeScreenshotToolStripMenuItem.Enabled = emulationToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            if (MessageBox.Show("Selected file does not exist anymore; it will be removed from the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                            {
                                RemoveFromRecentFilesList(recentFileInfo.Item1, recentFileInfo.Item2, recentFileInfo.Item3);
                                CreateRecentFilesMenu(recentFilesToolStripMenuItem);
                            }
                        }
                    };
                    rootMenuItem.DropDownItems.Add(recentFileMenuItem);
                }
                else
                {
                    /* Dummy entry */
                    rootMenuItem.DropDownItems.Add(new ToolStripMenuItem()
                    {
                        Text = "-",
                        ShortcutKeys = (Keys.Control | (Keys.F1 + i)),
                        Enabled = false
                    });
                }
            }
        }

        #endregion

        #region Backend Switching

        private void SwitchBackend(Type backendType)
        {
            if (typeof(IVideoBackend).IsAssignableFrom(backendType)) SwitchVideoBackend(backendType);
            else if (typeof(IAudioBackend).IsAssignableFrom(backendType)) SwitchAudioBackend(backendType);
            else if (typeof(IInputBackend).IsAssignableFrom(backendType)) SwitchInputBackend(backendType);
            else throw new ArgumentException(string.Format("Given type {0} is not a known backend type", backendType.AssemblyQualifiedName));
        }

        private void SwitchVideoBackend(Type videoBackendType)
        {
            if (!typeof(IVideoBackend).IsAssignableFrom(videoBackendType))
                throw new ArgumentException(string.Format("Given type {0} is not a video backend", videoBackendType.AssemblyQualifiedName));

            if (activeVideoBackend != null)
            {
                /* Don't try to switch backends if already using it */
                if (activeVideoBackend.GetType() == videoBackendType) return;

                /* Clean up currently active backend */
                if (activeMachine != null)
                    activeMachine.RenderScreen -= activeVideoBackend.OnRenderScreen;

                activeVideoBackend.Dispose();
                activeVideoBackend = null;
            }

            /* Create new backend */
            activeVideoBackend = (Activator.CreateInstance(videoBackendType, new object[] { scScreen }) as IVideoBackend);

            if (activeMachine != null)
            {
                /* Configure new backend & attach to active machine */
                activeVideoBackend.AspectRatio = activeMachine.AspectRatio;

                activeMachine.RenderScreen += activeVideoBackend.OnRenderScreen;
                activeMachine.ScreenViewportChange += activeVideoBackend.OnScreenViewportChange;
            }

            /* Handle UI & emulation config updates */
            foreach (ToolStripMenuItem menuItem in videoBackendToolStripMenuItem.DropDownItems)
                menuItem.Checked = ((menuItem.Tag as Type) == videoBackendType);

            emulatorConfig.VideoBackend = videoBackendType;
        }

        private void SwitchAudioBackend(Type audioBackendType)
        {
            if (!typeof(IAudioBackend).IsAssignableFrom(audioBackendType))
                throw new ArgumentException(string.Format("Given type {0} is not a audio backend", audioBackendType.AssemblyQualifiedName));

            if (activeAudioBackend != null)
            {
                /* Don't try to switch backends if already using it */
                if (activeAudioBackend.GetType() == audioBackendType) return;

                /* Clean up currently active backend */
                if (activeMachine != null)
                    activeMachine.AddSampleData -= activeAudioBackend.OnAddSampleData;

                activeAudioBackend.Stop();

                activeAudioBackend.Dispose();
                activeAudioBackend = null;
            }

            /* Create new backend */
            activeAudioBackend = (Activator.CreateInstance(audioBackendType, new object[] { 44100, 1 }) as IAudioBackend);

            if (activeMachine != null)
            {
                /* Configure new backend & attach to active machine */
                activeMachine.AddSampleData += activeAudioBackend.OnAddSampleData;
            }

            /* Handle UI & emulation config updates */
            foreach (ToolStripMenuItem menuItem in audioBackendToolStripMenuItem.DropDownItems)
                menuItem.Checked = ((menuItem.Tag as Type) == audioBackendType);

            emulatorConfig.AudioBackend = audioBackendType;
        }

        private void SwitchInputBackend(Type inputBackendType)
        {
            if (!typeof(IInputBackend).IsAssignableFrom(inputBackendType))
                throw new ArgumentException(string.Format("Given type {0} is not a input backend", inputBackendType.AssemblyQualifiedName));

            if (activeInputBackend != null)
            {
                /* Don't try to switch backends if already using it */
                if (activeInputBackend.GetType() == inputBackendType) return;

                /* Clean up currently active backend */
                if (activeMachine != null)
                    activeMachine.PollInput -= activeInputBackend.OnPollInput;

                activeInputBackend.Dispose();
                activeInputBackend = null;
            }

            /* Create new backend */
            activeInputBackend = (Activator.CreateInstance(inputBackendType, new object[] { }) as IInputBackend);

            if (activeMachine != null)
            {
                /* Configure new backend & attach to active machine */
                activeMachine.PollInput += activeInputBackend.OnPollInput;
            }

            /* Handle UI & emulation config updates */
            foreach (ToolStripMenuItem menuItem in inputBackendToolStripMenuItem.DropDownItems)
                menuItem.Checked = ((menuItem.Tag as Type) == inputBackendType);

            emulatorConfig.InputBackend = inputBackendType;
        }

        #endregion

        #region Recent Files Handling

        private void AddToRecentFilesList(Type machineType, int slotNumber, string mediaFileName)
        {
            if (!typeof(IMachineManager).IsAssignableFrom(machineType))
                throw new ArgumentException(string.Format("Given type {0} is not machine manager", machineType.AssemblyQualifiedName));

            if (!File.Exists(mediaFileName))
                throw new FileNotFoundException("Trying to add non-existent file to recent list");

            string recentFileString = string.Format("{0}/{1}/{2}", machineType.FullName, slotNumber, mediaFileName);

            List<string> recentFiles = emulatorConfig.RecentFiles;
            if (recentFiles.Contains(recentFileString)) recentFiles.RemoveAll(x => (x == recentFileString));
            recentFiles.Insert(0, recentFileString);

            emulatorConfig.RecentFiles = recentFiles;
        }

        private void RemoveFromRecentFilesList(Type machineType, int slotNumber, string mediaFileName)
        {
            if (!typeof(IMachineManager).IsAssignableFrom(machineType))
                throw new ArgumentException(string.Format("Given type {0} is not machine manager", machineType.AssemblyQualifiedName));

            string recentFileString = string.Format("{0}/{1}/{2}", machineType.FullName, slotNumber, mediaFileName);

            if (emulatorConfig.RecentFiles.Contains(recentFileString))
            {
                List<string> recentFiles = emulatorConfig.RecentFiles;
                recentFiles.RemoveAll(x => (x == recentFileString));
                emulatorConfig.RecentFiles = recentFiles;
            }
        }

        #endregion

        #region Emulation Initialization and Startup

        private void InitializeMachineManager(Type machineType)
        {
            if (!typeof(IMachineManager).IsAssignableFrom(machineType))
                throw new ArgumentException(string.Format("Given type {0} is not machine manager", machineType.AssemblyQualifiedName));

            if (activeVideoBackend == null)
                throw new Exception("Active video backend is null; something went wrong, can't initialize machine");

            if (activeAudioBackend == null)
                throw new Exception("Active audio backend is null; something went wrong, can't initialize machine");

            if (activeInputBackend == null)
                throw new Exception("Active input backend is null; something went wrong, can't initialize machine");

            activeMachine = (Activator.CreateInstance(machineType) as IMachineManager);
            activeMachine.Startup();

            activeVideoBackend.AspectRatio = activeMachine.AspectRatio;

            /* Video backend events */
            activeMachine.RenderScreen += activeVideoBackend.OnRenderScreen;
            activeMachine.ScreenViewportChange += activeVideoBackend.OnScreenViewportChange;

            /* Audio backend events */
            activeMachine.AddSampleData += activeAudioBackend.OnAddSampleData;

            /* Input backend events */
            activeMachine.PollInput += activeInputBackend.OnPollInput;

            /* Form events */
            activeMachine.FrameEnded += ActiveMachine_FrameEnded;

            /* Ensure clean slate */
            ResetEmulation();
        }

        private void InitializeMediaSlot(int slotNumber, IMedia media)
        {
            if (activeMachine == null)
                throw new Exception("Active machine is null; can't load media");

            activeMachine.LoadMedia(slotNumber, media);

            ResetEmulation();

            // TODO: all the other shit
        }

        private void InitializeFrameLimiter(double refreshRate)
        {
            /* Initialize frame limiter variables */
            startTime = frameCounter = 0;
            framesPerSecond = 0.0;

            interval = (long)TimeSpan.FromSeconds(1.0 / refreshRate).TotalMilliseconds;

            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        private void ResetEmulation()
        {
            if (activeMachine == null)
                throw new Exception("Active machine is null; can't reset");

            if (activeVideoBackend == null)
                throw new Exception("Active video backend is null; something went wrong, can't reset");

            if (activeAudioBackend == null)
                throw new Exception("Active audio backend is null; something went wrong, can't reset");

            if (activeInputBackend == null)
                throw new Exception("Active input backend is null; something went wrong, can't reset");

            activeAudioBackend.Stop();
            activeMachine.Reset();
            activeAudioBackend.Play();
        }

        #endregion

        #region Machine Events

        private void ActiveMachine_FrameEnded(object sender, EventArgs e)
        {
            // TODO: statusbar
            Text = framesPerSecond.ToString();
        }

        #endregion

        #region Form Events

        private void configureSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (activeMachine == null)
                throw new Exception("Active machine is null; can't change configuration");

            if (activeVideoBackend == null)
                throw new Exception("Active video backend is null; can't change configuration");

            if (activeAudioBackend == null)
                throw new Exception("Active audio backend is null; can't change configuration");

            if (activeInputBackend == null)
                throw new Exception("Active input backend is null; can't change configuration");

            using (NewConfigForm configForm = new NewConfigForm(activeMachine.Configuration, activeVideoBackend, activeAudioBackend, activeInputBackend))
            {
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    //
                }
            }
        }

        #endregion
    }
}
