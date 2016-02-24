using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace raw2cdng_v2
{
    class io
    {
        public static string[] RAWFileEnding = new string[101];// { "RAW", "R00", "R01", "R02", "R03", "R04", "R05", "R06", "R07", "R08", "R09", "R10", "R11", "R12", "R13", "R14", "R15", "R16", "R17", "R18", "R19", "R20" };
        public static string[] MLVFileEnding = new string[101];// { "MLV", "M00", "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12", "M13", "M14", "M15", "M16", "M17", "M18", "M19", "M20" };
        public static string blocknames = "RAWIMLVIWBALIDNTVIDFRTCILENSEXPOWAVIAUDFINFONULLXREFMARKDISOELVLSTYLBKUP";
        // old classic short header : public static byte[] RIFFheader = Properties.Resources.RIFFtemplate;
        public static byte[] riffHeader = Properties.Resources.riffbwf_Template;
        public static List<colormatrix> colormatrices = new List<colormatrix>();

        public static void fillupEndingLists()
        {
            
            for (int i = -1; i < 100; i++)
            {
                if (i == -1)
                {
                    RAWFileEnding[0] = "RAW";
                    MLVFileEnding[0] = "MLV";
                }
                else
                {
                    RAWFileEnding[i+1] = "R" + i.ToString("00") ;
                    MLVFileEnding[i+1] = "M" + i.ToString("00");
                }
            }
        }

        public static void setMatrices()
        {
            // --- fullformat sensors ---
            colormatrices.Add(new colormatrix()
            {
                modell =    "Canon EOS 5D Mark III",
                colormatrixA = new int[] { 7234, 10000, -1413, 10000, -600, 10000, -3631, 10000, 11150, 10000, 2850, 10000, -382, 10000, 1335, 10000, 6437, 10000 },
                colormatrixB = new int[] { 6722, 10000, -635, 10000, -963, 10000, -4287, 10000, 12460, 10000, 2028, 10000, -908, 10000, 2162, 10000, 5668, 10000 },
                forwardmatrixA = new int[] { 7868, 10000, 92, 10000, 1683, 10000, 2291, 10000, 8615, 10000, -906, 10000, 27, 10000, -4752, 10000, 12976, 10000 },
                forwardmatrixB = new int[] { 7637, 10000, 805, 10000, 1201, 10000, 2649, 10000, 9179, 10000, -1828, 10000, 137, 10000, -2456, 10000, 10570, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS 5D Mark II",
                colormatrixA = new int[] { 5309, 10000, -229, 10000, -336, 10000, -6241, 10000, 13265, 10000, 3337, 10000, -817, 10000, 1215, 10000, 6664, 10000 },
                colormatrixB = new int[] { 4716, 10000, 603, 10000, -830, 10000, -7798, 10000, 15474, 10000, 2480, 10000, -1496, 10000, 1937, 10000, 6651, 10000 },
                forwardmatrixA = new int[] { 8924, 10000, -1041, 10000, 1760, 10000, 4351, 10000, 6621, 10000, -972, 10000, 505, 10000, -1562, 10000, 9308, 10000 },
                forwardmatrixB = new int[] { 8924, 10000, -1041, 10000, 1760, 10000, 4351, 10000, 6621, 10000, -972, 10000, 505, 10000, -1562, 10000, 9308, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS 7D",
                colormatrixA = new int[] { 11620, 10000, -6350, 10000, 5, 10000, -2558, 10000, 10146, 10000, 2813, 10000, 24, 10000, 858, 10000, 6926, 10000 },
                colormatrixB = new int[] { 6844, 10000, -996, 10000, -856, 10000, -3876, 10000, 11761, 10000, 2396, 10000, -593, 10000, 1772, 10000, 6198, 10000 },
                forwardmatrixA = new int[] { 5445, 10000, 3536, 10000, 662, 10000, 1106, 10000, 10136, 10000, -1242, 10000, -374, 10000, -3559, 10000, 12184, 10000 },
                forwardmatrixB = new int[] { 7415, 10000, 1533, 10000, 695, 10000, 2499, 10000, 9997, 10000, -2497, 10000, -22, 10000, -1933, 10000, 10207, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS 6D",
                colormatrixA = new int[] { 7546, 10000, -1435, 10000, -929, 10000, -3846, 10000, 11488, 10000, 2692, 10000, -332, 10000, 1209, 10000, 6370, 10000 },
                colormatrixB = new int[] { 7034, 10000, -804, 10000, -1014, 10000, -4420, 10000, 12564, 10000, 2058, 10000, -851, 10000, 1994, 10000, 5758, 10000 },
                forwardmatrixA = new int[] { 7763, 10000, 65, 10000, 1815, 10000, 2364, 10000, 8351, 10000, -715, 10000, -59, 10000, -4228, 10000, 12538, 10000 },
                forwardmatrixB = new int[] { 7464, 10000, 1044, 10000, 1135, 10000, 2648, 10000, 9173, 10000, -1820, 10000, 113, 10000, -2154, 10000, 10292, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell = "Canon EOS 70D",
                colormatrixA = new int[] { 7546, 10000, -1435, 10000, -929, 10000, -3846, 10000, 11488, 10000, 2692, 10000, -332, 10000, 1209, 10000, 6370, 10000 },
                colormatrixB = new int[] { 7034, 10000, -804, 10000, -1014, 10000, -4420, 10000, 12564, 10000, 2058, 10000, -851, 10000, 1994, 10000, 5758, 10000 },
                forwardmatrixA = new int[] { 7763, 10000, 65, 10000, 1815, 10000, 2364, 10000, 8351, 10000, -715, 10000, -59, 10000, -4228, 10000, 12538, 10000 },
                forwardmatrixB = new int[] { 7464, 10000, 1044, 10000, 1135, 10000, 2648, 10000, 9173, 10000, -1820, 10000, 113, 10000, -2154, 10000, 10292, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS 60D",
                colormatrixA = new int[] { 7428, 10000, -1897, 10000, -491, 10000, -3505, 10000, 10963, 10000, 2929, 10000, -337, 10000, 1242, 10000, 6413, 10000 },
                colormatrixB = new int[] { 6719, 10000, -994, 10000, -925, 10000, -4408, 10000, 12426, 10000, 2211, 10000, -887, 10000, 2129, 10000, 6051, 10000 },
                forwardmatrixA = new int[] { 7550, 10000, 645, 10000, 1448, 10000, 2138, 10000, 8936, 10000, -1075, 10000, -5, 10000, -4306, 10000, 12562, 10000 },
                forwardmatrixB = new int[] { 7286, 10000, 1385, 10000, 972, 10000, 2600, 10000, 9468, 10000, -2068, 10000, 93, 10000, -2268, 10000, 10426, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =      "Canon EOS 50D",
                colormatrixA = new int[] { 5852, 10000, -578, 10000, -41, 10000, -4691, 10000, 11696, 10000, 3427, 10000, -886, 10000, 2323, 10000, 6879, 10000 },
                colormatrixB = new int[] { 4920, 10000, 616, 10000, -593, 10000, -6493, 10000, 13964, 10000, 2784, 10000, -1774, 10000, 3178, 10000, 7005, 10000 },
                forwardmatrixA = new int[] { 8716, 10000, -692, 10000, 1618, 10000, 3408, 10000, 8077, 10000, -1486, 10000, -13, 10000, -6583, 10000, 14847, 10000 },
                forwardmatrixB = new int[] { 9485, 10000, -1150, 10000, 1308, 10000, 4313, 10000, 7807, 10000, -2120, 10000, 293, 10000, -2826, 10000, 10785, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                // extracted from 500D-dng
                modell =     "Canon EOS 500D",
                colormatrixA = new int[] { 5660, 10000, -436, 10000, -88, 10000, -5034, 10000, 12093, 10000, 3347, 10000, -926, 10000, 2289, 10000, 6588, 10000 },
                colormatrixB = new int[] { 4763, 10000, 712, 10000, -646, 10000, -6821, 10000, 14399, 10000, 2640, 10000, -1921, 10000, 3276, 10000, 6561, 10000 },
                forwardmatrixA = new int[] { 9474, 10000, -1386, 10000, 1555, 10000, 4367, 10000, 7419, 10000, -1786, 10000, 398, 10000, -3048, 10000, 10901, 10000 },
                forwardmatrixB = new int[] { 9474, 10000, -1386, 10000, 1555, 10000, 4367, 10000, 7419, 10000, -1786, 10000, 398, 10000, -3048, 10000, 10901, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell = "Canon EOS 550D",
                colormatrixA = new int[] { 7755, 10000, -2449, 10000, -349, 10000, -3106, 10000, 10222, 10000, 3362, 10000, -156, 10000, 986, 10000, 6409, 10000 },
                colormatrixB = new int[] { 6941, 10000, -1164, 10000, -857, 10000, -3825, 10000, 11597, 10000, 2534, 10000, -416, 10000, 1540, 10000, 6039, 10000 },
                forwardmatrixA = new int[] { 7163, 10000, 1301, 10000, 1179, 10000, 1926, 10000, 9543, 10000, -1469, 10000, -278, 10000, -3830, 10000, 12359, 10000 },
                forwardmatrixB = new int[] { 7239, 10000, 1838, 10000, 566, 10000, 2467, 10000, 10246, 10000, -2713, 10000, -112, 10000, -1754, 10000, 10117, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS 600D",
                colormatrixA = new int[] { 7164, 10000, -1916, 10000, -431, 10000, -3361, 10000, 10600, 10000, 3200, 10000, -272, 10000, 1058, 10000, 6442, 10000 },
                colormatrixB = new int[] { 6461, 10000, -907, 10000, -882, 10000, -4300, 10000, 12184, 10000, 2378, 10000, -819, 10000, 1944, 10000, 5931, 10000 },
                forwardmatrixA = new int[] { 7486, 10000, 835, 10000, 1322, 10000, 2099, 10000, 9147, 10000, -1245, 10000, -12, 10000, -3822, 10000, 12085, 10000 },
                forwardmatrixB = new int[] { 7359, 10000, 1365, 10000, 918, 10000, 2610, 10000, 9687, 10000, -2297, 10000, 98, 10000, -2155, 10000, 10309, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS 650D",
                colormatrixA = new int[] { 6985, 10000, -1611, 10000, -397, 10000, -3596, 10000, 10749, 10000, 3295, 10000, -349, 10000, 1136, 10000, 6512, 10000 },
                colormatrixB = new int[] { 6602, 10000, -841, 10000, -939, 10000, -4472, 10000, 12458, 10000, 2247, 10000, -975, 10000, 2039, 10000, 6148, 10000 },
                forwardmatrixA = new int[] { 7747, 10000, 485, 10000, 1411, 10000, 2340, 10000, 8840, 10000, -1180, 10000, 105, 10000, -4147, 10000, 12293, 10000 },
                forwardmatrixB = new int[] { 7397, 10000, 1199, 10000, 1047, 10000, 2650, 10000, 9355, 10000, -2005, 10000, 193, 10000, -2113, 10000, 10171, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS 700D",
                colormatrixA = new int[] { 6985, 10000, -1611, 10000, -397, 10000, -3596, 10000, 10749, 10000, 3295, 10000, -349, 10000, 1136, 10000, 6512, 10000 },
                colormatrixB = new int[] { 6602, 10000, -841, 10000, -939, 10000, -4472, 10000, 12458, 10000, 2247, 10000, -975, 10000, 2039, 10000, 6148, 10000 },
                forwardmatrixA = new int[] { 7747, 10000, 485, 10000, 1411, 10000, 2340, 10000, 8840, 10000, -1180, 10000, 105, 10000, -4147, 10000, 12293, 10000 },
                forwardmatrixB = new int[] { 7397, 10000, 1199, 10000, 1047, 10000, 2650, 10000, 9355, 10000, -2005, 10000, 193, 10000, -2113, 10000, 10171, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS 1100D",
                colormatrixA = new int[] { 6873, 10000, -1696, 10000, -529, 10000, -3659, 10000, 10795, 10000, 3313, 10000, -362, 10000, 1165, 10000, 7234, 10000 },
                colormatrixB = new int[] { 6444, 10000, -904, 10000, -893, 10000, -4563, 10000, 12308, 10000, 2535, 10000, -903, 10000, 2016, 10000, 6728, 10000 },
                forwardmatrixA = new int[] { 7607, 10000, 647, 10000, 1389, 10000, 2337, 10000, 8876, 10000, -1213, 10000, 93, 10000, -3625, 10000, 11783, 10000 },
                forwardmatrixB = new int[] { 7357, 10000, 1377, 10000, 909, 10000, 2729, 10000, 9630, 10000, -2359, 10000, 104, 10000, -1940, 10000, 10087, 10000 }
            });
            colormatrices.Add(new colormatrix()
            {
                modell =     "Canon EOS M",
                colormatrixA = new int[] { 7357, 10000, 1377, 10000, 909, 10000, 2729, 10000, 9630, 10000, -2359, 10000, 104, 10000, -1940, 10000, 10087, 10000 },
                colormatrixB = new int[] { 6602, 10000, -841, 10000, -939, 10000, -4472, 10000, 12458, 10000, 2247, 10000, -975, 10000, 2039, 10000, 6148, 10000 },
                forwardmatrixA = new int[] { 7747, 10000, 485, 10000, 1411, 10000, 2340, 10000, 8840, 10000, -1180, 10000, 105, 10000, -4147, 10000, 12293, 10000 },
                forwardmatrixB = new int[] { 7397, 10000, 1199, 10000, 1047, 10000, 2650, 10000, 9355, 10000, -2005, 10000, 193, 10000, -2113, 10000, 10171, 10000 }
            });
        }

        public static bool isMLV(string f)
        {
            f = f.ToUpper();
            return f != null && f.EndsWith(".MLV", StringComparison.Ordinal);
        }
        public static bool isRAW(string f)
        {
            f = f.ToUpper();
            return f != null && f.EndsWith(".RAW", StringComparison.Ordinal);
        }

        public static bool isFolder(string f)
        {
            bool result = false;
            FileAttributes attr = File.GetAttributes(@f);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                result = true;
            else
                result = false;

            return result; 
        }

        public static void dirSearch(string sDir, ref List<string> fileList)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if(isMLV(f)||isRAW(f)) fileList.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        if (isMLV(f) || isRAW(f)) fileList.Add(f);
                    }
                    dirSearch(d, ref fileList);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }


        public static string setFileinfoData(string fn, filedata fd)
        {
            fd.fileName = fn;
            fd.fileNameOnly = Path.GetFileNameWithoutExtension(fn);
            fd.sourcePath = Path.GetDirectoryName(fn);
            fd.fileNameShort = calc.setFilenameShort(fd.fileNameOnly);
            fd.fileNameNum = calc.setFilenameNum(fd.fileNameOnly);
            fd.parentSourcePath = fd.sourcePath.Split(Path.DirectorySeparatorChar).Last();
                    
            fd.outputFilename = "";
            if (debugging.debugLogEnabled) debugging._saveDebug("[setFileinfoData] set/changed Filedata");
            string errorString = "";
            return errorString;
        }

        public static string createMLVBlockList(string filename, raw raw)
        {
            if (debugging.debugLogEnabled) debugging._saveDebug("[createMLVBlocklist] started");
            
            Blocks.mlvBlockList.Clear();
            
            // -- count stripFiles
            // -- and count fileSize
            int filesize = 0;
            raw.data.metaData.splitCount = 0;
            for (var i = 0; i < MLVFileEnding.Length; i++)
            {
                string searchSplittedFile = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename) + "." + MLVFileEnding[i];
                if (File.Exists(searchSplittedFile) == true)
                {
                    raw.data.metaData.splitCount++;
                    FileInfo fi = new FileInfo(searchSplittedFile);
	                filesize += (int)(fi.Length/1024/1024);
                }
                else
                {
                    break;
                }
            }
            raw.data.fileData.fileSize = filesize;
            // iterate thru all files
            // and put the vidfblocks into list
            if (debugging.debugLogEnabled)
            {
                debugging._saveDebug("[createMLVBlocklist] splitFiles counted (" + raw.data.metaData.splitCount + ")");
                debugging._saveDebug("[createMLVBlocklist] filesize counted (" + raw.data.fileData.fileSize + "MB)");
            }
            long offset;
            int bl;
            byte[] chunk = new byte[16];
            for (var j = 0; j < raw.data.metaData.splitCount; j++)
            {
                offset = 0;
                bl = 0;
                string fn = raw.data.fileData.sourcePath + Path.DirectorySeparatorChar + raw.data.fileData.fileNameOnly + "." + MLVFileEnding[j];

                if (debugging.debugLogEnabled) debugging._saveDebug("[createMLVBlocklist] indexing Blocks in "+fn);
                
                FileInfo fi = new FileInfo(fn);
                FileStream fs = fi.OpenRead();
                long fl = fs.Length;
                while ( ((fl - offset) >= 0) && (offset>=0) && (bl>=0) )
                {
                    try
                    {
                        fs.Position = offset;
                        fs.Read(chunk, 0, 16);
                        bl = BitConverter.ToInt32(chunk, 4);
                        Blocks.mlvBlockList.Add(new Blocks.mlvBlock()
                        {
                            blockTag = Encoding.ASCII.GetString(new byte[4] { chunk[0], chunk[1], chunk[2], chunk[3] }),
                            blockLength = bl,
                            fileOffset = offset,
                            timestamp = BitConverter.ToInt64(chunk, 8),
                            fileNo = j
                        });
                        offset += bl;
                        if (j > 0) { }
                    }
                    catch(Exception e)
                    {
                        if (debugging.debugLogEnabled)
                        {
                            debugging._saveDebug("[createMLVBlocklist] --- exception(!) --- ");
                            debugging._saveDebug("[createMLVBlocklist] maybe you ve lost some frames.");
                            debugging._saveDebug("[createMLVBlocklist] blockTag|offset|fileNo : " + Encoding.ASCII.GetString(new byte[4] { chunk[0], chunk[1], chunk[2], chunk[3] })+"|"+offset.ToString()+"|"+j.ToString());
                            debugging._saveDebug(debugging.getExceptionDetails(e));
                        }
                        //throw;
                    }

                }
            }
            // break/abort import if blocks werent read accordingly
            string errorString = "";
            return errorString;

        }
        
        public static bool isFRSP(Blocks.mlvBlock mlvBlock)
        {
            return (mlvBlock.blockLength > 5242880) ? true:false;
        }

        public static string readVIDFBlockData(raw raw)
        {
            string errorString = "";
            string debugString = "";
            int framecount = 0;

            if (debugging.debugLogEnabled) debugging._saveDebug("[readVIDFBlockData] started");

            foreach (Blocks.mlvBlock VIDFBlock in raw.VIDFBlocks)
            {
                FileInfo fi = new FileInfo(raw.data.fileData.sourcePath + Path.DirectorySeparatorChar + raw.data.fileData.fileNameOnly + "." + MLVFileEnding[VIDFBlock.fileNo]);
                FileStream fs = fi.OpenRead();
                byte[] vidfProp = new byte[32];
                fs.Position = VIDFBlock.fileOffset;
                fs.Read(vidfProp, 0, 32);
                VIDFBlock.MLVFrameNo = BitConverter.ToInt32(new byte[4] { vidfProp[16], vidfProp[17], vidfProp[18], vidfProp[19] }, 0);
                VIDFBlock.EDMACoffset = BitConverter.ToInt32(new byte[4] { vidfProp[28], vidfProp[29], vidfProp[30], vidfProp[31] }, 0); 
                            
                fs.Close();
                if (debugging.debugLogEnabled)
                {
                       debugging._saveDebug("[readVIDFBlockData] : " + VIDFBlock.MLVFrameNo + " at "+raw.data.fileData.fileNameOnly+"."+MLVFileEnding[VIDFBlock.fileNo] + " - "+VIDFBlock.fileOffset);
                }
            }
            return errorString;
        }

        public static raw createRAWBlockList(string filename, raw raw)
        {
            if (debugging.debugLogEnabled) debugging._saveDebug("[createRAWBlocklist] started");

            List<Blocks.rawBlock> rawList = new List<Blocks.rawBlock>();

            int fno = 0;
            bool isSplitted = false;

            string fn = raw.data.fileData.sourcePath + Path.DirectorySeparatorChar + raw.data.fileData.fileNameOnly + "." + RAWFileEnding[fno];
            FileInfo fi = new FileInfo(fn);
            FileStream fs = fi.OpenRead();
            long fl = fs.Length;
            long offset = 0;
            long delta = 0;
            long fileDelta = 0;

            for (int f = 0; f < raw.data.metaData.frames; f++)
            {
                // needed for Split-Detection and offset-delta
                fileDelta = fl - offset;
   
                if (fileDelta < raw.data.metaData.stripByteCount) isSplitted = true;
              
                // add entry
                rawList.Add(new Blocks.rawBlock() { 
                    fileNo = fno,
                    fileOffset = offset,
                    splitted = isSplitted
                });

                // change offset (delta) for next splitfile
                if( isSplitted == true )
                {
                    if (debugging.debugLogEnabled) debugging._saveDebug("[createRAWBlocklist] split on frame "+f);
                    isSplitted = false;
                    
                    fs.Close();
                    fn = raw.data.fileData.sourcePath + Path.DirectorySeparatorChar + raw.data.fileData.fileNameOnly + "." + RAWFileEnding[fno];
                    fi = new FileInfo(fn);
                    fs = fi.OpenRead();
                    
                    fl = fs.Length + fileDelta;
                    offset = -fileDelta+delta;
                    delta = fileDelta;
                    fno++;
                }

                offset += raw.data.metaData.stripByteCount;
            }
            raw.RAWBlocks = rawList;

            fs.Close();

            return raw;
        }

        public static string getMLVAttributes(string filename, List<Blocks.mlvBlock> bList, raw mData)
        {
            string errorString = "";
            if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] started");

            // get Data from RAWI Block
            if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] reading RAWI-Block");
            var RAWI = bList.FirstOrDefault(x => x.blockTag == "RAWI");
            if (RAWI == null)
            {
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] [!] NO RAWI Block");
                errorString += "[NORAWI]";
            }
            
                FileInfo fi = new FileInfo(mData.data.fileData.sourcePath + Path.DirectorySeparatorChar + mData.data.fileData.fileNameOnly + "." + MLVFileEnding[RAWI.fileNo]);
                string readPath = fi.DirectoryName;
                mData.data.fileData.creationTime = fi.CreationTime;
                mData.data.fileData.modificationTime = fi.LastWriteTime;

                string fn = fi.Name;
                FileStream fs = fi.OpenRead();

                fs.Position = RAWI.fileOffset;
                byte[] RAWIArray = new byte[RAWI.blockLength];
                fs.Read(RAWIArray, 0, RAWI.blockLength);

                mData.data.metaData.xResolution = BitConverter.ToUInt16(new byte[2] { RAWIArray[16], RAWIArray[17] }, 0);
                mData.data.metaData.yResolution = BitConverter.ToUInt16(new byte[2] { RAWIArray[18], RAWIArray[19] }, 0);
                mData.data.metaData.blackLevelOld = BitConverter.ToInt32(new byte[4] { RAWIArray[48], RAWIArray[49], RAWIArray[50], RAWIArray[51] }, 0);
                if (mData.data.metaData.blackLevelOld == 0) mData.data.metaData.blackLevelOld = 2037;
                mData.data.metaData.whiteLevelOld = BitConverter.ToInt32(new byte[4] { RAWIArray[52], RAWIArray[53], RAWIArray[54], RAWIArray[55] }, 0);
                if (mData.data.metaData.whiteLevelOld == 0) mData.data.metaData.whiteLevelOld = 15000;
                mData.data.metaData.blackLevelNew = mData.data.metaData.blackLevelOld;
                mData.data.metaData.whiteLevelNew = mData.data.metaData.whiteLevelOld;

                mData.data.metaData.bitsperSample = 14;// RAWIArray[56];
                mData.data.metaData.maximizer = Math.Pow(2, 16) / (mData.data.metaData.whiteLevelOld - mData.data.metaData.blackLevelOld);
                mData.data.metaData.maximize = true;

                byte[] colorMatrix = new byte[72];
                Array.Copy(RAWIArray, 104, colorMatrix, 0, 72);
                mData.data.metaData.colorMatrixA = colorMatrix;
                colorMatrix = null;

                mData.data.metaData.stripByteCount = mData.data.metaData.xResolution * mData.data.metaData.yResolution * mData.data.metaData.bitsperSample / 8;
                mData.data.metaData.stripByteCountReal = mData.data.metaData.stripByteCount;
            
            // from fileheader MLVI
            if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] reading MLVI-Block");
            var MLVI = bList.FirstOrDefault(x => x.blockTag == "MLVI");
            if (MLVI == null)
            {
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] [!] NO MLVI Block");
                errorString += "[NOMLVI]";
            }

            fs.Position = MLVI.fileOffset;
            byte[] MLVIArray = new byte[MLVI.blockLength];
            fs.Read(MLVIArray, 0, MLVI.blockLength);

            mData.data.metaData.frames = BitConverter.ToInt32(new byte[4] { MLVIArray[36], MLVIArray[37], MLVIArray[38], MLVIArray[39] }, 0);
            mData.data.metaData.lostFrames = 0;//BitConverter.ToInt32(new byte[4] { MLVIArray[21], MLVIArray[22], MLVIArray[23], MLVIArray[24] }, 0);
            mData.data.metaData.fpsNom = BitConverter.ToInt32(new byte[4] { MLVIArray[44], MLVIArray[45], MLVIArray[46], MLVIArray[47] }, 0);
            mData.data.metaData.fpsDen = BitConverter.ToInt32(new byte[4] { MLVIArray[48], MLVIArray[49], MLVIArray[50], MLVIArray[51] }, 0);
            mData.data.metaData.dropFrame = false;
            
            //small exception: if fps is lower than 1fps, set it to 1fps. for functional TC. 
            if (mData.data.metaData.fpsNom < 1000) mData.data.metaData.fpsNom = 1000;

            // handle dropped framerates
            if (mData.data.metaData.fpsNom == 23976)
            {
                mData.data.metaData.fpsNom = 24000;
                mData.data.metaData.fpsDen = 1001;
                mData.data.metaData.dropFrame = true;
            }
            if (mData.data.metaData.fpsNom == 29970)
            {
                mData.data.metaData.fpsNom = 30000;
                mData.data.metaData.fpsDen = 1001;
                mData.data.metaData.dropFrame = true;
            }
            if (mData.data.metaData.fpsNom == 59940)
            {
                mData.data.metaData.fpsNom = 60000;
                mData.data.metaData.fpsDen = 1001;
                mData.data.metaData.dropFrame = true;
            }


            Single fps_out = (Single)mData.data.metaData.fpsNom / (Single)mData.data.metaData.fpsDen;
            mData.data.metaData.fpsString = string.Format("{0:0.00}", fps_out);

            // modellname from IDNT
            if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] reading IDNT-Block");
            var IDNT = bList.FirstOrDefault(x => x.blockTag == "IDNT");
            if (IDNT == null)
            {
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] [!] NO IDNT Block");
                errorString += "[NOIDNT]";
            }

            fs.Position = IDNT.fileOffset;
            byte[] IDNTArray = new byte[IDNT.blockLength];
            fs.Read(IDNTArray, 0, IDNT.blockLength);

            byte[] modelName = new byte[32];
            Array.Copy(IDNTArray, 16, modelName, 0, 32);
            mData.data.metaData.modell = Encoding.ASCII.GetString(modelName).Replace("\0", "");

            byte[] modelid = new byte[52];
            Array.Copy(IDNTArray, 16, modelid, 0, 32);
            mData.data.metaData.modell = Encoding.ASCII.GetString(modelid).Replace("\0", "");
            
            // if modelstring unknown, set on colormatrixvalue
            // since 1.6.6
            if (null == colormatrices.FirstOrDefault(x => x.modell == mData.data.metaData.modell))
            {
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] [!] modelName unknown - estimating from colormtrix");
                setModellOnColorMatrix(mData);
            }

            mData.data.metaData.camId = "MAGIC" + Guid.NewGuid().ToString().Substring(0, 7);

            modelName = null;

            // whitebalance/colortemperature data from WBAL
            // since beta7
            if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] reading WBAL-Block");
            var WBAL = bList.FirstOrDefault(x => x.blockTag == "WBAL");
            if (WBAL == null)
            {
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] [!] NO WBAL Block");
                errorString += "[NOWBAL]";
            }

            fs.Position = WBAL.fileOffset;
            byte[] WBALArray = new byte[WBAL.blockLength];
            fs.Read(WBALArray, 0, WBAL.blockLength);

            mData.data.metaData.whiteBalanceMode = BitConverter.ToInt32(new byte[4] { WBALArray[16], WBALArray[17], WBALArray[18], WBALArray[19] }, 0);
            mData.data.metaData.whiteBalance = BitConverter.ToInt32(new byte[4] { WBALArray[20], WBALArray[21], WBALArray[22], WBALArray[23] }, 0);
            // RG and AB actually not used.
            mData.data.metaData.whiteBalanceGM = BitConverter.ToInt32(new byte[4] { WBALArray[20], WBALArray[21], WBALArray[22], WBALArray[23] }, 0);
            mData.data.metaData.whiteBalanceBA = BitConverter.ToInt32(new byte[4] { WBALArray[20], WBALArray[21], WBALArray[22], WBALArray[23] }, 0);

            mData.data.metaData.RGBfraction = new int[6];//{1024,1024,1024,1024,1024,1024};

            if (mData.data.metaData.whiteBalanceMode == 8 || mData.data.metaData.whiteBalanceMode < 6)
            {
                mData.data.metaData.whiteBalance = dng.whitebalancePresets[mData.data.metaData.whiteBalanceMode][0];
                mData.data.metaData.RGGBValues = new int[4]{
                    dng.whitebalancePresets[mData.data.metaData.whiteBalanceMode][1],
                    dng.whitebalancePresets[mData.data.metaData.whiteBalanceMode][2],
                    dng.whitebalancePresets[mData.data.metaData.whiteBalanceMode][3],
                    dng.whitebalancePresets[mData.data.metaData.whiteBalanceMode][4]
                };
                mData.data.metaData.RGBfraction = calc.convertToFraction(mData.data.metaData.RGGBValues);
            }
            else
            {
                // 9 is custom manual kelvin-value
                if (mData.data.metaData.whiteBalanceMode == 9)
                {
                    int nearestVal = 9999;
                    int accordingArray = 0;
                    for (var n = 0; n < dng.whitebalanceManual.Length; n++)
                    {
                        int wbal = dng.whitebalanceManual[n][0];
                        if (Math.Abs(wbal-mData.data.metaData.whiteBalance)<nearestVal)
                        {
                            nearestVal = Math.Abs(wbal - mData.data.metaData.whiteBalance);
                            accordingArray = n;
                        }
                    }
                    mData.data.metaData.RGGBValues = new int[4]{
                        dng.whitebalanceManual[accordingArray][1],
                        dng.whitebalanceManual[accordingArray][2],
                        dng.whitebalanceManual[accordingArray][3],
                        dng.whitebalanceManual[accordingArray][4]
                    };
                    mData.data.metaData.RGBfraction = calc.convertToFraction(mData.data.metaData.RGGBValues);
                }

                // 6 is custom whitebalance
                if (mData.data.metaData.whiteBalanceMode == 6)
                {
                    int sourceVal = BitConverter.ToInt32(new byte[4] { WBALArray[32], WBALArray[33], WBALArray[34], WBALArray[35] }, 0);
                    mData.data.metaData.RGBfraction[0] = (int)(1024/((double)sourceVal/1024));
                    int nearestVal = 9999;
                    int accordingArray = 0;
                    for (var n = 0; n < dng.whitebalanceManual.Length; n++)
                    {
                        int rFraction = dng.whitebalanceManual[n][1];
                        if (Math.Abs(rFraction - mData.data.metaData.RGBfraction[0]) < nearestVal)
                        {
                            nearestVal = Math.Abs(rFraction - mData.data.metaData.RGBfraction[0]);
                            accordingArray = n;
                        }
                    }
                    mData.data.metaData.RGGBValues = new int[4]{
                        dng.whitebalanceManual[accordingArray][1],
                        dng.whitebalanceManual[accordingArray][2],
                        dng.whitebalanceManual[accordingArray][3],
                        dng.whitebalanceManual[accordingArray][4]
                    };
                    mData.data.metaData.RGBfraction = calc.convertToFraction(mData.data.metaData.RGGBValues);
                    mData.data.metaData.whiteBalance = dng.whitebalanceManual[accordingArray][0];
                }

            }

            // short break :) override internal wb-data if CR2 is existent.
            string PhotoRAWFile = mData.data.fileData.sourcePath + Path.DirectorySeparatorChar + mData.data.fileData.fileNameOnly + ".CR2";
            string allRAWFile = mData.data.fileData.sourcePath + Path.DirectorySeparatorChar + "ALL.CR2";
            if (File.Exists(PhotoRAWFile) == true)
            {
                mData.data.metaData.photoRAW = true;
                mData.data.metaData.photoRAWe = false;
                mData.data.metaData.RGGBValues = calc.getRGGBValues(PhotoRAWFile);
                mData.data.metaData.RGBfraction = calc.convertToFraction(mData.data.metaData.RGGBValues);
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] there is a CR2");
            }
            if ((mData.data.metaData.photoRAW != true) && (File.Exists(allRAWFile) == true))
            {
                mData.data.metaData.photoRAW = true;
                mData.data.metaData.photoRAWe = true;
                mData.data.metaData.RGGBValues = calc.getRGGBValues(allRAWFile);
                mData.data.metaData.RGBfraction = calc.convertToFraction(mData.data.metaData.RGGBValues);
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] there is an ALL.CR2");
            }

            // used them only for tests - leaving for further checking..
            mData.data.metaData.wb_R = (double)mData.data.metaData.RGBfraction[0] / (double)mData.data.metaData.RGBfraction[1];
            mData.data.metaData.wb_G = (double)mData.data.metaData.RGBfraction[2] / (double)mData.data.metaData.RGBfraction[3];
            mData.data.metaData.wb_B = (double)mData.data.metaData.RGBfraction[4] / (double)mData.data.metaData.RGBfraction[5];


            // exposure data from EXPO
            if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] reading EXPO-Block");
            var EXPO = bList.FirstOrDefault(x => x.blockTag == "EXPO");
            if (EXPO == null)
            {
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] [!] NO EXPO Block");
                mData.data.metaData.errorString += "[NOEXPO]";
            }

            fs.Position = EXPO.fileOffset;
            byte[] EXPOArray = new byte[EXPO.blockLength];
            fs.Read(EXPOArray, 0, EXPO.blockLength);

            mData.data.lensData.isoValue = BitConverter.ToInt32(new byte[4] { EXPOArray[20], EXPOArray[21], EXPOArray[22], EXPOArray[23] }, 0);

            var shutterDen = BitConverter.ToInt64(new byte[8] { EXPOArray[32], EXPOArray[33], EXPOArray[34], EXPOArray[35], EXPOArray[36], EXPOArray[37], EXPOArray[38], EXPOArray[39] }, 0);
            int[] shutterD = calc.DoubleToFraction(1000000/shutterDen);
            if (shutterD[1] == 0) shutterD[1] = 1;
            mData.data.lensData.shutter = shutterD[1].ToString()+"/"+shutterD[0].ToString()+"s";


            // exposure data from LENS
            if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] reading LENS-Block");
            var LENS = bList.FirstOrDefault(x => x.blockTag == "LENS");
            if (LENS == null)
            {
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] [!] NO LENS Block");
                errorString += "[NOLENS]";
            }

            fs.Position = LENS.fileOffset;
            byte[] LENSArray = new byte[LENS.blockLength];
            fs.Read(LENSArray, 0, LENS.blockLength);

            mData.data.lensData.aperture = BitConverter.ToInt16(new byte[2] { LENSArray[20], LENSArray[21]}, 0);
            mData.data.lensData.focalLength = BitConverter.ToInt16(new byte[2] { LENSArray[16], LENSArray[17] }, 0);

            byte[] lensName = new byte[32];
            Array.Copy(LENSArray, 32, lensName, 0, 32);
            for (int i=0;i<lensName.Length;i++)
            {
                if (lensName[i] < 21 || lensName[i] > 120) lensName[i] = 0;
            }
            mData.data.lensData.lens = Encoding.ASCII.GetString(lensName).Replace("\0", "");

            // audioproperties from WAVI
            if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] reading WAVI-Block");
            var WAVI = bList.FirstOrDefault(x => x.blockTag == "WAVI");
            if (WAVI == null)
            {
                if (debugging.debugLogEnabled) debugging._saveDebug("[getMLVAttrib] [!] NO WAVI Block");
                // dont mark that as error
                //mData.data.metaData.errorString += "[NOWAVI]";
            }

            if (WAVI != null)
            {
                mData.data.audioData.hasAudio = true;
                fs.Position = WAVI.fileOffset;
                byte[] WAVIArray = new byte[WAVI.blockLength];
                fs.Read(WAVIArray, 0, WAVI.blockLength);

                mData.data.audioData.audioFormat = BitConverter.ToInt16(new byte[2] { WAVIArray[16], WAVIArray[17] }, 0);
                mData.data.audioData.audioChannels = BitConverter.ToInt16(new byte[2] { WAVIArray[18], WAVIArray[19] }, 0);
                mData.data.audioData.audioSamplingRate = BitConverter.ToInt32(new byte[4] { WAVIArray[20], WAVIArray[21], WAVIArray[22], WAVIArray[23] }, 0);
                mData.data.audioData.audioBytesPerSecond = BitConverter.ToInt32(new byte[4] { WAVIArray[24], WAVIArray[25], WAVIArray[26], WAVIArray[27] }, 0);
                mData.data.audioData.audioBlockAlign = BitConverter.ToInt16(new byte[2] { WAVIArray[28], WAVIArray[29] }, 0);
                mData.data.audioData.audioBitsPerSample = BitConverter.ToInt16(new byte[2] { WAVIArray[30], WAVIArray[31] }, 0);

                //mData.data.modell = Encoding.ASCII.GetString(modelName).Replace("\0", "");

                modelName = null;
                if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] audioData Object:", mData.data.audioData);
            }
            else
            {
                mData.data.audioData.hasAudio = false;
            }
            mData.data.metaData.duration = calc.frameToTC_s(mData.data.metaData.frames, ((double)mData.data.metaData.fpsNom / (double)mData.data.metaData.fpsDen)).Substring(3);

            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] metaData Object:", mData.data.metaData);
            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] fileData Object:", mData.data.fileData);
            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] lensData Object:", mData.data.lensData);

            return errorString;
        }

        public static void getRAWAttributes(string filename, raw rData)
        {
            if (debugging.debugLogEnabled) debugging._saveDebug("[getRAWAttrib] started");

            if (debugging.debugLogEnabled) debugging._saveDebug("[getRAWAttrib] counting Splitfiles");
            rData.data.metaData.splitCount = 0;
            int filesize = 0;
            for (var i = 0; i < RAWFileEnding.Length; i++)
            {
                string searchSplittedFile = rData.data.fileData.sourcePath + Path.DirectorySeparatorChar + rData.data.fileData.fileNameOnly + "." + RAWFileEnding[i];
                if (File.Exists(searchSplittedFile) == true)
                {
                    rData.data.metaData.splitCount++;
                    FileInfo fiTMP = new FileInfo(searchSplittedFile);
                    filesize += (int)(fiTMP.Length/1024/1024);
                }
                else
                {
                    break;
                }
            }
            rData.data.fileData.fileSize = filesize;
            if (debugging.debugLogEnabled)
            {
                debugging._saveDebug("[getRAWAttrib] found " + rData.data.metaData.splitCount + " Files");
                debugging._saveDebug("[getRAWAttrib] counted " + rData.data.fileData.fileSize + " MB");
            }
            // setting RAW WB to fix 4500°K
            rData.data.metaData.whiteBalance = dng.whitebalancePresets[0][0];
            rData.data.metaData.RGGBValues = new int[4]{
                dng.whitebalancePresets[0][1],
                dng.whitebalancePresets[0][2],
                dng.whitebalancePresets[0][3],
                dng.whitebalancePresets[0][4]
            };
            rData.data.metaData.RGBfraction = calc.convertToFraction(rData.data.metaData.RGGBValues);
            // used them only for tests - leaving for further checking..
            rData.data.metaData.wb_R = (double)rData.data.metaData.RGBfraction[0] / (double)rData.data.metaData.RGBfraction[1];
            rData.data.metaData.wb_G = (double)rData.data.metaData.RGBfraction[2] / (double)rData.data.metaData.RGBfraction[3];
            rData.data.metaData.wb_B = (double)rData.data.metaData.RGBfraction[4] / (double)rData.data.metaData.RGBfraction[5];
            
            string PhotoRAWFile = rData.data.fileData.sourcePath + Path.DirectorySeparatorChar + rData.data.fileData.fileNameOnly + ".CR2";
            string allRAWFile = rData.data.fileData.sourcePath + Path.DirectorySeparatorChar + "ALL.CR2";
            if (File.Exists(PhotoRAWFile) == true)
            {
                // 110 = Model | 829a = Exposure | 829d = Fno | 8827 = ISO | 920a = FocalLength
                // a432 = LensInfo | a433 = Lensmodel
                // Makernotes - MeasuredRGGB | WB_RGGB_Levels_Measured
                // -------------------
                //MessageBox.Show("CR2 is existent");
                rData.data.metaData.photoRAW = true;
                if (debugging.debugLogEnabled) debugging._saveDebug("[getRAWAttrib] there is a CR2. reading RGGBValues");
                // -------------------------------
                rData.data.metaData.RGGBValues = calc.getRGGBValues(PhotoRAWFile);
                rData.data.metaData.RGBfraction = calc.convertToFraction(rData.data.metaData.RGGBValues);
            }
            if ((rData.data.metaData.photoRAW != true) && (File.Exists(allRAWFile) == true))
            {
                rData.data.metaData.photoRAW = true;
                if (debugging.debugLogEnabled) debugging._saveDebug("[getRAWAttrib] there is a ALL.CR2. reading RGGBValues");
                // -------------------------------
                rData.data.metaData.RGGBValues = calc.getRGGBValues(allRAWFile);
                rData.data.metaData.RGBfraction = calc.convertToFraction(rData.data.metaData.RGGBValues);
            }
            FileInfo fi = new FileInfo(rData.data.fileData.sourcePath + Path.DirectorySeparatorChar + rData.data.fileData.fileNameOnly + "." + RAWFileEnding[rData.data.metaData.splitCount - 1]);
            string readPath = fi.DirectoryName;
            rData.data.fileData.creationTime = fi.CreationTime;
            rData.data.fileData.modificationTime = fi.LastWriteTime;
            string fn = fi.Name;
            FileStream fs = fi.OpenRead();

            if (debugging.debugLogEnabled) debugging._saveDebug("[getRAWAttrib] reading RAW Footer and Attribs");

            // -- Read 192 Bytes at EOF and values
            // description found on bitbucket -> Magic Lantern / src / raw.h and raw.c
            int nBytes = 192;
            byte[] ByteArray = new byte[nBytes];
            //long offset = fi.Length;
            fs.Seek(-192, SeekOrigin.End);
            int nBytesRead = fs.Read(ByteArray, 0, nBytes);


            rData.data.metaData.xResolution = BitConverter.ToUInt16(new byte[2] { ByteArray[4], ByteArray[5] }, 0);
            rData.data.metaData.yResolution = BitConverter.ToUInt16(new byte[2] { ByteArray[6], ByteArray[7] }, 0);
            rData.data.metaData.stripByteCount = BitConverter.ToInt32(new byte[4] { ByteArray[8], ByteArray[9], ByteArray[10], ByteArray[11] }, 0);
            rData.data.metaData.frames = BitConverter.ToInt32(new byte[4] { ByteArray[12], ByteArray[13], ByteArray[14], ByteArray[15] }, 0);
            rData.data.metaData.lostFrames = BitConverter.ToInt32(new byte[4] { ByteArray[16], ByteArray[17], ByteArray[18], ByteArray[19] }, 0);
            rData.data.metaData.fpsNom = BitConverter.ToInt32(new byte[4] { ByteArray[20], ByteArray[21], ByteArray[22], ByteArray[23] }, 0);
            if (rData.data.metaData.fpsNom == 0) rData.data.metaData.fpsNom = 24000;
            rData.data.metaData.fpsDen = 1000;

            //small exception: if fps is lower than 1fps, set it to 1fps. for functional TC. 
            if (rData.data.metaData.fpsNom < 1000) rData.data.metaData.fpsNom = 1000;

            // handle dropped framerates
            if (rData.data.metaData.fpsNom == 23976)
            {
                rData.data.metaData.fpsNom = 24000;
                rData.data.metaData.fpsDen = 1001;
                rData.data.metaData.dropFrame = true;
            }
            if (rData.data.metaData.fpsNom == 29970)
            {
                rData.data.metaData.fpsNom = 30000;
                rData.data.metaData.fpsDen = 1001;
                rData.data.metaData.dropFrame = true;
            }
            if (rData.data.metaData.fpsNom == 59940)
            {
                rData.data.metaData.fpsNom = 60000;
                rData.data.metaData.fpsDen = 1001;
                rData.data.metaData.dropFrame = true;
            }

            Single fps_out = (Single)rData.data.metaData.fpsNom / 1000;
            rData.data.metaData.fpsString = string.Format("{0:0.00}", fps_out);
            rData.data.metaData.duration = calc.frameToTC_s(rData.data.metaData.frames, (rData.data.metaData.fpsNom / rData.data.metaData.fpsDen));

            rData.data.metaData.blackLevelOld = BitConverter.ToInt32(new byte[4] { ByteArray[60], ByteArray[61], ByteArray[62], ByteArray[63] }, 0);
            if (rData.data.metaData.blackLevelOld == 0)
            {
                rData.data.metaData.blackLevelOld = 2037;
            }
            rData.data.metaData.whiteLevelOld = 15000;
            rData.data.metaData.blackLevelNew = rData.data.metaData.blackLevelOld;
            rData.data.metaData.whiteLevelNew = rData.data.metaData.whiteLevelOld;

            rData.data.metaData.bitsperSample = 14;// ByteArray[56];
            rData.data.metaData.maximize = true;
            rData.data.metaData.maximizer = (Math.Pow(2, 16)-1) / (rData.data.metaData.whiteLevelOld - rData.data.metaData.blackLevelOld);

            rData.data.metaData.stripByteCountReal = (rData.data.metaData.xResolution * rData.data.metaData.yResolution * rData.data.metaData.bitsperSample) / 8;

            byte[] colorMatrix = new byte[72];
            fs.Seek(-76, SeekOrigin.End);
            nBytesRead = fs.Read(colorMatrix, 0, 72);
            rData.data.metaData.colorMatrixA = colorMatrix;
            colorMatrix = null;
            fs.Close();

            if (debugging.debugLogEnabled) debugging._saveDebug("[getRAWAttrib,RAW] deciding model on first colorMatrix-Value");

            setModellOnColorMatrix(rData);

            rData.data.metaData.camId = "MAGIC" + Guid.NewGuid().ToString().Substring(0, 7);

            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] metaData Object:", rData.data.metaData);
            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] fileData Object:", rData.data.fileData);
            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] lensData Object:", rData.data.lensData);
        }

        // new since 1.6.9
        public static void readChunk(raw d, int startFrame, int frames)
        {
            long startOffset = 0;
            long endOffset = 0;
            long length = 1;

            if (d.data.metaData.isMLV)
            {
                // is MLV
                
                FileInfo fi = new FileInfo(d.data.fileData.sourcePath + Path.DirectorySeparatorChar + d.data.fileData.fileNameOnly + "." + MLVFileEnding[d.VIDFBlocks[startFrame].fileNo]);
                FileStream fs = fi.OpenRead();
                
                startOffset = d.VIDFBlocks[startFrame].fileOffset;
                endOffset = d.VIDFBlocks[startFrame + frames].fileOffset + 32 + d.VIDFBlocks[startFrame + frames].EDMACoffset + d.data.metaData.stripByteCountReal;
                
                // all chunks are in same file (raw.cs/createFrameChunks)
                    
                length = endOffset - startOffset;
                byte[] fileChunk = new byte[length];
            
                fs.Position = (long)(startOffset);
                fs.Read(fileChunk, 0, (int)length);

                // after that extract into frameList
                for (int i = 0; i < (1+frames); i++)
                {
                    byte[] tmpFrame = new byte[d.data.metaData.stripByteCountReal];
                    if (debugging.debugLogEnabled)
                    {
                        debugging._saveDebug("[readChunk] frame "+i+" : "+d.VIDFBlocks[i + startFrame].MLVFrameNo + " from " + d.data.fileData.fileNameOnly + "." + MLVFileEnding[d.VIDFBlocks[startFrame+i].fileNo]);
                    }
                    Array.Copy(fileChunk, d.VIDFBlocks[i + startFrame].fileOffset - d.VIDFBlocks[startFrame].fileOffset + 32 + d.VIDFBlocks[i + startFrame].EDMACoffset, tmpFrame, 0, d.data.metaData.stripByteCountReal);
                    d.frameList.Add(new frameData { frame = tmpFrame, frameNo = d.VIDFBlocks[i + startFrame].MLVFrameNo });
                }
                fileChunk = null;
                fs.Close();
                fi = null;
            }
            else
            {
                // is RAW

                FileInfo fi = new FileInfo(d.data.fileData.sourcePath + Path.DirectorySeparatorChar + d.data.fileData.fileNameOnly + "." + RAWFileEnding[d.RAWBlocks[startFrame].fileNo]);
                FileStream fs = fi.OpenRead();

                startOffset = d.RAWBlocks[startFrame].fileOffset;
                length = d.data.metaData.stripByteCount * (1+frames);
                long diff = fi.Length - startOffset;

                byte[] fileChunk = new byte[length];

                if (diff < length)
                {
                    // read two files
                
                    fs.Position = (long)(startOffset);
                    fs.Read(fileChunk, 0, (int)diff);
                    fs.Close();
                    fi = new FileInfo(d.data.fileData.sourcePath + Path.DirectorySeparatorChar + d.data.fileData.fileNameOnly + "." + RAWFileEnding[d.RAWBlocks[startFrame].fileNo + 1]);
                    fs = fi.OpenRead();
                    fs.Position = (long)(0);
                    fs.Read(fileChunk, (int)diff, (int)(length - diff));
                }
                else
                {
                    // read only one file

                    fs.Position = (long)(startOffset);
                    fs.Read(fileChunk, 0, (int)length);
                }
                fs.Close();
                fi = null;

                // after that extract into frameList
                for (int i = 0; i < (1+frames); i++)
                {
                     byte[] tmpFrame = new byte[d.data.metaData.stripByteCountReal];
                     Array.Copy(fileChunk, i * d.data.metaData.stripByteCount, tmpFrame, 0, d.data.metaData.stripByteCountReal);
                     d.frameList.Add(new frameData { frame = tmpFrame, frameNo = i+startFrame });
                }
                fileChunk = null;
            }
            if (debugging.debugLogEnabled) debugging._saveDebug("[readChunk] reading frames done - "+startFrame+"-"+(startFrame+frames));
        }

        public static byte[] readMLV(data param)
        {
            int usedFile = param.fileData.VIDFBlock.fileNo;
            param.rawData = new byte[param.metaData.stripByteCountReal];
            FileInfo fi = new FileInfo(param.fileData.sourcePath + Path.DirectorySeparatorChar + param.fileData.fileNameOnly + "." + MLVFileEnding[usedFile]);
            FileStream fs = fi.OpenRead();

            param.threadData.frame = param.fileData.VIDFBlock.MLVFrameNo;
            
            fs.Position = (long)((long)param.fileData.VIDFBlock.fileOffset + (long)32 + (long)param.fileData.VIDFBlock.EDMACoffset);
            fs.Read(param.rawData, 0, param.metaData.stripByteCountReal);

            fs.Close();
            
            return param.rawData;
        }

        public static byte[] readRAW(data param)
        {
            int usedFile = param.fileData.RAWBlock.fileNo;
            param.rawData = new byte[param.metaData.stripByteCountReal];
            
            if(!param.fileData.RAWBlock.splitted)
            {
            // if frame is not splitted
                FileInfo fi = new FileInfo(param.fileData.sourcePath + Path.DirectorySeparatorChar + param.fileData.fileNameOnly + "." + RAWFileEnding[usedFile]);
                FileStream fs = fi.OpenRead();
            
                byte[] chunkA = new byte[param.metaData.stripByteCountReal];

                fs.Position = (long)param.fileData.RAWBlock.fileOffset;
                fs.Read(param.rawData, 0, param.metaData.stripByteCountReal);
                fs.Close();

            }
            else
            {
            // if frame is splitted
                FileInfo fi = new FileInfo(param.fileData.sourcePath + Path.DirectorySeparatorChar + param.fileData.fileNameOnly + "." + RAWFileEnding[usedFile]);
                FileStream fs = fi.OpenRead();

                int chunkALength = (int)(fs.Length-param.fileData.RAWBlock.fileOffset);
            
                byte[] chunkA = new byte[chunkALength];

                fs.Position = (long)param.fileData.RAWBlock.fileOffset;
                fs.Read(param.rawData, 0, chunkALength);
                int chunkBLength = (int)(param.metaData.stripByteCountReal - (fs.Length - param.fileData.RAWBlock.fileOffset));
                fs.Close(); 
                
                byte[] chunkB = new byte[chunkBLength];
     
                FileInfo sfi = new FileInfo(param.fileData.sourcePath + Path.PathSeparator + param.fileData.fileNameOnly + "." + RAWFileEnding[usedFile+1]);
                FileStream sfs = fi.OpenRead();

                sfs.Position = 0;
                sfs.Read(param.rawData, 0, chunkBLength);
                sfs.Close();

                param.rawData = new byte[param.metaData.stripByteCountReal];
                System.Buffer.BlockCopy( chunkA, 0, param.rawData, 0, chunkALength );
                System.Buffer.BlockCopy( chunkB, 0, param.rawData, chunkALength, chunkBLength );
            }
            return param.rawData;
        }

        public static void setModellOnColorMatrix(raw rData)
        {
            int tmpMod = BitConverter.ToInt32(rData.data.metaData.colorMatrixA, 0);
            switch (tmpMod)
            {
                case 6722:
                    rData.data.metaData.modell = "Canon EOS 5D Mark III";
                    break;
                case 4716:
                    rData.data.metaData.modell = "Canon EOS 5D Mark II";
                    break;
                case 6461:
                    rData.data.metaData.modell = "Canon EOS 600D";
                    break;
                case 7034:
                    rData.data.metaData.modell = "Canon EOS 6D";
                    break;
                case 4763:
                    rData.data.metaData.modell = "Canon EOS 500D";
                    break;
                case 6719:
                    rData.data.metaData.modell = "Canon EOS 60D";
                    break;
                case 4920:
                    rData.data.metaData.modell = "Canon EOS 50D";
                    break;
                case 6602:
                    rData.data.metaData.modell = "Canon EOS 700D"; // thats for 650D, 100D, EOSM as well
                    break;
                case 6444:
                    rData.data.metaData.modell = "Canon EOS 1100D";
                    break;
                default:
                    rData.data.metaData.modell = "Canon EOS 7D";
                    break;
            }
        }
        public static WriteableBitmap showPicture(raw rawFile, quality quality)
        {
            if (debugging.debugLogEnabled) debugging._saveDebug("[showPicture] started");

            byte[] imageArray = new byte[rawFile.data.metaData.stripByteCountReal];

            if (rawFile.data.metaData.isMLV)
            {
                rawFile.data.fileData.VIDFBlock = rawFile.VIDFBlocks[rawFile.data.threadData.frame];
                imageArray = io.readMLV(rawFile.data);
            }
            else
            {
                rawFile.data.fileData.RAWBlock = rawFile.RAWBlocks[rawFile.data.threadData.frame];
                imageArray = io.readRAW(rawFile.data);
            }

            uint[] picSource = calc.maximize(calc.to16(imageArray, rawFile.data),rawFile.data);

            switch (quality)
            {
                case raw2cdng_v2.quality.low_grey:
                    return calc.doBitmapLQgrey(picSource, rawFile.data);
                case raw2cdng_v2.quality.low_mullim:
                    return calc.doBitmapLQmullim(picSource, rawFile.data);
                case raw2cdng_v2.quality.high_gamma2:
                    return calc.doBitmapHQ(picSource, rawFile.data);
                case raw2cdng_v2.quality.high_709:
                    return calc.doBitmapHQ709(picSource, rawFile.data);
                case raw2cdng_v2.quality.demosaic_VNG4:
                    return calc.demosaic_AHD(picSource, rawFile.data);
                case raw2cdng_v2.quality.debug_RGGB:
                    return calc.demosaic_debugRGGBPattern(picSource, rawFile.data);
                default:
                    return calc.doBitmapLQmullim(picSource, rawFile.data);
            }
        }

        public static void saveProxy(data r, uint[] imageArray)
        {
            // til now its all 8bit and based on jpg-pictures
            switch (r.convertData.ProxyKind)
            {
                case 0:
                    break;
                case 2:
                    // tif output
                    BitmapSource tifsource = calc.demosaic_debugRGGBPattern(imageArray,r);                 //calc.doBitmapHQ709(imageArray, r);
                    BitmapFrame tifframe = BitmapFrame.Create(tifsource);

                    String tifFileName = r.fileData._changedPath + r.fileData.outputFilename + string.Format("{0,5:D5}", r.threadData.frame) + ".tif";  //file name 
                    FileStream tifStream = new FileStream(tifFileName, FileMode.Create);
                    TiffBitmapEncoder tiffEncoder = new TiffBitmapEncoder();
                    tiffEncoder.Frames.Add(tifframe);
                    tiffEncoder.Save(tifStream);
                    tifStream.Close();

                    imageArray = null;
                    tiffEncoder = null;
                    tifStream = null;
                    tifframe = null;
                    tifsource = null;

                    break;
                case 1:
                case 3:
                case 4:
                    // jpg output
                    BitmapSource bitmapsource = calc.doBitmapHQ709(imageArray, r);
                    BitmapFrame bitmapframe = BitmapFrame.Create(bitmapsource);

                    String jpgFileName = r.fileData._changedPath + r.fileData.outputFilename + string.Format("{0,5:D5}", r.threadData.frame) + ".jpg";  //file name 
                    FileStream jpgStream = new FileStream(jpgFileName, FileMode.Create);
                    JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
                    // myEncoder = Encoder.Q; 
                    jpgEncoder.Frames.Add(bitmapframe);
                    jpgEncoder.Save(jpgStream);
                    jpgStream.Close();

                    imageArray = null;
                    jpgEncoder = null;
                    jpgStream = null;
                    bitmapframe = null;
                    bitmapsource = null;
                    break;
                default:
                    break;
            }
            // when we write debayered full resolution output, 10, 12 and 16bit will come  
        }

        public static bool saveAudio(string filename, raw mData)
        {
            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] started");
            if (debugging.debugLogEnabled) debugging._saveDebugObject("[saveAudio] audioData Object:", mData.data.audioData);

            bool success = false;
            // manage all Audiodata first as list of bytearrays
            // first entry wavFile[0] is header

            List<byte[]> wavFile = new List<byte[]>();
            wavFile.Add(riffHeader);

            // ebu bext references
            // https://tech.ebu.ch/docs/tech/tech3285.pdf
            // and https://tech.ebu.ch/docs/r/r099.pdf

            // set Name (originator)
            string bextName = mData.data.metaData.modell;
            byte[] byteBextName = Enumerable.Repeat((byte)0x00, 32).ToArray();

            // first fill with zeros
            Array.Copy(byteBextName, 0, wavFile[0], 0x138, 32);

            if (bextName.Length > 32)
            {
               bextName = bextName.Substring(0, 32);
            }
            byteBextName = System.Text.Encoding.ASCII.GetBytes(bextName);

            Array.Copy(byteBextName, 0, wavFile[0], 0x138, byteBextName.Length);

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] bextName: " + bextName);

            // set USID/UDI
            // CCOOO NNNNNNNNNNNN HHMMSS RRRRRRRRR
            Random rnd = new Random();
            int RRR= rnd.Next(100000000, 999999999);

            string UDI = "DECan" + mData.data.metaData.camId.ToUpper() + String.Format("{0:HHmmss}", mData.data.fileData.modificationTime) + RRR.ToString();
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(UDI), 0, wavFile[0], 0x158, 32);

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] UDI: " + UDI);

            // set timedate
            string dateTime = String.Format("{0:yyyy:MM:ddHH:mm:ss}", mData.data.fileData.modificationTime);
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(dateTime), 0, wavFile[0], 0x178, 18);

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] datetime: " + dateTime);
            
            //have to adjust samples if dropped frames
            double timeRefMultiplier = Math.Round((double)mData.data.metaData.fpsNom / 1000) / ((double)mData.data.metaData.fpsNom / 1000);

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] timeRefMultiplier: " + timeRefMultiplier);
   
            // set TimeRef (long)
            double time2frame = calc.dateTime2Frame(mData.data.fileData.modificationTime, timeRefMultiplier);
            long timeRef = (long)(mData.data.audioData.audioSamplingRate * time2frame);
            Array.Copy(BitConverter.GetBytes(timeRef), 0, wavFile[0], 0x18a, 8);

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] time2frame: " + time2frame);
            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] timeRef (samplingRate*time2frame): " + timeRef);

            //change xml-framerate
            string fpsString = mData.data.metaData.fpsNom.ToString() + "/" + mData.data.metaData.fpsDen.ToString();
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(fpsString), 0, wavFile[0], 0x4ce, fpsString.Length); // for example 25/1
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(fpsString), 0, wavFile[0], 0x4f7, fpsString.Length); // for example 25/1
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(fpsString), 0, wavFile[0], 0x521, fpsString.Length); // for example 25/1

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] fpsString: " + fpsString);

            // mark in xml its ndf (nondropframe) or not
            string DF = "DF ";
            if (!mData.data.metaData.dropFrame) DF = "NDF";
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(DF), 0, wavFile[0], 0x54B, 3);

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] DF: " + DF);

            // set chunksize fmt to 0x28
            Array.Copy(BitConverter.GetBytes(0x28), 0, wavFile[0], 0x29c, 4);

            //fmt area
            Array.Copy(BitConverter.GetBytes(mData.data.audioData.audioFormat), 0, wavFile[0], 0x2a0, 2); // 01 00
            Array.Copy(BitConverter.GetBytes(mData.data.audioData.audioChannels), 0, wavFile[0], 0x2a2, 2); // 02 00
            Array.Copy(BitConverter.GetBytes(mData.data.audioData.audioSamplingRate), 0, wavFile[0], 0x2a4, 4); // 80 bb 00 00
            Array.Copy(BitConverter.GetBytes(mData.data.audioData.audioBytesPerSecond), 0, wavFile[0], 0x2a8, 4); // 00 ee 02 00
            Array.Copy(BitConverter.GetBytes(mData.data.audioData.audioBlockAlign), 0, wavFile[0], 0x2ac, 2); // 00 04
            Array.Copy(BitConverter.GetBytes(mData.data.audioData.audioBitsPerSample), 0, wavFile[0], 0x2ae, 2); // 00 10

            // set XMLdata
            // read out audio chunks
            for (var i = 0; i < mData.AUDFBlocks.Count; i++)
            {
                Blocks.mlvBlock audioBlock = mData.AUDFBlocks[i];
                
                int usedFile = audioBlock.fileNo;
                byte[] audioBlockArray = new byte[audioBlock.blockLength];
                FileInfo fi = new FileInfo(mData.data.fileData.sourcePath + Path.DirectorySeparatorChar + mData.data.fileData.fileNameOnly + "." + MLVFileEnding[usedFile]);
                FileStream fs = fi.OpenRead();

                fs.Position = (long)((long)audioBlock.fileOffset);
                fs.Read(audioBlockArray, 0, audioBlock.blockLength);

                int chunkLength = audioBlock.blockLength - 24 - BitConverter.ToInt32(audioBlockArray, 20);
                byte[] audioChunk = new byte[chunkLength];

                Array.Copy(audioBlockArray, audioBlock.blockLength - chunkLength, audioChunk, 0, chunkLength);

                wavFile.Add(audioChunk);

                fs.Close();
            }

            int subChunkSize = 0;
            
            /* --- now calc SubChunkSize
            for (var i = 1; i < wavFile.Count(); i++)
            {
                subChunkSize += wavFile[i].Length;
            }
            */

            // why taking the real subchunksize?
            // readjust to the length of sequence
            // this is the solution for davinci resolve
            subChunkSize = (int)(mData.data.audioData.audioSamplingRate * mData.data.metaData.frames / (mData.data.metaData.fpsNom / mData.data.metaData.fpsDen) * mData.data.audioData.audioChannels * mData.data.audioData.audioBitsPerSample/8);

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] subChunkSize :"+subChunkSize);

            // put complete datalength into header
            Array.Copy(BitConverter.GetBytes(subChunkSize + 8192), 0, wavFile[0], 4, 4);
            
            // put subchunk-length into data-chunk-header
            Array.Copy(BitConverter.GetBytes(subChunkSize), 0, wavFile[0], 8188, 4);

            // now save all byte-arrays into file
            File.Delete(filename);

            FileStream _WAVFile = new FileStream(filename, System.IO.FileMode.Append);
            for (var i = 0; i < wavFile.Count; i++)
            {
               _WAVFile.Write(wavFile[i], 0, wavFile[i].Length);
            }
            _WAVFile.Close();
            success = true;

            if (debugging.debugLogEnabled) debugging._saveDebug("[saveAudio] success");

            return success;
        }
    }
}
