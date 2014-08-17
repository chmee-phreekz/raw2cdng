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
        public static string[] RAWFileEnding = new string[] { "RAW", "R00", "R01", "R02", "R03", "R04", "R05", "R06", "R07", "R08", "R09", "R10", "R11", "R12", "R13", "R14", "R15", "R16", "R17", "R18", "R19", "R20" };
        public static string[] MLVFileEnding = new string[] { "MLV", "M00", "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12", "M13", "M14", "M15", "M16", "M17", "M18", "M19", "M20" };
        public static string blocknames = "RAWIMLVIWBALIDNTVIDFRTCILENSEXPOWAVIAUDFINFONULLXREFMARKDISOELVLSTYLBKUP";
        // old classic short header : public static byte[] RIFFheader = Properties.Resources.RIFFtemplate;
        public static byte[] riffHeader = Properties.Resources.riffbwf_Template;

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
            raw.data.metaData.splitCount = 0;
            for (var i = 0; i < MLVFileEnding.Length; i++)
            {
                string searchSplittedFile = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename) + "." + MLVFileEnding[i];
                if (File.Exists(searchSplittedFile) == true)
                {
                    raw.data.metaData.splitCount++;
                }
                else
                {
                    break;
                }
            }
            // iterate thru all files
            // and
            // put into list
            if (debugging.debugLogEnabled) debugging._saveDebug("[createMLVBlocklist] splitFiles counted ("+raw.data.metaData.splitCount+")");

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
                while ((fl - offset) >= 0)
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
                            debugging._saveDebug("[createMLVBlocklist] blockTag|offset|fileNo : " + Encoding.ASCII.GetString(new byte[4] { chunk[0], chunk[1], chunk[2], chunk[3] })+"|"+offset.ToString()+"|"+j.ToString());
                            debugging._saveDebug(debugging.getExceptionDetails(e));
                        }
                        throw;
                    }

                }
            }
            // break/abort import if blocks werent read accordingly
            string errorString = "";
            return errorString;

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
                debugString += "[" + (framecount++) + "]";
                if (framecount % 25 == 0)
                    {
                        debugging._saveDebug("[readVIDFBlockData] done: " + debugString);
                        debugString = "";
                    }
                }
            }
            return errorString;
        }

        public static raw createRAWBlockList(string filename, raw raw)
        {
            if (debugging.debugLogEnabled) debugging._saveDebug("[createRAWBlocklist] started");

            List<Blocks.rawBlock> tmpList = new List<Blocks.rawBlock>();

            int fno = 0;
            bool isSplitted = false;

            string fn = raw.data.fileData.sourcePath + Path.DirectorySeparatorChar + raw.data.fileData.fileNameOnly + "." + RAWFileEnding[fno];
            FileInfo fi = new FileInfo(fn);
            FileStream fs = fi.OpenRead();
            long fl = fs.Length;
            long offset = 0;
            long delta = 0;

            for (int f = 0; f < raw.data.metaData.frames; f++)
            {
                if ((fl-offset) < raw.data.metaData.stripByteCount)
                {
                    isSplitted = true;
                }
                
                tmpList.Add(new Blocks.rawBlock() { 
                    fileNo = fno,
                    fileOffset = offset,
                    splitted = isSplitted
                });

                if((fl-offset) < raw.data.metaData.stripByteCount)
                {
                    isSplitted = false;
                    delta = fl - offset;
                    fs.Close();
                    fn = raw.data.fileData.sourcePath + Path.DirectorySeparatorChar + raw.data.fileData.fileNameOnly + "." + RAWFileEnding[fno];
                    fi = new FileInfo(fn);
                    fs = fi.OpenRead();
                    fl = fs.Length + delta;
                    offset = -delta;
                    fno++;
                }

                offset += raw.data.metaData.stripByteCount;
            }
            raw.RAWBlocks = tmpList;
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
                mData.data.metaData.colorMatrix = colorMatrix;
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
            mData.data.metaData.jpgConvertR = (double)mData.data.metaData.RGBfraction[0] / (double)mData.data.metaData.RGBfraction[1];
            mData.data.metaData.jpgConvertG = (double)mData.data.metaData.RGBfraction[2] / (double)mData.data.metaData.RGBfraction[3];
            mData.data.metaData.jpgConvertB = (double)mData.data.metaData.RGBfraction[4] / (double)mData.data.metaData.RGBfraction[5];


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


            // exposure data from EXPO
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
            for (var i = 0; i < RAWFileEnding.Length; i++)
            {
                string searchSplittedFile = rData.data.fileData.sourcePath + Path.DirectorySeparatorChar + rData.data.fileData.fileNameOnly + "." + RAWFileEnding[i];
                if (File.Exists(searchSplittedFile) == true)
                {
                    rData.data.metaData.splitCount++;
                }
                else
                {
                    break;
                }
            }
            if (debugging.debugLogEnabled) debugging._saveDebug("[getRAWAttrib] found "+rData.data.metaData.splitCount+" Files");

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
            Single fps_out = (Single)rData.data.metaData.fpsNom / 1000;
            rData.data.metaData.fpsString = string.Format("{0:0.00}", fps_out);
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
            rData.data.metaData.colorMatrix = colorMatrix;
            colorMatrix = null;
            fs.Close();

            if (debugging.debugLogEnabled) debugging._saveDebug("[getRAWAttrib] deciding model on first colorMatrix-Value");

            int tmpMod = BitConverter.ToInt32(rData.data.metaData.colorMatrix, 0);
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
                default:
                    rData.data.metaData.modell = "Canon EOS 7D";
                    break;
            }
            rData.data.metaData.camId = "MAGIC" + Guid.NewGuid().ToString().Substring(0, 7);

            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] metaData Object:", rData.data.metaData);
            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] fileData Object:", rData.data.fileData);
            if (debugging.debugLogEnabled) debugging._saveDebugObject("[getMLVAttributes] lensData Object:", rData.data.lensData);
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
                FileInfo fi = new FileInfo(param.fileData.sourcePath + Path.PathSeparator + param.fileData.fileNameOnly + "." + RAWFileEnding[usedFile]);
                FileStream fs = fi.OpenRead();

                int chunkALength = (int)(fs.Length-param.fileData.RAWBlock.fileOffset);
            
                byte[] chunkA = new byte[chunkALength];

                fs.Position = (long)param.fileData.RAWBlock.fileOffset;
                fs.Read(param.rawData, 0, chunkALength);
                fs.Close();

                int chunkBLength = (int)(param.metaData.stripByteCountReal - (fs.Length - param.fileData.RAWBlock.fileOffset));
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

        public static WriteableBitmap showPicture(raw rawFile)
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

            return calc.doBitmap(calc.to16(imageArray, rawFile.data), rawFile.data,false);
        }

        public static void saveProxy(data r, byte[] imageArray)
        {
            //if (debugging.debugLogEnabled) debugging._saveDebug("[saveProxy] started");

            BitmapSource bitmapsource = calc.doBitmap(imageArray, r, true);
            BitmapFrame bitmapframe = BitmapFrame.Create(bitmapsource);

            String jpgFileName = r.fileData._changedPath + r.fileData.outputFilename + string.Format("{0,5:D5}", r.threadData.frame) + ".jpg";  //file name 
            FileStream jpgStream = new FileStream(jpgFileName, FileMode.Create);
            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.Frames.Add(bitmapframe);
            jpgEncoder.Save(jpgStream);
            jpgStream.Close();

            imageArray = null;
            jpgEncoder = null;
            jpgStream = null;
            bitmapframe = null;
            bitmapsource = null;
                
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
            double time2frame = calc.creationTime2Frame(mData.data.fileData.creationTime, timeRefMultiplier);
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
            
            // now calc SubChunkSize
            for (var i = 1; i < wavFile.Count(); i++)
            {
                subChunkSize += wavFile[i].Length;
            }

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
