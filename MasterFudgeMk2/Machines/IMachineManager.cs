using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using MasterFudgeMk2.Common.EventArguments;
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
        /// Get system aspect ratio
        /// </summary>
        float AspectRatio { get; }
        /// <summary>
        /// Get system screen viewport, ex. Game Gear's 160x144 viewport
        /// </summary>
        Rectangle ScreenViewport { get; }

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
        /// Event handler for rendering
        /// </summary>
        event EventHandler<RenderScreenEventArgs> RenderScreen;
        /// <summary>
        /// Event handler for input polling
        /// </summary>
        event EventHandler<PollInputEventArgs> PollInput;
        /// <summary>
        /// Event handler for adding sound samples
        /// </summary>
        event EventHandler<AddSampleDataEventArgs> AddSampleData;
        /// <summary>
        /// Event handler for end of frame
        /// </summary>
        event EventHandler FrameEnded;

        /// <summary>
        /// Start machine up
        /// </summary>
        void Startup();
        /// <summary>
        /// Reset machine
        /// </summary>
        void Reset();
        /// <summary>
        /// Determine if machine can load media file
        /// </summary>
        /// <param name="mediaFile">File info for media file</param>
        /// <returns>True if machine can handle media</returns>
        bool CanLoadMedia(FileInfo mediaFile);
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
        /// Run machine for one frame
        /// </summary>
        void RunFrame();
        /// <summary>
        /// Run machine for one step
        /// </summary>
        void RunStep();
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
