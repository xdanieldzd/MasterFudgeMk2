# MasterFudgeMk2
MasterFudge is an emulator for various 8-bit game consoles and handhelds, written in C#, supporting Sega SG-1000, Mark III/Master System and Game Gear, as well as Coleco ColecoVision. Accuracy still leaves things to be desired, and it is, just like [its predecessor](https://github.com/xdanieldzd/MasterFudge), generally somewhat fudged together, hence the name.

## Status

### CPUs
* __Zilog Z80__: All opcodes implemented, documented and undocumented; the two undocumented flags are not fully supported yet; disassembly features incomplete

### VDPs
* __Texas Instruments TMS9918A__: Scanline-based, not fully accurate and possibly with some bugs here and there; still missing the multicolor graphics mode
  * __Sega 315-5246__: Master System II VDP, TMS9918A with additional graphics mode, line interrupts, etc.; also not fully accurate
  * __Sega 315-5378__: Game Gear VDP based on Master System II VDP, with higher color depth, etc.; also not fully accurate

### PSGs
* __Texas Instruments SN76489__: Fully emulated, accuracy is probably not very high, but still sounds decent enough
  * __Sega 315-5246__: Master System II PSG (integrated into VDP chip), SN76489 with minor differences in noise channel; same issues as SN76489

### Media
* __Sega__: Support for ROM-only, ROM+RAM and standard Sega mapper cartridges; cartridge RAM not yet saved, Codemasters and "Korean" mappers still unsupported
* __ColecoVision__: Only standard cartridges up to 32k supported, no MegaCart or the like

## Screenshots
* __Donkey Kong__ (ColecoVision):<br><br>
 ![Screenshot](https://raw.githubusercontent.com/xdanieldzd/MasterFudgeMk2/master/Screenshots/CV-DK.png)<br><br>
* __Girl's Garden__ (SG-1000):<br><br>
 ![Screenshot 3](https://raw.githubusercontent.com/xdanieldzd/MasterFudgeMk2/master/Screenshots/SG1000-Garden.png)<br><br>
* __Sonic the Hedgehog 2__ (Master System):<br><br>
 ![Screenshot 5](https://raw.githubusercontent.com/xdanieldzd/MasterFudgeMk2/master/Screenshots/SMS-S2.png)<br><br>
* __Gunstar Heroes__ (Game Gear):<br><br>
 ![Screenshot 5](https://raw.githubusercontent.com/xdanieldzd/MasterFudgeMk2/master/Screenshots/GG-Gunstar.png)<br><br>
