using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Windows.Media.Imaging;
using System.Threading;

namespace raw2cdng_v2
{
    public enum quality
    {
        // used for the playback-algorithm
        low_grey = 0,
        low_mullim = 1,
        high_gamma2 = 2,
        high_709 = 3,
        demosaic_VNG4 = 4,
        debug_RGGB = 9
    }
    public enum codec
    {
        // used for the UI-Button and the convertprocess
        mpeg2 = 0,
        mpeg4 = 1,
        prores = 2,
        prores444 = 3
    }

    public class colormatrix
    {
        public string modell { get; set; }
        public int[] colormatrixA { get; set; }
        public int[] colormatrixB { get; set; }
        public int[] forwardmatrixA { get; set; }
        public int[] forwardmatrixB { get; set; }
    }

    public class point
    {
        public int x { get; set; }
        public int y { get; set; }
        public bool isHot { get; set; }
    }
    
    public class raw
    {
        public bool convert { get; set; }
        public BitmapSource thumbnail { get; set; }
        public BitmapSource histogram { get; set; }
        public string ListviewTitle { get; set; }
        public string ListviewPropA { get; set; }
        public string ListviewPropB { get; set; }
        public string ListviewPropC { get; set; }

        public data data { get; set; }
        public List<Blocks.rawBlock> RAWBlocks { get; set; }
        public List<Blocks.mlvBlock> VIDFBlocks { get; set; }
        public List<Blocks.mlvBlock> AUDFBlocks { get; set; }
        public List<frameData> frameList { get; set; }
        public List<Blocks.frameChunk> chunkList { get; set; }
    }
    
    public class frameData
    {
        public byte[] frame { get; set; }
        public long frameNo { get; set; }
    }

    public class data
    {
        public byte[] rawData { get; set; }
        public metadata metaData { get; set; }
        public audiodata audioData { get; set; }
        public filedata fileData { get; set; }
        public lensdata lensData { get; set; }
        public threaddata threadData { get; set; }
        public convertSettings convertData { get; set; }
    }

    public class metadata
    {
        public int xResolution { get; set; }
        public int yResolution { get; set; }
        public int frames { get; set; }
        public int bitsperSample { get; set; }
        public int bitsperSampleChanged { get; set; }
        public bool isLog { get; set; }
        public byte[] colorMatrixA { get; set; }
        public byte[] colorMatrixB { get; set; }
        public byte[] forwardMatrixA { get; set; }
        public byte[] forwardMatrixB { get; set; }

        public int lostFrames { get; set; }
        public int fpsNom { get; set; }
        public int fpsDen { get; set; }
        public string duration { get; set; }
        public bool dropFrame { get; set; }
        public string fpsString { get; set; }
        public int stripByteCount { get; set; }
        public int stripByteCountReal { get; set; }
        public string modell { get; set; }
        public string camId { get; set; }
        public int apiVersion { get; set; }
        public int splitCount { get; set; }
        public bool photoRAW { get; set; }
        public bool photoRAWe { get; set; }
        public int[] RGGBValues { get; set; }
        public int[] RGBfraction { get; set; }
        public double wb_R { get; set; }
        public double wb_G { get; set; }
        public double wb_B { get; set; }
        public int whiteBalance { get; set; }
        public int whiteBalanceMode { get; set; }
        public int whiteBalanceGM { get; set; }
        public int whiteBalanceBA { get; set; }

        // variables for blackpoint and maximizing
        public int blackLevelOld { get; set; }
        public int blackLevelNew { get; set; }
        public int whiteLevelOld { get; set; }
        public int whiteLevelNew { get; set; }
        public bool maximize { get; set; }
        //public double gamma { get; set; }
        public double maximizer { get; set; }

        public BitmapImage previewPic { get; set; }
        public int previewFrame { get; set; }
        public string errorString { get; set; }

        public bool isMLV { get; set; }
        public bool isFRSP { get; set; }

        public byte[] DNGHeader { get; set; }
        public string version { get; set; }
        public byte[] versionString { get; set; }
        public string propertiesString { get; set; }
        public double[] verticalBandingCoeffs { get; set; }
        public bool verticalBandingNeeded { get; set; }

