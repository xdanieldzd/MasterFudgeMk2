using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Devices
{
    public partial class MOS6502
    {
        // TODO: make generic for ex. HuC6280

        static readonly string[] opcodeNamesBranches = new string[]
        {
            "BPL", "BMI", "BVC", "BVS", "BCC", "BCS", "BNE", "BEQ"
        };

        static readonly Dictionary<byte, string> opcodeNamesMisc = new Dictionary<byte, string>()
        {
            { 0x00, "BRK" }, { 0x20, "JSR" }, { 0x40, "RTI" }, { 0x60, "RTS" },
            { 0x08, "PHP" }, { 0x28, "PLP" }, { 0x48, "PHA" }, { 0x68, "PLA" }, { 0x88, "DEY" }, { 0xA8, "TAY" }, { 0xC8, "INY" }, { 0xE8, "INX" },
            { 0x18, "CLC" }, { 0x38, "SEC" }, { 0x58, "CLI" }, { 0x78, "SEI" }, { 0x98, "TYA" }, { 0xB8, "CLV" }, { 0xD8, "CLD" }, { 0xF8, "SED" },
            { 0x8A, "TXA" }, { 0x9A, "TXS" }, { 0xAA, "TAX" }, { 0xBA, "TSX" }, { 0xCA, "DEX" }, { 0xEA, "NOP" }
        };

        static readonly string[] opcodeNames01b = new string[]
        {
            "ORA", "AND", "EOR", "ADC", "STA", "LDA", "CMP", "SBC"
        };

        static readonly AddressingModes[] addressingModes01b = new AddressingModes[]
        {
            AddressingModes.IndirectX, AddressingModes.ZeroPage, AddressingModes.Immediate, AddressingModes.Absolute, AddressingModes.IndirectY, AddressingModes.ZeroPageX, AddressingModes.AbsoluteY, AddressingModes.AbsoluteX
        };

        static readonly string[] opcodeNames10b = new string[]
        {
            "ASL", "ROL", "LSR", "ROR", "STX", "LDX", "DEC", "INC"
        };

        static readonly AddressingModes[] addressingModes10bA = new AddressingModes[]
        {
            AddressingModes.Immediate,
            AddressingModes.ZeroPage,
            AddressingModes.Accumulator,
            AddressingModes.Absolute,
            AddressingModes.Invalid,
            AddressingModes.Invalid,
            AddressingModes.ZeroPageX,
            AddressingModes.AbsoluteX
        };

        static readonly AddressingModes[] addressingModes10bB = new AddressingModes[]
        {
            AddressingModes.Immediate,
            AddressingModes.ZeroPage,
            AddressingModes.Accumulator,
            AddressingModes.Absolute,
            AddressingModes.Invalid,
            AddressingModes.Invalid,
            AddressingModes.ZeroPageY,
            AddressingModes.AbsoluteX
        };

        static readonly AddressingModes[] addressingModes10bC = new AddressingModes[]
        {
            AddressingModes.Immediate,
            AddressingModes.ZeroPage,
            AddressingModes.Accumulator,
            AddressingModes.Absolute,
            AddressingModes.Invalid,
            AddressingModes.Invalid,
            AddressingModes.ZeroPageY,
            AddressingModes.AbsoluteY
        };

        static readonly string[] opcodeNames00b = new string[]
        {
            "<unk>", "BIT", "JMP", "JMP", "STY", "LDY", "CPY", "CPX"
        };

        static readonly AddressingModes[] addressingModes00bA = new AddressingModes[]
        {
            AddressingModes.Immediate,
            AddressingModes.ZeroPage,
            AddressingModes.Invalid,
            AddressingModes.Absolute,
            AddressingModes.Invalid,
            AddressingModes.Invalid,
            AddressingModes.ZeroPageX,
            AddressingModes.AbsoluteX
        };

        static readonly AddressingModes[] addressingModes00bB = new AddressingModes[]
        {
            AddressingModes.Immediate,
            AddressingModes.ZeroPage,
            AddressingModes.Invalid,
            AddressingModes.Indirect,
            AddressingModes.Invalid,
            AddressingModes.Invalid,
            AddressingModes.ZeroPageX,
            AddressingModes.AbsoluteX
        };

        static readonly Dictionary<AddressingModes, string> opcodeMnemonicsFormat = new Dictionary<AddressingModes, string>()
        {
            { AddressingModes.Invalid, string.Empty },
            { AddressingModes.Implied, string.Empty },
            { AddressingModes.Accumulator, "A" },
            { AddressingModes.Immediate, "#0x{0:X2}" },
            { AddressingModes.ZeroPage, "0x{0:X2}" },
            { AddressingModes.ZeroPageX, "0x{0:X2},X" },
            { AddressingModes.ZeroPageY, "0x{0:X2},Y" },
            { AddressingModes.Relative, "0x{1:X2}{0:X2}" },
            { AddressingModes.Absolute, "0x{1:X2}{0:X2}" },
            { AddressingModes.AbsoluteX, "0x{1:X2}{0:X2},X" },
            { AddressingModes.AbsoluteY, "0x{1:X2}{0:X2},Y" },
            { AddressingModes.Indirect, "(0x{1:X2}{0:X2})" },
            { AddressingModes.IndirectX, "(0x{0:X2},X)" },
            { AddressingModes.IndirectY, "(0x{0:X2}),Y" }
        };

        public static string DisasmFormat(MOS6502 cpu, byte op)
        {
            string opcodeName = null;
            AddressingModes opcodeAddressingMode = AddressingModes.Invalid;

            if ((op & 0x1F) == 0x10)
            {
                // Branches
                opcodeName = opcodeNamesBranches[((op >> 5) & 0x07)];
                opcodeAddressingMode = AddressingModes.Immediate;
            }
            else
            {
                if (opcodeNamesMisc.ContainsKey(op))
                {
                    // Misc opcodes
                    opcodeName = opcodeNamesMisc[op];

                    // JSR
                    if (op == 0x20)
                        opcodeAddressingMode = AddressingModes.Absolute;
                }
                else
                {
                    // Memory references
                    byte opType = (byte)((op >> 5) & 0x07);
                    byte opAddressing = (byte)((op >> 2) & 0x07);
                    byte opGroup = (byte)(op & 0x03);

                    switch (opGroup)
                    {
                        case 0x01:
                            opcodeName = opcodeNames01b[opType];
                            opcodeAddressingMode = addressingModes01b[opAddressing];
                            break;

                        case 0x02:
                            opcodeName = opcodeNames10b[opType];
                            if (opType == 0x04)
                                opcodeAddressingMode = addressingModes10bB[opAddressing];
                            else if (opType == 0x05)
                                opcodeAddressingMode = addressingModes10bC[opAddressing];
                            else
                                opcodeAddressingMode = addressingModes10bA[opAddressing];
                            break;

                        case 0x00:
                            opcodeName = opcodeNames00b[opType];
                            if (opType == 0x02)
                                opcodeAddressingMode = addressingModes00bA[opAddressing];
                            else
                                opcodeAddressingMode = addressingModes00bB[opAddressing];
                            break;
                    }
                }
            }

            return $"{opcodeName} {opcodeMnemonicsFormat[opcodeAddressingMode]}";
        }

        public static byte[] DisassembleReadOpcode(MOS6502 cpu, ushort address)
        {
            byte op = cpu.ReadMemory8(address);
            byte arg1 = cpu.ReadMemory8((ushort)(address + 1));
            byte arg2 = cpu.ReadMemory8((ushort)(address + 2));
            return new byte[] { op, arg1, arg2 };
        }

        public static string DisassembleMakeByteString(MOS6502 cpu, byte[] opcode)
        {
            return string.Join(" ", opcode.Select(x => $"{x:X2}").Take(opcodeLengths6502[opcode[0]]));
        }

        public static string PrintRegisters(MOS6502 cpu)
        {
            return $"A:{cpu.a:X2} X:{cpu.x:X2} Y:{cpu.y:X2} P:{(byte)cpu.p:X2} SP:{cpu.sp:X2}";
        }

        public static string PrintFlags(MOS6502 cpu)
        {
            return string.Format("[{7}{6}{5}{4}{3}{2}{1}{0}]",
                cpu.IsFlagSet(Flags.Carry) ? "C" : "-",
                cpu.IsFlagSet(Flags.Zero) ? "Z" : "-",
                cpu.IsFlagSet(Flags.InterruptDisable) ? "I" : "-",
                cpu.IsFlagSet(Flags.DecimalMode) ? "D" : "-",
                cpu.IsFlagSet(Flags.Brk) ? "B" : "-",
                cpu.IsFlagSet(Flags.UnusedBit5) ? "5" : "-",
                cpu.IsFlagSet(Flags.Overflow) ? "V" : "-",
                cpu.IsFlagSet(Flags.Sign) ? "S" : "-");
        }

        public static string PrintInterrupt(MOS6502 cpu)
        {
            return $"[INT:{(cpu.intState == Common.Enumerations.InterruptState.Assert ? "ASR" : "---")} NMI:{(cpu.nmiState == Common.Enumerations.InterruptState.Assert ? "ASR" : "---")}]";
        }

        public static string DisassembleOpcode(MOS6502 cpu, ushort address)
        {
            byte[] opcode = DisassembleReadOpcode(cpu, address);
            return $"0x{address:X4} | {DisassembleMakeByteString(cpu, opcode).PadRight(8)} | {string.Format(DisasmFormat(cpu, opcode[0]), opcode[1], opcode[2])}";
        }
    }
}
