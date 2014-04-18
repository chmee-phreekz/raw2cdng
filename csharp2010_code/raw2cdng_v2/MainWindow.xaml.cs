using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using winIO = System.IO;
using winForm = System.Windows.Forms;

/*
 * Copyright (C) 2014 chmee@phreekz
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the
 * Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */


namespace raw2cdng_v2
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        // -- timertick for _preview
        DispatcherTimer previewTimer = new DispatcherTimer();

        // -- old folderBrowserdialog
        System.Windows.Forms.FolderBrowserDialog _selectFolder = new System.Windows.Forms.FolderBrowserDialog();

        List<raw> rawFiles = new List<raw>();

        int allFramesCount;
        int CPUcores;

        string version = "1.5.0BETA2";

        // baseSettings for convert
        convertSettings convertData = new convertSettings()
        {
            bitdepth = 16,
            chromaSmoothing = false,
            format = "CDNG",
            maximize = false,
            maximizeValue = 1,
            pinkHighlight = false,
            proxyJpegs = false,
            verticalBanding = false
        };

        // settings load/save/change
        appSettings settings = new appSettings();
        bool toggleSettingsSave = false;



        public MainWindow()
        {
            InitializeComponent();

            _title.Text = "rawcdng " + version;
            allFramesCount = 0;

            CPUcores = Environment.ProcessorCount;
            _cores.Content = CPUcores.ToString();

            // -- init _preview Tick and small frameProgressLine
            previewTimer.Tick += new EventHandler(previewTimer_Tick);
            previewTimer.Interval = new TimeSpan(0,0,0,0,40);
            _previewProgressBar.Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            // ui init
            _progressAll.Value = 0;
            _progressOne.Value = 0;

            // ---- load settings from file ----
            loadSettings();

            // debug log
            debugging.debugLogFilename = Environment.CurrentDirectory + winIO.Path.DirectorySeparatorChar + "raw2cdng.2.debug.log";
            debugging._saveDebug (" ------------- "+version+" started");
        }

        // --- the important three events ---------------------

        private void batchList_Drop(object sender, DragEventArgs e)
        {
            // -- dragdrop files
            // -- prepare metadata
            // -- put into listview


            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // --- list of dropped files
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (io.isMLV(file) || io.isRAW(file))
                    {
                        raw importRaw = new raw();
                        importRaw.metaData = new rawdata();
                        importRaw.fileData = new filedata();
                        importRaw.threadData = new threaddata();
                        importRaw.lensData = new lensdata();
                        // write versionstring into author-tag
                        importRaw.metaData.version = version;

                        if (io.isMLV(file))
                        {
                            importRaw.metaData.isMLV = true;
                            io.setFileinfoData(file, importRaw.fileData);
                            io.createMLVBlockList(file, importRaw);
                            Blocks.mlvBlockList = Blocks.mlvBlockList.OrderBy(x => x.timestamp).ToList();
                            io.getMLVAttributes(file, Blocks.mlvBlockList, importRaw);
                            importRaw.metaData.AUDFBlocks = null;
                            if (importRaw.metaData.hasAudio)
                            {
                                importRaw.metaData.AUDFBlocks = Blocks.mlvBlockList.Where(x => x.blockTag == "AUDF").ToList();
                            }
                            importRaw.metaData.VIDFBlocks = null;
                            importRaw.metaData.VIDFBlocks = Blocks.mlvBlockList.Where(x => x.blockTag == "VIDF").ToList();
                            io.readVIDFBlockData(importRaw);
                            // correct frameCount
                            importRaw.metaData.frames = importRaw.metaData.VIDFBlocks.Count;

                            importRaw.fileData.convertIt = true;
                        }
                        if (io.isRAW(file))
                        {
                            importRaw.metaData.isMLV = false;
                            importRaw.metaData.hasAudio = false;
                            io.setFileinfoData(file, importRaw.fileData);
                            io.getRAWAttributes(file, importRaw);
                            io.createRAWBlockList(file, importRaw);
                            importRaw.metaData.RAWBlocks = Blocks.rawBlockList;
                            // then Framelist

                            importRaw.fileData.convertIt = true;
                        }
                        // check errors
                        // ..to be done..

                        // now set item
                        _batchList.Items.Add(new lvItem
                        {
                            //convert = true,
                            type = importRaw.metaData.isMLV ? "MLV" : "RAW",
                            filename = importRaw.fileData.fileNameOnly,
                            files = importRaw.metaData.splitCount.ToString(),
                            frames = importRaw.metaData.frames.ToString(),
                            duration = calc.frameToTC_s(importRaw.metaData.frames, (importRaw.metaData.fpsNom / importRaw.metaData.fpsDen)),
                            resolution = importRaw.metaData.xResolution.ToString() + "x" + importRaw.metaData.yResolution.ToString(),
                            fps = importRaw.metaData.fpsString,
                            audio = importRaw.metaData.hasAudio ? "\u2714" : "\u2715"
                        });

                        // and save raw into list
                        rawFiles.Add(importRaw);
                        _convert.IsEnabled = true;

                        debugging._saveDebug(" Item " + importRaw.fileData.fileNameOnly + " imported."+ (importRaw.metaData.isMLV? "MLV":"RAW"));
                        debugging._saveDebug(" ---- " + importRaw.metaData.xResolution + "x" + importRaw.metaData.yResolution + "px " + importRaw.metaData.fpsString + "fps " + importRaw.metaData.frames + " frames BL" + importRaw.metaData.blackLevelOld + " WL" + importRaw.metaData.whiteLevelOld);
                    }
                }
            }
        }

        private void _convert_Click(object sender, RoutedEventArgs e)
        {
            // stop preview if running
            if (previewTimer.IsEnabled)
            {
                previewTimer.Stop();
                previewTimer.IsEnabled = false;
            }
            // doing the work as a thread, leave GUI fluid
            ThreadPool.QueueUserWorkItem(doWork);
        }

        private void doWork(object state)
        {
            debugging._saveDebug("start converting to DNG");

            allFramesCount = 0;
            // -- count all frames for progressbarAll --
            foreach (raw file in rawFiles)
            {
                allFramesCount += file.metaData.frames;
            }

            debugging._saveDebug("all in all there are "+allFramesCount+" frames to convert");

            // some GUI-Things - progressbar
            this.Dispatcher.Invoke((Action)(() =>
               {
                   _progressAll.Maximum = allFramesCount;
                   _progressAll.Value = 0;
                   _convert.Content = "converting";
               }));
            allFramesCount = 0;
            
            // used for GUI refresh item in _batchList
            int itemList = 0;

            // set threadpool properties
            ThreadPool.SetMaxThreads(CPUcores, CPUcores);

            debugging._saveDebug(CPUcores + " Cores used");


            // and go.
            foreach (raw file in rawFiles)
            {
                
                debugging._saveDebug(" --> converting item " + file.fileData.fileNameOnly);

                 this.Dispatcher.Invoke((Action)(() =>
                {
                    // -- refresh _progressbar.One
                    _progressOne.Value = 0;
                    _progressOne.Maximum = file.metaData.frames;

                    // -- mark item as converting (red)
                    //_batchList.SelectedItem = itemList;
                    // does not work, wpf styles and containerchange-refreshes i think..
                }));

                // copy properties from GUI into rawobject
                file.convertData = convertData;
                // if maximized use the multiplier
                file.metaData.maximizer = (Math.Pow(2, convertData.bitdepth)-1) / (file.metaData.whiteLevelOld - file.metaData.blackLevelOld);

                // set new blacklevel whitelevel and bitdepth
                switch (settings.format)
                {
                    case 1:
                        // 16 bit normal
                        file.metaData.blackLevelNew = file.metaData.blackLevelOld;
                        file.metaData.whiteLevelNew = file.metaData.whiteLevelOld;
                        file.metaData.bitsperSampleChanged = 16;
                        file.metaData.maximize = false;
                        break;
                    case 2:
                        // 16 bit maximized
                        file.metaData.blackLevelNew = 0;
                        file.metaData.whiteLevelNew = 65535;
                        file.metaData.bitsperSampleChanged = 16;
                        file.metaData.maximize = true;
                        file.metaData.maximizer = file.metaData.whiteLevelNew / (file.metaData.whiteLevelOld - file.metaData.blackLevelOld);
                        break;
                    case 3:
                        // 12bit normal
                        file.metaData.blackLevelNew = file.metaData.blackLevelOld/4;
                        file.metaData.whiteLevelNew = file.metaData.whiteLevelOld/4;
                        file.metaData.bitsperSampleChanged = 12;
                        file.metaData.maximize = false;
                        break;
                    case 4:
                        // 12 bit maximized
                        file.metaData.blackLevelNew = 0;
                        file.metaData.whiteLevelNew = 4095;
                        file.metaData.bitsperSampleChanged = 12;
                        file.metaData.maximize = true;
                        break;
                    default:
                        file.metaData.blackLevelNew = file.metaData.blackLevelOld;
                        file.metaData.whiteLevelNew = file.metaData.whiteLevelOld;
                        file.metaData.bitsperSampleChanged = 16;
                        file.metaData.maximize = false;
                        break;
                }

                // prepare prefix variables
                string date = string.Format("{0:yyMMdd}", file.fileData.creationTime);
                string time = string.Format("{0:HHmmss}", file.fileData.creationTime);
                string parentSourcePath = file.fileData.sourcePath.Split(winIO.Path.DirectorySeparatorChar).Last();
                string bitdepth = file.metaData.bitsperSampleChanged.ToString();

                // set filename from prefix-generator
                file.fileData.outputFilename = settings.prefix.
                    Replace("[D]", date).
                    Replace("[T]", time).
                    Replace("[S]", file.fileData.fileNameShort).
                    Replace("[P]", parentSourcePath).
                    Replace("[B]", bitdepth).
                    Replace("[F]", file.fileData.fileNameOnly);
                // cut parenthesis-content
                // its for the filesequences
                if (file.fileData.outputFilename.IndexOf("(") > -1)
                {
                    file.fileData.destinationPath = file.fileData.outputFilename.Substring(0,file.fileData.outputFilename.IndexOf("("));
                    file.fileData.outputFilename = file.fileData.outputFilename.Replace("(", "").Replace(")", "");
                }
                else
                {
                    file.fileData.destinationPath = file.fileData.outputFilename;
                }

                // source or selected Path?
                if (settings.sourcePath)
                {
                    file.fileData.basePath = file.fileData.sourcePath;
                }
                else
                {
                    file.fileData.basePath = settings.outputPath;
                }

                // check/make destination path
                winIO.Directory.CreateDirectory(file.fileData.basePath + winIO.Path.DirectorySeparatorChar + file.fileData.destinationPath);

                // set dngheader
                // dngheader is a fileresource (DNGtemplate20)
                file.metaData.DNGHeader = dng.setDNGHeader(file);

                // destinationPath as its used in the threads
                file.fileData._changedPath = file.fileData.basePath + winIO.Path.DirectorySeparatorChar + file.fileData.destinationPath + winIO.Path.DirectorySeparatorChar;

                // init for multithreading
                int frameCount;
                frameCount = file.metaData.frames;
                int taskCount = frameCount;
                var cde = new CountdownEvent(taskCount);

                // start frameconvert
                for (int f = 0; f < frameCount; f++)
                {
                    //multithread
                    if (file.metaData.isMLV)
                    {
                        //its MLV
                        file.fileData.VIDFBlock = file.metaData.VIDFBlocks[f];
                        file.threadData.frame = file.fileData.VIDFBlock.MLVFrameNo;
                    }
                    else
                    {
                        // its RAW
                        file.fileData.RAWBlock = file.metaData.RAWBlocks[f];
                        file.threadData.frame = f;
                    }
                    raw para = new raw();
                    para = file.Copy(); // deep copy object from ObjectExtensions.cs

                    para.threadData.CDEvent = cde;

                    ThreadPool.QueueUserWorkItem(new WaitCallback(doFrame_Thread), para);
                    para = null;                    
                }
                // wait till all threads has ended
                cde.Wait();

                // now finally audio if existent
                if (file.metaData.hasAudio)
                {
                    io.saveAudio(file.fileData._changedPath + file.fileData.destinationPath + ".wav", file);
                }
                this.Dispatcher.Invoke((Action)(() =>
                {
                    // -- mark item as converted (green)
                    //_batchList.SelectedItem = itemList;
                    // does not work, wpf styles and containerchange-refreshes i think..
                }));
            }
            // clear GUI and some variables
            this.Dispatcher.Invoke((Action)(() =>
                {
                    _batchList.Items.Clear();
                    rawFiles.Clear();

                    _progressOne.Value = 0;
                    _progressAll.Value = 0;
                    _convert.IsEnabled = false;
                    _convert.Content = "convert";
                }));
        }

        private void doFrame_Thread(object state)
        {
            raw param = new raw();
            param = (raw)state;

            if (param.metaData.isMLV) param.rawData = io.readMLV(param);
            else param.rawData = io.readRAW(param);

            byte[] rawDataChanged;

            // -- prepare DNG output

            string justFilename = param.fileData._changedPath + param.fileData.outputFilename + string.Format("{0,5:D5}", param.threadData.frame);
            string finalOutputFilename = justFilename + ".dng";

            // write Timecode into dng
            param.metaData.DNGHeader = calc.changeTimeCode(param.metaData.DNGHeader, param.threadData.frame, 0x1db8, (int)(param.metaData.fpsNom / param.metaData.fpsDen), param.metaData.dropFrame);

            byte[] fillUp = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] tempRaw = new byte[fillUp.Length + param.rawData.Length];

            // workaround for 12bit, because first module needs 16bit-data
            if (param.metaData.bitsperSampleChanged == 12)
            {
                param.metaData.bitsperSampleChanged = 16;
                param.metaData.bitsperSample = 14;
                param.metaData.blackLevelNew = param.metaData.blackLevelOld;
                param.metaData.whiteLevelNew = param.metaData.whiteLevelOld;
                if (param.metaData.maximize)
                {
                    param.metaData.maximizer = 65535 / (param.metaData.whiteLevelOld - param.metaData.blackLevelOld);
                    param.metaData.blackLevelNew = 0;
                    param.metaData.whiteLevelNew = 65535;
                }
            }

            // ------- here's the magic - converting the data --------
            rawDataChanged = calc.to16(param.rawData, param);
            
            // if verticalBanding
            if (param.convertData.verticalBanding)
            {
                //if using chroma Smoothing, recalculate ev2raw/raw2ev
                // can be possibly done one time in the beginning.
                calc.reinitRAWEVArrays(param.metaData.blackLevelOld);
                rawDataChanged = calc.verticalBanding(rawDataChanged, param);
            }
            // if chroma Smoothing
            if (param.convertData.chromaSmoothing) rawDataChanged = calc.chromaSmoothing(rawDataChanged, param);

            // if pink Highlights
            if (param.convertData.pinkHighlight) rawDataChanged = calc.pinkHighlight(rawDataChanged, param);
            
            // if 12bit        
            if (param.convertData.bitdepth == 12) rawDataChanged = calc.from16to12(rawDataChanged, param);


            // -------- write the dng-header --
            using (System.IO.FileStream stream = new System.IO.FileStream(finalOutputFilename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
            {
                stream.Write(param.metaData.DNGHeader, 0, param.metaData.DNGHeader.Length);
            }

            // -- write the bytearray - converted raw
            using (System.IO.FileStream stream = new System.IO.FileStream(finalOutputFilename, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
            {
                stream.Write(rawDataChanged, 0, rawDataChanged.Length);
            }

            updateUI(1, param);

            allFramesCount++;
            param.threadData.CDEvent.Signal();

        }


        // --- GUI events ---------------------

        private void updateUI(byte kindOf, raw p)
        {
            if (kindOf == 1) // update after Frame
            {
                //p.GUIData.ProgressBarValSingle = (Single)p.threadData.frame / p.metaData.frames;
                //p.GUIData.ProgressBarValAll = (Single)allFramesCount / allFramesCount;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    _progressOne.Value = (int)p.threadData.frame;
                    _progressAll.Value = (int)allFramesCount;
                }));
            }
        }

        private void _exit_Click(object sender, RoutedEventArgs e)
        {
            // -- exit button --
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // -- enable window move --
            this.DragMove();
        }


        private void batchList_Click(object sender, RoutedEventArgs e)
        {
            int item = (sender as ListView).Items.IndexOf((sender as ListView).SelectedItems[0]);
            if (item != null)
            {
                // read picture and show
                WriteableBitmap im = io.showPicture(rawFiles[item]);
                _preview.Source = im;
                _lensLabel.Content = String.Format(
                    "{0} | {1} | ISO{2} | f/{3}",
                    rawFiles[item].lensData.lens,
                    rawFiles[item].lensData.shutter, 
                    rawFiles[item].lensData.isoValue, 
                    ((double)rawFiles[item].lensData.aperture / (double)100),
                    CultureInfo.InvariantCulture
                    ); 
            }
        }

        private void _preview_MouseEnter(object sender, MouseEventArgs e)
        {
                        
            previewTimer.Start();
            _previewProgressBar.Stroke = new SolidColorBrush(Color.FromRgb(0, 255, 0));

        }

        private void _preview_MouseLeave(object sender, MouseEventArgs e)
        {
            previewTimer.Stop();
            _previewProgressBar.Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        private void _takePath_Click(object sender, RoutedEventArgs e)
        {
            if (_selectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.outputPath = _selectFolder.SelectedPath;
                settings.Save();
                if (_selectFolder.SelectedPath.Length > 35)
                {
                    _takePath.Content = ".." + _selectFolder.SelectedPath.Substring(_selectFolder.SelectedPath.Length - 33);
                }
                else
                {
                    _takePath.Content = _selectFolder.SelectedPath;
                }
                settings.sourcePath = false;
            }
            else
            {
                _takePath.Content = "select destination path";
                _noPath.IsChecked = true;
                settings.sourcePath = true;
            }
        }

        private void _noPath_Checked(object sender, RoutedEventArgs e)
        {
            _takePath.Content = "sourcepath selected";
            settings.sourcePath = true;
        }

        private void _noPath_Unchecked(object sender, RoutedEventArgs e)
        {
            _takePath.Content = settings.outputPath;
            settings.sourcePath = false;
        }

        private void formatSelect_Click(object sender, RoutedEventArgs e)
        {

            if (_format16.IsChecked == true)
            {
                convertData.bitdepth = 16;
                convertData.maximize = false;
                settings.format = 1;
            }
            else if (_format16max.IsChecked == true)
            {
                convertData.bitdepth = 16;
                convertData.maximize = true;
                settings.format = 2;
            }
            else if (_format12.IsChecked == true)
            {
                convertData.bitdepth = 12;
                convertData.maximize = false;
                settings.format = 3;
            }
            else if (_format12max.IsChecked == true)
            {
                convertData.bitdepth = 12;
                convertData.maximize = true;
                settings.format = 4;
            }
            settings.Save();
        }

        private void _info_Click(object sender, RoutedEventArgs e)
        {
            // infowindow show
            infoWindow infoW = new infoWindow();
            infoW.Show();
        }

        private void _banding_Checked(object sender, RoutedEventArgs e)
        {
            convertData.verticalBanding = true;
            saveGUIsettings();
        }
        private void _banding_Unchecked(object sender, RoutedEventArgs e)
        {
            convertData.verticalBanding = false;
            saveGUIsettings();
        }

        private void _smoothing_Checked(object sender, RoutedEventArgs e)
        {
            convertData.chromaSmoothing = true;
            saveGUIsettings();
        }
        private void _smoothing_Unchecked(object sender, RoutedEventArgs e)
        {
            convertData.chromaSmoothing = false;
            saveGUIsettings();
        }

        private void _jpegs_Checked(object sender, RoutedEventArgs e)
        {
            convertData.proxyJpegs = true;
            saveGUIsettings();
        }
        private void _jpegs_Unchecked(object sender, RoutedEventArgs e)
        {
            convertData.proxyJpegs = false;
            saveGUIsettings();
        }

        private void _highlights_Checked(object sender, RoutedEventArgs e)
        {
            convertData.pinkHighlight = true;
            _format16max.IsChecked = true;
            convertData.bitdepth = 16;
            convertData.maximize = true;
            settings.format = 2;
            saveGUIsettings();
        }
        private void _highlights_Unchecked(object sender, RoutedEventArgs e)
        {
            convertData.pinkHighlight = false;
            saveGUIsettings();
        }

        private void _prefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            saveGUIsettings();
        }

        private void _noPath_Click(object sender, RoutedEventArgs e)
        {
            //empty
        }

        // ------- Helper ------------
        
        private void saveGUIsettings()
        {
            if (toggleSettingsSave == true)
            {
                settings.verticalBanding = convertData.verticalBanding;
                settings.proxyJpeg = convertData.proxyJpegs;
                settings.chromaSmooth = convertData.chromaSmoothing;
                settings.highlightFix = convertData.pinkHighlight;
                settings.outputPath = _takePath.Content.ToString();
                settings.prefix = _prefix.Text;
                //settings.format = settings.format;
                settings.Save();
            }
        }

        public class lvItem
        {
            public bool convert { get; set; }
            public string type { get; set; }
            public string filename { get; set; }
            public string files { get; set; }
            public string frames { get; set; }
            public string duration { get; set; }
            public string resolution { get; set; }
            public string fps { get; set; }
            public string audio { get; set; }
        }

        private void previewTimer_Tick(object sender, EventArgs e)
        {
            if (_batchList.SelectedItems.Count > 0)
            {
                int item = _batchList.Items.IndexOf(_batchList.SelectedItems[0]);
                raw r = rawFiles[item];
                r.metaData.previewFrame++;
                r.metaData.maximize = true;
                r.metaData.previewFrame = r.metaData.previewFrame % r.metaData.frames;
                Task.Factory.StartNew(() => previewBackground(r));
            }
        }

        public void previewBackground(raw r)
        {
            var frame = r.metaData.previewFrame;
            if (frame != null)
            {
                var maxFrames = r.metaData.frames;
                var progressPosX = 564 + 320 * frame / maxFrames;
                // read picture and show
                r.threadData.frame = frame;


                //_preview.Source = im;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    WriteableBitmap im = io.showPicture(r);
                    _preview.Source = im;
                    _lensLabel.Content = String.Format("{0:d5}", frame);
                    _previewProgressBar.Margin = new Thickness(progressPosX, 243, 0, 0);
                    _preview.InvalidateVisual();
                    _lensLabel.InvalidateVisual();
                    //this.InvalidateVisual();
                }));
            }
        }

        public void loadSettings()
        {
            settings = appSettings.Load();
            // --- set GUI to settingsData ---
            if (settings.outputPath == null) settings.outputPath = "";
            if (settings.outputPath != "")
            {
                _takePath.IsChecked = true;
                _noPath.IsChecked = false;
                _takePath.Content = settings.outputPath;
                settings.sourcePath = false;
            }
            else
            {
                _takePath.IsChecked = false;
                _noPath.IsChecked = false;
                _takePath.Content = "select destination path";
                settings.sourcePath = true;
            }
            switch (settings.format)
            {
                case 1:
                    _format16.IsChecked = true;
                    convertData.bitdepth = 16;
                    convertData.maximize = false;
                    break;
                case 2:
                    _format16max.IsChecked = true;
                    convertData.bitdepth = 16;
                    convertData.maximize = true;
                    break;
                case 3:
                    _format12.IsChecked = true;
                    convertData.bitdepth = 12;
                    convertData.maximize = false;
                    break;
                case 4:
                    _format12max.IsChecked = true;
                    convertData.bitdepth = 12;
                    convertData.maximize = true;
                    break;
                default:
                    _format16.IsChecked = true;
                    convertData.bitdepth = 16;
                    convertData.maximize = false;
                    break;
            }
            if (settings.verticalBanding == true)
            {
                _banding.IsChecked = true;
                convertData.verticalBanding = true;
            }
            else
            {
                _banding.IsChecked = false;
                convertData.verticalBanding = false;
            }
            if (settings.chromaSmooth == true)
            {
                _smoothing.IsChecked = true;
                convertData.chromaSmoothing = true;
            }
            else
            {
                _smoothing.IsChecked = false;
                convertData.chromaSmoothing = false;
            }
            if (settings.highlightFix == true)
            {
                _highlights.IsChecked = true;
                convertData.pinkHighlight = true;
            }
            else
            {
                _highlights.IsChecked = false;
                convertData.pinkHighlight = false;
            }
            if (settings.proxyJpeg == true)
            {
                _jpegs.IsChecked = true;
                convertData.proxyJpegs = true;
            }
            else
            {
                _jpegs.IsChecked = false;
                convertData.proxyJpegs = false;
            }
            
            if (settings.prefix == null) settings.prefix = "";
            if (settings.prefix != "")
            {
                _prefix.Text = settings.prefix;
            }
            else
            {
                settings.prefix = "[F]";
                _prefix.Text = settings.prefix;
            }

            toggleSettingsSave = true;

        }
        
        // ------- EOF ----------
    }
}