        public List<point> deadSensel { get; set; }
    }

    public class audiodata 
    {
        public bool hasAudio { get; set; }
        public string audioFile { get; set; }
        public int audioSamplingRate { get; set; }
        public int audioBitsPerSample { get; set; }
        public int audioFormat { get; set; }
        public int audioChannels { get; set; }
        public int audioBytesPerSecond { get; set; }
        public int audioBlockAlign { get; set; }
    }

    public class filedata
    {
        public string fileName { get; set; }
        public string fileNameOnly { get; set; }
        public string fileNameShort { get; set; }
        public string fileNameNum { get; set; }
        public string tempPath { get; set; }
        public string sourcePath { get; set; }
        public string parentSourcePath { get; set; }
        public string basePath { get; set; }
        public string destinationPath { get; set; }
        public string extraPathInfo { get; set; }
        public string _changedPath { get; set; }
        public string outputFilename { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime modificationTime { get; set; }
        public int fileSize { get; set; }
        
        public Blocks.mlvBlock VIDFBlock {get;set;}
        public Blocks.rawBlock RAWBlock { get; set; }

    }
    public class threaddata
    {
        public int frame { get; set; }
        public CountdownEvent CDEvent { get; set; }
    }
    public class lensdata
    {
        public string lens { get; set; }
        public int focalLength { get; set; }
        public int aperture { get; set; }
        public int isoValue { get; set; }
        public string shutter { get; set; }
    }
    public class convertSettings : NotifyBase
    {
        private int bitdepth = 16;
        public int BitDepth
        {
            get
            {
                return bitdepth;
            }

            set
            {
                if (bitdepth == value)
                    return;

                RaisePropertyChanging("BitDepth");
                bitdepth = value;
                RaisePropertyChanged("BitDepth");
            }
        }

        private bool maximize = false;
        public bool Maximize
        {
            get
            {
                return maximize;
            }

            set
            {
                if (maximize == value)
                    return;

                RaisePropertyChanging("Maximize");
                maximize = value;
                RaisePropertyChanged("Maximize");
            }
        }

        private double maximizeValue = 1;
        public double MaximizeValue
        {
            get
            {
                return maximizeValue;
            }

            set
            {
                if (maximizeValue == value)
                    return;

                RaisePropertyChanging("MaximizeValue");
                maximizeValue = value;
                RaisePropertyChanged("MaximizeValue");
            }
        }

        private string format = "CDNG";
        public string Format
        {
            get
            {
                return format;
            }

            set
            {
                if (format == value)
                    return;

                RaisePropertyChanging("Format");
                format = value;
                RaisePropertyChanged("Format");
            }
        }

        private bool videoProxy = false;
        public bool VideoProxy
        {
            get
            {
                return videoProxy;
            }

            set
            {
                if (videoProxy == value)
                    return;

                RaisePropertyChanging("VideoProxy");
                videoProxy = value;
                RaisePropertyChanged("VideoProxy");
            }
        }

        private int videoCodec = 0;
        public int VideoCodec
        {
            get
            {
                return videoCodec;
            }

            set
            {
                if (videoCodec == value)
                    return;

                RaisePropertyChanging("VideoCodec");
                videoCodec = value;
                RaisePropertyChanged("VideoCodec");
            }
        }

        private bool pinkHighlight = false;
        public bool PinkHighlight
        {
            get
            {
                return pinkHighlight;
            }

            set
            {
                if (pinkHighlight == value)
                    return;

                RaisePropertyChanging("PinkHighlight");
                pinkHighlight = value;
                RaisePropertyChanged("PinkHighlight");

                //pink highlights only with maximizing
                if (value == true)
                    this.maximize = true;
            }
        }

        private bool chromaSmoothing = false;
        public bool ChromaSmoothing
        {
            get
            {
                return chromaSmoothing;
            }

            set
            {
                if (chromaSmoothing == value)
                    return;

                RaisePropertyChanging("ChromaSmoothing");
                chromaSmoothing = value;
                RaisePropertyChanged("ChromaSmoothing");

                //chroma smoothing only with maximizing
                if (value == true)
                    this.maximize = true;
            }
        }

