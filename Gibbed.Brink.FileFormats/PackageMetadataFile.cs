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

using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.Helpers;

namespace Gibbed.Brink.FileFormats
{
    public class PackageMetadataFile
    {
        public List<PackageMetadata.Entry> Entries
            = new List<PackageMetadata.Entry>();

        public void Deserialize(Stream input)
        {
            var count1 = input.ReadValueU32(false);
            var unk1 = input.ReadValueU8();
            var data1 = input.ReadToMemoryStream(count1 * 4);

            var count2 = input.ReadValueU32(false);
            var unk2 = input.ReadValueU8();
            var data2 = input.ReadToMemoryStream(count2 * 4);

            var count3 = input.ReadValueU32(false);
            var count4 = input.ReadValueU32(false);

            var stringCount = input.ReadValueU32(false);
            var stringOffsets = new uint[stringCount];
            for (uint i = 0; i < stringCount; i++)
            {
                stringOffsets[i] = input.ReadValueU32(false);
            }
            var stringSize = input.ReadValueU32(false);
            var stringData = input.ReadToMemoryStream(stringSize);

            var entryCount = input.ReadValueU32(false);
            this.Entries.Clear();
            for (uint i = 0; i < entryCount; i++)
            {
                var entry = new PackageMetadata.Entry();

                stringData.Seek(stringOffsets[input.ReadValueU32(false)], SeekOrigin.Begin);
                entry.DirectoryName = stringData.ReadStringZ(Encoding.ASCII);

                stringData.Seek(stringOffsets[input.ReadValueU32(false)], SeekOrigin.Begin);
                entry.FileName = stringData.ReadStringZ(Encoding.ASCII);

                entry.Unknown08 = input.ReadValueU32(false);
                entry.Unknown0C = input.ReadValueU32(false);
                entry.Unknown10 = input.ReadValueU32(false);

                this.Entries.Add(entry);
            }
        }
    }
}
