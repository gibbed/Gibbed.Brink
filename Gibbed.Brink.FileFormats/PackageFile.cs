/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Brink.FileFormats
{
    public class PackageFile
    {
        public ushort MajorVersion;
        public ushort MinorVersion;
        public Package.CompressionFormat CompressionFormat;
        public uint DataBlockSize;
        public uint CompressedBlockValueSize;

        public List<Package.Entry> Entries
            = new List<Package.Entry>();
        public List<uint> CompressedBlockSizes
            = new List<uint>();

        public void Deserialize(Stream input)
        {
            if (input.Position + 32 > input.Length)
            {
                throw new EndOfStreamException("not enough data for header");
            }

            var magic = input.ReadValueU32(false);
            if (magic != 0x50534152) // PSAR
            {
                throw new FormatException("bad magic");
            }

            this.MajorVersion = input.ReadValueU16(false);
            if (this.MajorVersion != 1)
            {
                throw new FormatException("unsupported version");
            }
            this.MinorVersion = input.ReadValueU16(false);

            this.CompressionFormat = input.ReadValueEnum<Package.CompressionFormat>(false);
            var headerSize = input.ReadValueU32(false);
            var entrySize = input.ReadValueU32(false);
            var entryCount = input.ReadValueU32(false);
            this.DataBlockSize = input.ReadValueU32(false);
            this.CompressedBlockValueSize = input.ReadValueU32(false);

            if (entrySize != 30 || this.CompressedBlockValueSize != 2)
            {
                throw new FormatException("unsupported values in header");
            }
            else if (entryCount * entrySize > (headerSize - 32))
            {
                throw new FormatException("entry data exceeds header size");
            }

            var extraSize = headerSize - 32;
            var entryData = input.ReadToMemoryStream(entryCount * entrySize);
            extraSize -= entryCount * entrySize;

            if ((extraSize & 1) != 0)
            {
                throw new FormatException();
            }

            this.CompressedBlockSizes.Clear();
            for (uint i = 0; i < extraSize / 2; i++)
            {
                this.CompressedBlockSizes.Add(input.ReadValueU16(false));
            }

            this.Entries.Clear();
            for (uint i = 0; i < entryCount; i++)
            {
                var entry = new Package.Entry();
                entry.NameHash = entryData.ReadNameHash();

                entry.CompressedBlockSizeIndex = entryData.ReadValueS32(false);

                entry.UncompressedSize = 0;
                entry.UncompressedSize = ((long)entryData.ReadValueU8()) << 32;
                entry.UncompressedSize |= (long)entryData.ReadValueU32(false);

                entry.Offset = 0;
                entry.Offset = ((long)entryData.ReadValueU8()) << 32;
                entry.Offset |= (long)entryData.ReadValueU32(false);

                if (entry.CompressedBlockSizeIndex > this.CompressedBlockSizes.Count)
                {
                    throw new FormatException("entry compressed block size index out of bounds");
                }

                this.Entries.Add(entry);
            }
        }
    }
}
