﻿using System;

namespace Nescafe
{
    public class CpuMemory : Memory
    {
        // First 2KB of internal ram
        byte[] internalRam = new byte[2048];

        Console _console;

        public CpuMemory(Console console)
        {
            _console = console;
        }

        public void Reset()
        {
            Array.Clear(internalRam, 0, internalRam.Length);
        }

        // Return the index in internalRam of the address (handle mirroring)
        ushort HandleInternalRamMirror(ushort address)
        {
            return (ushort)(address % 0x800);
        }

        // Handles mirroring of PPU register addresses
        ushort GetPpuRegisterFromAddress(ushort address)
        {
            // Special case for OAMDMA ($4014) which is not alongside the other registers
            if (address == 0x4014) return address;
            else return (ushort)(0x2000 + ((address - 0x2000) % 8));
        }

        void WritePpuRegister(ushort address, byte data)
        {
            _console.Ppu.WriteToRegister(GetPpuRegisterFromAddress(address), data);
        }

        byte ReadPpuRegister(ushort address)
        {
            return _console.Ppu.ReadFromRegister(GetPpuRegisterFromAddress(address));
        }

        public override byte Read(ushort address)
        {
            byte data;
            if (address < 0x2000) // Internal CPU RAM 
            {
                ushort addressIndex = HandleInternalRamMirror(address);
                data = internalRam[addressIndex];
            }
            else if (address <= 0x3FFF) // PPU Registers
            {
                data = ReadPpuRegister(address);
            }
            else if (address >= 0x4020) // Handled by mapper (PRG rom, CHR rom/ram etc.)
            {
                data = _console.Cartridge.Mapper.Read(address);
            }
            else if (address == 0x4016) // Controller
            {
                data = _console.Controller.ReadControllerOutput();
            }
            else // Invalid Read
            {
                data = 0;
                // System.Console.WriteLine("Invalid CPU Memory Read from address: " + address.ToString("X4"));
            }

            return data;
        }

        public override void Write(ushort address, byte data)
        {
            if (address < 0x2000) // Internal CPU RAM
            {
                ushort addressIndex = HandleInternalRamMirror(address);
                internalRam[addressIndex] = data;
            }
            else if (address <= 0x3FFF || address == 0x4014) // PPU Registers
            {
                WritePpuRegister(address, data);
            }
            else if (address == 0x4016) // Controller
            {
                _console.Controller.WriteControllerInput(data);
            }
            else // Invalid Write
            {
                // System.Console.WriteLine("Invalid CPU Memory Write to address: " + address.ToString("X4"));
            }
        }
    }
}