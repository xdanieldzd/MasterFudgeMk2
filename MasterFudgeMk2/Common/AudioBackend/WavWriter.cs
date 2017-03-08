﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterFudgeMk2.Common.AudioBackend
{
    /* http://www.codeguru.com/columns/dotnet/making-sounds-with-waves-using-c.html */
    public class WavWriter
    {
        WaveHeader waveHeader;
        FormatChunk formatChunk;
        DataChunk dataChunk;

        public WavWriter()
        {
            waveHeader = new WaveHeader();
            formatChunk = new FormatChunk();
            dataChunk = new DataChunk();

            waveHeader.FileLength += formatChunk.Length();
        }

        public void AddSampleData(short[] leftBuffer, short[] rightBuffer)
        {
            dataChunk.AddSampleData(leftBuffer, rightBuffer);
            waveHeader.FileLength += (uint)(leftBuffer.Length + rightBuffer.Length);
        }

        public void AddSampleData(short[] stereoBuffer)
        {
            dataChunk.AddSampleData(stereoBuffer);
            waveHeader.FileLength += (uint)stereoBuffer.Length;
        }

        public void Save(string filename)
        {
            using (FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                file.Write(waveHeader.GetBytes(), 0, (int)waveHeader.Length());
                file.Write(formatChunk.GetBytes(), 0, (int)formatChunk.Length());
                file.Write(dataChunk.GetBytes(), 0, (int)dataChunk.Length());
            }
        }
    }

    class WaveHeader
    {
        const string fileTypeId = "RIFF";
        const string mediaTypeId = "WAVE";

        public string FileTypeId { get; private set; }
        public uint FileLength { get; set; }
        public string MediaTypeId { get; private set; }

        public WaveHeader()
        {
            FileTypeId = fileTypeId;
            MediaTypeId = mediaTypeId;
            FileLength = 4;     /* Minimum size is always 4 bytes */
        }

        public byte[] GetBytes()
        {
            List<byte> chunkData = new List<byte>();

            chunkData.AddRange(Encoding.ASCII.GetBytes(FileTypeId));
            chunkData.AddRange(BitConverter.GetBytes(FileLength));
            chunkData.AddRange(Encoding.ASCII.GetBytes(MediaTypeId));

            return chunkData.ToArray();
        }

        public uint Length()
        {
            return (uint)GetBytes().Length;
        }
    }

    class FormatChunk
    {
        const string chunkId = "fmt ";

        ushort bitsPerSample, channels;
        uint frequency;

        public string ChunkId { get; private set; }
        public uint ChunkSize { get; private set; }
        public ushort FormatTag { get; private set; }

        public ushort Channels
        {
            get { return channels; }
            set { channels = value; RecalcBlockSizes(); }
        }

        public uint Frequency
        {
            get { return frequency; }
            set { frequency = value; RecalcBlockSizes(); }
        }

        public uint AverageBytesPerSec { get; private set; }
        public ushort BlockAlign { get; private set; }

        public ushort BitsPerSample
        {
            get { return bitsPerSample; }
            set { bitsPerSample = value; RecalcBlockSizes(); }
        }

        public FormatChunk()
        {
            ChunkId = chunkId;
            ChunkSize = 16;
            FormatTag = 1;          /* MS PCM (Uncompressed wave file) */
            Channels = 2;           /* Default to stereo */
            Frequency = 44100;      /* Default to 44100hz */
            BitsPerSample = 16;     /* Default to 16bits */
            RecalcBlockSizes();
        }

        private void RecalcBlockSizes()
        {
            BlockAlign = (ushort)(channels * (bitsPerSample / 8));
            AverageBytesPerSec = frequency * BlockAlign;
        }

        public byte[] GetBytes()
        {
            List<byte> chunkBytes = new List<byte>();

            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
            chunkBytes.AddRange(BitConverter.GetBytes(FormatTag));
            chunkBytes.AddRange(BitConverter.GetBytes(Channels));
            chunkBytes.AddRange(BitConverter.GetBytes(Frequency));
            chunkBytes.AddRange(BitConverter.GetBytes(AverageBytesPerSec));
            chunkBytes.AddRange(BitConverter.GetBytes(BlockAlign));
            chunkBytes.AddRange(BitConverter.GetBytes(BitsPerSample));

            return chunkBytes.ToArray();
        }

        public uint Length()
        {
            return (uint)GetBytes().Length;
        }
    }

    class DataChunk
    {
        const string chunkId = "data";

        public string ChunkId { get; private set; }
        public uint ChunkSize { get; set; }
        public List<short> WaveData { get; private set; }

        public DataChunk()
        {
            ChunkId = chunkId;
            ChunkSize = 0;
            WaveData = new List<short>();
        }

        public byte[] GetBytes()
        {
            List<byte> chunkBytes = new List<byte>();

            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
            byte[] bufferBytes = new byte[WaveData.Count * 2];
            Buffer.BlockCopy(WaveData.ToArray(), 0, bufferBytes, 0, bufferBytes.Length);
            chunkBytes.AddRange(bufferBytes.ToList());

            return chunkBytes.ToArray();
        }

        public uint Length()
        {
            return (uint)GetBytes().Length;
        }

        public void AddSampleData(short[] leftBuffer, short[] rightBuffer)
        {
            short[] newWaveData = new short[leftBuffer.Length + rightBuffer.Length];

            int bufferOffset = 0;
            for (int index = 0; index < newWaveData.Length; index += 2)
            {
                newWaveData[index] = leftBuffer[bufferOffset];
                newWaveData[index + 1] = rightBuffer[bufferOffset];
                bufferOffset++;
            }
            WaveData.AddRange(newWaveData);

            ChunkSize += (uint)(newWaveData.Length * 2);
        }

        public void AddSampleData(short[] stereoBuffer)
        {
            WaveData.AddRange(stereoBuffer);

            ChunkSize += (uint)(stereoBuffer.Length * 2);
        }
    }
}
