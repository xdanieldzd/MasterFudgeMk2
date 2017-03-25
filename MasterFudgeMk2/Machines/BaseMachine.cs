using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Media;

namespace MasterFudgeMk2.Machines
{
    public abstract class BaseMachine : IMachineManager
    {
        public abstract string FriendlyName { get; }
        public abstract string FriendlyShortName { get; }
        public abstract string FileFilter { get; }

        public abstract double RefreshRate { get; }
        public abstract float AspectRatio { get; }
        public abstract Rectangle ScreenViewport { get; }

        public abstract bool SupportsBootingWithoutMedia { get; }
        public abstract bool CanCurrentlyBootWithoutMedia { get; }
        public abstract string[] MediaSlots { get; }

        public abstract MachineConfiguration Configuration { get; set; }

        public abstract List<Tuple<string, Type, double>> DebugChipInformation { get; }

        public event EventHandler<RenderScreenEventArgs> RenderScreen;
        protected virtual void OnRenderScreen(RenderScreenEventArgs e) { RenderScreen?.Invoke(this, e); }

        public event EventHandler<PollInputEventArgs> PollInput;
        protected virtual void OnPollInput(PollInputEventArgs e) { PollInput?.Invoke(this, e); }

        public event EventHandler<AddSampleDataEventArgs> AddSampleData;
        protected virtual void OnAddSampleData(AddSampleDataEventArgs e) { AddSampleData?.Invoke(this, e); }

        public event EventHandler FrameEnded;
        protected virtual void OnFrameEnded(EventArgs e) { FrameEnded?.Invoke(this, e); }

        protected bool emulationPaused;
        protected int currentCyclesInLine, currentMasterClockCyclesInFrame;
        protected abstract int totalMasterClockCyclesInFrame { get; }

        public abstract void Startup();

        public virtual void Reset()
        {
            emulationPaused = false;
            currentCyclesInLine = currentMasterClockCyclesInFrame = 0;
        }

        public abstract bool CanLoadMedia(FileInfo mediaFile);
        public abstract void LoadMedia(int slotNumber, IMedia media);
        public abstract void SaveMedia();
        public abstract void Shutdown();

        public virtual void RunFrame()
        {
            if (!emulationPaused)
            {
                PollInputEventArgs pollInputEventArgs = new PollInputEventArgs();
                PollInput?.Invoke(this, pollInputEventArgs);
                ParseInput(pollInputEventArgs);

                while (currentMasterClockCyclesInFrame < totalMasterClockCyclesInFrame)
                    RunStep();

                currentMasterClockCyclesInFrame -= totalMasterClockCyclesInFrame;
            }

            OnFrameEnded(EventArgs.Empty);
        }

        public abstract void RunStep();

        public virtual void Pause()
        {
            emulationPaused = true;
        }

        public virtual void Unpause()
        {
            emulationPaused = false;
        }

        protected abstract void ParseInput(PollInputEventArgs e);
    }
}
