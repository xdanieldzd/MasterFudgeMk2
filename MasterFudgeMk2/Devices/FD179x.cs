using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterFudgeMk2.Devices
{
    /* MSX floppy disk controller */

    /* http://problemkaputt.de/portar.htm#floppydiskcontroller
     * http://z00m.speccy.cz/docs/wd1793.htm
     * http://fms.komkon.org/MSX/Docs/Portar.txt
     */

    public class FD179x
    {
        enum SteppingMotorRates { Rate6ms = 0, Rate12ms = 1, Rate20ms = 2, Rate30ms = 3 }

        [Flags]
        enum StatusRegisterBits
        {
            Clear = 0x00,

            /* Type I commands */
            Busy = 0x01,
            Index = 0x02,
            Track0 = 0x04,
            CRCError = 0x08,
            SeekError = 0x10,
            HeadLoaded = 0x20,
            WriteProtect = 0x40,
            NotReady = 0x80,

            /* Type II & III commands */
            DataRequest = 0x02,
            LostData = 0x04,
            RecordNotFound = 0x10,
            RecordType = 0x20,
            WriteFault = 0x20
        }

        bool isBusy
        {
            get { return ((statusRegister & StatusRegisterBits.Busy) == StatusRegisterBits.Busy); }
            set
            {
                if (value)
                    statusRegister |= StatusRegisterBits.Busy;
                else
                    statusRegister &= ~StatusRegisterBits.Busy;
            }
        }

        StatusRegisterBits statusRegister;
        byte commandRegister;

        public byte StatusRegister
        {
            get
            {
                InterruptRequest = false;
                return (byte)statusRegister;
            }
        }
        public byte CommandRegister
        {
            set
            {
                commandRegister = value;
                InterruptRequest = false;

                isBusy = true;
            }
        }

        public byte TrackRegister { get; set; }
        public byte SectorRegister { get; set; }
        public byte DataRegister { get; set; }

        public bool InterruptRequest { get; private set; }
        public bool DataRequest { get; private set; }

        public byte DriveNumber { private get; set; }
        public byte SideNumber { private get; set; }
        public bool MotorOn { private get; set; }

        int currentTrackNumber;
        bool isStepForward;

        byte[] diskData;

        public FD179x() { }

        public void Reset()
        {
            statusRegister = StatusRegisterBits.Clear;
            commandRegister = 0x00;

            TrackRegister = 0x04;   /* Arbitrary starting track, but close to zero */
            SectorRegister = 0x00;
            DataRegister = 0x00;

            InterruptRequest = false;
            DataRequest = false;

            DriveNumber = 0;
            SideNumber = 0;
            MotorOn = false;

            currentTrackNumber = TrackRegister;
            isStepForward = false;

            // TODO: proper disk loading
            string filePath = @"D:\ROMs\MSX\Mappy (1984)(Namcot).dsk";
            if (File.Exists(filePath))
                diskData = File.ReadAllBytes(filePath);
        }

        public void Step()
        {
            // TODO: make drive actually work

            if (isBusy)
            {
                if ((commandRegister & 0xD0) == 0xD0)
                    HandleCommandType4();
                else if ((commandRegister & 0x80) != 0x80)
                    HandleCommandType1();
                else
                {
                    if ((commandRegister & 0x40) != 0x40)
                        HandleCommandType2();
                    else
                        HandleCommandType3();
                }
            }
        }

        private void HandleCommandType1()
        {
            SteppingMotorRates stepRate = (SteppingMotorRates)(commandRegister & 0x03);
            bool trackNumVerifyFlag = ((commandRegister & 0x04) == 0x04);
            bool headLoadFlag = ((commandRegister & 0x08) == 0x08);

            if ((commandRegister & 0xE0) == 0x00)
            {
                /* Is Restore or Seek? */
                if ((commandRegister & 0x10) == 0x00)
                {
                    /* Restore -- seek to track 0, interrupt when reached */
                    currentTrackNumber--;
                    if (currentTrackNumber <= 0)
                    {
                        currentTrackNumber = 0;

                        TrackRegister = 0x00;
                        statusRegister |= StatusRegisterBits.Track0;
                        HandleCommandFinished();
                    }
                }
                else
                {
                    /* Seek -- seek to track specified in data register, interrupt when reached */
                    if (DataRegister > currentTrackNumber)
                        currentTrackNumber++;
                    else if (DataRegister < currentTrackNumber)
                        currentTrackNumber--;

                    TrackRegister = (byte)currentTrackNumber;
                    if (TrackRegister == DataRegister) HandleCommandFinished();
                    if (TrackRegister == 0x00) statusRegister |= StatusRegisterBits.Track0;
                }
            }
            else
            {
                bool trackUpdateFlag = ((commandRegister & 0x10) == 0x10);

                /* Is Step, Step-In or Step-Out? */
                if ((commandRegister & 0x60) == 0x20)
                {
                    /* Step */
                    TrackStep(stepRate, isStepForward, trackUpdateFlag);
                    HandleCommandFinished();
                }
                else if ((commandRegister & 0x60) == 0x40)
                {
                    /* Step-In */
                    isStepForward = true;
                    TrackStep(stepRate, isStepForward, trackUpdateFlag);
                    HandleCommandFinished();
                }
                else
                {
                    /* Step-Out */
                    isStepForward = false;
                    TrackStep(stepRate, isStepForward, trackUpdateFlag);
                    HandleCommandFinished();
                }
            }
        }

        private void TrackStep(SteppingMotorRates stepRate, bool isStepForward, bool trackUpdateFlag)
        {
            // TODO: don't ignore stepping motor rate

            if (isStepForward)
            {
                currentTrackNumber++;
                if (currentTrackNumber > 76) currentTrackNumber = 76;
                if (trackUpdateFlag) TrackRegister = (byte)currentTrackNumber;
            }
            else
            {
                currentTrackNumber--;
                if (currentTrackNumber < 0) currentTrackNumber = 0;
                if (trackUpdateFlag) TrackRegister = (byte)currentTrackNumber;
            }

            if (TrackRegister == 0x00) statusRegister |= StatusRegisterBits.Track0;
        }

        private void HandleCommandType2()
        {
            bool sideCompareFlag = ((commandRegister & 0x02) == 0x02);
            bool delay15ms = ((commandRegister & 0x04) == 0x04);
            byte sideCompareNumber = (byte)((commandRegister & 0x08) >> 3);
            bool multipleRecordsFlag = ((commandRegister & 0x10) == 0x10);

            if ((commandRegister & 0x20) == 0x00)
            {
                /* Read Sector */
                statusRegister |= StatusRegisterBits.HeadLoaded;

                // TODO: actually read data
                if (true)
                {
                    statusRegister &= ~StatusRegisterBits.HeadLoaded;

                    DataRequest = true;
                    HandleCommandFinished();
                }
            }
            else
            {
                /* Write Sector */
                statusRegister |= StatusRegisterBits.HeadLoaded;

                // TODO: actually write data
                if (true)
                {
                    statusRegister &= ~StatusRegisterBits.HeadLoaded;

                    DataRequest = true;
                    HandleCommandFinished();
                }
            }
        }

        private void HandleCommandType3()
        {
            bool delay15ms = ((commandRegister & 0x04) == 0x04);

            if ((commandRegister & 0x10) == 0x00)
            {
                if ((commandRegister & 0x20) == 0x00)
                {
                    /* Read Address */
                    //
                }
                else
                {
                    /* Read Track */
                    //
                }
            }
            else
            {
                /* Write Track */
                //
            }
        }

        private void HandleCommandType4()
        {
            /* Force Interrupt */
            HandleCommandFinished();
        }

        private void HandleCommandFinished()
        {
            InterruptRequest = true;
            isBusy = false;
        }
    }
}
