using System;
using System.Collections.Generic;

using MasterFudgeMk2.Common.AudioBackend;
using MasterFudgeMk2.Common.VideoBackend;
using MasterFudgeMk2.Media;

namespace MasterFudgeMk2.Machines
{
    public interface IMachineManager
    {
        /// <summary>
        /// Get human-readable machine name, i.e. "Sega Game Gear"
        /// </summary>
        string FriendlyName { get; }
        /// <summary>
        /// Get human-readable short machine name, i.e. "Game Gear"
        /// </summary>
        string FriendlyShortName { get; }
        /// <summary>
        /// Get file filter for Open ROM dialog
        /// </summary>
        string FileFilter { get; }
        /// <summary>
        /// Get system refresh rate
        /// </summary>
        double RefreshRate { get; }
        /// <summary>
        /// Get flag if system supports booting without media
        /// </summary>
        bool SupportsBootingWithoutMedia { get; }
        /// <summary>
        /// Get flag if system can currently boot without media, i.e. bootstrap/BIOS is available
        /// </summary>
        bool CanCurrentlyBootWithoutMedia { get; }
        /// <summary>
        /// Get or set machine configuration data
        /// </summary>
        MachineConfiguration Configuration { get; set; }

        /// <summary>
        /// Get chip information for debugging purposes
        /// </summary>
        List<Tuple<string, Type, double>> DebugChipInformation { get; }

        /// <summary>
        /// Event handler for screen resizing
        /// </summary>
        event EventHandler<ScreenResizeEventArgs> OnScreenResize;
        /// <summary>
        /// Event handler for rendering
        /// </summary>
        event EventHandler<RenderScreenEventArgs> OnRenderScreen;
        /// <summary>
        /// Event handler for viewport changes, ex. Game Gear's 160x144 viewport
        /// </summary>
        event EventHandler<ScreenViewportChangeEventArgs> OnScreenViewportChange;
        /// <summary>
        /// Event handler for input polling
        /// </summary>
        event EventHandler<PollInputEventArgs> OnPollInput;
        /// <summary>
        /// Event handler for adding sound samples
        /// </summary>
        event EventHandler<AddSampleDataEventArgs> OnAddSampleData;

        /// <summary>
        /// Start machine up
        /// </summary>
        void Startup();
        /// <summary>
        /// Reset machine
        /// </summary>
        void Reset();
        /// <summary>
        /// Load media into machine
        /// </summary>
        /// <param name="media">Media to load</param>
        void LoadMedia(IMedia media);
        /// <summary>
        /// Save ex. cartridge RAM
        /// </summary>
        void SaveMedia();
        /// <summary>
        /// Shut machine down
        /// </summary>
        void Shutdown();
        /// <summary>
        /// Run machine
        /// </summary>
        void Run();
        /// <summary>
        /// Pause machine
        /// </summary>
        void Pause();
        /// <summary>
        /// Unpause machine
        /// </summary>
        void Unpause();
    }
}
