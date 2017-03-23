using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Media.MSX
{
    /* ROM header handler for standard cartridge ROMs -- http://www.konamiman.com/msx/msx2th/th-5b.txt (7.3.1) */
    public class RomHeader
    {
        public string CartridgeID { get; private set; }
        public ushort InitializationRoutineAddress { get; private set; }
        public ushort StatementExpansionRoutineAddress { get; private set; }
        public ushort DeviceExpansionRoutineAddress { get; private set; }
        public ushort BasicTextPointer { get; private set; }
        public byte[] Reserved { get; private set; }

        public bool IsValidCartridge { get { return (CartridgeID == "AB" || CartridgeID == "CD"); } }
        public bool IsSubRomCartridge { get { return (CartridgeID == "CD"); } }

        public RomHeader(byte[] romData)
        {
            CartridgeID = Encoding.ASCII.GetString(romData, 0x00, 2);
            InitializationRoutineAddress = BitConverter.ToUInt16(romData, 0x02);
            StatementExpansionRoutineAddress = BitConverter.ToUInt16(romData, 0x04);
            DeviceExpansionRoutineAddress = BitConverter.ToUInt16(romData, 0x06);
            BasicTextPointer = BitConverter.ToUInt16(romData, 0x08);
            Reserved = new byte[6];
            Buffer.BlockCopy(romData, 0x0A, Reserved, 0, 0x06);
        }
    }
}
