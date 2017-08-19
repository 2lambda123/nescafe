﻿using System;
using System.Threading;
using System.Diagnostics;

namespace Nescafe
{
    public class Console
    {
        public readonly Cpu Cpu;
        public readonly Ppu Ppu;

        public readonly CpuMemory CpuMemory;
        public readonly PpuMemory PpuMemory;

        public readonly Controller Controller;

        public Cartridge Cartridge { get; set; }

        public Action<byte[]> DrawAction { get; set; }

        public bool Stop { get; set; }

        bool _frameEvenOdd;

        public Console()
        {
            Controller = new Controller();

            CpuMemory = new CpuMemory(this);
            PpuMemory = new PpuMemory(this);

            Cpu = new Cpu(this);
            Ppu = new Ppu(this);
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            Cartridge = cartridge;

            Cpu.Reset();
            Ppu.Reset();

            CpuMemory.Reset();
            PpuMemory.Reset();

            _frameEvenOdd = false;
        }

        public void DrawFrame()
        {
            DrawAction(Ppu.BitmapData);
            _frameEvenOdd = !_frameEvenOdd;
        }

        void goUntilFrame()
        {
            bool orig = _frameEvenOdd;
            while (orig == _frameEvenOdd)
            {
                int cpuCycles = Cpu.Step();

                // 3 PPU cycles for each CPU cycle
                for (int i = 0; i < cpuCycles * 3; i++)
                {
                    Ppu.Step();
                }
            }
        }

        public void Start()
        {
            Stop = false;
            byte[] bitmapData = Ppu.BitmapData;

            while (!Stop)
            {
                Stopwatch frameWatch = Stopwatch.StartNew();
                goUntilFrame();
                frameWatch.Stop();

                long timeTaken = frameWatch.ElapsedMilliseconds;
                Thread.Sleep((int)((1000.0 / 60) - timeTaken));
            }
        }
    }    
}