﻿using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Volition.FileFormats
{
    public class AsmFileEntry
    {
        public string Name { get; set; }
        public byte Unk1 { get; set; }
        public byte Unk2 { get; set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public int HeaderFileSize { get; set; }
        public int DataFileSize { get; set; }

        public void Deserialize(Stream input)
        {
            this.Name = input.ReadStringASCII(input.ReadValueU16());
            this.Unk1 = input.ReadValueU8();
            this.Unk2 = input.ReadValueU8();
            this.Unk3 = input.ReadValueU8();
            this.Unk4 = input.ReadValueU8();
            this.HeaderFileSize = input.ReadValueS32();
            this.DataFileSize = input.ReadValueS32();
        }

        public void Serialize(Stream output)
        {
            output.WriteValueU16((ushort)this.Name.Length);
            output.WriteStringASCII(this.Name.Substring(0, (ushort)this.Name.Length));
            output.WriteValueU8(this.Unk1);
            output.WriteValueU8(this.Unk2);
            output.WriteValueU8(this.Unk3);
            output.WriteValueU8(this.Unk4);
            output.WriteValueS32(this.HeaderFileSize);
            output.WriteValueS32(this.DataFileSize);
        }
    }

    public class AsmEntry
    {
        public string Unk0 { get; set; }
        public byte Unk1 { get; set; }
        public short Unk2 { get; set; }
        public int Unk4 { get; set; }
        public int Unk6 { get; set; }
        public List<int> FileSizes  { get; set; }
        public List<AsmFileEntry> Files { get; set; }
        
        public AsmEntry()
        {
            this.FileSizes = new List<int>();
            this.Files = new List<AsmFileEntry>();
        }

        public void Deserialize(Stream input)
        {
            this.Unk0 = input.ReadStringASCII(input.ReadValueU16());
            this.Unk1 = input.ReadValueU8();
            this.Unk2 = input.ReadValueS16();
            short subEntryCount = input.ReadValueS16();
            this.Unk4 = input.ReadValueS32();
            int unk7count = input.ReadValueS32();
            this.Unk6 = input.ReadValueS32();
            
            this.FileSizes.Clear();
            for (int i = 0; i < unk7count; i++)
            {
                this.FileSizes.Add(input.ReadValueS32());
            }

            this.Files.Clear();
            for (short i = 0; i < subEntryCount; i++)
            {
                AsmFileEntry subEntry = new AsmFileEntry();
                subEntry.Deserialize(input);
                this.Files.Add(subEntry);
            }
        }

        public void Serialize(Stream output)
        {
            output.WriteValueU16((ushort)this.Unk0.Length);
            output.WriteStringASCII(this.Unk0.Substring(0, (ushort)this.Unk0.Length));
            output.WriteValueU8(this.Unk1);
            output.WriteValueS16(this.Unk2);
            output.WriteValueS16((short)this.Files.Count);
            output.WriteValueS32(this.Unk4);
            output.WriteValueS32(this.FileSizes.Count);
            output.WriteValueS32(this.Unk6);

            for (int i = 0; i < this.FileSizes.Count; i++)
            {
                output.WriteValueS32(this.FileSizes[i]);
            }

            for (short i = 0; i < (short)this.Files.Count; i++)
            {
                this.Files[i].Serialize(output);
            }
        }
    }

    public class AsmFile
    {
        public short Version;
        public List<AsmEntry> Entries = new List<AsmEntry>();

        public void Deserialize(Stream input)
        {
            if (input.ReadValueU32() != 0xBEEFFEED)
            {
                throw new FormatException("not an asm file");
            }

            this.Version = input.ReadValueS16();
            if (this.Version != 5)
            {
                throw new FormatException("unsupported asm version " + this.Version.ToString());
            }

            short count = input.ReadValueS16();

            this.Entries.Clear();
            for (short i = 0; i < count; i++)
            {
                AsmEntry entry = new AsmEntry();
                entry.Deserialize(input);
                this.Entries.Add(entry);
            }
        }

        public void Serialize(Stream output)
        {
            output.WriteValueU32(0xBEEFFEED);
            output.WriteValueS16(this.Version);
            
            output.WriteValueS16((short)this.Entries.Count);
            for (short i = 0; i < (short)this.Entries.Count; i++)
            {
                this.Entries[i].Serialize(output);
            }
        }
    }
}
