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
        private const int EV_RESOLUTION = 65536;

        public static int[] ev2raw = new int[24 * EV_RESOLUTION];
        public static int[] raw2ev = new int[EV_RESOLUTION];
        public static int zero = 0;
        public static int full = 65535;

        public static void reinitRAWEVArrays(int black, int white)
        {
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
            
        }

        // --- bitdepth conversion ---

        public static byte[] to16(byte[] source, data rData)
        {
            // preparing variables
            int resx = rData.metaData.xResolution;
            int resy = rData.metaData.yResolution;
            int bl = rData.metaData.blackLevelOld;
            bool maximize = rData.metaData.maximize;
            double maximizer = rData.metaData.maximizer;

            // ------------- and go ----
            int chunks = resx * resy * 14 / 8;
            byte[] Dest = new Byte[chunks / 14 * 16];
            UInt32 tt = 0;
            int senselA, senselB, senselC, senselD, senselE, senselF, senselG, senselH;
            for (var t = 0; t < chunks; t += 14)
            {
                if (maximize == true)
                {
                    senselA = (int)((source[t] >> 2) | (source[t + 1] << 6));
                    senselB = (int)(((source[t] & 0x3) << 12) | (source[t + 3] << 4) | (source[t + 2] >> 4));
                    senselC = (int)(((source[t + 2] & 0x0f) << 10) | (source[t + 5] << 2) | (source[t + 4] >> 6));
                    senselD = (int)(((source[t + 4] & 0x3f) << 8) | (source[t + 7]));
                    senselE = (int)((source[t + 9] >> 2) | (source[t + 6] << 6));
                    senselF = (int)(((source[t + 9] & 0x3) << 12) | (source[t + 8] << 4) | (source[t + 11] >> 4));
                    senselG = (int)(((source[t + 11] & 0x0f) << 10) | (source[t + 10] << 2) | (source[t + 13] >> 6));
                    senselH = (int)(((source[t + 13] & 0x3f) << 8) | (source[t + 12]));

                    // debias sensel
                    senselA = senselA - (int)bl;
                    senselB = senselB - (int)bl;
                    senselC = senselC - (int)bl;
                    senselD = senselD - (int)bl;
                    senselE = senselE - (int)bl;
                    senselF = senselF - (int)bl;
                    senselG = senselG - (int)bl;
                    senselH = senselH - (int)bl;

                    // maximize to 16bit
                    senselA = (int)(senselA * maximizer);
                    senselB = (int)(senselB * maximizer);
                    senselC = (int)(senselC * maximizer);
                    senselD = (int)(senselD * maximizer);
                    senselE = (int)(senselE * maximizer);
                    senselF = (int)(senselF * maximizer);
                    senselG = (int)(senselG * maximizer);
                    senselH = (int)(senselH * maximizer);

                    // do max on overflow
                    if (senselA > 65535) senselA = 65535;
                    if (senselB > 65535) senselB = 65535;
                    if (senselC > 65535) senselC = 65535;
                    if (senselD > 65535) senselD = 65535;
                    if (senselE > 65535) senselE = 65535;
                    if (senselF > 65535) senselF = 65535;
                    if (senselG > 65535) senselG = 65535;
                    if (senselH > 65535) senselH = 65535;

                    // -- react on underflow
                    if (senselA < 0) senselA = 0;
                    if (senselB < 0) senselB = 0;
                    if (senselC < 0) senselC = 0;
                    if (senselD < 0) senselD = 0;
                    if (senselE < 0) senselE = 0;
                    if (senselF < 0) senselF = 0;
                    if (senselG < 0) senselG = 0;
                    if (senselH < 0) senselH = 0;

                }
                else
                {
                    // no maximizing
                    senselA = (int)((source[t] >> 2) | (source[t + 1] << 6));
                    senselB = (int)(((source[t] & 0x3) << 12) | (source[t + 3] << 4) | (source[t + 2] >> 4));
                    senselC = (int)(((source[t + 2] & 0x0f) << 10) | (source[t + 5] << 2) | (source[t + 4] >> 6));
                    senselD = (int)(((source[t + 4] & 0x3f) << 8) | (source[t + 7]));
                    senselE = (int)((source[t + 9] >> 2) | (source[t + 6] << 6));
                    senselF = (int)(((source[t + 9] & 0x3) << 12) | (source[t + 8] << 4) | (source[t + 11] >> 4));
                    senselG = (int)(((source[t + 11] & 0x0f) << 10) | (source[t + 10] << 2) | (source[t + 13] >> 6));
                    senselH = (int)(((source[t + 13] & 0x3f) << 8) | (source[t + 12]));

                }

                Dest[tt++] = (byte)(senselA & 0xff);
                Dest[tt++] = (byte)(senselA >> 8);

                Dest[tt++] = (byte)(senselB & 0xff);
                Dest[tt++] = (byte)(senselB >> 8);

                Dest[tt++] = (byte)(senselC & 0xff);
                Dest[tt++] = (byte)(senselC >> 8);

                Dest[tt++] = (byte)(senselD & 0xff);
                Dest[tt++] = (byte)(senselD >> 8);

                Dest[tt++] = (byte)(senselE & 0xff);
                Dest[tt++] = (byte)(senselE >> 8);

                Dest[tt++] = (byte)(senselF & 0xff);
                Dest[tt++] = (byte)(senselF >> 8);

                Dest[tt++] = (byte)(senselG & 0xff);
                Dest[tt++] = (byte)(senselG >> 8);

                Dest[tt++] = (byte)(senselH & 0xff);
                Dest[tt++] = (byte)(senselH >> 8);

            }
            return Dest;
        }

        public static byte[] from16to12(byte[] source, data rData)
        {
            // preparing variables
            int resx = rData.metaData.xResolution;
            int resy = rData.metaData.yResolution;

            // ------------- and go ----

            int chunks = resx * resy * 16 / 8;
            byte[] Dest = new Byte[chunks / 16 * 12 + 72];
            UInt32 tt = 0;
            int senselA, senselB, senselC, senselD, senselE, senselF, senselG, senselH;
            int senselI, senselJ, senselK, senselL, senselM, senselN, senselO, senselP;
            int senselQ, senselR, senselS, senselT, senselU, senselV, senselW, senselX;

            for (var t = 0; t < chunks; t += 48)
            {
                //read 16bit data and shift 2 bits.
                senselA = (source[t] | (source[t + 1] << 8)) >> 2;  
                senselB = (source[t + 2] | (source[t + 3] << 8)) >> 2;
                senselC = (source[t + 4] | (source[t + 5] << 8)) >> 2;
                senselD = (source[t + 6] | (source[t + 7] << 8)) >> 2;
                senselE = (source[t + 8] | (source[t + 9] << 8)) >> 2;
                senselF = (source[t + 10] | (source[t + 11] << 8)) >> 2;
                senselG = (source[t + 12] | (source[t + 13] << 8)) >> 2;
                senselH = (source[t + 14] | (source[t + 15] << 8)) >> 2;

                senselI = (source[t + 16] | (source[t + 17] << 8)) >> 2;
                senselJ = (source[t + 18] | (source[t + 19] << 8)) >> 2;
                senselK = (source[t + 20] | (source[t + 21] << 8)) >> 2;
                senselL = (source[t + 22] | (source[t + 23] << 8)) >> 2;
                senselM = (source[t + 24] | (source[t + 25] << 8)) >> 2;
                senselN = (source[t + 26] | (source[t + 27] << 8)) >> 2;
                senselO = (source[t + 28] | (source[t + 29] << 8)) >> 2;
                senselP = (source[t + 30] | (source[t + 31] << 8)) >> 2;

                senselQ = (source[t + 32] | (source[t + 33] << 8)) >> 2;
                senselR = (source[t + 34] | (source[t + 35] << 8)) >> 2;
                senselS = (source[t + 36] | (source[t + 37] << 8)) >> 2;
                senselT = (source[t + 38] | (source[t + 39] << 8)) >> 2;
                senselU = (source[t + 40] | (source[t + 41] << 8)) >> 2;
                senselV = (source[t + 42] | (source[t + 43] << 8)) >> 2;
                senselW = (source[t + 44] | (source[t + 45] << 8)) >> 2;
                senselX = (source[t + 46] | (source[t + 47] << 8)) >> 2;

                Dest[tt++] = (byte)((senselA >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselA & 0xF) << 4) | (senselB >> 8));
                Dest[tt++] = (byte)(senselB & 0xff);

                Dest[tt++] = (byte)((senselC >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselC & 0xF) << 4) | (senselD >> 8));
                Dest[tt++] = (byte)(senselD & 0xff);

                Dest[tt++] = (byte)((senselE >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselE & 0xF) << 4) | (senselF >> 8));
                Dest[tt++] = (byte)(senselF & 0xff);
                //9
                Dest[tt++] = (byte)((senselG >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselG & 0xF) << 4) | (senselH >> 8));
                Dest[tt++] = (byte)(senselH & 0xff);

                Dest[tt++] = (byte)((senselI >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselI & 0xF) << 4) | (senselJ >> 8));
                Dest[tt++] = (byte)(senselJ & 0xff);

                Dest[tt++] = (byte)((senselK >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselK & 0xF) << 4) | (senselL >> 8));
                Dest[tt++] = (byte)(senselL & 0xff);
                //18
                Dest[tt++] = (byte)((senselM >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselM & 0xF) << 4) | (senselN >> 8));
                Dest[tt++] = (byte)(senselN & 0xff);

                Dest[tt++] = (byte)((senselO >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselO & 0xF) << 4) | (senselP >> 8));
                Dest[tt++] = (byte)(senselP & 0xff);

                Dest[tt++] = (byte)((senselQ >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselQ & 0xF) << 4) | (senselR >> 8));
                Dest[tt++] = (byte)(senselR & 0xff);
                //27
                Dest[tt++] = (byte)((senselS >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselS & 0xF) << 4) | (senselT >> 8));
                Dest[tt++] = (byte)(senselT & 0xff);

                Dest[tt++] = (byte)((senselU >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselU & 0xF) << 4) | (senselV >> 8));
                Dest[tt++] = (byte)(senselV & 0xff);

                Dest[tt++] = (byte)((senselW >> 4) & 0xff);
                Dest[tt++] = (byte)(((senselW & 0xF) << 4) | (senselX >> 8));
                Dest[tt++] = (byte)(senselX & 0xff);
                //36
            }
            return Dest;
        }

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

        public static byte[] pinkHighlight(byte[] pic, data param)
        {
            int halfResx = param.metaData.xResolution / 2;
            int halfResy = param.metaData.yResolution / 2;
            int whitelevel = (int)(param.metaData.whiteLevelOld * 2.8);
            //int lowWL = whitelevel;

            for (var y = 0; y < halfResy; y++)
            {
                for (var x = 0; x < halfResx; x++)
                {
                    // adress for both rows
                    int rowRG = (x * 2 * 2 + (y * 2 + 0) * halfResx * 4);
                    int rowGB = (x * 2 * 2 + (y * 2 + 1) * halfResx * 4);

                    // sensel-values
                    int r1 = (pic[rowRG] | pic[rowRG + 1] << 8);
                    int g1 = (pic[rowRG + 2] | pic[rowRG + 3] << 8);
                    int g2 = (pic[rowGB] | pic[rowGB + 1] << 8);
                    int b1 = (pic[rowGB + 2] | pic[rowGB + 3] << 8);

                    if (r1 > whitelevel) r1 = whitelevel;
                    //if (g1 > whitelevel) g1 = whitelevel; // whitelevel + (g1 - whitelevel) * 2;
                    //if (g2 > whitelevel) g2 = whitelevel; // whitelevel + (g2 - whitelevel) * 2;
                    if (b1 > whitelevel) b1 = whitelevel;

                    pic[rowRG] = (byte)(r1 & 0xff);
                    pic[rowRG + 1] = (byte)(r1 >> 8);
                    pic[rowRG + 2] = (byte)(g1 & 0xff);
                    pic[rowRG + 3] = (byte)(g1 >> 8);
                    pic[rowGB] = (byte)(g2 & 0xff);
                    pic[rowGB + 1] = (byte)(g2 >> 8);
                    pic[rowGB + 2] = (byte)(b1 & 0xff);
                    pic[rowGB + 3] = (byte)(b1 >> 8);

                }
            }
            return pic;
        }

        public static byte[] verticalBanding(byte[] pic, data param)
        {
            // verticalBanding from ml/a1ex here
            // 16bit byte[] in
            // 16bit byte[] out
            return pic;
        }

        public static byte[] chromaSmoothing(byte[] picIn, data param)
        {
            // chroma Smoothing 2x2 written by a1ex
            // to be found in Magic Lantern / modules / dual_iso / chroma_smooth.c 
            
            // is half recoded and put into sourcecode - but not ready yet.
            //please look into calc.cs as well, there is ev2raw and raw2ev
            
            int CHROMA_SMOOTH_MAX_IJ = 2;
            int CHROMA_SMOOTH_FILTER_SIZE = 5;

            int xres = param.metaData.xResolution;
            int yres = param.metaData.yResolution;
            int xresHalf = xres / 2;
            int yresHalf = yres / 2;

            byte[] picOut = new byte[picIn.Length];

            /*
                        for (int y = 4; y < yres - 5; y += 2)
                        {
                            for (int x = 4; x < xres - 4; x += 2)
                            {
                                int rowRG = (x * 2 * 2 + (y * 2 + 0) * halfResx * 4);
                                int rowGB = (x * 2 * 2 + (y * 2 + 1) * halfResx * 4);
                                
                                int g1 = picIn[x+1 +     y * xres];
                                int g2 = picIn[x   + (y+1) * yres];
                                int ge = (raw2ev[g1] + raw2ev[g2]) / 2;
            
                                // looks ugly in darkness
                                if (ge < 2*EV_RESOLUTION) continue;

                                int i,j;
                                int k = 0;
                                int[] med_r = new int[CHROMA_SMOOTH_FILTER_SIZE];
                                int[] med_b = new int[CHROMA_SMOOTH_FILTER_SIZE];
                                for (i = -CHROMA_SMOOTH_MAX_IJ; i <= CHROMA_SMOOTH_MAX_IJ; i += 2)
                                {
                                    for (j = -CHROMA_SMOOTH_MAX_IJ; j <= CHROMA_SMOOTH_MAX_IJ; j += 2)
                                    {
                                        //#ifdef CHROMA_SMOOTH_2X2
                                        if (Math.Abs(i) + Math.Abs(j) == 4) continue;
                    
                                        int ar  = picIn[x+i   +   (y+j) * xres];
                                        int ag1 = picIn[x+i+1 +   (y+j) * xres];
                                        int ag2 = picIn[x+i   + (y+j+1) * xres];
                                        int ab  = picIn[x+i+1 + (y+j+1) * xres];
                    
                                        int age = (raw2ev[ag1] + raw2ev[ag2]) / 2;
                                        med_r[k] = raw2ev[ar] - age;
                                        med_b[k] = raw2ev[ab] - age;
                                        k++;
                                     }
                                }
                                int dr = opt_med5(ref med_r);
                                int db = opt_med5(ref med_b);

                                if (ge + dr <= EV_RESOLUTION) continue;
                                if (ge + db <= EV_RESOLUTION) continue;

                                int gedr = ge + dr;
                                int gedb = ge + db;
                                int fullArray = 14 * full;

                                picOut[x   +     y * xres] = ev2raw[COERCE(ref gedr, ref zero, ref fullArray)];
                                picOut[x+1 + (y+1) * yres] = ev2raw[COERCE(ge + db, 0, 14*EV_RESOLUTION-1)];
     
                            }
                        }
                         */
            return picOut;
    
        }

        // --- preview helper ---

        public static WriteableBitmap doBitmap(byte[] imageData, data param, bool convert)
        {
            int halfResx = param.metaData.xResolution / 2;
            int halfResy = param.metaData.yResolution / 2;
            int whole = halfResx * halfResy;

            byte[] imageData8 = new byte[halfResx * halfResy * 3];

            int mul = 32;
            double gamma = 0.5;
            
            if (convert) mul = 64;
            
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
                    rowRG = (x * 2 * 2 + (y * 2 + 0) * halfResx * 4);
                    rowGB = (x * 2 * 2 + (y * 2 + 1) * halfResx * 4);

                    b1 = (imageData[rowRG] | imageData[rowRG + 1] << 8) / mul;
                    g1 = (imageData[rowRG + 2] | imageData[rowRG + 3] << 8) / mul;
                    g2 = (imageData[rowGB] | imageData[rowGB + 1] << 8) / mul;
                    r1 = (imageData[rowGB + 2] | imageData[rowGB + 3] << 8) / mul;

                    if (convert)
                    {
                        b1 = (int)((Math.Pow((double)b1 / 256, gamma)) * 256);
                        g1 = (int)((Math.Pow((double)g1 / 256, gamma)) * 324);
                        g2 = (int)((Math.Pow((double)g2 / 256, gamma)) * 324);
                        r1 = (int)((Math.Pow((double)r1 / 256, gamma)) * 256);
                    }
                    

                    bitmapPos = (x + y * halfResx) * 3;

                    gNew = (g1 + g2)/4;

                    imageData8[bitmapPos] = (byte)((r1 > 255) ? 255 : r1);  //(byte)(((r1*2)>255)?255:(r1*2));
                    imageData8[bitmapPos + 1] = (byte)((gNew > 255) ? 255 : gNew);  //(byte)((((g1+g2)/2)>255)?255:((g1+g2)/2));
                    imageData8[bitmapPos + 2] = (byte)((b1 > 255) ? 255 : b1);  //(byte)(((b1 * 2) > 255) ? 255 : (b1 * 2));
                }
            }
            /* ------------------------ this is the debayered png..
            int rMul, gMul, bMul; 
            for (var i = 0; i < whole; i++)
            {
                int r = (int)Math.Floor((double)( i/ resx));
                int c = i - (r * resx);
                bool row = Convert.ToBoolean(r % 2);
                bool column = Convert.ToBoolean(c % 2);

                bmPos = i * 3;

                rMul = 0;
                gMul = 0;
                bMul = 0;
                int greyValue = (int)((imageData[i * 2] + imageData[i * 2 + 1] * 256) / 256);

                if (!column && !row)
                { 
                    // is r1
                    rMul = 1;
                    imageData8[bmPos + 0] = (byte)(greyValue);
                }
                if (column && !row)
                {
                    // is g1
                    gMul = 1;
                    imageData8[bmPos + 1] = (byte)(greyValue);
                }
                if (!column && row)
                {
                    // is g2
                    gMul = 1;
                    imageData8[bmPos + 1] = (byte)(greyValue);
                }
                if (column && row)
                {
                    // is b1
                    bMul = 1;
                    imageData8[bmPos + 2] = (byte)(greyValue);
                }
                
                //greyValue = 255;
                //imageData8[bmPos + 0] = (byte)(greyValue * rMul);
                //imageData8[bmPos + 1] = (byte)(greyValue * gMul);
                //imageData8[bmPos + 2] = (byte)(greyValue * bMul);
            }*/
        
            WriteableBitmap wbm = new WriteableBitmap(halfResx, halfResy, 96, 96, PixelFormats.Bgr24, null);
            wbm.WritePixels(new Int32Rect(0, 0, halfResx, halfResy), imageData8, 3*halfResx, 0);
            
            //imageData8 = null;
            return wbm;
        }

        // --- dng-tag helper ---
        
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
            int frames = frame % (int)Math.Round(framerate);
            int seconds = (int)Math.Floor((double)frame / framerate) % 60;
            int minutes = (int)Math.Floor((double)frame / framerate / 60);
            int hours = (int)Math.Floor((double)frame / framerate / 1440);
            return String.Format("{0:d2}",(byte)hours)+":"+String.Format("{0:d2}",(byte)minutes)+":"+String.Format("{0:d2}",(byte)seconds)+":"+String.Format("{0:d2}",(byte)frames);

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

        public static double creationTime2Frame(DateTime dt, double framerate)
        {
            TimeSpan ts = new TimeSpan(dt.Hour,dt.Minute,dt.Second);
            return ts.TotalSeconds* framerate;
        }

        // --- Helper ---

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

        // helper written by ml-community

        public static int opt_med5(ref int[] p)
        {
            PIX_SORT(ref p[0],ref p[1]) ; 
            PIX_SORT(ref p[3],ref p[4]) ; 
            PIX_SORT(ref p[0],ref p[3]) ;
            PIX_SORT(ref p[1],ref p[4]) ; 
            PIX_SORT(ref p[1],ref p[2]) ; 
            PIX_SORT(ref p[2],ref p[3]) ;
            PIX_SORT(ref p[1],ref p[2]) ; 
            return(p[2]) ;
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
    }
}
