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

using System.Text;

namespace Gibbed.Brink.FileFormats
{
    public static class FileExtensions
    {
        public static string Detect(byte[] guess, int read)
        {
            if (read == 0)
            {
                return "null";
            }

            if (
                read >= 2 &&
                guess[0] == 'P' &&
                guess[1] == 'K')
            {
                return "zip";
            }
            else if (
                read >= 4 &&
                guess[0] == 0x89 &&
                guess[1] == 'P' &&
                guess[2] == 'N' &&
                guess[3] == 'G')
            {
                return "png";
            }
            else if (
                read >= 4 &&
                guess[0] == 'R' &&
                guess[1] == 'I' &&
                guess[2] == 'F' &&
                guess[3] == 'F')
            {
                return "wav";
            }
            else if (
                read >= 4 &&
                guess[1] == 'g' &&
                guess[2] == 'm' &&
                guess[3] == 'i')
            {
                return "img";
            }
            else if (
                read >= 4 &&
                guess[1] == 'l' &&
                guess[2] == 'd' &&
                guess[3] == 'm')
            {
                return "mdl";
            }
            else if (
                read >= 4 &&
                guess[1] == '5' &&
                guess[2] == 'd' &&
                guess[3] == 'm')
            {
                return "md5";
            }
            else if (
                read >= 4 &&
                guess[0] == 'A' &&
                guess[1] == 'N' &&
                guess[2] == 'M' &&
                guess[3] == 'B')
            {
                return "anmb";
            }
            else if (
                read >= 4 &&
                guess[0] == 'A' &&
                guess[1] == 'N' &&
                guess[2] == 'M' &&
                guess[3] == 'V')
            {
                return "anmv";
            }
            else if (
                read >= 5 &&
                guess[0] == 0 &&
                guess[1] == 0 &&
                guess[2] == 4 &&
                guess[3] == 0 &&
                guess[4] == 1)
            {
                return "sdmd2";
            }
            else if (
                read == 6 &&
                Encoding.ASCII.GetString(guess, 0, 6) == "dummy\n")
            {
                return "dummy";
            }
            else if (
                read >= 9 &&
                Encoding.ASCII.GetString(guess, 0, 9) == "#version ")
            {
                return "shader";
            }

            return "unknown";
        }
    }
}
