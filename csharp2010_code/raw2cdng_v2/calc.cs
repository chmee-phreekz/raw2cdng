using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace raw2cdng_v2
{
    class calc
    {
        // helper arrays
        private const int EV_RESOLUTION = 32768;

        public static int[] ev2raw = new int[24 * EV_RESOLUTION];
        public static int[] raw2ev = new int[EV_RESOLUTION];
        public static int zero = 0;
        public static int full14 = 16383;
        public static int full16 = 65535;

        public static void calcRAWEV_Arrays(int black, int white)
        {
            /*
            for (var i = 0; i < 65536; i++) calc.raw2ev[i] = (int)(Math.Log(Math.Max(1, i - black))/Math.Log(2) * EV_RESOLUTION) ;
            for (var i = -10*EV_RESOLUTION; i < 0; i++)
            {
                int val = (int)(black + 4 - Math.Round(4 * Math.Pow(2, ((double)(-i / EV_RESOLUTION)))));
                ev2raw[i+(10*EV_RESOLUTION)] = COERCE(ref val, ref zero,ref black);
            }

            for (var i = 0; i < 14*EV_RESOLUTION; i++)
            {
                int val = (int)(black - 4 + Math.Round(4 * Math.Pow(2, ((double)i / EV_RESOLUTION))));
                ev2raw[i+(10*EV_RESOLUTION)] = COERCE(ref val, ref black,ref full);
                if (i >= raw2ev[white])
                {
                    ev2raw[i] = Math.Max(ev2raw[i], white);
                }
            }
            */
        }

        // --- bitdepth conversion ---
        // 5DIII valuerange
        // 14bit original - 2048-15.000 = ~12.900
        // 16bit - 8192-60.000 - maximized 0-65535
        // 12bit - 512-3750 = ~3.200 - maximized 0-4095 (

        public static uint[] to16(byte[] source, data rData)
        {
            // preparing variables
            int resx = rData.metaData.xResolution;
            int resy = rData.metaData.yResolution;
            uint bl = (uint)rData.metaData.blackLevelOld;
            bool maximize = rData.metaData.maximize;
            double maximizer = rData.metaData.maximizer;

            // ------------- and go ----
            int chunks = resx * resy * 14 / 8;
            uint[] Dest = new uint[resx*resy];
            UInt32 tt = 0;
            uint senselA, senselB, senselC, senselD, senselE, senselF, senselG, senselH;
            for (var t = 0; t < chunks; t += 14)
            {
                // no maximizing
                senselA = (uint)((source[t] >> 2) | (source[t + 1] << 6));
                senselB = (uint)(((source[t] & 0x3) << 12) | (source[t + 3] << 4) | (source[t + 2] >> 4));
                senselC = (uint)(((source[t + 2] & 0x0f) << 10) | (source[t + 5] << 2) | (source[t + 4] >> 6));
                senselD = (uint)(((source[t + 4] & 0x3f) << 8) | (source[t + 7]));
                senselE = (uint)((source[t + 9] >> 2) | (source[t + 6] << 6));
                senselF = (uint)(((source[t + 9] & 0x3) << 12) | (source[t + 8] << 4) | (source[t + 11] >> 4));
                senselG = (uint)(((source[t + 11] & 0x0f) << 10) | (source[t + 10] << 2) | (source[t + 13] >> 6));
                senselH = (uint)(((source[t + 13] & 0x3f) << 8) | (source[t + 12]));

                Dest[tt++] = senselA;
                Dest[tt++] = senselB;
                Dest[tt++] = senselC;
                Dest[tt++] = senselD;
                Dest[tt++] = senselE;
                Dest[tt++] = senselF;
                Dest[tt++] = senselG;
                Dest[tt++] = senselH;

            }
            return Dest;
        }

        public static byte[] toBytes(uint[] pic, data rData)
        {
            byte[] byteOutput = new byte[rData.metaData.xResolution * rData.metaData.yResolution * 2];
            int t = 0;
            foreach (UInt16 sensel in pic)
            {
                byteOutput[t++] = (byte)(sensel & 0xFF);
                byteOutput[t++] = (byte)(sensel >> 8);
            }
            return byteOutput;
        }

        public static uint[] maximize(uint[] source, data rData)
        {
            uint[] dest = new uint[source.Length];
            uint bl = (uint)rData.metaData.blackLevelOld;
            double maximizer = rData.metaData.maximizer;
            for (int pos = 0; pos < (rData.metaData.xResolution * rData.metaData.yResolution); pos++)
            {
                int sensel = (int)source[pos];
                
                // debias sensel
                sensel = sensel - (int)bl;
                // maximize to 16bit
                sensel = (int)(sensel * maximizer);
                // do min/max
                dest[pos] = (uint)COERCE(ref sensel,ref zero,ref full16);
            }
            return dest;
        }

        public static byte[] from16to12(uint[] source, data rData)
        {
            // preparing variables
            int resx = rData.metaData.xResolution;
            int resy = rData.metaData.yResolution;

            // ------------- and go ----

            int chunks = resx * resy;
            byte[] Dest = new Byte[chunks /2 *3];
            UInt32 tt = 0;

            for (var t = 0; t < chunks; t += 2)
            {
                uint sA = source[t] >> 4;
                uint sB = source[t + 1] >> 4;
                Dest[tt++] = (byte)((sA >> 4) & 0xff);
                Dest[tt++] = (byte)(((sA & 0xF) << 4) | (sB >> 8));
                Dest[tt++] = (byte)(sB & 0xff);
            }
            return Dest;
        }

        // not used anymore, because all adjuster work with 16bit values
        public static byte[] to12(byte[] source, data rData)
        {
            // preparing variables
            int resx = rData.metaData.xResolution;
            int resy = rData.metaData.yResolution;
            int bl = rData.metaData.blackLevelOld;
            bool maximize = rData.metaData.maximize;
            double maximizer = rData.metaData.maximizer;

            // ------------- and go ----

            int chunks = resx * resy * 14 / 8;
            byte[] Dest = new Byte[chunks / 14 * 12 + 42];
            UInt32 tt = 0;
            int senselA, senselB, senselC, senselD, senselE, senselF, senselG, senselH;
            int senselI, senselJ, senselK, senselL, senselM, senselN, senselO, senselP;
            int senselQ, senselR, senselS, senselT, senselU, senselV, senselW, senselX;

            for (var t = 0; t < chunks; t += 42)
            {
                if (maximize == true)
                {
                    senselA = (int)((source[t] >> 2) | (source[t + 1] << 6)) - (int)bl;
                    senselB = (int)(((source[t] & 0x3) << 12) | (source[t + 3] << 4) | (source[t + 2] >> 4)) - (int)bl;
                    senselC = (int)(((source[t + 2] & 0x0f) << 10) | (source[t + 5] << 2) | (source[t + 4] >> 6)) - (int)bl;
                    senselD = (int)(((source[t + 4] & 0x3f) << 8) | (source[t + 7])) - (int)bl;
                    senselE = (int)((source[t + 9] >> 2) | (source[t + 6] << 6)) - (int)bl;
                    senselF = (int)(((source[t + 9] & 0x3) << 12) | (source[t + 8] << 4) | (source[t + 11] >> 4)) - (int)bl;
                    senselG = (int)(((source[t + 11] & 0x0f) << 10) | (source[t + 10] << 2) | (source[t + 13] >> 6)) - (int)bl;
                    senselH = (int)(((source[t + 13] & 0x3f) << 8) | (source[t + 12])) - (int)bl;

                    senselI = (int)((source[t + 14] >> 2) | (source[t + 15] << 6)) - (int)bl;
                    senselJ = (int)(((source[t + 14] & 0x3) << 12) | (source[t + 17] << 4) | (source[t + 16] >> 4)) - (int)bl;
                    senselK = (int)(((source[t + 16] & 0x0f) << 10) | (source[t + 19] << 2) | (source[t + 18] >> 6)) - (int)bl;
                    senselL = (int)(((source[t + 18] & 0x3f) << 8) | (source[t + 21])) - (int)bl;
                    senselM = (int)((source[t + 23] >> 2) | (source[t + 20] << 6)) - (int)bl;
                    senselN = (int)(((source[t + 23] & 0x3) << 12) | (source[t + 22] << 4) | (source[t + 25] >> 4)) - (int)bl;
                    senselO = (int)(((source[t + 25] & 0x0f) << 10) | (source[t + 24] << 2) | (source[t + 27] >> 6)) - (int)bl;
                    senselP = (int)(((source[t + 27] & 0x3f) << 8) | (source[t + 26])) - (int)bl;

                    senselQ = (int)((source[t + 28] >> 2) | (source[t + 29] << 6)) - (int)bl;
                    senselR = (int)(((source[t + 28] & 0x3) << 12) | (source[t + 31] << 4) | (source[t + 30] >> 4)) - (int)bl;
                    senselS = (int)(((source[t + 30] & 0x0f) << 10) | (source[t + 33] << 2) | (source[t + 32] >> 6)) - (int)bl;
                    senselT = (int)(((source[t + 32] & 0x3f) << 8) | (source[t + 35])) - (int)bl;
                    senselU = (int)((source[t + 37] >> 2) | (source[t + 34] << 6)) - (int)bl;
                    senselV = (int)(((source[t + 37] & 0x3) << 12) | (source[t + 36] << 4) | (source[t + 39] >> 4)) - (int)bl;
                    senselW = (int)(((source[t + 39] & 0x0f) << 10) | (source[t + 38] << 2) | (source[t + 41] >> 6)) - (int)bl;
                    senselX = (int)(((source[t + 41] & 0x3f) << 8) | (source[t + 40])) - (int)bl;

                    // maximize to 12bit
                    senselA = (int)(senselA * maximizer);
                    senselB = (int)(senselB * maximizer);
                    senselC = (int)(senselC * maximizer);
                    senselD = (int)(senselD * maximizer);
                    senselE = (int)(senselE * maximizer);
                    senselF = (int)(senselF * maximizer);
                    senselG = (int)(senselG * maximizer);
                    senselH = (int)(senselH * maximizer);
                    senselI = (int)(senselI * maximizer);
                    senselJ = (int)(senselJ * maximizer);
                    senselK = (int)(senselK * maximizer);
                    senselL = (int)(senselL * maximizer);
                    senselM = (int)(senselM * maximizer);
                    senselN = (int)(senselN * maximizer);
                    senselO = (int)(senselO * maximizer);
                    senselP = (int)(senselP * maximizer);
                    senselQ = (int)(senselQ * maximizer);
                    senselR = (int)(senselR * maximizer);
                    senselS = (int)(senselS * maximizer);
                    senselT = (int)(senselT * maximizer);
                    senselU = (int)(senselU * maximizer);
                    senselV = (int)(senselV * maximizer);
                    senselW = (int)(senselW * maximizer);
                    senselX = (int)(senselX * maximizer);

                    // check on overflow
                    if (senselA > 4095) senselA = 4095;
                    if (senselB > 4095) senselB = 4095;
                    if (senselC > 4095) senselC = 4095;
                    if (senselD > 4095) senselD = 4095;
                    if (senselE > 4095) senselE = 4095;
                    if (senselF > 4095) senselF = 4095;
                    if (senselG > 4095) senselG = 4095;
                    if (senselH > 4095) senselH = 4095;
                    if (senselI > 4095) senselI = 4095;
                    if (senselJ > 4095) senselJ = 4095;
                    if (senselK > 4095) senselK = 4095;
                    if (senselL > 4095) senselL = 4095;
                    if (senselM > 4095) senselM = 4095;
                    if (senselN > 4095) senselN = 4095;
                    if (senselO > 4095) senselO = 4095;
                    if (senselP > 4095) senselP = 4095;
                    if (senselQ > 4095) senselQ = 4095;
                    if (senselR > 4095) senselR = 4095;
                    if (senselS > 4095) senselS = 4095;
                    if (senselT > 4095) senselT = 4095;
                    if (senselU > 4095) senselU = 4095;
                    if (senselV > 4095) senselV = 4095;
                    if (senselW > 4095) senselW = 4095;
                    if (senselX > 4095) senselX = 4095;


                    // -- react on underflow
                    if (senselA < 0) senselA = 0;
                    if (senselB < 0) senselB = 0;
                    if (senselC < 0) senselC = 0;
                    if (senselD < 0) senselD = 0;
                    if (senselE < 0) senselE = 0;
                    if (senselF < 0) senselF = 0;
                    if (senselG < 0) senselG = 0;
                    if (senselH < 0) senselH = 0;
                    if (senselI < 0) senselI = 0;
                    if (senselJ < 0) senselJ = 0;
                    if (senselK < 0) senselK = 0;
                    if (senselL < 0) senselL = 0;
                    if (senselM < 0) senselM = 0;
                    if (senselN < 0) senselN = 0;
                    if (senselO < 0) senselO = 0;
                    if (senselP < 0) senselP = 0;
                    if (senselQ < 0) senselQ = 0;
                    if (senselR < 0) senselR = 0;
                    if (senselS < 0) senselS = 0;
                    if (senselT < 0) senselT = 0;
                    if (senselU < 0) senselU = 0;
                    if (senselV < 0) senselV = 0;
                    if (senselW < 0) senselW = 0;
                    if (senselX < 0) senselX = 0;

                }
                else
                {
                    senselA = (int)((source[t] >> 2) | (source[t + 1] << 6));
                    senselB = (int)(((source[t] & 0x3) << 12) | (source[t + 3] << 4) | (source[t + 2] >> 4));
                    senselC = (int)(((source[t + 2] & 0x0f) << 10) | (source[t + 5] << 2) | (source[t + 4] >> 6));
                    senselD = (int)(((source[t + 4] & 0x3f) << 8) | (source[t + 7]));
                    senselE = (int)((source[t + 9] >> 2) | (source[t + 6] << 6));
                    senselF = (int)(((source[t + 9] & 0x3) << 12) | (source[t + 8] << 4) | (source[t + 11] >> 4));
                    senselG = (int)(((source[t + 11] & 0x0f) << 10) | (source[t + 10] << 2) | (source[t + 13] >> 6));
                    senselH = (int)(((source[t + 13] & 0x3f) << 8) | (source[t + 12]));

                    senselI = (int)((source[t + 14] >> 2) | (source[t + 15] << 6));
                    senselJ = (int)(((source[t + 14] & 0x3) << 12) | (source[t + 17] << 4) | (source[t + 16] >> 4));
                    senselK = (int)(((source[t + 16] & 0x0f) << 10) | (source[t + 19] << 2) | (source[t + 18] >> 6));
                    senselL = (int)(((source[t + 18] & 0x3f) << 8) | (source[t + 21]));
                    senselM = (int)((source[t + 23] >> 2) | (source[t + 20] << 6));
                    senselN = (int)(((source[t + 23] & 0x3) << 12) | (source[t + 22] << 4) | (source[t + 25] >> 4));
                    senselO = (int)(((source[t + 25] & 0x0f) << 10) | (source[t + 24] << 2) | (source[t + 27] >> 6));
                    senselP = (int)(((source[t + 27] & 0x3f) << 8) | (source[t + 26]));

                    senselQ = (int)((source[t + 28] >> 2) | (source[t + 29] << 6));
                    senselR = (int)(((source[t + 28] & 0x3) << 12) | (source[t + 31] << 4) | (source[t + 30] >> 4));
                    senselS = (int)(((source[t + 30] & 0x0f) << 10) | (source[t + 33] << 2) | (source[t + 32] >> 6));
                    senselT = (int)(((source[t + 32] & 0x3f) << 8) | (source[t + 35]));
                    senselU = (int)((source[t + 37] >> 2) | (source[t + 34] << 6));
                    senselV = (int)(((source[t + 37] & 0x3) << 12) | (source[t + 36] << 4) | (source[t + 39] >> 4));
                    senselW = (int)(((source[t + 39] & 0x0f) << 10) | (source[t + 38] << 2) | (source[t + 41] >> 6));
                    senselX = (int)(((source[t + 41] & 0x3f) << 8) | (source[t + 40]));
                    senselA = senselA >> 2;
                    senselB = senselB >> 2;
                    senselC = senselC >> 2;
                    senselD = senselD >> 2;
                    senselE = senselE >> 2;
                    senselF = senselF >> 2;
                    senselG = senselG >> 2;
                    senselH = senselH >> 2;
                    senselI = senselI >> 2;
                    senselJ = senselJ >> 2;
                    senselK = senselK >> 2;
                    senselL = senselL >> 2;
                    senselM = senselM >> 2;
                    senselN = senselN >> 2;
                    senselO = senselO >> 2;
                    senselP = senselP >> 2;
                    senselQ = senselQ >> 2;
                    senselR = senselR >> 2;
                    senselS = senselS >> 2;
                    senselT = senselT >> 2;
                    senselU = senselU >> 2;
                    senselV = senselV >> 2;
                    senselW = senselW >> 2;
                    senselX = senselX >> 2;
                }

                Dest[tt++] = (byte)((senselA >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselA & 0xF) << 4) | (senselB >> 8));
                Dest[tt++] = (byte)(senselB & 0xff);

                Dest[tt++] = (byte)((senselC >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselC & 0xF) << 4) | (senselD >> 8));
                Dest[tt++] = (byte)(senselD & 0xff);

                Dest[tt++] = (byte)((senselE >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselE & 0xF) << 4) | (senselF >> 8));
                Dest[tt++] = (byte)(senselF & 0xff);

                Dest[tt++] = (byte)((senselG >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselG & 0xF) << 4) | (senselH >> 8));
                Dest[tt++] = (byte)(senselH & 0xff);

                Dest[tt++] = (byte)((senselI >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselI & 0xF) << 4) | (senselJ >> 8));
                Dest[tt++] = (byte)(senselJ & 0xff);

                Dest[tt++] = (byte)((senselK >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselK & 0xF) << 4) | (senselL >> 8));
                Dest[tt++] = (byte)(senselL & 0xff);

                Dest[tt++] = (byte)((senselM >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselM & 0xF) << 4) | (senselN >> 8));
                Dest[tt++] = (byte)(senselN & 0xff);

                Dest[tt++] = (byte)((senselO >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselO & 0xF) << 4) | (senselP >> 8));
                Dest[tt++] = (byte)(senselP & 0xff);

                Dest[tt++] = (byte)((senselQ >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselQ & 0xF) << 4) | (senselR >> 8));
                Dest[tt++] = (byte)(senselR & 0xff);

                Dest[tt++] = (byte)((senselS >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselS & 0xF) << 4) | (senselT >> 8));
                Dest[tt++] = (byte)(senselT & 0xff);

                Dest[tt++] = (byte)((senselU >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselU & 0xF) << 4) | (senselV >> 8));
                Dest[tt++] = (byte)(senselV & 0xff);

                Dest[tt++] = (byte)((senselW >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselW & 0xF) << 4) | (senselX >> 8));
                Dest[tt++] = (byte)(senselX & 0xff);

            }
            return Dest;
        }

        public static void pinkHighlight(ref uint[] pic, data param)
        {
            int xRes = param.metaData.xResolution;
            int yRes = param.metaData.yResolution;
            
            // thats the value green and red/blue drifts in bmcc-premiere code.
            int limitedWhitelevel = 41721; // 46480; // 42000;
            //double maxRange = 1.8;

            for (var y = 0; y < yRes; y+=2)
            {
                for (var x = 0; x < xRes; x+=2)
                {
                    // adress for both rows
                    int rowRG = x + (y + 0) * xRes;
                    int rowGB = x + (y + 1) * xRes;

                    // sensel-values
                    int r1 = (int)pic[rowRG];
                    int g1 = (int)pic[rowRG + 1];
                    int g2 = (int)pic[rowGB];
                    int b1 = (int)pic[rowGB + 1];

                    // limiting r and b to values of g1/g2
                    r1 = COERCE(ref r1, ref zero, ref limitedWhitelevel);
                    //if (r1 > whitelevel) r1 = whitelevel;
                    //if (g1 > whitelevel) g1 = whitelevel; // whitelevel + (g1 - whitelevel) * 2;
                    //if (g2 > whitelevel) g2 = whitelevel; // whitelevel + (g2 - whitelevel) * 2;
                    //if (b1 > whitelevel) b1 = whitelevel;
                    b1 = COERCE(ref b1, ref zero, ref limitedWhitelevel);

                    pic[rowRG] = (uint)r1;
                    pic[rowRG + 1] = (uint)g1;
                    pic[rowGB] = (uint)g2;
                    pic[rowGB + 1] = (uint)b1;
                }
            }
        }

        public static double[] calcVerticalBandingCoeff(uint[] pic, raw r)
        {
            int xRes = r.data.metaData.xResolution;
            int yRes = r.data.metaData.yResolution;

            double[] coeffs = new double[9];
            int[][] histogram = new int[8][];
            int column = 0;
            
            for (var i = 0; i < 8; i++) histogram[i] = new int[65536];

            // -- compute Histograms
            // 8 histograms , assume green is enough
            for (var x = 0; x < xRes; x+=2)
            {
                column = x % 8;
                for (var y = 0; y < yRes; y+=2)
                {
                    int rowRG = (x + (y + 0) * xRes);
                    int rowGB = (x + (y + 1) * xRes);

                    //int r1 = pic[rowRG];
                    int g1 = (int)pic[rowRG + 1];
                    int g2 = (int)pic[rowGB];
                    //int b1 = pic[rowGB + 1];
                    histogram[column][g2]++;
                    histogram[column + 1][g1]++;
                }
            }

            // calculate median per readout-channel (not rgb)
            int val = 0;
            int[] median = new int[8];
            double[] d_median = new double[8];
            for (int i = 0; i < 8; i++)
            {
                val = 0;
                median[i] = 0;
                foreach (int value in histogram[i]) median[i] += value * val++;
                d_median[i] = (double)median[i] / (double)65536;
            }

            // find most frequent Value
            // assuming this is the base value to be corrected to
            double frequentVal = d_median.GroupBy(item => item).OrderByDescending(g => g.Count()).Select(g => g.Key).First();

            // -- compute median correction factor
            // look for histogram difference
            for (int i = 0; i < 8; i++) coeffs[i] = d_median[i] / frequentVal;

            // -- decide if correction needed 
            // find min max - if values differs more than 0.01
            double min = 65535.0;
            double max = 0.0;
            coeffs[8] = 0;
            for (int i = 0; i < 8; i++)
            {
                if (coeffs[i] > max) max = coeffs[i];
                if (coeffs[i] < min) min = coeffs[i];
            }
            if ((max - min) > 0.01) coeffs[8] = 1;

            // finally draw a histogram as bitmap
            // only for gui 
            r.histogram = createHistogramBMP(histogram[0]);

            return coeffs;
        }

        public static uint[] fixVerticalBanding(uint[] picIn, data param)
        {
            // vertical Banding idea by a1ex
            // to be found in Magic Lantern / modules / lv_rec / raw2dng.c
            // https://bitbucket.org/hudson/magic-lantern/src/c38da103d1842fce7da5a3a5f2d5d71990ed4f0c/modules/lv_rec/raw2dng.c?at=unified

            uint[] picOut = new uint[picIn.Length];
            int xRes = param.metaData.xResolution;
            int yRes = param.metaData.yResolution;

            for (var x = 0; x < xRes; x+=2)
            {
                int column = x % 8;
                double coeffEven = param.metaData.verticalBandingCoeffs[column];
                double coeffOdd = param.metaData.verticalBandingCoeffs[column + 1];

                for (var y = 0; y < yRes; y+=2)
                {
                    int rowRG = x + (y + 0) * xRes;
                    int rowGB = x + (y + 1) * xRes;

                    int r1 = (int)((double)picIn[rowRG] / (double)coeffEven);
                    int g1 = (int)((double)picIn[rowRG + 1] / (double)coeffOdd);
                    int g2 = (int)((double)picIn[rowGB] / (double)coeffEven);
                    int b1 = (int)((double)picIn[rowGB + 1] / (double)coeffOdd);

                    r1 = COERCE(ref r1,ref zero,ref full16);
                    g1 = COERCE(ref g1,ref zero, ref full16);
                    g2 = COERCE(ref g2,ref zero, ref full16);
                    b1 = COERCE(ref b1,ref zero, ref full16);

                    // back to array
                    picOut[rowRG] = (uint)r1;
                    picOut[rowRG + 1] = (uint)g1;
                    picOut[rowGB] = (uint)g2;
                    picOut[rowGB + 1] = (uint)b1;
                }
            }
            return picOut;
        }

        public static void findDeadSensels(uint[] source, data param)
        {
            // input source is 16bit without maximizing(!)
            // assuming below blacklevel are defective sensels
            // we mark them as dead

            // while writing that i found, it was the same
            // approach as from escho
            
            int ResX = param.metaData.xResolution;
            int ResY = param.metaData.yResolution;
            
            // basic variables
            //int threshold = 2;
            uint senselValue = 0;

            for (var y = 2; y < (ResY-2); y+=2)
            {
                for (var x = 1; x < (ResX-2); x+=2)
                {
                    senselValue = source[x + y * ResX];
                    
                    // look for dead pixel
                    if (senselValue < (param.metaData.blackLevelOld-256))
                    {
                        param.metaData.deadSensel.Add(new point { x = x, y = y, isHot = false });
                    }

                    //look for neighbour green data
                    int neighboursVal = 0;
                    for (int dy = -1; dy < 2; dy++)
                    {
                        for (int dx = -1; dx < 2; dx++)
                        {
                            if ((Math.Abs(dx) + Math.Abs(dy)) == 2)
                            {
                                // only look on green neighbours -> distance 2
                                neighboursVal += (int)source[(x + dx) + (y + dy) * ResX];
                            }
                        }
                    }
                    if (senselValue/4 > neighboursVal)
                    {
                        param.metaData.deadSensel.Add(new point { x = x, y = y, isHot = true });
                    }
                }
            }
        }

        public static void fixDeadSensels()
        {
            // assuming, dead sensels are zero 0
            // and hot pixels are full well 16384
            // we replace with neighbour values
            // and we assume they re static
            // so finding is a onetimer
            // replacing is done on values from metadata.deadhot
            // preparing variables
        }

        public static void chromaSmoothing(ref uint[] picIn, data param)
        {
            // chroma Smoothing 2x2 written by a1ex & escho ?!
            // seems the algorithm in lv_rec is the right one for single frames
            // https://bitbucket.org/hudson/magic-lantern/src/tip/modules/lv_rec/raw2dng.c
            // to be found in Magic Lantern / modules / dual_iso / chroma_smooth.c
            // or in mlvfs - https://bitbucket.org/dmilligan/mlvfs/src/2b61f5707f91f58578b151b37b3f62dc138b8249/mlvfs/chroma_smooth.c?at=master
            
            int xRes = param.metaData.xResolution;
            int yRes = param.metaData.yResolution;
            int xResHalf = xRes / 2;
            int yResHalf = yRes / 2;
            int whiteLevel = param.metaData.whiteLevelOld;
            int blackLevel = param.metaData.blackLevelOld;

            //byte[] picOut = new byte[picIn.Length];

            for (var y = 4; y < (yRes-5); y+=2)
            {
                for (var x = 4; x < (xRes - 4); x += 2)
                {
                    //int ge = (int)( voP(picIn, param, x+1, y) + voP(picIn, param, x, y+1) ) /2;
                    //if (ge < (2*param.metaData.blackLevelOld)) continue;

                    int eh = 0;
                    int i, j;
                    int k = 0;
                    int[] med_r = new int[5];
                    int[] med_b = new int[5];

                    // horizontal interpolation
                    for (i = -2; i <= 2; i += 2)
                    {
                        for (j = -2; j <= 2; j += 2)
                        {
                            if (Math.Abs(i) + Math.Abs(j) == 4) continue;

                            int r = (int)voP(picIn, param, x + i, y + j);
                            int g1 = (int)voP(picIn, param, x + i + 1, y + j);
                            int g2 = (int)voP(picIn, param, x + i, y + j + 1);
                            int g3 = (int)voP(picIn, param, x + i - 1, y + j);
                            int g5 = (int)voP(picIn, param, x + i + 2, y + j + 1);
                            int b = (int)voP(picIn, param, x + i + 1, y + j + 1);

                            int gr = (g1 + g3) / 2;
                            int gb = (g2 + g5) / 2;
                            eh += Math.Abs(g1 - g3) + Math.Abs(g2 - g5);

                            med_r[k] = r - gr;
                            med_b[k] = b - gb;
                            k++;
                        }
                    }
                    int drh = opt_med5(ref med_r);
                    int dbh = opt_med5(ref med_b);

                    int ev = 0;
                    k = 0;

                    // vertical interpolation
                    for (i = -2; i <= 2; i += 2)
                    {
                        for (j = -2; j <= 2; j += 2)
                        {
                            if (Math.Abs(i) + Math.Abs(j) == 4) continue;

                            int r = (int)voP(picIn, param, x + i, y + j);
                            int g1 = (int)voP(picIn, param, x + i + 1, y + j);
                            int g2 = (int)voP(picIn, param, x + i, y + j + 1);
                            int g4 = (int)voP(picIn, param, x + i, y + j - 1);
                            int g6 = (int)voP(picIn, param, x + i + 1, y + j + 2);
                            int b = (int)voP(picIn, param, x + i + 1, y + j + 1);

                            int gr = (g2 + g4) / 2;
                            int gb = (g1 + g6) / 2;
                            ev += Math.Abs(g2 - g4) + Math.Abs(g1 - g6);

                            med_r[k] = r - gr;
                            med_b[k] = b - gb;
                            k++;
                        }
                    }
                    int drv = opt_med5(ref med_r);
                    int dbv = opt_med5(ref med_b);

                    int ng1 = (int)picIn[x + 1 + y * xRes];
                    int ng2 = (int)picIn[x + (y + 1) * xRes];
                    int ng3 = (int)picIn[x - 1 + (y) * xRes];
                    int ng4 = (int)picIn[x + (y - 1) * xRes];
                    int ng5 = (int)picIn[x + 2 + (y + 1) * xRes];
                    int ng6 = (int)picIn[x + 1 + (y + 2) * xRes];

                    int grv = (ng2 + ng4) / 2;
                    int grh = (ng1 + ng3) / 2;
                    int gbv = (ng1 + ng6) / 2;
                    int gbh = (ng2 + ng5) / 2;

                    int a_gr = ev < eh ? grv : grh;
                    int a_gb = ev < eh ? gbv : gbh;
                    int a_dr = ev < eh ? drv : drh;
                    int a_db = ev < eh ? dbv : dbh;

                    int thr = 64;
                    if (voP(picIn, param, x, y) < blackLevel + thr || voP(picIn, param, x + 1, y + 1) < blackLevel + thr || Math.Abs(drv - drh) < thr || Math.Abs(grv - grh) < thr || Math.Abs(gbv - gbh) < thr)
                    {
                        a_dr = (drv + drh) / 2;
                        a_db = (dbv + dbh) / 2;
                        a_gr = (ng1 + ng2 + ng3 + ng4) / 4;
                        a_gb = (ng1 + ng2 + ng5 + ng6) / 4;
                    }

                    /* replace red and blue pixels with filtered values, keep green pixels unchanged */
                    /* don't touch overexposed areas */
                    int grdr = a_gr + a_dr;
                    int gbdb = a_gb + a_db;

                    if (picIn[x + y * xRes] < whiteLevel)
                    {
                        picIn[x + y * xRes] = (uint)COERCE(ref grdr, ref blackLevel, ref whiteLevel);
                    }
                    if (picIn[x + 1 + (y + 1) * xRes] < whiteLevel)
                    {
                        picIn[x + 1 + (y + 1) * xRes] = (uint)COERCE(ref gbdb, ref blackLevel, ref whiteLevel);
                    }
                }
            }
        }

        // -----------------------------------------------------------------------
        // ------------------------ preview calculations -------------------------
        // -----------------------------------------------------------------------

        public static WriteableBitmap doBitmapLQgrey(uint[] imageData, data param)
        {
            int xRes = param.metaData.xResolution;
            int yRes = param.metaData.yResolution;
            int whole = xRes*yRes;

            byte[] imageData8 = new byte[whole / 4 * 3];

            int div = 64;

            // basic variables
            int rowRG = 0;
            int rowGB = 0;
            int b1 = 0;
            int g1 = 0;
            int g2 = 0;
            int r1 = 0;
            int bitmapPos = 0;
            double grey = 0;

            for (var y = 0; y < yRes; y+=2)
            {
                for (var x = 0; x < xRes; x+=2)
                {
                    rowRG = (x + (y + 0) * xRes);
                    rowGB = (x + (y + 1) * xRes);

                    r1 = (int)(imageData[rowRG] / param.metaData.wb_R);
                    g1 = (int)(imageData[rowRG + 1] / param.metaData.wb_G);
                    g2 = (int)(imageData[rowGB] / param.metaData.wb_G);
                    b1 = (int)(imageData[rowGB + 1] / param.metaData.wb_B);

                    // coeffs taken from rgb->y(uv) conversion
                    grey = (g1 + g2) / 2 * 0.11 + b1*0.59 + r1*0.30;
                    grey = (grey / div);
                    grey = (byte)((grey > 255) ? 255 : grey);

                    bitmapPos = (x / 2 + y / 2 * xRes / 2) * 3;
                    
                    imageData8[bitmapPos] = (byte)grey;
                    imageData8[bitmapPos + 1] = (byte)grey;
                    imageData8[bitmapPos + 2] = (byte)grey;
                }
            }
            WriteableBitmap wbm = new WriteableBitmap(xRes/2, yRes/2, 96, 96, PixelFormats.Bgr24, null);
            wbm.WritePixels(new Int32Rect(0, 0, xRes/2, yRes/2), imageData8, 3 * xRes/2, 0);
            return wbm;
        }

        public static WriteableBitmap doBitmapLQmullim(uint[] imageData, data param)
        {
            int xRes = param.metaData.xResolution;
            int yRes = param.metaData.yResolution;
            int whole = xRes * yRes;

            byte[] imageData8 = new byte[whole / 4 * 3];

            int div = 64;

            // basic variables
            int rowRG = 0;
            int rowGB = 0;
            int b1 = 0;
            int g1 = 0;
            int g2 = 0;
            int r1 = 0;
            int bitmapPos = 0;
            int gNew = 0;

            for (var y = 0; y < yRes; y += 2)
            {
                for (var x = 0; x < xRes; x += 2)
                {
                    rowRG = (x + (y + 0) * xRes);
                    rowGB = (x + (y + 1) * xRes);

                    r1 = (int)(imageData[rowRG] / param.metaData.wb_R);
                    g1 = (int)(imageData[rowRG + 1] / param.metaData.wb_G);
                    g2 = (int)(imageData[rowGB] / param.metaData.wb_G);
                    b1 = (int)(imageData[rowGB + 1] / param.metaData.wb_B);

                    gNew = (g1 + g2) / 2;

                    b1 = (b1 / div);
                    gNew = (gNew / div);
                    r1 = (r1 / div);

                    bitmapPos = (x / 2 + y / 2 * xRes / 2) * 3;

                    imageData8[bitmapPos] = (byte)((b1 > 255) ? 255 : b1);
                    imageData8[bitmapPos + 1] = (byte)((gNew > 255) ? 255 : gNew);
                    imageData8[bitmapPos + 2] = (byte)((r1 > 255) ? 255 : r1);
                }
            }
            WriteableBitmap wbm = new WriteableBitmap(xRes / 2, yRes / 2, 96, 96, PixelFormats.Bgr24, null);
            wbm.WritePixels(new Int32Rect(0, 0, xRes / 2, yRes / 2), imageData8, 3 * xRes / 2, 0);
            return wbm;
        }

        public static WriteableBitmap doBitmapHQ(uint[] imageData, data param)
        {
            int halfResx = param.metaData.xResolution / 2;
            int halfResy = param.metaData.yResolution / 2;
            int whole = halfResx * halfResy;

            byte[] imageData8 = new byte[halfResx * halfResy * 3];

            int div = 65536;

            double gamma = 0.5;

            // basic variables
            int rowRG = 0;
            int rowGB = 0;
            int b1 = 0;
            int g1 = 0;
            int g2 = 0;
            int r1 = 0;
            int bitmapPos = 0;
            int gNew = 0;

            for (var y = 0; y < halfResy; y++)
            {
                for (var x = 0; x < halfResx; x++)
                {
                    rowRG = (x * 2  + (y * 2 + 0) * halfResx *2);
                    rowGB = (x * 2  + (y * 2 + 1) * halfResx *2);

                    r1 = (int)(imageData[rowRG] / param.metaData.wb_R);
                    g1 = (int)(imageData[rowRG + 1] / param.metaData.wb_G);
                    g2 = (int)(imageData[rowGB] / param.metaData.wb_G);
                    b1 = (int)(imageData[rowGB + 1] / param.metaData.wb_B);

                    gNew = (g1 + g2) / 2;

                        b1 = (int)((Math.Pow((double)b1 / div, gamma)) * 256);
                        gNew = (int)((Math.Pow((double)gNew / div, gamma)) * 256);
                        //g1 = (int)((Math.Pow((double)g1 / div, gamma)) * 256);
                        //g2 = (int)((Math.Pow((double)g2 / div, gamma)) * 256);
                        r1 = (int)((Math.Pow((double)r1 / div, gamma)) * 256);
                    bitmapPos = (x + y * halfResx) * 3;

                    imageData8[bitmapPos] = (byte)((b1 > 255) ? 255 : b1);
                    imageData8[bitmapPos + 1] = (byte)((gNew > 255) ? 255 : gNew);
                    imageData8[bitmapPos + 2] = (byte)((r1 > 255) ? 255 : r1);
                }
            }
            WriteableBitmap wbm = new WriteableBitmap(halfResx, halfResy, 96, 96, PixelFormats.Bgr24, null);
            wbm.WritePixels(new Int32Rect(0, 0, halfResx, halfResy), imageData8, 3 * halfResx, 0);
            return wbm;
        }

        public static WriteableBitmap doBitmapHQ709(uint[] imageData, data param)
        {
            int halfResx = param.metaData.xResolution / 2;
            int halfResy = param.metaData.yResolution / 2;
            int whole = halfResx * halfResy;
            byte[] imageData8 = new byte[halfResx * halfResy * 3];

            // basic variables
            int rowRG = 0;
            int rowGB = 0;
            double b1 = 0;
            double g1 = 0;
            double g2 = 0;
            double r1 = 0;
            int bitmapPos = 0;
            double gNew = 0;

            for (var y = 0; y < halfResy; y++)
            {
                for (var x = 0; x < halfResx; x++)
                {
                    rowRG = (x * 2 + (y * 2 + 0) * halfResx * 2);
                    rowGB = (x * 2 + (y * 2 + 1) * halfResx * 2);

                    r1 = (int)(imageData[rowRG] / param.metaData.wb_R);
                    g1 = (int)(imageData[rowRG + 1] / param.metaData.wb_G);
                    g2 = (int)(imageData[rowGB] / param.metaData.wb_G);
                    b1 = (int)(imageData[rowGB + 1] / param.metaData.wb_B);

                    gNew = (g1 + g2)/2;

                    if (r1 > 65535) r1 = 65535;
                    if (gNew > 65535) gNew = 65535;
                    if (b1 > 65535) b1 = 65535;

                    r1 = calc.Rec709[(int)r1];
                    gNew = calc.Rec709[(int)gNew];
                    b1 = calc.Rec709[(int)b1];

                    bitmapPos = (x + y * halfResx) * 3;

                    imageData8[bitmapPos] = (byte)b1;
                    imageData8[bitmapPos + 1] = (byte)gNew;
                    imageData8[bitmapPos + 2] = (byte)r1;
                }
            }
            WriteableBitmap wbm = new WriteableBitmap(halfResx, halfResy, 96, 96, PixelFormats.Bgr24, null);
            wbm.WritePixels(new Int32Rect(0, 0, halfResx, halfResy), imageData8, 3 * halfResx, 0);
            return wbm;
        }

        // -----------------------------------------------------------------------
        // --------------------------------------- demosaicing -------------------
        // -----------------------------------------------------------------------
        
        // -- first attempt to demosaic the raw-data
        // -- AHD algorithm taken from coffins dcraw
        // -- http://cybercom.net/~dcoffin/dcraw/dcraw.c - line 4692
        // -- another algo would be vng4 - line 4247
        // --

        public static WriteableBitmap demosaic_debugRGGBPattern(uint[] input, data param)
        {
            int ResX = param.metaData.xResolution;
            int ResY = param.metaData.yResolution;
            byte[] imageData8RGB = new byte[ResX * ResY * 3];
            int c = 0;
            // -------
            for (int y = 0; y < param.metaData.yResolution; y++)
            {
                for (int x = 0; x < param.metaData.xResolution; x++)
                {
                    // decide what color
                    bool dx = x % 2 == 0 ? true : false;
                    bool dy = y % 2 == 0 ? true : false;
                    int pos = x + y * ResX;
                    if (dx)
                    {
                        if (dy)
                        {
                            //r1
                            imageData8RGB[c++] = 0;
                            imageData8RGB[c++] = 0;
                            imageData8RGB[c++] = (byte)(input[pos] / 256);
                        }
                        else
                        {
                            //g1
                            imageData8RGB[c++] = 0;
                            imageData8RGB[c++] = (byte)(input[pos] / 256);
                            imageData8RGB[c++] = 0;
                        }
                    }
                    else
                    {
                        if (dy)
                        {
                            //g2
                            imageData8RGB[c++] = 0;
                            imageData8RGB[c++] = (byte)(input[pos] / 256);
                            imageData8RGB[c++] = 0;
                        }
                        else
                        {
                            //b1
                            imageData8RGB[c++] = (byte)(input[pos] / 256);
                            imageData8RGB[c++] = 0;
                            imageData8RGB[c++] = 0;
                        }
                    }
                }
            }

            // -------
            WriteableBitmap wbm = new WriteableBitmap(ResX, ResY, 96, 96, PixelFormats.Bgr24, null);
            wbm.WritePixels(new Int32Rect(0, 0, ResX, ResY), imageData8RGB, 3 * ResX, 0);

            //imageData8 = null;
            return wbm;
        
        }
    
        public static WriteableBitmap demosaic_AHD(uint[] imageData, data param)
        {
            int ResX = param.metaData.xResolution;
            int ResY = param.metaData.yResolution;
            int ResXmin = ResX - 2;
            int ResYmin = ResY - 2;

            int whole = ResX * ResY;
            uint[] imageData16RGB = new uint[whole * 3];

            // basic variables
            int arrPos = 0;
            double r1 = 0;
            double g1 = 0;
            double g2 = 0;
            double b1 = 0;
            int bitmapPos = 0;
            double gNew = 0;

            for (var y = 0; y < ResY; y++)
            {
                for (var x = 0; x < ResX; x++)
                {
                    arrPos = (x+ y * ResX) *2;
                    r1 = 0;
                    b1 = 0;
                    g1 = 0;
                    g2 = 0;
                    int[] tempCol;
                    List<int> senselQuad = new List<int>();

                    if (x > 1 && x < ResXmin && y > 1 && y < ResYmin)
                    {
                        // tempsave a 5x5 sensel array for demosaicing
                        for(int dy=-2;dy<3;dy++)
                        {
                            for(int dx=-2;dx<3;dx++)
                            {
                                senselQuad.Add((int)voP(imageData,param,x+dx,y+dy));
                            }
                        }
                        
                        if ((y % 2 == 0) && (x % 2 == 0))
                        {
                            // is red 1
                            r1 = voP(imageData, param, x, y) / param.metaData.wb_R; // ((double)param.metaData.RGBfraction[4] / (double)param.metaData.RGBfraction[5]);
                            tempCol = VNG4_demosaic(senselQuad,0x00);
                            //gNew = bicubicGreen(imageData, param, x, y) / param.metaData.wb_G;
                        }
                        if ((y % 2 == 0) && (x % 2 == 1))
                        {
                            // is green 1
                            g1 = voP(imageData, param, x, y) / param.metaData.wb_G; // ((double)param.metaData.RGBfraction[2] / (double)param.metaData.RGBfraction[3]);
                            tempCol = VNG4_demosaic(senselQuad, 0x01);
                            //gNew = g1;
                        }
                        if ((y % 2 == 1) && (x % 2 == 0))
                        {
                            // is green 2
                            g2 = voP(imageData, param, x, y) / param.metaData.wb_G; // ((double)param.metaData.RGBfraction[2] / (double)param.metaData.RGBfraction[3]);
                            tempCol = VNG4_demosaic(senselQuad, 0x02);
                            //gNew = g2;
                        }
                        if ((y % 2 == 1) && (x % 2 == 1))
                        {
                            // is blue 1
                            b1 = voP(imageData, param, x, y) / param.metaData.wb_B; //(imageData[arrPos] | imageData[arrPos + 1] << 8) / param.metaData.wb_R; // ((double)param.metaData.RGBfraction[0] / (double)param.metaData.RGBfraction[1]);
                            tempCol = VNG4_demosaic(senselQuad, 0x04);
                            //gNew = bicubicGreen(imageData, param, x, y) / param.metaData.wb_G;
                        }
                        if (b1 > 65535) b1 = 65535;
                        b1 = calc.Rec709[(int)b1];

                        if (gNew > 65535) gNew = 65535;
                        gNew = calc.Rec709[(int)gNew];

                        if (r1 > 65535) r1 = 65535;
                        r1 = calc.Rec709[(int)r1];
                    }

                    bitmapPos = (x + y * ResX) * 3;

                    imageData16RGB[bitmapPos] = 0;// (byte)r1; // (byte)r1;
                    imageData16RGB[bitmapPos + 1] = (byte)gNew;
                    imageData16RGB[bitmapPos + 2] = 0;// (byte)b1;
                }
            }
            WriteableBitmap wbm = new WriteableBitmap(ResX, ResY, 96, 96, PixelFormats.Bgr24, null);
            wbm.WritePixels(new Int32Rect(0, 0, ResX, ResY), imageData16RGB, 3 * ResX, 0);

            //imageData8 = null;
            return wbm;
        }

        public static uint voP(uint[] imageData, data param, int x, int y)
        {
            return imageData[(x + y * param.metaData.xResolution)];
        }

        public static int bicubicGreen(uint[] imageData, data param, int x, int y)
        {
            int val=0;
            double Neighbour1 = voP(imageData, param, x - 1, y) + voP(imageData, param, x + 1, y) + voP(imageData, param, x, y - 1) + voP(imageData, param, x, y + 1);
//            double Neighbour2 = valOnPos(imageData, param, x - 2, y - 1) + valOnPos(imageData, param, x - 2, y + 1) + valOnPos(imageData, param, x - 1, y - 2) + valOnPos(imageData, param, x + 1, y - 2) + valOnPos(imageData, param, x + 1, y - 2) + valOnPos(imageData, param, x + 1, y + 2) + valOnPos(imageData, param, x + 2, y + 1) + valOnPos(imageData, param, x + 2, y - 1);
//            double Neighbour3 = valOnPos(imageData, param, x - 3, y) + valOnPos(imageData, param, x + 3, y) + valOnPos(imageData, param, x, y - 3) + valOnPos(imageData, param, x, y + 3);
//            val = (int)(Neighbour1 / 4 * 0.66 + Neighbour2 / 8 * 0.24 + Neighbour3 / 4 * 0.1); 
              val = (int)(Neighbour1 / 4);
              val = 0;
            return val; 
        }

        public static int[] VNG4_demosaic(List<int> sensel, int col)
        {
            /* vng algorithm from
             * calculate 4 layers R G1 G2 B
            
             *  http://research.microsoft.com/en-us/um/people/lhe/papers/icassp04.demosaicing.pdf
             *  http://scien.stanford.edu/pages/labsite/1999/psych221/projects/99/tingchen/main.htm
             *  http://stackoverflow.com/questions/6114099/fast-integer-abs-function
             *  http://scien.stanford.edu/pages/labsite/1999/psych221/projects/99/dixiedog/vision.htm
             *  http://scien.stanford.edu/pages/labsite/1999/psych221/projects/99/tingchen/algodep/vargra.html
             */

            int[] result = new int[4];
            int[] gradients;

            // calculate 8 gradients
            int N = Math.Abs(sensel[7] - sensel[17]) + Math.Abs(sensel[2] - sensel[12]) + Math.Abs(sensel[6] - sensel[16]) / 2 + Math.Abs(sensel[8] - sensel[18]) / 2 + Math.Abs(sensel[1] - sensel[11]) / 2 + Math.Abs(sensel[3] - sensel[13]) / 2;
            int E = Math.Abs(sensel[13] - sensel[11]) + Math.Abs(sensel[14] - sensel[12]) + Math.Abs(sensel[8] - sensel[6]) / 2 + Math.Abs(sensel[18] - sensel[16]) / 2 + Math.Abs(sensel[9] - sensel[7]) / 2 + Math.Abs(sensel[19] - sensel[17]) / 2;
            int S = Math.Abs(sensel[17] - sensel[7]) + Math.Abs(sensel[22] - sensel[12]) + Math.Abs(sensel[18] - sensel[8]) / 2 + Math.Abs(sensel[16] - sensel[6]) / 2 + Math.Abs(sensel[23] - sensel[13]) / 2 + Math.Abs(sensel[21] - sensel[11]) / 2;
            int W = Math.Abs(sensel[11] - sensel[13]) + Math.Abs(sensel[10] - sensel[12]) + Math.Abs(sensel[16] - sensel[18]) / 2 + Math.Abs(sensel[6] - sensel[8]) / 2 + Math.Abs(sensel[15] - sensel[17]) / 2 + Math.Abs(sensel[5] - sensel[7]) / 2;
            int NE = Math.Abs(sensel[8] - sensel[16]) + Math.Abs(sensel[4] - sensel[12]) + Math.Abs(sensel[7] - sensel[11]) / 2 + Math.Abs(sensel[13] - sensel[17]) / 2 + Math.Abs(sensel[3] - sensel[7]) / 2 + Math.Abs(sensel[9] - sensel[13]) / 2;
            int SE = Math.Abs(sensel[18] - sensel[6]) + Math.Abs(sensel[24] - sensel[12]) + Math.Abs(sensel[13] - sensel[7]) / 2 + Math.Abs(sensel[17] - sensel[11]) / 2 + Math.Abs(sensel[19] - sensel[13]) / 2 + Math.Abs(sensel[23] - sensel[17]) / 2;
            int NW = Math.Abs(sensel[6] - sensel[18]) + Math.Abs(sensel[0] - sensel[12]) + Math.Abs(sensel[11] - sensel[17]) / 2 + Math.Abs(sensel[7] - sensel[13]) / 2 + Math.Abs(sensel[5] - sensel[11]) / 2 + Math.Abs(sensel[1] - sensel[7]) / 2;
            int SW = Math.Abs(sensel[16] - sensel[8]) + Math.Abs(sensel[20] - sensel[12]) + Math.Abs(sensel[17] - sensel[13]) / 2 + Math.Abs(sensel[11] - sensel[7]) / 2 + Math.Abs(sensel[21] - sensel[17]) / 2 + Math.Abs(sensel[15] - sensel[11]) / 2;

            gradients = new int[] { N, E, S, W, NE, SE, NW, SW };
            // threshold of gradients
            int min = ARR_MIN(gradients);
            int max = ARR_MAX(gradients);
            int T = (int)(1.5*min + 0.5 * (max - min));

            // select and add subsets that are lower T
            foreach (int grad in gradients)
            {

            }

            return result;
        }

        public static WriteableBitmap createHistogramBMP(int[] h, int x = 256, int y = 128)
        {
            byte[] imageData8 = new byte[256 * 128 * 3];
            int[] mergeHist = new int[256];
            int column = -1;

            // first squeeze from 16^2 to 8^2 fields
            for (int n = 0; n < 65536; n++)
            {
                if (n % 258 == 0)
                {
                    //if(n!=0) mergeHist[column] = (int)(mergeHist[column] / 256);
                    column++;
                }
                else
                {
                    mergeHist[column] += h[n];
                }
            }
            // rescale histogram logarithmic because of massive values
            for (int n = 0; n < 256; n++) mergeHist[n] = (int)(Math.Log(mergeHist[n])*33);

            //find maxval
              int maxval = 0;
            foreach (int v in mergeHist) if (v > maxval) maxval = v;
            // factor to change inputdata to outputdata
            double ySqueeze = 128 / (double)maxval;

            // for simplicity, fill bmp with black
            int bitmapPos; 
            for (int pos = 0; pos < imageData8.Length; pos++) imageData8[pos] = 255;
            // set the ELV lines
            for (int elv = 0; elv < 8; elv++)
            {
                int dx = (1 << elv) - 1;
                for (int dy = 0; dy < 128; dy++)
                {
                    bitmapPos = (dx + dy * 256) * 3;
                    // e0ffe0
                    imageData8[bitmapPos] = 0xb0;
                    imageData8[bitmapPos + 1] = 0xcf;
                    imageData8[bitmapPos + 2] = 0xb0;
                }
            }
            byte colmul = 1;
            // now draw the data into the bitmap
            for (int dx=0; dx<256; dx++)
            {
                colmul = 0;
                if (dx < 6 || dx > 245) colmul = 1;
                for (int dy = 0; dy < 128; dy++)
                {
                    bitmapPos = (dx + (127-dy)*256) * 3;
                    
                    if ((mergeHist[dx]*ySqueeze) > dy)
                    {
                        imageData8[bitmapPos] = (byte)(0);
                        imageData8[bitmapPos + 1] = (byte)(0);
                        imageData8[bitmapPos + 2] = (byte)(255*colmul);
                    }
                }
            }
            WriteableBitmap wbm = new WriteableBitmap(256, 128, 96, 96, PixelFormats.Bgr24, null);
            wbm.WritePixels(new Int32Rect(0, 0, 256, 128), imageData8, 3 * 256, 0);
            return wbm;
        }

        // ----------------------
        // --- dng-tag helper ---
        // ----------------------

        public static string setFilenameShort(string t)
        {
            t = Regex.Replace(t, @"[^0-9A-Za-z]+", "");
            if (t.Length < 8)
            {
                Random rnd = new Random();
                int uniqueChar;
                for (int g = 0; g < 8; g++)
                {
                    uniqueChar = 0;
                    foreach (byte b in System.Text.Encoding.UTF8.GetBytes(t.ToCharArray()))
                    {
                        uniqueChar += b;
                    }
                    uniqueChar = 66 + uniqueChar % 22;
                    t = t + Convert.ToChar(uniqueChar);
                }
            }
            if (t.Length > 8)
            {
                t = t.Substring(0, 8);
            }
            return t.ToUpper();
        }

        public static string setFilenameNum(string t)
        {
            return Regex.Replace(t, @"[^0-9]+", "");
        }

        public static int[] getRGGBValues(string filename)
        {
            // read the Data from the CR2

            // Here reading tiff header, then first IFD, finding EXIF-offset
            FileInfo rFile = new FileInfo(filename);
            //string rFile = isPhotoRAW;
            FileStream rStream = rFile.OpenRead();
            byte[] exifBytes = new byte[8192];
            int exifBytesRead = rStream.Read(exifBytes, 0, 8192);
            int IFD0count = exifBytes[16];
            byte[,] IFD0Data = new byte[IFD0count, 12];
            //MessageBox.Show(IFD0count.ToString() + " IFD0 entries");
            int exifOffset = 0;
            for (int i = 0; i < IFD0count; i++)
            {
                for (int b = 0; b < 12; b++)
                {
                    IFD0Data[i, b] = exifBytes[i * 12 + b + 18];

                }
                if ((IFD0Data[i, 0] + IFD0Data[i, 1] * 256) == 34665)
                {
                    // found Exif-Tags
                    exifOffset = IFD0Data[i, 8] + IFD0Data[i, 9] * 256;
                }

            }
            int makernoteOffset = 0;
            int exifCount = exifBytes[exifOffset];
            byte[,] exifData = new byte[exifCount, 12];
            // there are exifCount.ToString() entries
            for (int i = 0; i < exifCount; i++)
            {
                for (int b = 0; b < 12; b++)
                {
                    exifData[i, b] = exifBytes[i * 12 + b + exifOffset + 2];

                }
                if ((exifData[i, 0] + exifData[i, 1] * 256) == 33434)
                {
                    // found Exposure
                    //makernoteOffset = exifData[i, 8] + exifData[i, 9] * 256;
                }
                if ((exifData[i, 0] + exifData[i, 1] * 256) == 37500)
                {
                    makernoteOffset = exifData[i, 8] + exifData[i, 9] * 256;
                    // found Makernotes - Offset 0x0" + makernoteOffset.ToString("X"));
                }
            }

            int mnCount = exifBytes[makernoteOffset];
            int RGGBoffset = 0;
            byte[,] makernoteData = new byte[mnCount, 12];
            for (int i = 0; i < mnCount; i++)
            {
                for (int b = 0; b < 12; b++)
                {
                    makernoteData[i, b] = exifBytes[i * 12 + b + makernoteOffset + 2];

                }
                if ((makernoteData[i, 0] + makernoteData[i, 1] * 256) == 16385)
                {
                    RGGBoffset = makernoteData[i, 8] + makernoteData[i, 9] * 256;
                    // found RGGB-Subdata on Offset 0x  + RGGBoffset.ToString("X")

                }
            }

            int[] RGGB_measured = new int[4];
            RGGB_measured[0] = exifBytes[RGGBoffset + 146] + exifBytes[RGGBoffset + 147] * 256;
            RGGB_measured[1] = exifBytes[RGGBoffset + 148] + exifBytes[RGGBoffset + 149] * 256;
            RGGB_measured[2] = exifBytes[RGGBoffset + 150] + exifBytes[RGGBoffset + 151] * 256;
            RGGB_measured[3] = exifBytes[RGGBoffset + 152] + exifBytes[RGGBoffset + 153] * 256;
            //MessageBox.Show("RGGB measured = " + RGGB_measured[0].ToString() + " " + RGGB_measured[1].ToString() + " " + RGGB_measured[2].ToString() + " " + RGGB_measured[3].ToString());

            return RGGB_measured;
        }

        public static int[] convertToFraction(int[] RGGBValues)
        {
            int[] convertedData = new int[6];

            int[] tempConverted = new int[2];
            tempConverted = DoubleToFraction((double)RGGBValues[1] / (double)RGGBValues[0]);
            convertedData[0] = tempConverted[1];
            convertedData[1] = tempConverted[2];
            convertedData[2] = RGGBValues[1];
            convertedData[3] = RGGBValues[2];
            tempConverted = DoubleToFraction((double)RGGBValues[2] / (double)RGGBValues[3]);
            convertedData[4] = tempConverted[1];
            convertedData[5] = tempConverted[2];
            //MessageBox.Show("RGBValues" + convertedData[0] + "/" + convertedData[1] + " " + convertedData[2] + "/" + convertedData[3] + " " + convertedData[4] + "/" + convertedData[5]);
            return convertedData;
        }

        public static int[] DoubleToFraction(double num, double epsilon = 0.0000001, int maxIterations = 30)
        {
            double[] d = new double[maxIterations + 2];
            d[1] = 1;
            double z = num;
            double n = 1;
            int t = 1;

            int wholeNumberPart = (int)num;
            double decimalNumberPart = num - Convert.ToDouble(wholeNumberPart);

            while (t < maxIterations && Math.Abs(n / d[t] - num) > epsilon)
            {
                t++;
                z = 1 / (z - (int)z);
                d[t] = d[t - 1] * (int)z + d[t - 2];
                n = (int)(decimalNumberPart * d[t] + 0.5);
            }

            //MessageBox.Show((wholeNumberPart > 0 ? wholeNumberPart.ToString() + " " : "") + n.ToString()+":"+ d[t].ToString());
            return new int[] { wholeNumberPart, (int)n, (int)d[t] };
        }

        public static byte[] frameToTC_b(int frame, double framerate)
        {
            int hours = (int)Math.Floor((double)frame / framerate / 3600);
            frame = frame - (hours * 60 * 60 * (int)framerate);
            int minutes = (int)Math.Floor((double)frame / framerate / 60);
            frame = frame - (minutes*60*(int)framerate);
            int seconds = (int)Math.Floor((double)frame / framerate) % 60;
            frame = frame - (seconds*(int)framerate);
            int frames = frame % (int)Math.Round(framerate);
            return new byte[] { (byte)hours, (byte)minutes, (byte)seconds, (byte)frames };
        }

        public static string frameToTC_s(int frame, double framerate)
        {
            byte[] tc = frameToTC_b(frame, framerate);
            return String.Format("{0:d2}",(byte)tc[0])+":"+String.Format("{0:d2}",(byte)tc[1])+":"+String.Format("{0:d2}",(byte)tc[2])+":"+String.Format("{0:d2}",(byte)tc[3]);
        }

        public static byte[] changeTimeCode(byte[] header, int frame, int offset, int framerate, bool dropFrame)
        {
            byte[] TC = frameToTC_b(frame, framerate);

            byte[] tmp_bytes = BitConverter.GetBytes(TC[0]);
            header[offset + 3] = setConvertedTC(tmp_bytes[0], false);
            
            tmp_bytes = BitConverter.GetBytes(TC[1]);
            header[offset + 2] = setConvertedTC(tmp_bytes[0],false);

            tmp_bytes = BitConverter.GetBytes(TC[2]);
            header[offset + 1] = setConvertedTC(tmp_bytes[0],false);

            tmp_bytes = BitConverter.GetBytes(TC[3]);
            header[offset] = setConvertedTC(tmp_bytes[0],dropFrame);
           
            return header;
        }

        public static double dateTime2Frame(DateTime dt, double framerate)
        {
            TimeSpan ts = new TimeSpan(dt.Hour,dt.Minute,dt.Second);
            return ts.TotalSeconds* framerate;
        }

        // --- Helper ---

        public static void setListviewStrings(raw r)
        {
            r.ListviewTitle = r.data.fileData.parentSourcePath + " / "+r.data.fileData.fileNameOnly+" - "+r.data.metaData.duration+(r.data.audioData.hasAudio?" (with Audio)":"");
            r.ListviewPropA = r.data.metaData.modell + " | " + r.data.metaData.xResolution + "x" + r.data.metaData.yResolution + "px @ " + r.data.metaData.fpsString + "fps";
            r.ListviewPropB = r.data.metaData.whiteBalance.ToString() + "°K | "+dng.WBpreset[r.data.metaData.whiteBalanceMode] +" - " + r.data.metaData.frames.ToString() + " frames in " + r.data.metaData.splitCount + (r.data.metaData.isMLV ? " mlv" : " raw") + (r.data.metaData.splitCount > 1 ? "-files" : "-file");
            r.ListviewPropC = "recorded " +r.data.fileData.modificationTime +" - TC "+ calc.frameToTC_s((int)calc.dateTime2Frame(r.data.fileData.modificationTime, r.data.metaData.fpsNom / r.data.metaData.fpsDen), (r.data.metaData.fpsNom / r.data.metaData.fpsDen)); 
        }

        public static byte setConvertedTC(int orig, bool dropFrame)
        {
            byte dest;
            int Einer = orig % 10;
            int Zehner = (orig / 10) % 10;
            dest = (byte)(Einer | (Zehner << 4));
            if (dropFrame)
            {
                dest = (byte)(dest + 128);
            }
            return dest;
        }

        public static int GetUInt16(BitArray array)
        {
            int[] value = new int[1];
            array.CopyTo(value, 0);
            return (UInt16)value[0];
        }

        public static unsafe void SwapX2(Byte[] Source)
        {
            fixed (Byte* pSource = &Source[0])
            {
                Byte* bp = pSource;
                Byte* bp_stop = bp + Source.Length;

                while (bp < bp_stop)
                {
                    *(UInt16*)bp = (UInt16)(*bp << 8 | *(bp + 1));
                    bp += 2;
                }
            }
        }

        public static byte ReverseByte(byte inByte)
        {
            byte result = 0x00;
            byte mask = 0x00;

            for (mask = 0x80; Convert.ToInt32(mask) > 0; mask >>= 1)
            {
                result >>= 1;
                byte tempbyte = (byte)(inByte & mask);
                if (tempbyte != 0x00)
                    result |= 0x80;
            }
            return (result);
        }

        public static Int32 reverseInt32(Int32 x)
        {
            x = ((x >> 1) & 0x55555555) | ((x & 0x55555555) << 1);
            x = ((x >> 2) & 0x33333333) | ((x & 0x33333333) << 2);
            x = ((x >> 4) & 0x0f0f0f0f) | ((x & 0x0f0f0f0f) << 4);
            x = ((x >> 8) & 0x00ff00ff) | ((x & 0x00ff00ff) << 8);
            x = ((x >> 16) & 0xffff) | ((x & 0xffff) << 16);
            return x;
        }
        
        public static byte[] reverseBytes(byte[] source)
        {
            long t = 0;
            long len = source.Length;
            byte[] Dest = new Byte[len];
            foreach (byte sb in source)
            {
                Dest[t++] = ReverseByte(sb);
            }
            return Dest;
        }

        public static byte[] int2byteArray(int[] input)
        {
            byte[] output = new byte[input.Length * sizeof(int)];
            Buffer.BlockCopy(input, 0, output, 0, output.Length);
            return output;
        }

        // ----- helper written by ml-community
        // ----- for chroma smoothing

        public static int opt_med5(ref int[] p)
        {
            PIX_SORT(ref p[0],ref p[1]) ; 
            PIX_SORT(ref p[3],ref p[4]) ; 
            PIX_SORT(ref p[0],ref p[3]) ;
            PIX_SORT(ref p[1],ref p[4]) ; 
            PIX_SORT(ref p[1],ref p[2]) ; 
            PIX_SORT(ref p[2],ref p[3]) ;
            PIX_SORT(ref p[1],ref p[2]) ; 
            return p[2];
        }

        public static void PIX_SORT(ref int a,ref int b) 
        { 
            if (a>b) PIX_SWAP(ref a,ref b); 
        }

        public static void PIX_SWAP(ref int a,ref int b) 
        { 
            int temp=a;
            a=b;
            b=temp;
        }

        public static int COERCE(ref int x, ref int lo, ref int hi)
        {
            return Math.Max(Math.Min(x, hi), lo);
        }
        
        public static int ARR_MIN(int[] arr)
        {
            if (arr.Length > 0)
            {
                int min = arr[0];
                for (int i = 1; i < arr.Length; i++)
                {
                    if (arr[i] < min) min = arr[i];
                }
                return min;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static int ARR_MAX(int[] arr)
        {
            if (arr.Length > 0)
            {
                int max = arr[0];
                for (int i = 1; i < arr.Length; i++)
                {
                    if (arr[i] > max) max = arr[i];
                }
                return max;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        // ----- LUTS for faster playback and jpg-conversion
 
        public static int[] Rec709 = new int[130000];

        public static void calculateRec709LUT()
        {
            double temp = 0;
            for (int n = 0; n < Rec709.Length; n++)
            {
                temp = (double)n/65535;
                if (temp < 0.018)
                {
                    temp = temp * 4.5;
                }
                else
                {
                    temp = Math.Pow((temp * 1.099), 0.45) - 0.099;
                }
                if (temp > 1) temp = 1;

                Rec709[n] = (int)(temp * 255) ;
            }
        }

    }
}
