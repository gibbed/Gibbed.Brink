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

using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Brink.FileFormats
{
    public static partial class StreamHelpers
    {
        public static Package.NameHash ReadNameHash(this Stream stream)
        {
            var hash = new Package.NameHash();
            hash.A = stream.ReadValueU32(false);
            hash.B = stream.ReadValueU32(false);
            hash.C = stream.ReadValueU32(false);
            hash.D = stream.ReadValueU32(false);
            return hash;
        }

        public static void WriteNameHash(this Stream stream, Package.NameHash hash)
        {
            stream.WriteValueU32(hash.A, false);
            stream.WriteValueU32(hash.B, false);
            stream.WriteValueU64(hash.C, false);
            stream.WriteValueU64(hash.D, false);
        }
    }
}
