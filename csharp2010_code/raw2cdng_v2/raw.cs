using System;
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
        lowgrey = 0,
        lowmullim = 1,
        highgamma2 = 2,
        high709 =3
    }
    public class raw
    {
        public data data { get; set; }
        public double[] verticalStripes{get;set;}
        public bool verticalBandingNeeded { get; set; }
        public List<Blocks.rawBlock> RAWBlocks { get; set; }
        public List<Blocks.mlvBlock> VIDFBlocks { get; set; }
        public List<Blocks.mlvBlock> AUDFBlocks { get; set; }
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
        public byte[] colorMatrix { get; set; }
        public int lostFrames { get; set; }
        public int fpsNom { get; set; }
        public int fpsDen { get; set; }
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
        public double jpgConvertR { get; set; }
        public double jpgConvertG { get; set; }
        public double jpgConvertB { get; set; }
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

        public byte[] DNGHeader { get; set; }
        public string version { get; set; }
        public byte[] versionString { get; set; }
        public string propertiesString { get; set; }
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
        public bool convertIt { get; set; }
        public string fileName { get; set; }
        public string fileNameOnly { get; set; }
        public string fileNameShort { get; set; }
        public string fileNameNum { get; set; }
        public string tempPath { get; set; }
        public string sourcePath { get; set; }
        public string basePath { get; set; }
        public string destinationPath { get; set; }
        public string extraPathInfo { get; set; }
        public string _changedPath { get; set; }
        public string outputFilename { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime modificationTime { get; set; }
        
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
    }

    public class convertSettings
    {
        public int bitdepth { get; set; }
        public bool maximize { get; set; }
        public double maximizeValue { get; set; }
        public string format { get; set; }
        public bool pinkHighlight { get; set; }
        public bool chromaSmoothing { get; set; }
        public bool verticalBanding { get; set; }
        public bool proxyJpegs { get; set; }

    }

 
}
