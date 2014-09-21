using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using winIO = System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Diagnostics;

namespace raw2cdng_v2
{
    /// <summary>
    /// Interaktionslogik für mlvplay.xaml
    /// </summary>
    public partial class mlvplay : Window
    {
        public String file { get; set; }
        public raw importRaw {get;set;}
        // -- timertick for _preview
        DispatcherTimer playbackTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        // for realtime changes
        long milliseconds;
        double fps;
        int wbP = 0;
        int wbM = 0;
        double wbGM = 1;
        int originalWB = 0;
        string originalWBText = "";
        int[] originalFractions = new int[6];
        int playbackQuality = 0;
        string output = "";

        public mlvplay()
        {
            InitializeComponent();

            // calculate LUTs
            calc.calculateRec709LUT();

            fps = 0;
            // -- init _preview Tick and small frameProgressLine
            playbackTimer.Tick += new EventHandler(playbackTimer_Tick);
            playbackTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);

        }

        public mlvplay(String file): this()
        {

            //
            if (io.isMLV(file) || io.isRAW(file))
            {
                //if (settings.debugLogEnabled) debugging._saveDebug("[drop] -- file " + file + " will be analyzed now.");
                importRaw = new raw();
                data importData = new data();
                importData.metaData = new metadata();
                importData.fileData = new filedata();
                importData.fileData.tempPath = winIO.Path.GetTempPath();

                importData.threadData = new threaddata();
                importData.lensData = new lensdata();
                importData.audioData = new audiodata();
                importData.metaData.errorString = "";

                importRaw.data = importData;

                if (io.isMLV(file))
                {
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] is MLV ");
                    importRaw.data.metaData.isMLV = true;
                    importRaw.data.metaData.errorString += io.setFileinfoData(file, importRaw.data.fileData);
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] FileinfoData set");
                    importRaw.data.metaData.errorString += io.createMLVBlockList(file, importRaw);
                    Blocks.mlvBlockList = Blocks.mlvBlockList.OrderBy(x => x.timestamp).ToList();
                    importRaw.data.metaData.errorString += io.getMLVAttributes(file, Blocks.mlvBlockList, importRaw);
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] MLV Attributes and Blocklist created and sorted. Blocks: "+Blocks.mlvBlockList.Count);
                    importRaw.AUDFBlocks = null;
                    if (importRaw.data.audioData.hasAudio)
                    {
                        importRaw.AUDFBlocks = Blocks.mlvBlockList.Where(x => x.blockTag == "AUDF").ToList();
                        //if (settings.debugLogEnabled) debugging._saveDebug("[drop] hasAudio. AUDF-List created. Blocks: " + importRaw.AUDFBlocks.Count);
                    }
                    importRaw.VIDFBlocks = null;
                    importRaw.VIDFBlocks = Blocks.mlvBlockList.Where(x => x.blockTag == "VIDF").ToList();
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] VIDF-List created. Blocks: " + importRaw.VIDFBlocks.Count);
                    importRaw.data.metaData.errorString += io.readVIDFBlockData(importRaw);
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] VIDF-Blockdata read and created.");
                    // correct frameCount
                    importRaw.data.metaData.frames = importRaw.VIDFBlocks.Count;

                    importRaw.convert = true;
                }
                if (io.isRAW(file))
                {
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] is RAW ");
                    importRaw.data.metaData.isMLV = false;
                    importRaw.data.audioData.hasAudio = false;
                    importRaw.RAWBlocks = null;
                    io.setFileinfoData(file, importRaw.data.fileData);
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] FileinfoData set");
                    io.getRAWAttributes(file, importRaw);
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] RAW Attributes read and set.");
                    io.createRAWBlockList(file, importRaw);
                    //if (settings.debugLogEnabled) debugging._saveDebug("[drop] RAW Blocklist created and sorted. Blocks: " + importRaw.RAWBlocks.Count);

                    // then Framelist
                    importRaw.convert = true;
                }
                // save file-WB values.
                originalWB = importRaw.data.metaData.whiteBalance;
                originalFractions = importRaw.data.metaData.RGBfraction;
                originalWBText = "(filedata) " + originalWB + "°K | " + dng.WBpreset[importRaw.data.metaData.whiteBalanceMode];

                // save all Properties as String
                importRaw.data.metaData.propertiesString = importRaw.data.metaData.xResolution + "x" + importRaw.data.metaData.yResolution + "px || " + importRaw.data.lensData.isoValue + "ISO | " + importRaw.data.lensData.shutter + " | " + importRaw.data.metaData.fpsString + "fps";

                // set properties of the window
                this.Width = importRaw.data.metaData.xResolution / 2;
                this.Height = importRaw.data.metaData.yResolution / 2;
                _playback.Width = importRaw.data.metaData.xResolution / 2;
                _playback.Height = importRaw.data.metaData.yResolution / 2;
                _playback.Source = null;
                this.Title = "mlvplay " + importRaw.data.fileData.fileNameOnly;
                _wbLabel.Content = originalWBText;
                playbackTimer.Start();
            }

        }
        private void playbackTimer_Tick(object sender, EventArgs e)
        {

            stopwatch.Start();
            importRaw.data.metaData.previewFrame++;
            importRaw.data.metaData.maximize = true;
            importRaw.data.metaData.previewFrame = importRaw.data.metaData.previewFrame % importRaw.data.metaData.frames;
            Task.Factory.StartNew(() => previewBackground(importRaw));
                //if (settings.debugLogEnabled) debugging._saveDebug("[previewTimer_Tick] show previewframe " + r.data.metaData.previewFrame + " from " + r.data.fileData.fileNameOnly);
        }
        
        public void previewBackground(raw r)
        {
            var frame = r.data.metaData.previewFrame;
            if (frame > -1)
            {
                var maxFrames = r.data.metaData.frames;
                var progressPosX = r.data.metaData.xResolution/2 * frame / maxFrames;
                // read picture and show
                r.data.threadData.frame = frame;

                //_preview.Source = im;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    switch (playbackQuality % 4)
                    {
                        case 0:
                            _playback.Source = io.showPicture(r,quality.high709);
                            output = "HQ 709";
                            break;
                        case 1:
                            _playback.Source = io.showPicture(r,quality.highgamma2);
                            output = "HQ gamma 2.0";
                            break;
                        case 2:
                            _playback.Source = io.showPicture(r,quality.lowmullim);
                            output = "LQ mul/lim";
                            break;
                        case 3:
                            _playback.Source = io.showPicture(r,quality.lowgrey);
                            output = "LQ grey";
                            break;

                        default:
                            break;
                    }
                    _playbackLabel.Content = r.data.metaData.propertiesString;
                    _fpsLabel.Content = String.Format("{0:0.00}", fps) + " fps | " + String.Format("{0:d5}", frame) + " of " + maxFrames;
                    _qualityLabel.Content = output;
                    _wbLabel.Content = originalWBText;
                    //_playbackProgressBar.Margin = new Thickness(progressPosX, 0, 0, 0);
                    _playback.InvalidateVisual();
                    _playbackLabel.InvalidateVisual();
                    _fpsLabel.InvalidateVisual();
                }));
            }

            if (frame % 30 == 0)
            {
                stopwatch.Stop();
                milliseconds = stopwatch.ElapsedMilliseconds;
                fps = 30000 / (double)milliseconds;
                stopwatch.Reset();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.O:
                    wbGM = 1;
                    importRaw.data.metaData.RGBfraction = originalFractions;
                    changePreviewWB(importRaw.data);

                    importRaw.data.metaData.whiteBalance = originalWB;
                    originalWBText = "(filedata) " + originalWB + "°K | " + dng.WBpreset[importRaw.data.metaData.whiteBalanceMode];
                    // revert to WB-filevalues..
                    break;
                case Key.P:
                    wbP+=1;
                    wbGM = 1;
                    if (wbP > 5) wbP = 0;
                    importRaw.data.metaData.RGBfraction = calc.convertToFraction(new int[]{dng.whitebalancePresets[wbP][1],dng.whitebalancePresets[wbP][2],dng.whitebalancePresets[wbP][3],dng.whitebalancePresets[wbP][4]});
                    changePreviewWB(importRaw.data);

                    importRaw.data.metaData.whiteBalance = dng.whitebalancePresets[wbP][0];
                    originalWBText = importRaw.data.metaData.whiteBalance + "°K | " + dng.WBpreset[wbP];
                    // change to presets    
                    break;
                case Key.Right:
                    wbM += 1;
                    if (wbM == dng.whitebalanceManual.Length) wbM = 0;
                    importRaw.data.metaData.RGBfraction = calc.convertToFraction(new int[]{dng.whitebalanceManual[wbM][1],dng.whitebalanceManual[wbM][2],dng.whitebalanceManual[wbM][3],dng.whitebalanceManual[wbM][4]});
                    changePreviewWB(importRaw.data);

                    importRaw.data.metaData.whiteBalance = dng.whitebalanceManual[wbM][0];
                    originalWBText = importRaw.data.metaData.whiteBalance + "°K | manual";
                    // increment manual wb    
                    break;
                case Key.Left:
                    wbM -= 1;
                    if (wbM < 0) wbM = dng.whitebalanceManual.Length-1;
                    importRaw.data.metaData.RGBfraction = calc.convertToFraction(new int[]{dng.whitebalanceManual[wbM][1],dng.whitebalanceManual[wbM][2],dng.whitebalanceManual[wbM][3],dng.whitebalanceManual[wbM][4]});
                    changePreviewWB(importRaw.data);

                    importRaw.data.metaData.whiteBalance = dng.whitebalanceManual[wbM][0];
                    originalWBText = importRaw.data.metaData.whiteBalance+"°K | manual";
                    // decrement manual wb    
                    break;
                case Key.Up:
                    if(wbGM<1.25) wbGM+=0.01;
                    importRaw.data.metaData.RGBfraction = calc.convertToFraction(new int[] { (int)(dng.whitebalanceManual[wbM][1] * wbGM), (int)(dng.whitebalanceManual[wbM][2]/wbGM), (int)(dng.whitebalanceManual[wbM][3]/wbGM), (int)(dng.whitebalanceManual[wbM][4] *wbGM) });
                    changePreviewWB(importRaw.data);
                    
                    importRaw.data.metaData.whiteBalance = dng.whitebalanceManual[wbM][0];
                    originalWBText = importRaw.data.metaData.whiteBalance + "°K | manual + GM-Shift "+wbGM;
                    // add gm-shift   
                    break;
                case Key.Down:
                    if(wbGM>0.80) wbGM-=0.01;
                    importRaw.data.metaData.RGBfraction = calc.convertToFraction(new int[] { (int)(dng.whitebalanceManual[wbM][1] * wbGM), (int)(dng.whitebalanceManual[wbM][2] / wbGM), (int)(dng.whitebalanceManual[wbM][3] / wbGM), (int)(dng.whitebalanceManual[wbM][4] * wbGM) });
                    changePreviewWB(importRaw.data);

                    importRaw.data.metaData.whiteBalance = dng.whitebalanceManual[wbM][0];
                    originalWBText = importRaw.data.metaData.whiteBalance + "°K | manual + GM-Shift " + wbGM;
                    // sub gm-shift    
                    break;
                case Key.Q:
                    playbackQuality++;
                    // change Quality on Q
                    break;
                case Key.X:
                    Application.Current.Shutdown();
                    // close on X
                    break;
                case Key.Escape:
                    Application.Current.Shutdown();
                    // close on ESC
                    break;
                default:
                    break;

            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_playback.Width = this.ActualWidth;
            //_playback.Height = this.ActualHeight;

        }

        public static void changePreviewWB(data d)
        {
            d.metaData.wb_B = (double)d.metaData.RGBfraction[0] / (double)d.metaData.RGBfraction[1];
            d.metaData.wb_G = (double)d.metaData.RGBfraction[2] / (double)d.metaData.RGBfraction[3];
            d.metaData.wb_R = (double)d.metaData.RGBfraction[4] / (double)d.metaData.RGBfraction[5];

        }
    }
}
