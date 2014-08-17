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

        public mlvplay()
        {
            InitializeComponent();
        
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

                    importRaw.data.fileData.convertIt = true;
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
                    importRaw.data.fileData.convertIt = true;
                }

                // save all Properties as String
                importRaw.data.metaData.propertiesString = importRaw.data.metaData.xResolution+"x"+importRaw.data.metaData.yResolution+"px || "+importRaw.data.lensData.isoValue+"ISO | "+importRaw.data.lensData.shutter+" | "+importRaw.data.metaData.whiteBalance+"°K | "+importRaw.data.metaData.fpsString+"fps";

                // set properties of the window
                _playback.Width = importRaw.data.metaData.xResolution / 2;
                _playback.Height = importRaw.data.metaData.yResolution / 2;
                _playback.Source = null;
                this.Title = "mlvplay " + importRaw.data.fileData.fileNameOnly;
                playbackTimer.Start();
            }

        }
        private void playbackTimer_Tick(object sender, EventArgs e)
        {
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
                    _playback.Source = io.showPicture(r);
                    _playbackLabel.Content = r.data.metaData.propertiesString+" || "+String.Format("{0:d5}", frame)+" of "+maxFrames;
                    //_playbackProgressBar.Margin = new Thickness(progressPosX, 0, 0, 0);
                    _playback.InvalidateVisual();
                    _playbackLabel.InvalidateVisual();
                }));
            }
        }
    }
}
