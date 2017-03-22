using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Drawing;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Various.MSX1
{
    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Generic MSX"; } }
        public override string FriendlyShortName { get { return "MSX"; } }
        public override string FileFilter { get { return "MSX ROMs (*.rom)|*.rom"; } }

        public override double RefreshRate { get { return refreshRate; } }
        public override float AspectRatio { get { return (576.0f / 486.0f); } }
        public override Rectangle ScreenViewport
        {
            get
            {
                int screenHeight = (int)((vdp.NumTotalScanlines / 100.0f) * 93.0f);
                int borderHeight = (vdp.NumTotalScanlines - screenHeight);
                return new Rectangle(0, borderHeight, TMS9918A.NumActivePixelsPerScanline, (screenHeight - borderHeight));
            }
        }

        public override bool SupportsBootingWithoutMedia { get { return true; } }
        public override bool CanCurrentlyBootWithoutMedia { get { return File.Exists(configuration.BiosPath); } }
        public override MachineConfiguration Configuration { get { return configuration; } set { configuration = (value as Configuration); } }

        public override List<Tuple<string, Type, double>> DebugChipInformation
        {
            get
            {
                return new List<Tuple<string, Type, double>>
                {
                    { "CPU", cpu.GetType(), cpuClock },
                    { "VDP", vdp.GetType(), vdpClock },
                    { "PSG", psg.GetType(), psgClock }
                };
            }
        }

        /* Constants */
        const double masterClock = 10738635;
        const double refreshRate = 59.922743;
        const int ramSize = 1 * 65536;  // TODO: 64k atm, make selectable in settings (8/16/32/64k)

        const double cpuClock = (masterClock / 3.0);
        const double vdpClock = (masterClock / 1.0);
        const double psgClock = cpuClock;

        /* Devices on bus */
        IMedia cartridge;
        byte[] wram;
        Z80A cpu;
        TMS9918A vdp;
        object psg; // TODO: General Instrument AY-3-8910, huh...
        i8255 ppi;

        byte[] bios;

        byte portSystemControl, portAVControl;

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();

            cartridge = null;

            cpu = new Z80A(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPort, WritePort);
            wram = new byte[ramSize];
            vdp = new TMS9918A(vdpClock, refreshRate, false);
            psg = null;
            ppi = new i8255();

            if (CanCurrentlyBootWithoutMedia)
                bios = File.ReadAllBytes(configuration.BiosPath);
        }

        public override void Startup()
        {
            cpu.Startup();
            //psg.Startup();
            vdp.Startup();

            Reset();
        }

        public override void Reset()
        {
            cartridge?.Reset();

            cpu.Reset();
            //psg.Reset();
            vdp.Reset();
            ppi.Reset();

            portSystemControl = portAVControl = 0x00;

            base.Reset();
        }

        public override void LoadMedia(IMedia media)
        {
            cartridge = media;
        }

        public override void SaveMedia()
        {
            //
        }

        public override void Shutdown()
        {
            cartridge?.Unload();

            //psg?.Shutdown();
        }

        public override void RunStep()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
                OnRenderScreen(new RenderScreenEventArgs(TMS9918A.NumActivePixelsPerScanline, vdp.NumTotalScanlines, vdp.OutputFramebuffer));

            cpu.SetInterruptLine(vdp.InterruptLine);

            //psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            //
        }

        private byte ReadMemory(ushort address)
        {
            byte primarySlot = (byte)(ppi.ReadPort(0xA8) & 0x03);

            if (primarySlot == 0x00)
            {
                /* Main ROM */
                if (address >= 0x0000 && address <= 0x7FFF)
                {
                    /* BIOS, BASIC */
                    return bios[address & (bios.Length - 1)];
                }
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                if (cartridge != null)
                {
                    return cartridge.Read(address);
                }
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
            }
            else if (primarySlot == 0x03)
            {
                /* Main RAM */
                return wram[address & (ramSize - 1)];   // TODO: ram doesn't mirror? does it?
            }

            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            byte primarySlot = (byte)(ppi.ReadPort(0xA8) & 0x03);

            if (primarySlot == 0x00)
            {
                /* Main ROM -- cannot write */
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                cartridge?.Write(address, value);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
            }
            else if (primarySlot == 0x03)
            {
                /* Main RAM */
                wram[address & (ramSize - 1)] = value;  // TODO: again, ram mirroring...?
            }
        }

        private byte ReadPort(byte port)
        {
            switch (port)
            {
                /* VDP */
                case 0x98: return vdp.ReadDataPort();               /* Data port */
                case 0x99: return vdp.ReadControlPort();            /* Status flags */

                /* PPI */
                case 0xA8:
                case 0xA9:
                case 0xAA: return ppi.ReadPort(port);               /* Ports A, B, C */

                /* Special ports */
                case 0xF7: return portAVControl;                    /* A/V control */

                /* Unmapped or unimplemented... */
                default: return 0x00;
            }
        }

        public void WritePort(byte port, byte value)
        {
            switch (port)
            {
                /* VDP */
                case 0x98: vdp.WriteDataPort(value); break;         /* Data port */
                case 0x99: vdp.WriteControlPort(value); break;      /* Control port */

                /* PPI */
                case 0xA8:
                case 0xAA:
                case 0xAB: ppi.WritePort(port, value); break;       /* Ports A, C and Control */

                /* Special ports */
                case 0xF5: portSystemControl = value; break;        /* System control */
                case 0xF7: portAVControl = value; break;            /* A/V control */
            }
        }
    }
}
