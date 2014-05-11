using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace raw2cdng_v2
{
    class dng
    {
        public static byte[] DNGtemplate = Properties.Resources.dngtemplate20;
        
        public static byte[] setDNGHeader(raw Data)
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

            // colormatrix1
            Data.metaData.colorMatrix.CopyTo(DNGtemplate, 0x1b7a);
            // colormatrix2
            Data.metaData.colorMatrix.CopyTo(DNGtemplate, 0x1bc2);

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
