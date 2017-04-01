using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Devices
{
    /* MSX floppy disk controller */

    /* http://problemkaputt.de/portar.htm#floppydiskcontroller
     * http://z00m.speccy.cz/docs/wd1793.htm
     * http://fms.komkon.org/MSX/Docs/Portar.txt
     */

    public class FD179x
    {
        public byte StatusRegister { get; private set; }
        public byte CommandRegister { private get; set; }

        public byte TrackRegister { get; set; }
        public byte SectorRegister { get; set; }
        public byte DataRegister { get; set; }

        public bool InterruptRequest { get; private set; }
        public bool DataRequest { get; private set; }

        public bool SelectDrive0 { private get; set; }
        public bool SelectDrive1 { private get; set; }
        public bool SelectSide1 { private get; set; }
        public bool MotorOn { private get; set; }

        //
        int tempTimer = 0;

        public FD179x() { }

        public void Reset()
        {
            StatusRegister = 0x00;
            CommandRegister = 0x00;

            TrackRegister = 0x00;
            SectorRegister = 0x00;
            DataRegister = 0x00;

            InterruptRequest = false;
            DataRequest = false;

            SelectDrive0 = false;
            SelectDrive1 = false;
            SelectSide1 = false;
            MotorOn = false;
        }

        public void Step()
        {
            // TODO: make drive actually work
            if (tempTimer == 0)
            {
                tempTimer = 2048;
            }
            else if (tempTimer < 1024)
            {
                InterruptRequest = false;
                StatusRegister = 0x00;
                tempTimer--;
            }
            else
            {
                InterruptRequest = true;
                StatusRegister = 0x80;
                tempTimer--;
            }
        }
    }
}
