﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Devices
{
    public partial class Z80A
    {
        static SimpleOpcodeDelegate[] opcodesPrefixED = new SimpleOpcodeDelegate[]
        {
			/* 0x00 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0x10 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0x20 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0x30 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0x40 */
			new SimpleOpcodeDelegate((c) => { c.PortInput(ref c.bc.High, c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.WritePort(c.bc.Low, c.bc.High); }),
            new SimpleOpcodeDelegate((c) => { c.Subtract16(ref c.hl, c.bc.Word, true); }),
            new SimpleOpcodeDelegate((c) => { c.LoadMemory16(c.ReadMemory16(c.pc), c.bc.Word); c.pc += 2; }),
            new SimpleOpcodeDelegate((c) => { c.Negate(); }),
            new SimpleOpcodeDelegate((c) => { c.iff1 = c.iff2; c.Return(); }),
            new SimpleOpcodeDelegate((c) => { c.im = 0; }),
            new SimpleOpcodeDelegate((c) => { c.i = c.af.High; }),
            new SimpleOpcodeDelegate((c) => { c.PortInput(ref c.bc.Low, c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.WritePort(c.bc.Low, c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.Add16(ref c.hl, c.bc.Word, true); }),
            new SimpleOpcodeDelegate((c) => { c.LoadRegister16(ref c.bc.Word, c.ReadMemory16(c.ReadMemory16(c.pc))); c.pc += 2; }),
            new SimpleOpcodeDelegate((c) => { c.Negate(); }),
            new SimpleOpcodeDelegate((c) => { c.Return(); c.iff1 = c.iff2; }),
            new SimpleOpcodeDelegate((c) => { c.im = 0; }),
            new SimpleOpcodeDelegate((c) => { c.r = c.af.High; }),
            /* 0x50 */
			new SimpleOpcodeDelegate((c) => { c.PortInput(ref c.de.High, c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.WritePort(c.bc.Low, c.de.High); }),
            new SimpleOpcodeDelegate((c) => { c.Subtract16(ref c.hl, c.de.Word, true); }),
            new SimpleOpcodeDelegate((c) => { c.LoadMemory16(c.ReadMemory16(c.pc), c.de.Word); c.pc += 2; }),
            new SimpleOpcodeDelegate((c) => { c.Negate(); }),
            new SimpleOpcodeDelegate((c) => { c.iff1 = c.iff2; c.Return(); }),
            new SimpleOpcodeDelegate((c) => { c.im = 1; }),
            new SimpleOpcodeDelegate((c) => { c.LoadRegister8(ref c.af.High, c.i, true); }),
            new SimpleOpcodeDelegate((c) => { c.PortInput(ref c.de.Low, c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.WritePort(c.bc.Low, c.de.Low); }),
            new SimpleOpcodeDelegate((c) => { c.Add16(ref c.hl, c.de.Word, true); }),
            new SimpleOpcodeDelegate((c) => { c.LoadRegister16(ref c.de.Word, c.ReadMemory16(c.ReadMemory16(c.pc))); c.pc += 2; }),
            new SimpleOpcodeDelegate((c) => { c.Negate(); }),
            new SimpleOpcodeDelegate((c) => { c.iff1 = c.iff2; c.Return(); }),
            new SimpleOpcodeDelegate((c) => { c.im = 2; }),
            new SimpleOpcodeDelegate((c) => { c.LoadRegister8(ref c.af.High, c.r, true); }),
            /* 0x60 */
            new SimpleOpcodeDelegate((c) => { c.PortInput(ref c.hl.High, c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.WritePort(c.bc.Low, c.hl.High); }),
            new SimpleOpcodeDelegate((c) => { c.Subtract16(ref c.hl, c.hl.Word, true); }),
            new SimpleOpcodeDelegate((c) => { c.LoadMemory16(c.ReadMemory16(c.pc), c.hl.Word); c.pc += 2; }),
            new SimpleOpcodeDelegate((c) => { c.Negate(); }),
            new SimpleOpcodeDelegate((c) => { c.iff1 = c.iff2; c.Return(); }),
            new SimpleOpcodeDelegate((c) => { c.im = 0; }),
            new SimpleOpcodeDelegate((c) => { c.RotateRight4B(); }),
            new SimpleOpcodeDelegate((c) => { c.PortInput(ref c.hl.Low, c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.WritePort(c.bc.Low, c.hl.Low); }),
            new SimpleOpcodeDelegate((c) => { c.Add16(ref c.hl, c.hl.Word, true); }),
            new SimpleOpcodeDelegate((c) => { c.LoadRegister16(ref c.hl.Word, c.ReadMemory16(c.ReadMemory16(c.pc))); c.pc += 2; }),
            new SimpleOpcodeDelegate((c) => { c.Negate(); }),
            new SimpleOpcodeDelegate((c) => { c.iff1 = c.iff2; c.Return(); }),
            new SimpleOpcodeDelegate((c) => { c.im = 0; }),
            new SimpleOpcodeDelegate((c) => { c.RotateLeft4B(); }),
            /* 0x70 */
            new SimpleOpcodeDelegate((c) => { c.PortInputFlagsOnly(c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.WritePort(c.bc.Low, 0x00); }),
            new SimpleOpcodeDelegate((c) => { c.Subtract16(ref c.hl, c.sp, true); }),
            new SimpleOpcodeDelegate((c) => { c.LoadMemory16(c.ReadMemory16(c.pc), c.sp); c.pc += 2; }),
            new SimpleOpcodeDelegate((c) => { c.Negate(); }),
            new SimpleOpcodeDelegate((c) => { c.iff1 = c.iff2; c.Return(); }),
            new SimpleOpcodeDelegate((c) => { c.im = 1; }),
            new SimpleOpcodeDelegate((c) => { /* NOP */ }),
            new SimpleOpcodeDelegate((c) => { c.PortInput(ref c.af.High, c.bc.Low); }),
            new SimpleOpcodeDelegate((c) => { c.WritePort(c.bc.Low, c.af.High); }),
            new SimpleOpcodeDelegate((c) => { c.Add16(ref c.hl, c.sp, true); }),
            new SimpleOpcodeDelegate((c) => { c.LoadRegister16(ref c.sp, c.ReadMemory16(c.ReadMemory16(c.pc))); c.pc += 2; }),
            new SimpleOpcodeDelegate((c) => { c.Negate(); }),
            new SimpleOpcodeDelegate((c) => { c.iff1 = c.iff2; c.Return(); }),
            new SimpleOpcodeDelegate((c) => { c.im = 2; }),
            new SimpleOpcodeDelegate((c) => { /* NOP */ }),
            /* 0x80 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0x90 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0xA0 */
            new SimpleOpcodeDelegate((c) => { c.LoadIncrement(); }),
            new SimpleOpcodeDelegate((c) => { c.CompareIncrement(); }),
            new SimpleOpcodeDelegate((c) => { c.PortInputIncrement(); }),
            new SimpleOpcodeDelegate((c) => { c.PortOutputIncrement(); }),
            new SimpleOpcodeDelegate((c) => { /* A4 - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* A5 - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* A6 - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* A7 - nothing */ }),
            new SimpleOpcodeDelegate((c) => { c.LoadDecrement(); }),
            new SimpleOpcodeDelegate((c) => { c.CompareDecrement(); }),
            new SimpleOpcodeDelegate((c) => { c.PortInputDecrement(); }),
            new SimpleOpcodeDelegate((c) => { c.PortOutputDecrement(); }),
            new SimpleOpcodeDelegate((c) => { /* AC - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* AD - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* AE - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* AF - nothing */ }),
            /* 0xB0 */
            new SimpleOpcodeDelegate((c) => { c.LoadIncrementRepeat(); }),
            new SimpleOpcodeDelegate((c) => { c.CompareIncrementRepeat(); }),
            new SimpleOpcodeDelegate((c) => { c.PortInputIncrementRepeat(); }),
            new SimpleOpcodeDelegate((c) => { c.PortOutputIncrementRepeat(); }),
            new SimpleOpcodeDelegate((c) => { /* B4 - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* B5 - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* B6 - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* B7 - nothing */ }),
            new SimpleOpcodeDelegate((c) => { c.LoadDecrementRepeat(); }),
            new SimpleOpcodeDelegate((c) => { c.CompareDecrementRepeat(); }),
            new SimpleOpcodeDelegate((c) => { c.PortInputDecrementRepeat(); }),
            new SimpleOpcodeDelegate((c) => { c.PortOutputDecrementRepeat(); }),
            new SimpleOpcodeDelegate((c) => { /* BC - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* BD - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* BE - nothing */ }),
            new SimpleOpcodeDelegate((c) => { /* BF - nothing */ }),
            /* 0xC0 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0xD0 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0xE0 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            /* 0xF0 */
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ }),
            new SimpleOpcodeDelegate((c) => { /* NOP (2x) */ })
      };
    }
}
