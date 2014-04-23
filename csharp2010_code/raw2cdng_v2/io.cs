using System;
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

        public static bool setFileinfoData(string fn, filedata fd)
        {
            fd.fileName = fn;
            fd.fileNameOnly = Path.GetFileNameWithoutExtension(fn);
            fd.sourcePath = Path.GetDirectoryName(fn);
            fd.fileNameShort = calc.setFilenameShort(fd.fileNameOnly);
            fd.fileNameNum = calc.setFilenameNum(fd.fileNameOnly);
            fd.outputFilename = ""; 
            return true;
        }

        public static raw createMLVBlockList(string filename, raw raw)
        {
            Blocks.mlvBlockList.Clear();
            // -- count stripFiles
            raw.metaData.splitCount = 0;
            for (var i = 0; i < MLVFileEnding.Length; i++)
            {
                string searchSplittedFile = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename) + "." + MLVFileEnding[i];
                if (File.Exists(searchSplittedFile) == true)
                {
                    raw.metaData.splitCount++;
                }
                else
                {
                    break;
                }
            }
            // iterate thru all files
            // and
            // put into list

            long offset;
            int bl;
            byte[] chunk = new byte[16];
            for (var j = 0; j < raw.metaData.splitCount; j++)
            {
                offset = 0;
                bl = 0;
                string fn = raw.fileData.sourcePath + Path.DirectorySeparatorChar + raw.fileData.fileNameOnly + "." + MLVFileEnding[j];
                FileInfo fi = new FileInfo(fn);
                FileStream fs = fi.OpenRead();
                long fl = fs.Length;
                while ((fl - offset) > 0)
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
            }
            return raw;

        }

        public static void readVIDFBlockData(raw raw)
        {
            foreach (Blocks.mlvBlock VIDFBlock in raw.metaData.VIDFBlocks)
            {
                FileInfo fi = new FileInfo(raw.fileData.sourcePath + Path.DirectorySeparatorChar + raw.fileData.fileNameOnly + "." + MLVFileEnding[VIDFBlock.fileNo]);
                FileStream fs = fi.OpenRead();
                byte[] vidfProp = new byte[32];
                fs.Position = VIDFBlock.fileOffset;
                fs.Read(vidfProp, 0, 32);
                VIDFBlock.MLVFrameNo = BitConverter.ToInt32(new byte[4] { vidfProp[16], vidfProp[17], vidfProp[18], vidfProp[19] }, 0);
                VIDFBlock.EDMACoffset = BitConverter.ToInt32(new byte[4] { vidfProp[28], vidfProp[29], vidfProp[30], vidfProp[31] }, 0); 
                            
                fs.Close();
            }
        }

        public static raw createRAWBlockList(string filename, raw raw)
        {
            Blocks.rawBlockList.Clear();

            int fno = 0;
            bool isSplitted = false;

            string fn = raw.fileData.sourcePath + Path.DirectorySeparatorChar + raw.fileData.fileNameOnly + "." + RAWFileEnding[fno];
            FileInfo fi = new FileInfo(fn);
            FileStream fs = fi.OpenRead();
            long fl = fs.Length;
            long offset = 0;
            long delta = 0;

            for (int f = 0; f < raw.metaData.frames; f++)
            {
                if ((fl-offset) < raw.metaData.stripByteCount)
                {
                    isSplitted = true;
                }
                
                Blocks.rawBlockList.Add(new Blocks.rawBlock() { 
                    fileNo = fno,
                    fileOffset = offset,
                    splitted = isSplitted
                });

                if((fl-offset) < raw.metaData.stripByteCount)
                {
                    isSplitted = false;
                    delta = fl - offset;
                    fs.Close();
                    fn = raw.fileData.sourcePath + Path.DirectorySeparatorChar + raw.fileData.fileNameOnly + "." + RAWFileEnding[fno];
                    fi = new FileInfo(fn);
                    fs = fi.OpenRead();
                    fl = fs.Length + delta;
                    offset = -delta;
                    fno++;
                }
                
                offset += raw.metaData.stripByteCount;
            }
            return raw;
        }

        public static void getMLVAttributes(string filename, List<Blocks.mlvBlock> bList, raw mData)
        {
            string PhotoRAWFile = mData.fileData.sourcePath + Path.DirectorySeparatorChar + mData.fileData.fileNameOnly + ".CR2";
            string allRAWFile = mData.fileData.sourcePath + Path.DirectorySeparatorChar + "ALL.CR2";
            if (File.Exists(PhotoRAWFile) == true)
            {
                mData.metaData.photoRAW = true;
                mData.metaData.RGGBValues = calc.getRGGBValues(PhotoRAWFile);
                mData.metaData.RGBfraction = calc.convertToFraction(mData.metaData.RGGBValues);
            }
            if ((mData.metaData.photoRAW != true) && (File.Exists(allRAWFile) == true))
            {
                mData.metaData.photoRAW = true;
                mData.metaData.RGGBValues = calc.getRGGBValues(allRAWFile);
                mData.metaData.RGBfraction = calc.convertToFraction(mData.metaData.RGGBValues);
            }

            // get Data from RAWI Block
            var RAWI = bList.FirstOrDefault(x => x.blockTag == "RAWI");

            FileInfo fi = new FileInfo(mData.fileData.sourcePath + Path.DirectorySeparatorChar + mData.fileData.fileNameOnly + "." + MLVFileEnding[RAWI.fileNo]);
            string readPath = fi.DirectoryName;
            mData.fileData.creationTime = fi.CreationTime;
            string fn = fi.Name;
            FileStream fs = fi.OpenRead();

            fs.Position = RAWI.fileOffset;
            byte[] RAWIArray = new byte[RAWI.blockLength];
            fs.Read(RAWIArray, 0, RAWI.blockLength);

            mData.metaData.xResolution = BitConverter.ToUInt16(new byte[2] { RAWIArray[16], RAWIArray[17] }, 0);
            mData.metaData.yResolution = BitConverter.ToUInt16(new byte[2] { RAWIArray[18], RAWIArray[19] }, 0);
            mData.metaData.blackLevelOld = BitConverter.ToInt32(new byte[4] { RAWIArray[48], RAWIArray[49], RAWIArray[50], RAWIArray[51] }, 0);
            if (mData.metaData.blackLevelOld == 0) mData.metaData.blackLevelOld = 2037;
            mData.metaData.whiteLevelOld = BitConverter.ToInt32(new byte[4] { RAWIArray[52], RAWIArray[53], RAWIArray[54], RAWIArray[55] }, 0);
            if (mData.metaData.whiteLevelOld == 0) mData.metaData.whiteLevelOld = 15000;
            mData.metaData.blackLevelNew = mData.metaData.blackLevelOld;
            mData.metaData.whiteLevelNew = mData.metaData.whiteLevelOld;

            mData.metaData.bitsperSample = 14;// RAWIArray[56];
            mData.metaData.maximizer = Math.Pow(2, 16) / (mData.metaData.whiteLevelOld - mData.metaData.blackLevelOld);
            mData.metaData.maximize = true;

            byte[] colorMatrix = new byte[72];
            Array.Copy(RAWIArray, 104, colorMatrix, 0, 72);
            mData.metaData.colorMatrix = colorMatrix;
            colorMatrix = null;

            mData.metaData.stripByteCount = mData.metaData.xResolution * mData.metaData.yResolution * mData.metaData.bitsperSample / 8;
            mData.metaData.stripByteCountReal = mData.metaData.stripByteCount;

            // from fileheader MLVI
            var MLVI = bList.FirstOrDefault(x => x.blockTag == "MLVI");

            fs.Position = MLVI.fileOffset;
            byte[] MLVIArray = new byte[MLVI.blockLength];
            fs.Read(MLVIArray, 0, MLVI.blockLength);

            mData.metaData.frames = BitConverter.ToInt32(new byte[4] { MLVIArray[36], MLVIArray[37], MLVIArray[38], MLVIArray[39] }, 0);
            mData.metaData.lostFrames = 0;//BitConverter.ToInt32(new byte[4] { MLVIArray[21], MLVIArray[22], MLVIArray[23], MLVIArray[24] }, 0);
            mData.metaData.fpsNom = BitConverter.ToInt32(new byte[4] { MLVIArray[44], MLVIArray[45], MLVIArray[46], MLVIArray[47] }, 0);
            mData.metaData.fpsDen = BitConverter.ToInt32(new byte[4] { MLVIArray[48], MLVIArray[49], MLVIArray[50], MLVIArray[51] }, 0);
            mData.metaData.dropFrame = false;

            Single fps_out = (Single)mData.metaData.fpsNom / (Single)mData.metaData.fpsDen;
            mData.metaData.fpsString = string.Format("{0:0.00}", fps_out);

            // modellname from IDNT
            var IDNT = bList.FirstOrDefault(x => x.blockTag == "IDNT");

            fs.Position = IDNT.fileOffset;
            byte[] IDNTArray = new byte[IDNT.blockLength];
            fs.Read(IDNTArray, 0, IDNT.blockLength);

            byte[] modelName = new byte[32];
            Array.Copy(IDNTArray, 16, modelName, 0, 32);
            mData.metaData.modell = Encoding.ASCII.GetString(modelName).Replace("\0", "");

            byte[] modelid = new byte[52];
            Array.Copy(IDNTArray, 16, modelid, 0, 32);
            mData.metaData.modell = Encoding.ASCII.GetString(modelid).Replace("\0", "");

            modelName = null;

            // exposure data from EXPO
            var EXPO = bList.FirstOrDefault(x => x.blockTag == "EXPO");

            fs.Position = EXPO.fileOffset;
            byte[] EXPOArray = new byte[EXPO.blockLength];
            fs.Read(EXPOArray, 0, EXPO.blockLength);

            mData.lensData.isoValue = BitConverter.ToInt32(new byte[4] { EXPOArray[20], EXPOArray[21], EXPOArray[22], EXPOArray[23] }, 0);

            var shutterDen = BitConverter.ToInt64(new byte[8] { EXPOArray[32], EXPOArray[33], EXPOArray[34], EXPOArray[35], EXPOArray[36], EXPOArray[37], EXPOArray[38], EXPOArray[39] }, 0);
            int[] shutterD = calc.DoubleToFraction(1000000/shutterDen);
            if (shutterD[1] == 0) shutterD[1] = 1;
            mData.lensData.shutter = shutterD[1].ToString()+"/"+shutterD[0].ToString()+"s";


            // exposure data from EXPO
            var LENS = bList.FirstOrDefault(x => x.blockTag == "LENS");

            fs.Position = LENS.fileOffset;
            byte[] LENSArray = new byte[LENS.blockLength];
            fs.Read(LENSArray, 0, LENS.blockLength);

            mData.lensData.aperture = BitConverter.ToInt16(new byte[2] { LENSArray[20], LENSArray[21]}, 0);
            mData.lensData.focalLength = BitConverter.ToInt16(new byte[2] { LENSArray[16], LENSArray[17] }, 0);

            byte[] lensName = new byte[32];
            Array.Copy(LENSArray, 32, lensName, 0, 32);
            for (int i=0;i<lensName.Length;i++)
            {
                if (lensName[i] < 21 || lensName[i] > 120) lensName[i] = 0;
            }
            mData.lensData.lens = Encoding.ASCII.GetString(lensName).Replace("\0", "");

            // audioproperties from WAVI
            var WAVI = bList.FirstOrDefault(x => x.blockTag == "WAVI");

            if (WAVI != null)
            {
                mData.metaData.hasAudio = true;
                fs.Position = WAVI.fileOffset;
                byte[] WAVIArray = new byte[WAVI.blockLength];
                fs.Read(WAVIArray, 0, WAVI.blockLength);

                //byte[] modelName = new byte[32];
                //Array.Copy(IDNTArray, 16, modelName, 0, 32);
                mData.metaData.audioFormat = BitConverter.ToInt16(new byte[2] { WAVIArray[16], WAVIArray[17] }, 0);
                mData.metaData.audioChannels = BitConverter.ToInt16(new byte[2] { WAVIArray[18], WAVIArray[19] }, 0);
                mData.metaData.audioSamplingRate = BitConverter.ToInt32(new byte[4] { WAVIArray[20], WAVIArray[21], WAVIArray[22], WAVIArray[23] }, 0);
                mData.metaData.audioBytesPerSecond = BitConverter.ToInt32(new byte[4] { WAVIArray[24], WAVIArray[25], WAVIArray[26], WAVIArray[27] }, 0);
                mData.metaData.audioBlockAlign = BitConverter.ToInt16(new byte[2] { WAVIArray[28], WAVIArray[29] }, 0);
                mData.metaData.audioBitsPerSample = BitConverter.ToInt16(new byte[2] { WAVIArray[30], WAVIArray[31] }, 0);

                //mData.modell = Encoding.ASCII.GetString(modelName).Replace("\0", "");

                modelName = null;
            }
            else
            {
                mData.metaData.hasAudio = false;
            }

            //return mData;
        }

        public static void getRAWAttributes(string filename, raw rData)
        {
            //if (dLog) { sd._saveDebug("* started getRAWAttributes"); }
            // fegatex, roundup, brommethane, bayer, basf, 

            rData.metaData.splitCount = 0;
            for (var i = 0; i < RAWFileEnding.Length; i++)
            {
                string searchSplittedFile = rData.fileData.sourcePath + Path.DirectorySeparatorChar + rData.fileData.fileNameOnly + "." + RAWFileEnding[i];
                if (File.Exists(searchSplittedFile) == true)
                {
                    rData.metaData.splitCount++;
                }
                else
                {
                    break;
                }
            }
            string PhotoRAWFile = rData.fileData.sourcePath + Path.DirectorySeparatorChar + rData.fileData.fileNameOnly + ".CR2";
            string allRAWFile = rData.fileData.sourcePath + Path.DirectorySeparatorChar + "ALL.CR2";
            if (File.Exists(PhotoRAWFile) == true)
            {
                // 110 = Model | 829a = Exposure | 829d = Fno | 8827 = ISO | 920a = FocalLength
                // a432 = LensInfo | a433 = Lensmodel
                // Makernotes - MeasuredRGGB | WB_RGGB_Levels_Measured
                // -------------------
                //MessageBox.Show("CR2 is existent");
                rData.metaData.photoRAW = true;
                // -------------------------------
                rData.metaData.RGGBValues = calc.getRGGBValues(PhotoRAWFile);
                rData.metaData.RGBfraction = calc.convertToFraction(rData.metaData.RGGBValues);
                //if (dLog) { sd._saveDebug("- found according CR2 and using RGGB Whitebalance-Values."); }
            }
            if ((rData.metaData.photoRAW != true) && (File.Exists(allRAWFile) == true))
            {
                rData.metaData.photoRAW = true;
                // -------------------------------
                rData.metaData.RGGBValues = calc.getRGGBValues(allRAWFile);
                rData.metaData.RGBfraction = calc.convertToFraction(rData.metaData.RGGBValues);
                //if (dLog) { sd._saveDebug("- found ALL.CR2 and using Whitebalance values RGGB"); }
            }
            FileInfo fi = new FileInfo(rData.fileData.sourcePath + Path.DirectorySeparatorChar + rData.fileData.fileNameOnly + "." + RAWFileEnding[rData.metaData.splitCount - 1]);
            string readPath = fi.DirectoryName;
            rData.fileData.creationTime = fi.CreationTime;
            string fn = fi.Name;
            FileStream fs = fi.OpenRead();

            //if (dLog) { sd._saveDebug("- reading 192 Bytes Footer of RAW-File "); }

            // -- Read 192 Bytes at EOF and values
            int nBytes = 192;
            byte[] ByteArray = new byte[nBytes];
            //long offset = fi.Length;
            fs.Seek(-192, SeekOrigin.End);
            int nBytesRead = fs.Read(ByteArray, 0, nBytes);


            rData.metaData.xResolution = BitConverter.ToUInt16(new byte[2] { ByteArray[4], ByteArray[5] }, 0);
            rData.metaData.yResolution = BitConverter.ToUInt16(new byte[2] { ByteArray[6], ByteArray[7] }, 0);
            rData.metaData.stripByteCount = BitConverter.ToInt32(new byte[4] { ByteArray[8], ByteArray[9], ByteArray[10], ByteArray[11] }, 0);
            rData.metaData.frames = BitConverter.ToInt32(new byte[4] { ByteArray[12], ByteArray[13], ByteArray[14], ByteArray[15] }, 0);
            rData.metaData.lostFrames = BitConverter.ToInt32(new byte[4] { ByteArray[16], ByteArray[17], ByteArray[18], ByteArray[19] }, 0);
            rData.metaData.fpsNom = BitConverter.ToInt32(new byte[4] { ByteArray[20], ByteArray[21], ByteArray[22], ByteArray[23] }, 0);
            rData.metaData.fpsDen = 1000;
            Single fps_out = (Single)rData.metaData.fpsNom / 1000;
            rData.metaData.fpsString = string.Format("{0:0.00}", fps_out);
            rData.metaData.blackLevelOld = BitConverter.ToInt32(new byte[4] { ByteArray[60], ByteArray[61], ByteArray[62], ByteArray[63] }, 0);
            if (rData.metaData.blackLevelOld == 0)
            {
                rData.metaData.blackLevelOld = 2037;
            }
            rData.metaData.whiteLevelOld = 15000;
            rData.metaData.blackLevelNew = rData.metaData.blackLevelOld;
            rData.metaData.whiteLevelNew = rData.metaData.whiteLevelOld;

            rData.metaData.bitsperSample = 14;// ByteArray[56];
            rData.metaData.maximize = true;
            rData.metaData.maximizer = Math.Pow(2, 16) / (rData.metaData.whiteLevelOld - rData.metaData.blackLevelOld);
            

            rData.metaData.stripByteCountReal = (rData.metaData.xResolution * rData.metaData.yResolution * rData.metaData.bitsperSample) / 8;

            //int real = (rData.metaData.xResolution*rData.metaData.yResolution)*14/8;
            //long diff = real - rData.metaData.stripByteCount;
            byte[] colorMatrix = new byte[72];
            fs.Seek(-76, SeekOrigin.End);
            nBytesRead = fs.Read(colorMatrix, 0, 72);
            rData.metaData.colorMatrix = colorMatrix;
            colorMatrix = null;
            fs.Close();

            int tmpMod = BitConverter.ToInt32(rData.metaData.colorMatrix, 0);
            switch (tmpMod)
            {
                case 6722:
                    rData.metaData.modell = "Canon EOS 5D Mark III";
                    break;
                case 4716:
                    rData.metaData.modell = "Canon EOS 5D Mark II";
                    break;
                case 6461:
                    rData.metaData.modell = "Canon EOS 600D";
                    break;
                case 7034:
                    rData.metaData.modell = "Canon EOS 6D";
                    break;
                case 4763:
                    rData.metaData.modell = "Canon EOS 500D";
                    break;
                case 6719:
                    rData.metaData.modell = "Canon EOS 60D";
                    break;
                case 4920:
                    rData.metaData.modell = "Canon EOS 50D";
                    break;
                default:
                    rData.metaData.modell = "Canon EOS 7D";
                    break;
            }
        }

        public static byte[] readMLV(raw param)
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

        public static byte[] readRAW(raw param)
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
            //debugging._saveDebug("show Frame "+rawFile.threadData.frame+" from "+rawFile.fileData.fileNameOnly);

            byte[] imageArray = new byte[rawFile.metaData.stripByteCountReal];

            if (rawFile.metaData.isMLV)
            {
                rawFile.fileData.VIDFBlock = rawFile.metaData.VIDFBlocks[rawFile.threadData.frame];
                imageArray = io.readMLV(rawFile);
            }
            else
            {
                rawFile.fileData.RAWBlock = rawFile.metaData.RAWBlocks[rawFile.threadData.frame];
                imageArray = io.readRAW(rawFile);
            }

            return calc.doBitmap(calc.to16(imageArray, rawFile), rawFile);
        }

        /* old. new one with bext references
        public static bool saveAudio(string filename, raw mData)
        {
            bool success = false;
            if (mData.metaData.hasAudio)
            {
                List<byte[]> wavFile = new List<byte[]>();
                wavFile.Add(RIFFheader);

                // copy Audiovalues to wavHeader (wavFile[0])
                Array.Copy(BitConverter.GetBytes(mData.metaData.audioFormat), 0, wavFile[0], 20, 2);
                Array.Copy(BitConverter.GetBytes(mData.metaData.audioChannels), 0, wavFile[0], 22, 2);
                Array.Copy(BitConverter.GetBytes(mData.metaData.audioSamplingRate), 0, wavFile[0], 24, 4);
                Array.Copy(BitConverter.GetBytes(mData.metaData.audioBytesPerSecond), 0, wavFile[0], 28, 4);
                Array.Copy(BitConverter.GetBytes(mData.metaData.audioBlockAlign), 0, wavFile[0], 32, 2);
                Array.Copy(BitConverter.GetBytes(mData.metaData.audioBitsPerSample), 0, wavFile[0], 34, 2);

                // forgot to set subchunk1size to 16
                Array.Copy(BitConverter.GetBytes(16), 0, wavFile[0], 16, 4);

                // read out audio chunks
                for (var i = 0; i < mData.metaData.AUDFBlocks.Count; i++)
                {
                    Blocks.mlvBlock audioBlock = mData.metaData.AUDFBlocks[i];
                    int usedFile = audioBlock.fileNo;
                    byte[] audioBlockArray = new byte[audioBlock.blockLength];
                    FileInfo fi = new FileInfo(mData.fileData.sourcePath + Path.DirectorySeparatorChar + mData.fileData.fileNameOnly + "." + MLVFileEnding[usedFile]);
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

                // put subchunk-data into header
                Array.Copy(BitConverter.GetBytes(subChunkSize + 44), 0, wavFile[0], 4, 4);
                Array.Copy(BitConverter.GetBytes(subChunkSize), 0, wavFile[0], 40, 4);

                // now save all byte-arrays into file
                File.Delete(filename);

                FileStream _WAVFile = new FileStream(filename, System.IO.FileMode.Append);
                for (var i = 0; i < wavFile.Count; i++)
                {
                    _WAVFile.Write(wavFile[i], 0, wavFile[i].Length);
                }
                _WAVFile.Close();
                success = true;
            }
            else
            {
            }
            return success;
        }
        */

        public static bool saveAudio(string filename, raw mData)
        {
            bool success = false;
            // manage all Audiodata first as list of bytearrays
            // first entry wavFile[0] is header

            List<byte[]> wavFile = new List<byte[]>();
            wavFile.Add(riffHeader);

            // ebu bext references
            // https://tech.ebu.ch/docs/tech/tech3285.pdf
            // and https://tech.ebu.ch/docs/r/r099.pdf

            // set Name 
            string bextName = mData.fileData.outputFilename;
            byte[] byteBextName = Enumerable.Repeat((byte)0x00, 32).ToArray();

            Array.Copy(byteBextName, 0, wavFile[0], 0x138, 32);

            if (bextName.Length > 32)
            {
               bextName = bextName.Substring(0, 32);
            }
            byteBextName = System.Text.Encoding.ASCII.GetBytes(bextName);

            Array.Copy(byteBextName, 0, wavFile[0], 0x138, byteBextName.Length);

            // set UDI
            // CCOOO NNNNNNNNNNNN HHMMSS RRRRRRRRR
            Random rnd = new Random();
            int RRR= rnd.Next(100000000, 999999999);
            string UDI = "DEMLA" + "MAGICLANTERN" + String.Format("{0:HHmmss}", mData.fileData.creationTime) + RRR.ToString();
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(UDI), 0, wavFile[0], 0x158, 32);
            
            // set timedate
            string dateTime = String.Format("{0:yyyy-MM-ddHH-mm-ss}", mData.fileData.creationTime);
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(dateTime), 0, wavFile[0], 0x178, 18);
            
            //have to adjust samples if dropped frames
            double timeRefMultiplier = Math.Round((double)mData.metaData.fpsNom / 1000) / ((double)mData.metaData.fpsNom / 1000);
               
            // set TimeRef (long)
            long timeRef = (long)(mData.metaData.audioSamplingRate * calc.creationTime2Frame(mData.fileData.creationTime,timeRefMultiplier));
            Array.Copy(BitConverter.GetBytes(timeRef), 0, wavFile[0], 0x18a, 8);

            //change xml-framerate
            string fpsString = mData.metaData.fpsNom.ToString() + "/" + mData.metaData.fpsDen.ToString();
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(fpsString), 0, wavFile[0], 0x4ce, fpsString.Length); // for example 25/1
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(fpsString), 0, wavFile[0], 0x4f7, fpsString.Length); // for example 25/1
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(fpsString), 0, wavFile[0], 0x521, fpsString.Length); // for example 25/1

            // mark in xml its ndf (nondropframe) or not
            string DF = "DF ";
            if(!mData.metaData.dropFrame) DF ="NDF";
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(DF), 0, wavFile[0], 0x54B, 3);

            // set chunksize fmt to 0x28
            Array.Copy(BitConverter.GetBytes(0x28), 0, wavFile[0], 0x29c, 4);

            //fmt area
            Array.Copy(BitConverter.GetBytes(mData.metaData.audioFormat), 0, wavFile[0], 0x2a0, 2); // 01 00
            Array.Copy(BitConverter.GetBytes(mData.metaData.audioChannels), 0, wavFile[0], 0x2a2, 2); // 02 00
            Array.Copy(BitConverter.GetBytes(mData.metaData.audioSamplingRate), 0, wavFile[0], 0x2a4, 4); // 80 bb 00 00
            Array.Copy(BitConverter.GetBytes(mData.metaData.audioBytesPerSecond), 0, wavFile[0], 0x2a8, 4); // 00 ee 02 00
            Array.Copy(BitConverter.GetBytes(mData.metaData.audioBlockAlign), 0, wavFile[0], 0x2ac, 2); // 00 04
            Array.Copy(BitConverter.GetBytes(mData.metaData.audioBitsPerSample), 0, wavFile[0], 0x2ae, 2); // 00 10

            // set XMLdata
            // read out audio chunks
            for (var i = 0; i < mData.metaData.AUDFBlocks.Count; i++)
            {
                Blocks.mlvBlock audioBlock = mData.metaData.AUDFBlocks[i];
                int usedFile = audioBlock.fileNo;
                byte[] audioBlockArray = new byte[audioBlock.blockLength];
                FileInfo fi = new FileInfo(mData.fileData.sourcePath + Path.DirectorySeparatorChar + mData.fileData.fileNameOnly + "." + MLVFileEnding[usedFile]);
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

            return success;
        }


    }
}
