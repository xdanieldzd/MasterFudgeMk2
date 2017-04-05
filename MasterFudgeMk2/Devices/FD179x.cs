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
     * 
     * http://fms.komkon.org/MSX/Docs/Disk.txt
     * https://www.win.tue.nl/~aeb/linux/fs/fat/fat-1.html
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
        DiskStructure diskStructure;

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

            diskStructure = new DiskStructure(diskData);
            // TODO: actually try and use disk structure to read stuff
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
                    /* Step -- step in same direction as last Step command */
                    TrackStep(stepRate, isStepForward, trackUpdateFlag);
                    HandleCommandFinished();
                }
                else if ((commandRegister & 0x60) == 0x40)
                {
                    /* Step-In -- step forward, towards track 76 */
                    isStepForward = true;
                    TrackStep(stepRate, isStepForward, trackUpdateFlag);
                    HandleCommandFinished();
                }
                else
                {
                    /* Step-Out -- step backwards, towards track 0 */
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

    // TODO: move to separate file?

    class DiskStructure
    {
        public DiskBootRecord BootRecord { get; private set; }
        public ushort[][] FatCopies { get; private set; }
        public DiskDirectoryEntry[] DirectoryEntries { get; private set; }

        public DiskStructure(byte[] diskData)
        {
            BootRecord = new DiskBootRecord(diskData);

            if (BootRecord.DiskType != 0xF9) throw new NotImplementedException(string.Format("Non-3.5 inch disks not implemented, tried parsing type 0x{0:X2}", BootRecord.DiskType));

            int fatOffset = (BootRecord.SectorsPerBootRecord * BootRecord.BytesPerSector);
            int directoryOffset = (fatOffset + ((BootRecord.NumberOfFatCopies * BootRecord.SectorsPerFat) * BootRecord.BytesPerSector));

            FatCopies = new ushort[BootRecord.NumberOfFatCopies][];
            for (int numFat = 0; numFat < BootRecord.NumberOfFatCopies; numFat++)
            {
                List<ushort> fatEntries = new List<ushort>();

                for (int numSector = 0; numSector < BootRecord.SectorsPerFat; numSector++)
                {
                    int dataOffset = fatOffset + (numFat * (BootRecord.SectorsPerFat * BootRecord.BytesPerSector)) + (numSector * BootRecord.BytesPerSector);
                    for (int offset = 0; offset < BootRecord.BytesPerSector; offset += 3)
                    {
                        fatEntries.Add((ushort)(diskData[dataOffset + offset] | ((diskData[dataOffset + offset + 1] & 0xF) << 8)));
                        fatEntries.Add((ushort)((diskData[dataOffset + offset + 2] << 4) | (diskData[dataOffset + offset + 1] >> 4)));
                    }
                }

                FatCopies[numFat] = fatEntries.ToArray();
            }

            DirectoryEntries = new DiskDirectoryEntry[BootRecord.EntriesPerRootDirectory];
            for (int i = 0; i < DirectoryEntries.Length; i++) DirectoryEntries[i] = new DiskDirectoryEntry(diskData, directoryOffset + (i * 0x20));
        }
    }

    class DiskBootRecord
    {
        public uint JumpTo80x86BootProcedure { get; private set; }  /* Not used on MSX, but must begin with 0xE9 or 0xEB */
        public string DiskName { get; private set; }                /* ASCII, 8 bytes */
        public ushort BytesPerSector { get; private set; }
        public byte SectorsPerCluster { get; private set; }
        public ushort SectorsPerBootRecord { get; private set; }
        public byte NumberOfFatCopies { get; private set; }
        public ushort EntriesPerRootDirectory { get; private set; }
        public ushort SectorsPerDisk { get; private set; }
        public byte DiskType { get; private set; }
        public ushort SectorsPerFat { get; private set; }
        public ushort SectorsPerTrack { get; private set; }
        public ushort HeadsPerDisk { get; private set; }
        public ushort NumberOfReservedSectors { get; private set; }
        public byte[] MsxBootProcedure { get; private set; }        /* 0x1E2 bytes */

        public DiskBootRecord(byte[] diskData)
        {
            JumpTo80x86BootProcedure = (uint)((diskData[0x00] << 16) | (diskData[0x01] << 8) | diskData[0x02]);
            DiskName = Encoding.ASCII.GetString(diskData, 0x03, 0x08);
            BytesPerSector = BitConverter.ToUInt16(diskData, 0x0B);
            SectorsPerCluster = diskData[0x0D];
            SectorsPerBootRecord = BitConverter.ToUInt16(diskData, 0x0E);
            NumberOfFatCopies = diskData[0x10];
            EntriesPerRootDirectory = BitConverter.ToUInt16(diskData, 0x11);
            SectorsPerDisk = BitConverter.ToUInt16(diskData, 0x13);
            DiskType = diskData[0x15];
            SectorsPerFat = BitConverter.ToUInt16(diskData, 0x16);
            SectorsPerTrack = BitConverter.ToUInt16(diskData, 0x18);
            HeadsPerDisk = BitConverter.ToUInt16(diskData, 0x1A);
            NumberOfReservedSectors = BitConverter.ToUInt16(diskData, 0x1C);
            MsxBootProcedure = new byte[0x1E2];
            Buffer.BlockCopy(diskData, 0x1E, MsxBootProcedure, 0, MsxBootProcedure.Length);
        }
    }

    [Flags]
    enum DiskFileAttributes
    {
        ReadOnly = 0x01,
        Hidden = 0x02,
        System = 0x04,
        VolumeLabel = 0x08,
        Subdirectory = 0x10,
        Archive = 0x20,
        UnusedBit6 = 0x40,
        UnusedBit7 = 0x80
    }

    class DiskDirectoryEntry
    {
        public string Filename { get; private set; }
        public string FileExtension { get; private set; }
        public DiskFileAttributes FileAttribute { get; private set; }
        public byte[] Reserved { get; private set; }
        public ushort Timestamp { get; private set; }
        public ushort Datestamp { get; private set; }
        public ushort PointerToFirstCluster { get; private set; }
        public uint FileSizeInBytes { get; private set; }

        public DateTime DateTimeStamp { get; private set; }

        public DiskDirectoryEntry(byte[] diskData, int offset)
        {
            Filename = Encoding.ASCII.GetString(diskData, offset, 0x08);
            FileExtension = Encoding.ASCII.GetString(diskData, offset + 0x08, 0x03);
            FileAttribute = (DiskFileAttributes)diskData[offset + 0x0B];
            Reserved = new byte[0x0A];
            Buffer.BlockCopy(diskData, offset + 0x0C, Reserved, 0, Reserved.Length);
            Timestamp = BitConverter.ToUInt16(diskData, offset + 0x16);
            Datestamp = BitConverter.ToUInt16(diskData, offset + 0x18);
            PointerToFirstCluster = BitConverter.ToUInt16(diskData, offset + 0x1A);
            FileSizeInBytes = BitConverter.ToUInt32(diskData, offset + 0x1C);

            if (Datestamp != 0x0000 && Timestamp != 0x0000)
            {
                // TODO: adding 1980 places Mappy in 2000?
                DateTimeStamp = new DateTime((1980 + (Datestamp >> 9)), ((Datestamp >> 5) & 0x0F), (Datestamp & 0x1F), (Timestamp >> 11), ((Timestamp >> 5) & 0x3F), ((Timestamp & 0x1F) << 1));
            }
        }
    }
}
