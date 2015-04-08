using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace raw2cdng_v2
{
    class dng
    {
        public static byte[] DNGtemplate = Properties.Resources.dngtemplate20;

        // strings to the WB-presets
        public static string[] WBpreset = new string[] { "AWB", "Daylight", "Cloudy", "Tungsten", "Fluorescent", "Flash", "Manual WB", "", "Shade", "Manual °K" };

        // Whitebalance Data are extracted from 5DIII
        public static UInt16[][] whitebalancePresets = new UInt16[][] {
            new UInt16[]{4500,1896,1024,1024,1872}, // 0=Auto (took 4500°K) r and b are nearly same
            new UInt16[]{5200,2052,1024,1024,1678}, // 1=Daylight
            new UInt16[]{6000,2208,1024,1024,1544}, // 2=Cloudy
            new UInt16[]{3200,1479,1024,1024,2497}, // 3=Tungsten
            new UInt16[]{3714,1796,1024,1024,2399}, // 4=Fluorescent
            new UInt16[]{6320,2284,1024,1024,1520}, // 5=Flash
            new UInt16[]{0},                        // 6=fillup (its custom)
            new UInt16[]{0},                        // 7=fillup (not used)
            new UInt16[]{7000,2362,1024,1024,1431}  // 8=Shade
        };
        // for 6 and 9
        public static UInt16[][] whitebalanceManual = new UInt16[][] {
            new UInt16[]{2500,1174,1024,1024,3168},
            new UInt16[]{2600,1218,1024,1024,3057},
            new UInt16[]{2700,1265,1024,1024,2954},
            new UInt16[]{2800,1316,1024,1024,2849},
            new UInt16[]{2900,1358,1024,1024,2781},
            new UInt16[]{3000,1402,1024,1024,2717},
            new UInt16[]{3100,1440,1024,1024,2602},
            new UInt16[]{3200,1479,1024,1024,2497},
            new UInt16[]{3300,1515,1024,1024,2427},
            new UInt16[]{3400,1553,1024,1024,2356},
            new UInt16[]{3500,1591,1024,1024,2289},
            new UInt16[]{3600,1626,1024,1024,2236},
            new UInt16[]{3700,1659,1024,1024,2180},
            new UInt16[]{3800,1694,1024,1024,2127},
            new UInt16[]{3900,1725,1024,1024,2085},
            new UInt16[]{4000,1753,1024,1024,2044},
            new UInt16[]{4100,1786,1024,1024,2005},
            new UInt16[]{4200,1817,1024,1024,1967},
            new UInt16[]{4300,1843,1024,1024,1935},
            new UInt16[]{4400,1869,1024,1024,1903},
            new UInt16[]{4500,1896,1024,1024,1872},
            new UInt16[]{4600,1924,1024,1024,1843},
            new UInt16[]{4700,1949,1024,1024,1811},
            new UInt16[]{4800,1971,1024,1024,1783},
            new UInt16[]{4900,1990,1024,1024,1756},
            new UInt16[]{5000,2013,1024,1024,1730},
            new UInt16[]{5100,2032,1024,1024,1705},
            new UInt16[]{5200,2052,1024,1024,1678},
            new UInt16[]{5300,2072,1024,1024,1659},
            new UInt16[]{5400,2093,1024,1024,1641},
            new UInt16[]{5500,2114,1024,1024,1623},
            new UInt16[]{5600,2136,1024,1024,1606},
            new UInt16[]{5700,2153,1024,1024,1591},
            new UInt16[]{5800,2171,1024,1024,1574},
            new UInt16[]{5900,2189,1024,1024,1560},
            new UInt16[]{6000,2208,1024,1024,1544},
            new UInt16[]{6100,2226,1024,1024,1533},
            new UInt16[]{6200,2241,1024,1024,1522},
            new UInt16[]{6300,2255,1024,1024,1509},
            new UInt16[]{6400,2270,1024,1024,1498},
            new UInt16[]{6500,2284,1024,1024,1485},
            new UInt16[]{6600,2300,1024,1024,1475},
            new UInt16[]{6700,2315,1024,1024,1464},
            new UInt16[]{6800,2330,1024,1024,1452},
            new UInt16[]{6900,2346,1024,1024,1442},
            new UInt16[]{7000,2362,1024,1024,1431},
            new UInt16[]{7100,2378,1024,1024,1423},
            new UInt16[]{7200,2389,1024,1024,1415},
            new UInt16[]{7300,2399,1024,1024,1407},
            new UInt16[]{7400,2411,1024,1024,1398},
            new UInt16[]{7500,2422,1024,1024,1391},
            new UInt16[]{7600,2433,1024,1024,1383},
            new UInt16[]{7700,2450,1024,1024,1374},
            new UInt16[]{7800,2461,1024,1024,1367},
            new UInt16[]{7900,2473,1024,1024,1360},
            new UInt16[]{8000,2485,1024,1024,1351},
            new UInt16[]{8100,2497,1024,1024,1344},
            new UInt16[]{8200,2509,1024,1024,1337},
            new UInt16[]{8300,2521,1024,1024,1329},
            new UInt16[]{8400,2533,1024,1024,1324},
            new UInt16[]{8500,2539,1024,1024,1319},
            new UInt16[]{8600,2551,1024,1024,1314},
            new UInt16[]{8700,2558,1024,1024,1307},
            new UInt16[]{8800,2564,1024,1024,1303},
            new UInt16[]{8900,2576,1024,1024,1298},
            new UInt16[]{9000,2583,1024,1024,1291},
            new UInt16[]{9100,2589,1024,1024,1287},
            new UInt16[]{9200,2602,1024,1024,1282},
            new UInt16[]{9300,2608,1024,1024,1277},
            new UInt16[]{9400,2615,1024,1024,1271},
            new UInt16[]{9500,2628,1024,1024,1266},
            new UInt16[]{9600,2635,1024,1024,1262},
            new UInt16[]{9700,2641,1024,1024,1256},
            new UInt16[]{9800,2655,1024,1024,1251},
            new UInt16[]{9900,2661,1024,1024,1247},
            new UInt16[]{10000,2668,1024,1024,1241}
        };
        
        public static byte[] setDNGHeader(data Data)
        {
            // -- change data in template

            // -- xres --
            byte[] tmp_bytes = BitConverter.GetBytes(Data.metaData.xResolution);
            DNGtemplate[0x1e] = tmp_bytes[0];
            DNGtemplate[0x1f] = tmp_bytes[1];
            // -- xres to Defaultcropsize
            DNGtemplate[0x1b6a] = tmp_bytes[0];
            DNGtemplate[0x1b6b] = tmp_bytes[1];
            // -- xres to ActiveArea
            DNGtemplate[0x1d1c] = tmp_bytes[0];
            DNGtemplate[0x1d1d] = tmp_bytes[1];

            // -- yres --
            tmp_bytes = BitConverter.GetBytes(Data.metaData.yResolution);
            DNGtemplate[0x2a] = tmp_bytes[0];
            DNGtemplate[0x2b] = tmp_bytes[1];
            // -- yres to Rows per Strip
            DNGtemplate[0x96] = tmp_bytes[0];
            DNGtemplate[0x97] = tmp_bytes[1];
            // -- yres to Defaultcropsize
            DNGtemplate[0x1b72] = tmp_bytes[0];
            DNGtemplate[0x1b73] = tmp_bytes[1];
            // -- yres to ActiveArea
            DNGtemplate[0x1d18] = tmp_bytes[0];
            DNGtemplate[0x1d19] = tmp_bytes[1];

            // -- stripByteCounts --
            tmp_bytes = BitConverter.GetBytes(Data.metaData.stripByteCountReal / Data.metaData.bitsperSample * Data.metaData.bitsperSampleChanged);
            DNGtemplate[0xa2] = tmp_bytes[0];
            DNGtemplate[0xa3] = tmp_bytes[1];
            DNGtemplate[0xa4] = tmp_bytes[2];
            DNGtemplate[0xa5] = tmp_bytes[3];

            // -- Default CropOrigin to Zero--
            DNGtemplate[0x1b5a] = 0;
            DNGtemplate[0x1b5b] = 0;
            DNGtemplate[0x1b62] = 0;
            DNGtemplate[0x1b63] = 0;

            // -- Blacklevel change
            DNGtemplate[0x138] = 04;
            // manual set of blacklevel
            tmp_bytes = BitConverter.GetBytes(Data.metaData.blackLevelNew);
            DNGtemplate[0x13e] = tmp_bytes[0];
            DNGtemplate[0x13f] = tmp_bytes[1];

            // -- Whitelevel change
            DNGtemplate[0x144] = 04;
            tmp_bytes = BitConverter.GetBytes(Data.metaData.whiteLevelNew);
            DNGtemplate[0x14a] = tmp_bytes[0];
            DNGtemplate[0x14b] = tmp_bytes[1];


            //Bits per Sample = 10 12 14 or 16
            DNGtemplate[0x36] = (byte)Data.metaData.bitsperSampleChanged;
            DNGtemplate[0x37] = 0;

            // Whitebalance from MLV/RAW/CR2 - c627 and c628
            if (Data.metaData.whiteBalance!=0)
            {
                // RGGBValues 
                // -- first. erase values

                // R
                Array.Copy(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, DNGtemplate, 0x1cb2, 8);

                // G
                Array.Copy(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, DNGtemplate, 0x1cba, 8);

                // B
                Array.Copy(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, DNGtemplate, 0x1cc2, 8);

                // now set new Values

                // R
                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[0]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cb2, 2);

                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[1]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cb6, 2);

                // G
                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[2]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cba, 2);

                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[3]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cbe, 2);

                // B
                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[4]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cc2, 2);

                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[5]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cc6, 2);
            }
            if (Data.metaData.photoRAW == true)
            {
                // RGGBValues 
                // -- first. erase values

                // R
                Array.Copy(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, DNGtemplate, 0x1cb2, 8);

                // G
                Array.Copy(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, DNGtemplate, 0x1cba, 8);

                // B
                Array.Copy(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, DNGtemplate, 0x1cc2, 8);

                // now set new Values

                // R
                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[0]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cb2, 2);

                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[1]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cb6, 2);

                // G
                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[2]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cba, 2);

                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[3]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cbe, 2);

                // B
                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[4]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cc2, 2);

                tmp_bytes = BitConverter.GetBytes(Data.metaData.RGBfraction[5]);
                Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1cc6, 2);

            }

            // colormatrixA
            Data.metaData.colorMatrixA.CopyTo(DNGtemplate, 0x1b7a);
            // colormatrixB
            Data.metaData.colorMatrixB.CopyTo(DNGtemplate, 0x1bc2);
            // forwardmatrixA
            Data.metaData.forwardMatrixA.CopyTo(DNGtemplate, 0x1d20);
            // forwardmatrixB
            Data.metaData.forwardMatrixB.CopyTo(DNGtemplate, 0x1d68);
            
            // REELNAME
            byte[] reelNameArray = System.Text.Encoding.UTF8.GetBytes(Data.fileData.fileNameShort);
            reelNameArray.CopyTo(DNGtemplate, 0x1db0);

            //UniqueName length
            if (Data.metaData.modell.Length > 36) Data.metaData.modell = Data.metaData.modell.Substring(0, 36);
            DNGtemplate[0x62] = (byte)(Data.metaData.modell.Length + 1);
            DNGtemplate[0x10a] = (byte)(Data.metaData.modell.Length + 1);
            
            //UniqueName
            byte[] modellNameA = new Byte[36];
            for (var i = 0; i < modellNameA.Length; i++) modellNameA[i] = 0;
            System.Text.Encoding.UTF8.GetBytes(Data.metaData.modell).CopyTo(modellNameA, 0);
            modellNameA.CopyTo(DNGtemplate, 0x1dcc);
            modellNameA.CopyTo(DNGtemplate, 0x1dfc);

            // framerate
            DNGtemplate[0x270] = 0x0a; //instead 05 -> 0a
            tmp_bytes = BitConverter.GetBytes(Data.metaData.fpsNom);
            Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1dc2, 4);

            tmp_bytes = BitConverter.GetBytes(Data.metaData.fpsDen);
            Array.Copy(tmp_bytes, 0, DNGtemplate, 0x1dc6, 4);

            //camID as serialNumber
            byte[] serial = new Byte[13];
            for (var i = 0; i < serial.Length; i++) serial[i] = 0;
            System.Text.Encoding.ASCII.GetBytes(Data.metaData.camId).CopyTo(serial, 0);
            serial.CopyTo(DNGtemplate, 0x1cea);
            
            return DNGtemplate;
        }


    }
}