        private bool verticalBanding = false;
        public bool VerticalBanding
        {
            get
            {
                return verticalBanding;
            }

            set
            {
                if (verticalBanding == value)
                    return;

                RaisePropertyChanging("VerticalBanding");
                verticalBanding = value;
                RaisePropertyChanged("VerticalBanding");
            }
        }

        private bool isProxy = false;
        public bool IsProxy
        {
            get
            {
                return isProxy;
            }

            set
            {
                if (isProxy == value)
                    return;

                RaisePropertyChanging("IsProxy");
                isProxy = value;
                RaisePropertyChanged("IsProxy");
            }
        }

        private bool proxyTif = false;
        public bool ProxyTif
        {
            get
            {
                return proxyTif;
            }

            set
            {
                if (proxyTif == value)
                    return;

                RaisePropertyChanging("IsProxy");
                proxyTif = value;
                RaisePropertyChanged("IsProxy");
            }
        }
        
        private int proxyKind = 0;
        public int ProxyKind
        {
            get
            {
                return proxyKind;
            }

            set
            {
                if (proxyKind == value)
                    return;

                RaisePropertyChanging("ProxyKind");
                proxyKind = value;
                RaisePropertyChanged("ProxyKind");
            }
        }

    }

    public class Blocks
    {
        public class mlvBlock
        {
            public string blockTag{get;set;}
            public long fileOffset { get; set; }
            public int fileNo { get; set; }
            public int blockLength{get;set;}
            public long timestamp{get;set;}
            public int EDMACoffset { get; set; }
            public int MLVFrameNo { get; set; }
        }
        public static List<mlvBlock> mlvBlockList = new List<mlvBlock>();

        public class rawBlock
        {
            public int fileNo { get; set; }
            public long fileOffset { get; set; }
            public bool splitted { get; set; }
        }
        public static List<rawBlock> rawBlockList = new List<rawBlock>();

        public class frameChunk
        {
            public int start { get; set; }
            public int end { get; set; }
            public int l {get;set;}
        }
        public List<frameChunk> chunkList = new List<frameChunk>();

        public static List<frameChunk> createframeChunks(int chunkLength, List<Blocks.mlvBlock> vidf)
        {
            List<frameChunk> cL = new List<frameChunk>();
            int n = 0;
            int c = n;
            do
            {
                cL.Add(new frameChunk());
                cL.Last().start = n;
                c = n;
                do
                {
                    if (n == vidf.Count()) break;
                    if (n - c == chunkLength) break;
                    if (vidf[c].fileNo != vidf[n].fileNo) break;
                    n++;
                } while (true);
                cL.Last().end = n-1;
                cL.Last().l = cL.Last().end - cL.Last().start +1;
            } while (n != vidf.Count());

            if (debugging.debugLogEnabled)
            {
                for (int z = 0; z < cL.Count();z++ )
                { 
                    debugging._saveDebug("[createFrameChunk] list " + z + " : " + cL[z].start + " to " +cL[z].end+" in file "+vidf[cL[z].end].fileNo);
                }
            }
            return cL;
        }
        public static List<frameChunk> createframeChunksRaw(int chunkLength, List<Blocks.rawBlock> rawblocks)
        {
            List<frameChunk> cL = new List<frameChunk>();
            int n = 0;
            int c = n;
            do
            {
                cL.Add(new frameChunk());
                cL.Last().start = n;
                c = n;
                do
                {

                    if (n == rawblocks.Count()) break;
                    if (n - c == chunkLength) break;
                    n++;
                } while (true);
                cL.Last().end = n - 1;
                cL.Last().l = cL.Last().end - cL.Last().start +1;
            } while (n != rawblocks.Count());

            if (debugging.debugLogEnabled)
            {
                for (int z = 0; z < cL.Count(); z++)
                {
                    debugging._saveDebug("[createFrameChunkRaw] list " + z + " : " + cL[z].start + " to " + cL[z].end + " in file " + rawblocks[cL[z].end].fileNo);
                }
            }
            return cL;
        }
    }

}
