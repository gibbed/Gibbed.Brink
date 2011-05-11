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
using Gibbed.Helpers;

namespace Gibbed.Brink.FileFormats.Package
{
    public struct NameHash
    {
        public uint A;
        public uint B;
        public uint C;
        public uint D;

        public NameHash(uint a, uint b, uint c, uint d)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.D = d;
        }

        public NameHash(byte[] bytes)
        {
            if (bytes.Length != 16)
            {
                throw new ArgumentException("must be 16 bytes", "bytes");
            }

            this.A = BitConverter.ToUInt32(bytes, 0).Swap();
            this.B = BitConverter.ToUInt32(bytes, 4).Swap();
            this.C = BitConverter.ToUInt32(bytes, 8).Swap();
            this.D = BitConverter.ToUInt32(bytes, 12).Swap();
        }

        public override string ToString()
        {
            return string.Format("{0:X8}{1:X8}{2:X8}{3:X8}",
                this.A, this.B, this.C, this.D);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return (NameHash)obj == this;
        }

        public static bool operator !=(NameHash a, NameHash b)
        {
            return
                a.A != b.A ||
                a.B != b.B ||
                a.C != b.C ||
                a.D != b.D;
        }

        public static bool operator ==(NameHash a, NameHash b)
        {
            return
                a.A == b.A &&
                a.B == b.B &&
                a.C == b.C &&
                a.D == b.D;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.A.GetHashCode();
                hash = hash * 23 + this.B.GetHashCode();
                hash = hash * 23 + this.C.GetHashCode();
                hash = hash * 23 + this.D.GetHashCode();
                return hash;
            }
        }
    }
}
