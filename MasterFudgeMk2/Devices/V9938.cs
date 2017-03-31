using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Devices
{
    /* Yamaha V9938, found in MSX2 systems */
    public class V9938 : TMS9918A
    {
        public V9938(double clockRate, double refreshRate, bool isPalChip) : base(clockRate, refreshRate, isPalChip)
        {
            registers = new byte[0x2E];

            screenUsage = new byte[NumActivePixelsPerScanline * NumTotalScanlines];

            outputFramebuffer = new byte[(NumActivePixelsPerScanline * NumTotalScanlines) * 4];
        }

        // TODO: EVERYTHING
    }
}
