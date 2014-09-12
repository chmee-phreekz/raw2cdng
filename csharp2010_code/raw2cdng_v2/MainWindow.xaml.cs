﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Diagnostics;
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

    public partial class MainWindow : Window, INotifyPropertyChanging, INotifyPropertyChanged
    {
        // -- timertick for _preview
        DispatcherTimer previewTimer = new DispatcherTimer();
        // -- timertick for _draggedProgress
        //DispatcherTimer progressDragDrop = new DispatcherTimer();

        // -- old folderBrowserdialog
        System.Windows.Forms.FolderBrowserDialog _selectFolder = new System.Windows.Forms.FolderBrowserDialog();

        private ObservableCollection<raw> rawFiles = new ObservableCollection<raw>();
        public ObservableCollection<raw> RawFiles
        {
            get { return this.rawFiles; }
        }
        
        private raw selectedRawFile = null;
        public raw SelectedRawFile
        {
            get
            {
                return selectedRawFile;
            }

            set
            {
                if (selectedRawFile == value)
                    return;

                RaisePropertyChanging("SelectedRawFile");
                selectedRawFile = value;
                RaisePropertyChanged("SelectedRawFile");
            }
        }

        int allFramesCount;

        private int cpuCores = Environment.ProcessorCount;
        public int CPUcores
        {
            get
            {
                return this.cpuCores;
            }
        }

        private string version = "1.6.0";
        public string Version
        {
            get
            {
                return this.version;
            }
        }

        // baseSettings for convert

        private convertSettings convertData = null;
        public convertSettings ConvertData
        {
            get
            {
                if (convertData == null)
                    this.convertData = initConvertData();
                return convertData;
            }

            set
            {
                if (convertData == value)
                    return;

                RaisePropertyChanging("ConvertData");
                convertData = value;
                RaisePropertyChanged("ConvertData");
            }
        }

        bool ffmpegExists = false;

        //List<string> proxyKinds = new List<string>();

        // settings load/save/change
        appSettings settings = new appSettings();
        bool toggleSettingsSave = false;

        // colors
        //SolidColorBrush green;
        //SolidColorBrush white;

        private bool takePathIsChecked = false;
        public bool TakePathIsChecked
        {
            get
            {
                return takePathIsChecked;
            }

            set
            {
                if (takePathIsChecked == value)
                    return;

                RaisePropertyChanging("TakePathIsChecked");
                takePathIsChecked = value;
                RaisePropertyChanged("TakePathIsChecked");
            }
        }

        private bool noPathIsChecked = false;
        public bool NoPathIsChecked
        {
            get
            {
                return noPathIsChecked;
            }

            set
            {
                if (noPathIsChecked == value)
                    return;

                RaisePropertyChanging("NoPathIsChecked");
                noPathIsChecked = value;
                RaisePropertyChanged("NoPathIsChecked");
            }
        }

        private string takePath = "select destination path";
        public string TakePath
        {
            get
            {
                return takePath;
            }

            set
            {
                if (takePath == value)
                    return;

                RaisePropertyChanging("TakePath");
                takePath = value;
                RaisePropertyChanged("TakePath");
            }
        }

        private bool logDebugIsChecked = false;
        public bool LogDebugIsChecked
        {
            get
            {
                return logDebugIsChecked;
            }

            set
            {
                if (logDebugIsChecked == value)
                    return;

                RaisePropertyChanging("LogDebugIsChecked");
                logDebugIsChecked = value;
                RaisePropertyChanged("LogDebugIsChecked");

                settings.debugLogEnabled = value;
                debugging.debugLogEnabled = value;
                saveGUIsettings();
            }
        }

        //private bool bandingIsChecked = false;
        //public bool BandingIsChecked
        //{
        //    get
        //    {
        //        return bandingIsChecked;
        //    }

        //    set
        //    {
        //        if (bandingIsChecked == value)
        //            return;

        //        RaisePropertyChanging("BandingIsChecked");
        //        bandingIsChecked = value;
        //        RaisePropertyChanged("BandingIsChecked");
        //    }
        //}

        //private bool smoothingIsChecked = false;
        //public bool SmoothingIsChecked
        //{
        //    get
        //    {
        //        return smoothingIsChecked;
        //    }

        //    set
        //    {
        //        if (smoothingIsChecked == value)
        //            return;

        //        RaisePropertyChanging("SmoothingIsChecked");
        //        smoothingIsChecked = value;
        //        RaisePropertyChanged("SmoothingIsChecked");
        //    }
        //}

        //private bool highlightsIsChecked = false;
        //public bool HighlightsIsChecked
        //{
        //    get
        //    {
        //        return highlightsIsChecked;
        //    }

        //    set
        //    {
        //        if (highlightsIsChecked == value)
        //            return;

        //        RaisePropertyChanging("HighlightsIsChecked");
        //        highlightsIsChecked = value;
        //        RaisePropertyChanged("HighlightsIsChecked");
        //    }
        //}

        //private bool proxyIsChecked = false;
        //public bool ProxyIsChecked
        //{
        //    get
        //    {
        //        return proxyIsChecked;
        //    }

        //    set
        //    {
        //        if (proxyIsChecked == value)
        //            return;

        //        RaisePropertyChanging("ProxyIsChecked");
        //        proxyIsChecked = value;
        //        RaisePropertyChanged("ProxyIsChecked");
        //    }
        //}

        //private bool proxyKindIsEnabled = false;
        //public bool ProxyKindIsEnabled
        //{
        //    get
        //    {
        //        return proxyKindIsEnabled;
        //    }

        //    set
        //    {
        //        if (proxyKindIsEnabled == value)
        //            return;

        //        RaisePropertyChanging("ProxyKindIsEnabled");
        //        proxyKindIsEnabled = value;
        //        RaisePropertyChanged("ProxyKindIsEnabled");
        //    }
        //}

        //private string proxyKind = "jpg";
        //public string ProxyKind
        //{
        //    get
        //    {
        //        return proxyKind;
        //    }

        //    set
        //    {
        //        if (proxyKind == value)
        //            return;

        //        RaisePropertyChanging("ProxyKind");
        //        proxyKind = value;
        //        RaisePropertyChanged("ProxyKind");
        //    }
        //}

        private string prefix = "Cam01_[D]_[T]_[S](_F)";
        public string Prefix
        {
            get
            {
                return prefix;
            }

            set
            {
                if (prefix == value)
                    return;

                RaisePropertyChanging("Prefix");
                prefix = value;
                RaisePropertyChanged("Prefix");
            }
        }
        
        private bool loadingDroppedData = false;
        public bool LoadingDroppedData
        {
            get
            {
                return loadingDroppedData;
            }

            set
            {
                if (loadingDroppedData == value)
                    return;

                RaisePropertyChanging("LoadingDroppedData");
                loadingDroppedData = value;
                RaisePropertyChanged("LoadingDroppedData");
            }
        }
        
        private int droppedDataCount = 0;
        public int DroppedDataCount
        {
            get
            {
                return droppedDataCount;
            }

            set
            {
                if (droppedDataCount == value)
                    return;

                RaisePropertyChanging("DroppedDataCount");
                droppedDataCount = value;
                RaisePropertyChanged("DroppedDataCount");
            }
        }

        private int progressedDroppedDataCount = 0;
        public int ProgressedDroppedDataCount
        {
            get
            {
                return progressedDroppedDataCount;
            }

            set
            {
                if (progressedDroppedDataCount == value)
                    return;

                RaisePropertyChanging("ProgressedDroppedDataCount");
                progressedDroppedDataCount = value;
                RaisePropertyChanged("ProgressedDroppedDataCount");
            }
        }

        private int framesToProgress = 1;
        public int FramesToProgress
        {
            get
            {
                return framesToProgress;
            }

            set
            {
                if (framesToProgress == value)
                    return;

                RaisePropertyChanging("FramesToProgress");
                framesToProgress = value;
                RaisePropertyChanged("FramesToProgress");
            }
        }

        private int framesProgressed = 0;
        public int FramesProgressed
        {
            get
            {
                return framesProgressed;
            }

            set
            {
                if (framesProgressed == value)
                    return;

                RaisePropertyChanging("FramesProgressed");
                framesProgressed = value;
                RaisePropertyChanged("FramesProgressed");
            }
        }

        private int totalFramesToProgress = 1;
        public int TotalFramesToProgress
        {
            get
            {
                return totalFramesToProgress;
            }

            set
            {
                if (totalFramesToProgress == value)
                    return;

                RaisePropertyChanging("TotalFramesToProgress");
                totalFramesToProgress = value;
                RaisePropertyChanged("TotalFramesToProgress");
            }
        }

        private int totalFramesProgressed = 0;
        public int TotalFramesProgressed
        {
            get
            {
                return totalFramesProgressed;
            }

            set
            {
                if (totalFramesProgressed == value)
                    return;

                RaisePropertyChanging("TotalFramesProgressed");
                totalFramesProgressed = value;
                RaisePropertyChanged("TotalFramesProgressed");
            }
        }

        private string currentAction = null;
        public string CurrentAction
        {
            get
            {
                return currentAction;
            }

            set
            {
                if (currentAction == value)
                    return;

                RaisePropertyChanging("CurrentAction");
                currentAction = value;
                RaisePropertyChanged("CurrentAction");
            }
        }

        private bool batchListIsEnabled = true;
        public bool BatchListIsEnabled
        {
            get
            {
                return batchListIsEnabled;
            }

            set
            {
                if (batchListIsEnabled == value)
                    return;

                RaisePropertyChanging("BatchListIsEnabled");
                batchListIsEnabled = value;
                RaisePropertyChanged("BatchListIsEnabled");
            }
        }

        public int BatchListItemCount
        {
            get
            {
                if (this.RawFiles == null)
                    return 0;
                return this.RawFiles.Count;
            }
        }
        
        private WriteableBitmap previewSource = null;
        public WriteableBitmap PreviewSource
        {
            get
            {
                return previewSource;
            }

            set
            {
                if (previewSource == value)
                    return;

                RaisePropertyChanging("PreviewSource");
                previewSource = value;
                RaisePropertyChanged("PreviewSource");
            }
        }

        private string previewLensData = null;
        public string PreviewLensData
        {
            get
            {
                return previewLensData;
            }

            set
            {
                if (previewLensData == value)
                    return;

                RaisePropertyChanging("PreviewLensData");
                previewLensData = value;
                RaisePropertyChanged("PreviewLensData");
            }
        }
        
        private bool convertingInProgress = false;
        public bool ConvertingInProgress
        {
            get
            {
                return convertingInProgress;
            }

            set
            {
                if (convertingInProgress == value)
                    return;

                RaisePropertyChanging("ConvertingInProgress");
                convertingInProgress = value;
                RaisePropertyChanged("ConvertingInProgress");
            }
        }

        private ObservableCollection<string> proxyKinds = new ObservableCollection<string>();
        public ObservableCollection<string> ProxyKinds
        {
            get
            {
                return proxyKinds;
            }

            set
            {
                if (proxyKinds == value)
                    return;

                RaisePropertyChanging("ProxyKinds");
                proxyKinds = value;
                RaisePropertyChanged("ProxyKinds");
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            //_batchList.DataContext = rawFiles;
            //_batchList.ItemsSource = rawFiles;

            // calculate LUTs
            calc.calculatetRec709LUT();
            int[] test = calc.Rec709;

            //colors
            //green = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            //white = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            //_title.Text = "raw2cdng " + version;
            allFramesCount = 0;

            //CPUcores = Environment.ProcessorCount;
            //_cores.Content = CPUcores.ToString();

            // -- init _preview Tick and small frameProgressLine
            previewTimer.Tick += previewTimer_Tick;
            previewTimer.Interval = TimeSpan.FromMilliseconds(25);

            //progressDragDrop.Tick += progressDragDrop_Tick;
            //progressDragDrop.Interval = TimeSpan.FromMilliseconds(20);


            //_previewProgressBar.Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            //_dragDropProgressBar.Height = 0;
            //_dragDropProgressBar.Width = 0;

            // ui init
            //_progressAll.Value = 0;
            //_progressOne.Value = 0;

            // ---- load settings from file ----

            // standard proxy
            proxyKinds.Add("jpg");

            // ask for ffmpeg in same directory, if - proxy will be mpeg2 as well
            if (winIO.File.Exists(Environment.CurrentDirectory + winIO.Path.DirectorySeparatorChar + "ffmpeg.exe"))
            {
                ffmpegExists = true;
                proxyKinds.Add("mpg2");
                proxyKinds.Add("mpeg4");
            }
            else ffmpegExists = false;

            loadSettings();
            debugging.debugLogFilename = Environment.CurrentDirectory + winIO.Path.DirectorySeparatorChar + "raw2cdng.2.debug.log";

            //this.ProxyKind = proxyKinds[settings.proxyKind];

            this.ConvertData.PropertyChanged += ConvertData_PropertyChanged;

            // debug log
            if (settings.debugLogEnabled)
            {
                debugging._startNewDebug(" ------------- " + version + " started at " + String.Format("{0:yy.MM.dd HH:mm:ss -- }", DateTime.Now) + "\r\n");
                debugging._saveDebug("[init][settings] -- ffmpeg Exists: " + (ffmpegExists ? "true" : "false"));
            }
        }

        void ConvertData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.saveGUIsettings();
        }

        private convertSettings initConvertData()
        {
            convertSettings convertData = new convertSettings()
            {
                bitdepth = 16,
                ChromaSmoothing = false,
                format = "CDNG",
                maximize = false,
                maximizeValue = 1,
                PinkHighlight = false,
                IsProxy = false,
                ProxyKind = 0,
                VerticalBanding = false
            };

            return convertData;
        }

        // --- the important three events ---------------------

        private void batchList_Drop(object sender, DragEventArgs e)
        {
            // -- dragdrop files
            // -- prepare metadata
            // -- put into listview


            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // draggedProgress Start
                //_dragDropProgressBar.Height = 12;
                //_dragDropProgressBar.Width = 200;                
                //progressDragDrop.Start();

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // do it in a task because of progress
                Task dragDropDataTask = Task.Factory.StartNew(() => dragDropData(files));
            }
        }

        private void dragDropData(string[] files)
        {
            if (settings.debugLogEnabled)
            {
                debugging._saveDebug("[drop] started");
                debugging._saveDebug("[drop] Files dropped:");
                debugging._saveDebug("[drop] ------");
            }

            // since beta8 -folderimport
            List<string> fileList = new List<string>();
            foreach (string file in files)
            {
                if (io.isFolder(file))
                {
                    if (settings.debugLogEnabled) debugging._saveDebug("[drop] -- " + file + " is a folder. seeking for MLV and RAW.");
                    io.dirSearch(file, ref fileList);
                }
                else
                {
                    if (settings.debugLogEnabled) debugging._saveDebug("[drop] -- file " + file + ".");
                    fileList.Add(file);
                }
            }
            debugging._saveDebug("[drop] ------");

            // --- list of dropped files
            fileList.Sort();

            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(()=>{

                this.LoadingDroppedData = true;
                this.DroppedDataCount = fileList.Count;

            }), DispatcherPriority.Normal, null);
            

            foreach (string file in fileList)
            {

                if (io.isMLV(file) || io.isRAW(file))
                {
                    if (settings.debugLogEnabled) debugging._saveDebug("[drop] -- file " + file + " will be analyzed now.");
                    raw importRaw = new raw();
                    data importData = new data();
                    importData.metaData = new metadata();
                    importData.fileData = new filedata();
                    importData.fileData.tempPath = winIO.Path.GetTempPath();

                    importData.threadData = new threaddata();
                    importData.lensData = new lensdata();
                    importData.audioData = new audiodata();
                    // write versionstring into author-tag
                    // not done yet.
                    importData.metaData.version = version;
                    importData.metaData.errorString = "";

                    // prepare versionstring as byte[]
                    // later copy it to the end of file
                    importData.metaData.versionString = new byte[version.Length];
                    importData.metaData.versionString = System.Text.Encoding.ASCII.GetBytes(version);
                    importRaw.data = importData;

                    if (io.isMLV(file))
                    {
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] is MLV ");
                        importRaw.data.metaData.isMLV = true;
                        importRaw.data.metaData.errorString += io.setFileinfoData(file, importRaw.data.fileData);
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] FileinfoData set");
                        importRaw.data.metaData.errorString += io.createMLVBlockList(file, importRaw);
                        Blocks.mlvBlockList = Blocks.mlvBlockList.OrderBy(x => x.timestamp).ToList();
                        importRaw.data.metaData.errorString += io.getMLVAttributes(file, Blocks.mlvBlockList, importRaw);
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] MLV Attributes and Blocklist created and sorted. Blocks: " + Blocks.mlvBlockList.Count);
                        importRaw.AUDFBlocks = null;
                        if (importRaw.data.audioData.hasAudio)
                        {
                            importRaw.AUDFBlocks = Blocks.mlvBlockList.Where(x => x.blockTag == "AUDF").ToList();
                            if (settings.debugLogEnabled) debugging._saveDebug("[drop] hasAudio. AUDF-List created. Blocks: " + importRaw.AUDFBlocks.Count);
                        }
                        importRaw.VIDFBlocks = null;
                        importRaw.VIDFBlocks = Blocks.mlvBlockList.Where(x => x.blockTag == "VIDF").ToList();
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] VIDF-List created. Blocks: " + importRaw.VIDFBlocks.Count);
                        importRaw.data.metaData.errorString += io.readVIDFBlockData(importRaw);
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] VIDF-Blockdata read and created.");
                        // correct frameCount
                        importRaw.data.metaData.frames = importRaw.VIDFBlocks.Count;

                        importRaw.convert = true;
                    }
                    if (io.isRAW(file))
                    {
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] is RAW ");
                        importRaw.data.metaData.isMLV = false;
                        importRaw.data.audioData.hasAudio = false;
                        importRaw.RAWBlocks = null;
                        io.setFileinfoData(file, importRaw.data.fileData);
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] FileinfoData set");
                        io.getRAWAttributes(file, importRaw);
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] RAW Attributes read and set.");
                        io.createRAWBlockList(file, importRaw);
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] RAW Blocklist created and sorted. Blocks: " + importRaw.RAWBlocks.Count);

                        // then Framelist
                        importRaw.convert = true;
                    }
                    // check errors
                    // lookinto errorString and decide if !=""
                    if (importRaw.data.metaData.errorString == "")
                    {
                        calc.setListviewStrings(importRaw);

                        // now set item
                        if (settings.debugLogEnabled) debugging._saveDebug("[drop] adding " + importRaw.data.fileData.fileNameOnly + " to batchList");

                        // and save raw into list
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            // create thumbnail
                            importRaw.thumbnail = io.showPicture(importRaw, quality.lowmullim);

                            // calculate coeffs and histograms
                            if (importRaw.data.metaData.isMLV)
                            {
                                importRaw.data.fileData.VIDFBlock = importRaw.VIDFBlocks[0];
                                importRaw.data.metaData.verticalBandingCoeffs = calc.calcVerticalBandingCoeff(calc.to16(io.readMLV(importRaw.data), importRaw.data), importRaw);
                            }
                            else
                            {
                                importRaw.data.fileData.RAWBlock = importRaw.RAWBlocks[0];
                                importRaw.data.metaData.verticalBandingCoeffs = calc.calcVerticalBandingCoeff(calc.to16(io.readRAW(importRaw.data), importRaw.data), importRaw);
                            }


                            if (importRaw.data.metaData.verticalBandingCoeffs[8] == 1) importRaw.data.metaData.verticalBandingNeeded = true;
                            else importRaw.data.metaData.verticalBandingNeeded = false;

                            if (settings.debugLogEnabled) debugging._saveDebug("[drop] verticalBanding enabled. decision: " + (importRaw.data.metaData.verticalBandingNeeded ? "do it" : "not needed"));
                            if (settings.debugLogEnabled)
                            {
                                string tmpCoeffs = "";
                                foreach (double v in importRaw.data.metaData.verticalBandingCoeffs) tmpCoeffs += v.ToString() + " ";
                                debugging._saveDebug("[drop] verticalBanding CoEffs: " + tmpCoeffs);
                            }

                            rawFiles.Add(importRaw);
                            //                      cr2 = importRaw.data.metaData.photoRAW ? (importRaw.data.metaData.photoRAWe ? "\u2714" : "\u2714|") : "\u2715",
                        }));
                    }
                    if (settings.debugLogEnabled)
                    {
                        debugging._saveDebug("[drop] ** Item " + importRaw.data.fileData.fileNameOnly + " imported | " + (importRaw.data.metaData.isMLV ? "MLV" : "RAW"));
                        debugging._saveDebug("[drop] * res " + importRaw.data.metaData.xResolution + "x" + importRaw.data.metaData.yResolution + "px | " + importRaw.data.metaData.fpsString + "fps | " + importRaw.data.metaData.frames + " frames | BL" + importRaw.data.metaData.blackLevelOld + " | WL" + importRaw.data.metaData.whiteLevelOld);
                        if (importRaw.data.metaData.isMLV) debugging._saveDebug("[drop] * modell " + importRaw.data.metaData.modell + " | vidfblocks " + importRaw.VIDFBlocks.Count() + " | has " + (importRaw.data.audioData.hasAudio ? "" : "no ") + "audio");
                        else debugging._saveDebug("[drop] * modell " + importRaw.data.metaData.modell + " | rawblocks " + importRaw.RAWBlocks.Count() + " | has " + (importRaw.data.audioData.hasAudio ? "" : "no") + "audio");
                        if (importRaw.data.metaData.errorString != "")
                        {
                            debugging._saveDebug("[drop][!] ** Item " + importRaw.data.fileData.fileNameOnly + " was not imported. [errorString]: " + importRaw.data.metaData.errorString);
                        }
                    }

                }
                                
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(progressedDroppedFile), DispatcherPriority.Normal, null);                

            }
            //progressDragDrop.Stop();

            this.LoadingDroppedData = false;
            this.DroppedDataCount = 0;
            this.ProgressedDroppedDataCount = 0;

            this.Dispatcher.Invoke((Action)(() =>
            {
                //_dragDropProgressBar.Height = 0;
                //_dragDropProgressBar.Width = 0;                

                if (this.BatchListItemCount > 0)
                {
                    _convert.IsEnabled = true;
                }
            }));
        }

        private void _convert_Click(object sender, RoutedEventArgs e)
        {
            // stop preview if running
            if (previewTimer.IsEnabled)
            {
                previewTimer.Stop();
                previewTimer.IsEnabled = false;
            }
            allFramesCount = 0;
            // -- count all frames for progressbarAll --
            foreach (raw file in rawFiles)
            {
                if (file.convert) allFramesCount += file.data.metaData.frames;
            }
            if (settings.debugLogEnabled) debugging._saveDebug("[convert_Click] all in all there are " + allFramesCount + " frames to convert");

            // disable GUI
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.TotalFramesToProgress = allFramesCount;
                this.TotalFramesProgressed = 0;
                _convert.Content = "converting";
                this.BatchListIsEnabled = false;
                this.ConvertingInProgress = true;
            }));

            // doing the work as a thread, leave GUI fluid
            ThreadPool.QueueUserWorkItem(doWork);
        }

        private void doWork(object state)
        {
            if (settings.debugLogEnabled) debugging._saveDebug("[doWork] started");
            allFramesCount = 0;

            // set threadpool properties
            ThreadPool.SetMaxThreads(CPUcores, CPUcores);

            if (settings.debugLogEnabled) debugging._saveDebug("[doWork] " + CPUcores + " Cores used");

            // variables for _actionOutput while converting
            int convertPosition = 0;
            int convertAmount = rawFiles.Count();

            // and go.
            while (rawFiles.Count != 0)
            {
                raw file = rawFiles[0];
                if (file.convert)
                {
                    if (settings.debugLogEnabled) debugging._saveDebug("[doWork] -> converting item " + file.data.fileData.fileNameOnly);

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        // -- refresh _progressbar.One
                        this.FramesToProgress = file.data.metaData.frames;
                        this.FramesProgressed = 0;
                        
                        convertPosition++;
                        this.CurrentAction = "converting " + convertPosition + "/" + convertAmount + " - " + file.data.fileData.fileNameOnly;
                        this.SelectedRawFile = file;
                        _batchList.ScrollIntoView(file);
                    }));

                    // copy properties from GUI into rawobject
                    file.data.convertData = convertData;
                    // empty rawData - while preview it was filled. lead to "out of memory" exception.
                    file.data.rawData = null;

                    // if maximized use the multiplier
                    file.data.metaData.maximizer = (Math.Pow(2, convertData.bitdepth) - 1) / (file.data.metaData.whiteLevelOld - file.data.metaData.blackLevelOld);

                    if (file.data.convertData.ChromaSmoothing)
                    {
                        //if using chroma Smoothing, recalculate ev2raw/raw2ev
                        calc.calcRAWEV_Arrays(file.data.metaData.blackLevelNew, file.data.metaData.blackLevelNew);
                    }

                    // set new blacklevel whitelevel and bitdepth
                    switch (settings.format)
                    {
                        case 1:
                            // 16 bit normal
                            file.data.metaData.blackLevelNew = file.data.metaData.blackLevelOld;
                            file.data.metaData.whiteLevelNew = file.data.metaData.whiteLevelOld;
                            file.data.metaData.bitsperSampleChanged = 16;
                            file.data.metaData.maximize = false;
                            break;
                        case 2:
                            // 16 bit maximized
                            file.data.metaData.blackLevelNew = 0;
                            file.data.metaData.whiteLevelNew = 65535;
                            file.data.metaData.bitsperSampleChanged = 16;
                            file.data.metaData.maximize = true;
                            file.data.metaData.maximizer = file.data.metaData.whiteLevelNew / (file.data.metaData.whiteLevelOld - file.data.metaData.blackLevelOld);
                            break;
                        case 3:
                            // 12bit normal - is disabled - now ARRIRAW tbd!
                            file.data.metaData.blackLevelNew = file.data.metaData.blackLevelOld / 4;
                            file.data.metaData.whiteLevelNew = file.data.metaData.whiteLevelOld / 4;
                            file.data.metaData.bitsperSampleChanged = 12;
                            file.data.metaData.maximize = false;
                            break;
                        case 4:
                            // 12 bit maximized
                            file.data.metaData.blackLevelNew = 0;
                            file.data.metaData.whiteLevelNew = 4095;
                            file.data.metaData.bitsperSampleChanged = 12;
                            file.data.metaData.maximize = true;
                            file.data.metaData.maximizer = 65535 / (file.data.metaData.whiteLevelOld - file.data.metaData.blackLevelOld);
                            break;
                        default:
                            file.data.metaData.blackLevelNew = file.data.metaData.blackLevelOld;
                            file.data.metaData.whiteLevelNew = file.data.metaData.whiteLevelOld;
                            file.data.metaData.bitsperSampleChanged = 16;
                            file.data.metaData.maximize = false;
                            break;
                    }

                    if (settings.debugLogEnabled) debugging._saveDebug("[doWork] settings format " + settings.format + " with BL" + file.data.metaData.blackLevelNew + " WL" + file.data.metaData.whiteLevelNew + " with+" + (file.data.metaData.maximize ? "" : "out") + " maximizingvalue " + file.data.metaData.maximizer);

                    // prepare prefix variables
                    string date = string.Format("{0:yyMMdd}", file.data.fileData.creationTime);
                    string datetime = String.Format("{0:yyyy-MM-dd_HHmm}", file.data.fileData.creationTime);
                    string modifiedDate = string.Format("{0:yyMMdd}", file.data.fileData.modificationTime);
                    string modifiedDatetime = String.Format("{0:yyyy-MM-dd_HHmm}", file.data.fileData.modificationTime);

                    string time = string.Format("{0:HHmmss}", file.data.fileData.creationTime);
                    string modifiedTime = string.Format("{0:HHmmss}", file.data.fileData.modificationTime);

                    string bitdepth = file.data.metaData.bitsperSampleChanged.ToString();

                    // set filename from prefix-generator
                    file.data.fileData.outputFilename = settings.prefix.
                        Replace("[D]", date).
                        Replace("[D2]", datetime).
                        Replace("[M]", modifiedDate).
                        Replace("[M2]", modifiedDatetime).
                        Replace("[T]", time).
                        Replace("[T2]", modifiedTime).
                        Replace("[S]", file.data.fileData.fileNameShort).
                        Replace("[C]", file.data.fileData.fileNameNum).
                        Replace("[P]", file.data.fileData.parentSourcePath).
                        Replace("[B]", bitdepth).
                        Replace("[F]", file.data.fileData.fileNameOnly);
                    // cut parenthesis-content
                    // its for the filesequences
                    if (file.data.fileData.outputFilename.IndexOf("(") > -1)
                    {
                        file.data.fileData.destinationPath = file.data.fileData.outputFilename.Substring(0, file.data.fileData.outputFilename.IndexOf("("));
                        file.data.fileData.outputFilename = file.data.fileData.outputFilename.Replace("(", "").Replace(")", "");
                    }
                    else
                    {
                        file.data.fileData.destinationPath = file.data.fileData.outputFilename;
                    }

                    // source or selected Path?
                    if (settings.sourcePath)
                    {
                        file.data.fileData.basePath = file.data.fileData.sourcePath;
                    }
                    else
                    {
                        file.data.fileData.basePath = settings.outputPath;
                    }
                    if (settings.debugLogEnabled)
                    {
                        debugging._saveDebug("[doWork] prefix - filename generator " + settings.prefix);
                        debugging._saveDebug("[doWork] destinationPath -> " + file.data.fileData.destinationPath);
                        debugging._saveDebug("[doWork] outputFilename  -> " + file.data.fileData.outputFilename);
                    }

                    // check/make destination path
                    winIO.Directory.CreateDirectory(file.data.fileData.basePath + winIO.Path.DirectorySeparatorChar + file.data.fileData.destinationPath);
                    if (settings.debugLogEnabled) debugging._saveDebug("[doWork] Directory " + file.data.fileData.basePath + winIO.Path.DirectorySeparatorChar + file.data.fileData.destinationPath + " created");

                    // set dngheader
                    // dngheader is a fileresource (DNGtemplate20)
                    file.data.metaData.DNGHeader = dng.setDNGHeader(file.data);
                    if (settings.debugLogEnabled) debugging._saveDebug("[doWork] took DNGtemplate and changed values");

                    // destinationPath as its used in the threads
                    file.data.fileData._changedPath = file.data.fileData.basePath + winIO.Path.DirectorySeparatorChar + file.data.fileData.destinationPath + winIO.Path.DirectorySeparatorChar;

                    // init for multithreading
                    int frameCount;
                    frameCount = file.data.metaData.frames;
                    if (settings.debugLogEnabled) debugging._saveDebug("[doWork] * init frameCount for multithreaded Convert");

                    int taskCount = frameCount;
                    var cde = new CountdownEvent(taskCount);

                    // init strings for debugging
                    string debugString = "";

                    // start frameconvert
                    for (int f = 0; f < frameCount; f++)
                    {
                        //multithread
                        if (file.data.metaData.isMLV)
                        {
                            //its MLV
                            file.data.fileData.VIDFBlock = file.VIDFBlocks[f];
                            file.data.threadData.frame = file.data.fileData.VIDFBlock.MLVFrameNo;
                            if (settings.debugLogEnabled)
                            {
                                debugString += "[" + f + "]";
                                if (f % 25 == 0)
                                {
                                    debugging._saveDebug("[doWork][for] read/convert MLV VIDF: " + debugString);
                                    debugString = "";
                                }
                            }
                        }
                        else
                        {
                            // its RAW
                            file.data.fileData.RAWBlock = file.RAWBlocks[f];
                            file.data.threadData.frame = f;
                            if (settings.debugLogEnabled)
                            {
                                debugString += "[" + f + "]";
                                if (f % 25 == 0)
                                {
                                    debugging._saveDebug("[doWork][for] read/convert RAW: " + debugString);
                                    debugString = "";
                                }
                            }
                        }
                        data para = file.data.Copy(); // deep copy object from ObjectExtensions.cs
                        para.threadData.CDEvent = cde;

                        // start convert thread here
                        ThreadPool.QueueUserWorkItem(new WaitCallback(doFrame_Thread), para);
                        para = null;

                    }
                    // wait till all threads has ended
                    cde.Wait();

                    if (settings.debugLogEnabled) debugging._saveDebug("[doWork] convert Done");

                    // now audio if existent
                    if (file.data.audioData.hasAudio)
                    {
                        if (settings.debugLogEnabled) debugging._saveDebug("[doWork] hasAudio file -> " + file.data.fileData.destinationPath + ".wav");
                        io.saveAudio(file.data.fileData._changedPath + file.data.fileData.destinationPath + ".wav", file);
                    }

                    // proxy clicked and ffmpeg existent. ok. lets do.
                    // new: if videoproxy, jpgs will be deleted.
                    if (ffmpegExists && settings.isProxy)
                    {
                        if ((settings.proxyKind != 0) && (settings.proxyKind < 3))
                        {
                            executeFFMPEG(file);
                            foreach (winIO.FileInfo f in new winIO.DirectoryInfo(file.data.fileData._changedPath).GetFiles("*.jpg"))
                            {
                                f.Delete();
                            }
                        }
                    }
                }
                this.Dispatcher.Invoke((Action)(() =>
                {
                    rawFiles.Remove(file);
                }));
            }

            // clear GUI and some variables
            // re enable buttons
            this.Dispatcher.Invoke((Action)(() =>
            {
                rawFiles.Clear();
                this.PreviewSource = null;

                this.FramesProgressed = 0;
                this.FramesToProgress = 1;
                this.TotalFramesProgressed = 0;
                this.TotalFramesToProgress = 1;
                _convert.IsEnabled = false;                

                this.BatchListIsEnabled = true;
                this.ConvertingInProgress = false;

                //if (settings.isProxy) this.ProxyKindIsEnabled = true;

            }));
        }

        private void doFrame_Thread(object state)
        {
            data param = (data)state;

            if (param.metaData.isMLV) param.rawData = io.readMLV(param);
            else param.rawData = io.readRAW(param);

            byte[] rawDataChanged;
            // in use if 12bit
            byte[] fillUp = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            // -- prepare DNG output

            string justFilename = param.fileData._changedPath + param.fileData.outputFilename + string.Format("{0,5:D5}", param.threadData.frame);
            string finalOutputFilename = justFilename + ".dng";

            // write Timecode into dng
            int timestamp = param.threadData.frame + (int)calc.dateTime2Frame(param.fileData.creationTime, (double)(param.metaData.fpsNom / param.metaData.fpsDen));
            param.metaData.DNGHeader = calc.changeTimeCode(param.metaData.DNGHeader, timestamp, 0x1dba, (int)Math.Round((double)(param.metaData.fpsNom / param.metaData.fpsDen)), param.metaData.dropFrame);

            /*            // workaround for 12bit, because first module needs 16bit-data
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
            */

            // ------- here's the magic - converting the data --------
            // -------------------------------------------------------
            rawDataChanged = calc.to16(param.rawData, param);

            // if verticalBanding (and if its needed. delta >0.01)
            if (param.convertData.VerticalBanding && param.metaData.verticalBandingNeeded)
            {
                //coeffs calculated in [dowork]
                // raw2ev/ev2raw-tables are calculated in _doWork
                rawDataChanged = calc.fixVerticalBanding(rawDataChanged, param);
            }
            // if chroma Smoothing
            if (param.convertData.ChromaSmoothing) rawDataChanged = calc.chromaSmoothing(rawDataChanged, param);

            // if proxy jpeg
            if (param.convertData.IsProxy) io.saveProxy(param, rawDataChanged);

            // if pink Highlights
            if (param.convertData.PinkHighlight) rawDataChanged = calc.pinkHighlight(rawDataChanged, param);

            // if 12bit        
            if (param.metaData.bitsperSampleChanged == 12)
            {
                // for 12 bit we have to add some spare fields to work over all resolutions
                byte[] tempRaw = new byte[fillUp.Length + rawDataChanged.Length];
                rawDataChanged.CopyTo(tempRaw, 0);
                fillUp.CopyTo(tempRaw, rawDataChanged.Length);

                rawDataChanged = calc.from16to12(tempRaw, param);
                tempRaw = null;
            }

            // ----- dng IO operations header and rawdata -----
            // ------------------------------------------------
            using (System.IO.FileStream stream = new System.IO.FileStream(finalOutputFilename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
            {
                stream.Write(param.metaData.DNGHeader, 0, param.metaData.DNGHeader.Length);
            }

            // -- write the bytearray - converted raw
            using (System.IO.FileStream stream = new System.IO.FileStream(finalOutputFilename, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
            {
                stream.Write(rawDataChanged, 0, rawDataChanged.Length);
                // write the versionstring at EOF of dng
                stream.Write(param.metaData.versionString, 0, param.metaData.versionString.Length);
            }

            updateUI(1, param);

            allFramesCount++;
            param.threadData.CDEvent.Signal();

            // clean some variables
            param.rawData = null;
            param.metaData.DNGHeader = null;
            rawDataChanged = null;
        }

        // --- GUI events ---------------------

        private void updateUI(byte kindOf, data p)
        {
            if (kindOf == 1) // update after Frame
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    //_progressOne.Value = (int)p.threadData.frame;
                    this.FramesProgressed = p.threadData.frame;
                    //_progressAll.Value = (int)allFramesCount;
                    this.TotalFramesProgressed = allFramesCount;
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
            if (settings.debugLogEnabled) debugging._saveDebug("[batchList_Click] showPicture");
            if ((sender as ListView).SelectedItems.Count == 0) return;
            object itemSender = (sender as ListView).SelectedItems[0];
            int item = (sender as ListView).Items.IndexOf(itemSender);
            // if on zero was a bad idea - the first entry is 0 ;)
            // maybe deleting the if

            // read picture and show
            WriteableBitmap im = io.showPicture(rawFiles[item], quality.high709);
            this.PreviewSource = im;
            this.PreviewLensData = String.Format(
                "{0} | {1} | ISO{2} | f/{3} | {4}°K",
                rawFiles[item].data.lensData.lens,
                rawFiles[item].data.lensData.shutter,
                rawFiles[item].data.lensData.isoValue,
                ((double)rawFiles[item].data.lensData.aperture / (double)100),
                rawFiles[item].data.metaData.whiteBalance,
                CultureInfo.InvariantCulture
                );
            if (settings.debugLogEnabled) debugging._saveDebug("[batchList_Click] read Data from " + rawFiles[item].data.fileData.fileNameOnly + " frame " + rawFiles[item].data.threadData.frame);
            if (settings.debugLogEnabled) debugging._saveDebug("[batchList_Click] * " + rawFiles[item].data.fileData.fileNameOnly);
        }

        private void _preview_MouseEnter(object sender, MouseEventArgs e)
        {
            previewTimer.Start();
            //_previewProgressBar.Stroke = green;
        }

        private void _preview_MouseLeave(object sender, MouseEventArgs e)
        {
            previewTimer.Stop();
            //_previewProgressBar.Stroke = white;
        }

        private void _takePath_Click(object sender, RoutedEventArgs e)
        {
            if (_selectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.outputPath = _selectFolder.SelectedPath;
                settings.Save();
                if (_selectFolder.SelectedPath.Length > 35)
                {
                    this.TakePath = ".." + _selectFolder.SelectedPath.Substring(_selectFolder.SelectedPath.Length - 33);
                }
                else
                {
                    this.TakePath = _selectFolder.SelectedPath;
                }
                settings.sourcePath = false;
            }
            else
            {
                this.TakePath = "select destination path";
                this.NoPathIsChecked = true;
                settings.sourcePath = true;
            }
        }

        private void _noPath_Checked(object sender, RoutedEventArgs e)
        {
            this.TakePath = "sourcepath selected";
            settings.sourcePath = true;
        }
        private void _noPath_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TakePath = settings.outputPath;
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

        //private void _banding_Checked(object sender, RoutedEventArgs e)
        //{
        //    convertData.verticalBanding = true;
        //    saveGUIsettings();
        //}
        //private void _banding_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    convertData.verticalBanding = false;
        //    saveGUIsettings();
        //}

        //private void _smoothing_Checked(object sender, RoutedEventArgs e)
        //{
        //    convertData.chromaSmoothing = true;
        //    saveGUIsettings();
        //}
        //private void _smoothing_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    convertData.chromaSmoothing = false;
        //    saveGUIsettings();
        //}

        //private void _proxy_Checked(object sender, RoutedEventArgs e)
        //{
        //    convertData.isProxy = true;
        //    //this.ProxyKindIsEnabled = true;
        //    _proxy.Content = "proxy enabled";
        //    saveGUIsettings();
        //}
        //private void _proxy_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    convertData.sProxy = false;
        //    //this.ProxyKindIsEnabled = false;
        //    _proxy.Content = "proxy disabled";
        //    saveGUIsettings();
        //}
        

        //private void _highlights_Checked(object sender, RoutedEventArgs e)
        //{
        //    convertData.pinkHighlight = true;
        //    convertData.maximize = true;
        //    saveGUIsettings();
        //}
        //private void _highlights_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    convertData.pinkHighlight = false;
        //    saveGUIsettings();
        //}

        private void _prefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            saveGUIsettings();
        }

        private void _noPath_Click(object sender, RoutedEventArgs e)
        {
            //empty
        }

        //private void _logDebug_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    settings.debugLogEnabled = false;
        //    debugging.debugLogEnabled = false;
        //    saveGUIsettings();
        //}
        //private void _logDebug_Checked(object sender, RoutedEventArgs e)
        //{
        //    settings.debugLogEnabled = true;
        //    debugging.debugLogEnabled = true;
        //    saveGUIsettings();
        //}

        // ------- helper for listview items
        //private void convert_Checked(object sender, RoutedEventArgs e)
        //{
        //    var cb = sender as CheckBox;
        //    var item = cb.DataContext;
        //    //_batchList.SelectedItem = item;
        //}
        //private void convert_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    var cb = sender as CheckBox;
        //    var item = cb.DataContext;
        //    //_batchList.SelectedItem = item;
        //}

        // ------- Helper ------------

        private void saveGUIsettings()
        {
            if (toggleSettingsSave == true)
            {
                settings.verticalBanding = convertData.VerticalBanding;
                settings.isProxy = convertData.IsProxy;
                settings.proxyKind = convertData.ProxyKind;
                settings.chromaSmooth = convertData.ChromaSmoothing;
                settings.highlightFix = convertData.PinkHighlight;
                settings.outputPath = this.TakePath.ToString();
                settings.prefix = this.Prefix;
                settings.debugLogEnabled = this.LogDebugIsChecked;
                //settings.format = settings.format;
                settings.Save();
            }
        }

        private void previewTimer_Tick(object sender, EventArgs e)
        {
            if (this.SelectedRawFile == null) return;
            raw r = this.SelectedRawFile;
            r.data.metaData.previewFrame++;
            r.data.metaData.maximize = true;
            r.data.metaData.previewFrame = r.data.metaData.previewFrame % r.data.metaData.frames;
            Task.Factory.StartNew(() => previewBackground(r));
            if (settings.debugLogEnabled) debugging._saveDebug("[previewTimer_Tick] show previewframe " + r.data.metaData.previewFrame + " from " + r.data.fileData.fileNameOnly);
        }

        private void progressedDroppedFile()
        {
            this.ProgressedDroppedDataCount++;
        }

        //private void progressDragDrop_Tick(object sender, EventArgs e)
        //{
        //    Task.Factory.StartNew(() => draggedProgress());
        //}

        //public void draggedProgress()
        //{
        //    int w = 0;
        //    this.Dispatcher.Invoke((Action)(() =>
        //    {
        //        w = (int)_dragDropProgressBar.Value;
        //        w = (w + 1) % 100;
        //        _dragDropProgressBar.Value = w;
        //        _dragDropProgressBar.InvalidateVisual();
        //    }));

        //}

        public void previewBackground(raw r)
        {
            var frame = r.data.metaData.previewFrame;
            if (frame > -1)
            {
                var maxFrames = r.data.metaData.frames;
                var progressPosX = _preview.ActualWidth * frame / maxFrames;
                // read picture and show
                r.data.threadData.frame = frame;

                //_preview.Source = im;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.PreviewSource = io.showPicture(r, quality.high709);
                    this.PreviewLensData = String.Format("{0:d5}", frame);
                    _previewProgressBar.Margin = new Thickness(progressPosX, 0, 0, 0);
                    _preview.InvalidateVisual();
                    //_lensLabel.InvalidateVisual();
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
                this.TakePathIsChecked = true;
                this.NoPathIsChecked = false;
                this.TakePath = settings.outputPath;
                settings.sourcePath = false;
            }
            else
            {
                this.TakePathIsChecked = false;
                this.NoPathIsChecked = false;
                this.TakePath = "select destination path";
                settings.sourcePath = true;
            }
            if (settings.debugLogEnabled == true)
            {
                this.LogDebugIsChecked = true;
                debugging.debugLogEnabled = true;
            }
            else
            {
                this.LogDebugIsChecked = false;
                debugging.debugLogEnabled = false;
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
            //if (settings.verticalBanding == true)
            //{
            //    this.BandingIsChecked = true;
            //    convertData.VerticalBanding = true;
            //}
            //else
            //{
            //    this.BandingIsChecked = false;
            //    convertData.VerticalBanding = false;
            //}
            convertData.VerticalBanding = settings.verticalBanding;

            //if (settings.chromaSmooth == true)
            //{
            //    this.SmoothingIsChecked = true;
            //    convertData.ChromaSmoothing = true;
            //}
            //else
            //{
            //    this.SmoothingIsChecked = false;
            //    convertData.ChromaSmoothing = false;
            //}
            convertData.ChromaSmoothing = settings.chromaSmooth;

            //if (settings.highlightFix == true)
            //{
            //    this.HighlightsIsChecked = true;
            //    convertData.PinkHighlight = true;
            //}
            //else
            //{
            //    this.HighlightsIsChecked = false;
            //    convertData.PinkHighlight = false;
            //}
            convertData.PinkHighlight = settings.highlightFix;

            //if (settings.proxyKind == null) settings.proxyKind = 0;

            if (!ffmpegExists) settings.proxyKind = 0;
            convertData.ProxyKind = settings.proxyKind;

            convertData.IsProxy = settings.isProxy;
            //if (settings.isProxy == true)
            //{
            //    this.ProxyIsChecked = true;
            //    //this.ProxyKindIsEnabled = true;
            //    //this.ProxyKind = proxyKinds[settings.proxyKind];
            //    convertData.IsProxy = true;
            //}
            //else
            //{
            //    this.ProxyIsChecked = false;
            //    //this.ProxyKindIsEnabled = false;
            //    //this.ProxyKind = proxyKinds[settings.proxyKind];
            //    convertData.IsProxy = false;
            //}

            if (settings.prefix == null) settings.prefix = "";
            if (settings.prefix != "")
            {
                this.Prefix = settings.prefix;
            }
            else
            {
                settings.prefix = "[F]";
                this.Prefix = settings.prefix;
            }
            if (settings.debugLogEnabled)
            {
                this.LogDebugIsChecked = true;
            }
            else
            {
                this.LogDebugIsChecked = false;
            }
            toggleSettingsSave = true;

        }

        // ----------- ffmpeg things ----------

        public void executeFFMPEG(raw r)
        {
            try
            {
                string inputjpgFiles = r.data.fileData.basePath + winIO.Path.DirectorySeparatorChar + r.data.fileData.destinationPath + winIO.Path.DirectorySeparatorChar + r.data.fileData.outputFilename + "%05d.jpg";
                string inputAudioFile = r.data.fileData.basePath + winIO.Path.DirectorySeparatorChar + r.data.fileData.destinationPath + winIO.Path.DirectorySeparatorChar + r.data.fileData.destinationPath + ".wav";
                string outputFile = r.data.fileData.basePath + winIO.Path.DirectorySeparatorChar + r.data.fileData.destinationPath + winIO.Path.DirectorySeparatorChar + r.data.fileData.destinationPath;
                string commandline = "";

                // ffmpegprocess - if existent
                Process ffmpegprocess = new Process();
                commandline = "-r " + r.data.metaData.fpsNom + "/" + r.data.metaData.fpsDen + " -f image2 -i " + inputjpgFiles;

                // if proxyKind==1 -> mpg2
                if (r.data.convertData.ProxyKind == 1)
                {
                    if (winIO.File.Exists(outputFile + ".mpg")) winIO.File.Delete(outputFile + ".mpg");
                    if (r.data.audioData.hasAudio)
                    {
                        commandline += " -i " + inputAudioFile + " -codec:v mpeg2video -qscale:v 2 -codec:a mp2 -b:a 192k -shortest " + outputFile + ".mpg";
                    }
                    else
                    {
                        commandline += " -codec:v mpeg2video -qscale:v 2 " + outputFile + ".mpg";
                    }
                }

                // if proxyKind==2 -> mpeg4
                if (r.data.convertData.ProxyKind == 2)
                {
                    if (winIO.File.Exists(outputFile + ".mp4")) winIO.File.Delete(outputFile + ".mp4");
                    if (r.data.audioData.hasAudio)
                    {
                        commandline += " -i " + inputAudioFile + " -c:v h264 -preset veryfast -crf 20 -codec:a mp3 -b:a 192k -shortest " + outputFile + ".mp4";
                    }
                    else
                    {
                        commandline += " -c:v h264 -preset slow -crf 20 " + outputFile + ".mp4";
                    }
                }

                if (settings.debugLogEnabled)
                {
                    commandline += " -loglevel debug";
                    debugging._saveDebug("[execute_FFMPEG] commandline-output: ffmpeg.exe " + commandline);
                }

                ProcessStartInfo info = new ProcessStartInfo("ffmpeg.exe", commandline);

                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;

                ffmpegprocess.StartInfo = info;

                ffmpegprocess.EnableRaisingEvents = true;
                ffmpegprocess.ErrorDataReceived += new DataReceivedEventHandler(ffmpeg_ErrorDataReceived);
                ffmpegprocess.OutputDataReceived += new DataReceivedEventHandler(ffmpeg_OutputDataReceived);
                //ffmpegprocess.Exited += new EventHandler(ffmpeg_Exited);
                ffmpegprocess.Start();
                ffmpegprocess.BeginOutputReadLine();
                ffmpegprocess.BeginErrorReadLine();
                ffmpegprocess.WaitForExit();
            }
            catch (Exception e)
            {
                if (settings.debugLogEnabled) debugging._saveDebug("[ffmpeg][error]: " + e.ToString());
                // if (ffmpegprocess != null) ffmpegprocess.Dispose();
            }
        }

        public void ffmpeg_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine("Input line: {0} ({1:m:s:fff})", lineCount++, DateTime.Now);
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.CurrentAction = e.Data;
            }));
            if (settings.debugLogEnabled) debugging._saveDebug("[ffmpeg]: " + e.Data);
            //Console.WriteLine(e.Data);
            //Console.WriteLine();
        }

        public void ffmpeg_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.PreviewLensData = e.Data;
            }));
            if (settings.debugLogEnabled) debugging._saveDebug("[ffmpeg]: " + e.Data);
            //Console.WriteLine(e.Data);
        }

        // ------- EOF ----------

        // ----------- INotify implementation ----------
        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanging(string propertyName)
        {
            if (this.PropertyChanging != null)
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}