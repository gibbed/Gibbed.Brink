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
using Gibbed.Brink.FileFormats;
using Gibbed.Helpers;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using NDesk.Options;

namespace Gibbed.Brink.Unpack
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool extractUnknowns = true;
            bool overwriteFiles = false;

            OptionSet options = new OptionSet()
            {
                {
                    "o|overwrite",
                    "overwrite existing files",
                    v => overwriteFiles = v != null
                },
                {
                    "u|no-unknowns",
                    "don't extract unknown files",
                    v => extractUnknowns = v == null
                },
                {
                    "h|help",
                    "show this message and exit", 
                    v => showHelp = v != null
                },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_sdpk2 [output_dir]", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            string inputPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, null);

            var manager = Setup.Manager.Load();
            if (manager.ActiveProject != null)
            {
                manager.ActiveProject.Load();
            }
            else
            {
                Console.WriteLine("Warning: no active project loaded.");
            }
            Setup.Project project = manager.ActiveProject;

            using (var input = File.Open(inputPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var pkg = new PackageFile();
                pkg.Deserialize(input);

                long current = 1;
                long total = pkg.Entries.Count;

                foreach (var entry in pkg.Entries)
                {
                    string name = null;
                    bool isUnknown = false;

                    if (project != null)
                    {
                        name = project.GetFileName(entry.NameHash);
                    }

                    if (name == null)
                    {
                        if (extractUnknowns == false)
                        {
                            continue;
                        }

                        isUnknown = true;

                        string extension;

                        // detect type
                        {
                            var guess = new byte[16];
                            int read = 0;

                            if (entry.UncompressedSize > 0)
                            {
                                input.Seek(entry.Offset, SeekOrigin.Begin);
                                var uncompressedBlockSize = (uint)Math.Min(pkg.BlockSize, entry.UncompressedSize);
                                var compressedBlockSize = pkg.CompressedBlockSizes[entry.CompressedBlockSizeIndex];

                                if (compressedBlockSize > uncompressedBlockSize)
                                {
                                    throw new InvalidOperationException();
                                }

                                if (compressedBlockSize == 0 ||
                                    entry.UncompressedSize == compressedBlockSize)
                                {
                                    if (compressedBlockSize == 0)
                                    {
                                        compressedBlockSize = pkg.BlockSize;
                                    }

                                    read = input.Read(guess, 0, (int)Math.Min(uncompressedBlockSize, guess.Length));
                                }
                                else
                                {
                                    var compressedBlock = input.ReadToMemoryStream(compressedBlockSize);
                                    var uncompressedBlock = new InflaterInputStream(compressedBlock);
                                    read = uncompressedBlock.Read(guess, 0, (int)Math.Min(uncompressedBlockSize, guess.Length));
                                }
                            }

                            extension = FileExtensions.Detect(guess, read);
                        }

                        name = entry.NameHash.ToString();
                        name = Path.ChangeExtension(name, "." + extension);
                        name = Path.Combine(extension, name);
                        name = Path.Combine("__UNKNOWN", name);
                    }
                    else
                    {
                        name = name.Replace("/", "\\");
                        if (name.StartsWith("\\") == true)
                        {
                            name = name.Substring(1);
                        }
                    }

                    Console.WriteLine("[{0}/{1}] {2}",
                        current, total, name);
                    current++;

                    var entryPath = Path.Combine(outputPath, name);
                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                    if (overwriteFiles == false &&
                        File.Exists(entryPath) == true)
                    {
                        continue;
                    }

                    using (var output = File.Create(entryPath))
                    {
                        if (entry.UncompressedSize > 0)
                        {
                            input.Seek(entry.Offset, SeekOrigin.Begin);

                            var index = entry.CompressedBlockSizeIndex;
                            var uncompressedSize = entry.UncompressedSize;
                            while (uncompressedSize > 0)
                            {
                                var uncompressedBlockSize = Math.Min(pkg.BlockSize, uncompressedSize);
                                var compressedBlockSize = pkg.CompressedBlockSizes[index++];

                                if (compressedBlockSize > uncompressedBlockSize)
                                {
                                    throw new InvalidOperationException();
                                }

                                if (compressedBlockSize == 0 ||
                                    uncompressedSize == compressedBlockSize)
                                {
                                    if (compressedBlockSize == 0)
                                    {
                                        compressedBlockSize = pkg.BlockSize;
                                    }

                                    output.WriteFromStream(input, compressedBlockSize);

                                    if (uncompressedBlockSize != compressedBlockSize)
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    uncompressedSize -= compressedBlockSize;
                                }
                                else
                                {
                                    var compressedBlock = input.ReadToMemoryStream(compressedBlockSize);
                                    var uncompressedBlock = new InflaterInputStream(compressedBlock);
                                    output.WriteFromStream(uncompressedBlock, uncompressedBlockSize);
                                    uncompressedSize -= uncompressedBlockSize;

                                    // why would there be junk data...? :argh:
                                    if (compressedBlock.Position != compressedBlock.Length)
                                    {
                                        //throw new InvalidOperationException();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
