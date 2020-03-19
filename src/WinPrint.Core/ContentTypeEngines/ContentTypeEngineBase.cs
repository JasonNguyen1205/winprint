﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Helpers;
using Serilog;
using WinPrint.Core.Models;
using WinPrint.Core.Services;

namespace WinPrint.Core.ContentTypeEngines {
    /// <summary>
    /// Base class for Content/File Type Engines (CTEs)
    /// </summary>
    public abstract class ContentTypeEngineBase : ModelBase, INotifyPropertyChanged {
        public new event PropertyChangedEventHandler PropertyChanged;
        protected new void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected new bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            OnSettingsChanged(true);
            return true;
        }

        // if bool is true, reflow. Otherwise just paint
        public event EventHandler<bool> SettingsChanged;
        protected void OnSettingsChanged(bool reflow) => SettingsChanged?.Invoke(this, reflow);

        /// <summary>
        /// ContentType identifier (shorthand for class name). 
        /// </summary>
        public virtual string GetContentType() {
            return _contentType;
        }
        private static readonly string _contentType = "base";

        /// <summary>
        /// Calculated page size. Set by Sheet view model.
        /// </summary>
        public SizeF PageSize;

        /// <summary>
        /// Holds content settings for the CTE. These are used as defaults when a Sheet does not
        /// specify any.
        /// </summary>
        public ContentSettings ContentSettings { get => contentSettings; set => SetField(ref contentSettings, value); }
        private ContentSettings contentSettings;// = new ContentSettings();

        //internal int numPages = 0;
        //public int NumPages {
        //    get => numPages;
        //    set => SetField(ref numPages, value);
        //}

        internal string filePath = null;

        /// <summary>
        /// The contents of the file to be printed.
        /// </summary>
        public string Document {
            get => document; set {
                //LogService.TraceMessage($"Document is {document.Length} chars.");
                SetField(ref document, value);
            }
        }
        internal string document = null;

        internal StringFormat stringFormat = new StringFormat(StringFormat.GenericTypographic) {
            FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.LineLimit | StringFormatFlags.FitBlackBox |
                            StringFormatFlags.DisplayFormatControl | StringFormatFlags.MeasureTrailingSpaces,
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };
        internal const TextRenderingHint textRenderingHint = TextRenderingHint.ClearTypeGridFit;


        ///// <summary>
        ///// Loads the file specified into Document property.
        ///// </summary>
        ///// <param name="filePath"></param>
        ///// <returns>True if file was read. False if the file was empty or failed to read.</returns>
        //public async virtual Task<bool> LoadAsync(string filePath) {
        //    LogService.TraceMessage();
        //    this.filePath = filePath;
        //    using StreamReader streamToPrint = new StreamReader(filePath);
        //    Document = await streamToPrint.ReadToEndAsync();
        //    return !String.IsNullOrEmpty(Document);
        //}

        /// <summary>
        /// Get total count of pages. Set any local page-size related values (e.g. linesPerPage).
        /// </summary>
        /// <param name="e"></param>
        /// <returns>Number of sheets.</returns>
        public virtual async Task<int> RenderAsync(System.Drawing.Printing.PrinterResolution printerResolution, EventHandler<string> reflowProgress) {
            LogService.TraceMessage();
            if (Document == null)
                throw new ArgumentNullException("Document can't be null for Render");
            return await Task.FromResult(0);
        }

        /// <summary>
        /// Paints a single page
        /// </summary>
        /// <param name="g">Graphics with 0,0 being the origin of the Page</param>
        /// <param name="pageNum">Page number to print</param>
        public abstract void PaintPage(Graphics g, int pageNum);

        /// <summary>
        /// Creates the appropriate Content Type Engine instance given a content type string.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static async Task<ContentTypeEngineBase> CreateContentTypeEngine(string contentType) {
            ContentTypeEngineBase cte = null;

            Debug.Assert(!string.IsNullOrEmpty(contentType));

            switch (contentType) {
                case "text/html":
                    cte = HtmlCte.Create();
                    break;

                case "text/plain":
                    cte = TextCte.Create();
                    break;

                // TODO: Figure out if we really want to use the sourcecode CTE.
                //case "text/sourcecode":
                //    cte = CodeCte.Create();
                //    ((CodeCte)cte).Language = contentType;
                //    break;

                default:
                    // It must be a language. Verify node.js and Prism are installed
                    if (await ServiceLocator.Current.NodeService.IsInstalled()) {
                        // contentType == Language
                        cte = PrismCte.Create();
                        ((PrismCte)cte).Language = contentType;
                    }
                    else {
                        Log.Information("Node.js must be installed for Prism-based ({lang}) syntax highlighting. Using {def} instead.", contentType, "text/plain");
                        cte = TextCte.Create();
                    }
                    break;
            }

            Debug.Assert(cte != null);
            return cte;
        }

        /// <summary>
        /// Returns the content type name and language name given a file path. If the content type
        /// cannot be determiend from FilesAssocaitons the default of "text/plain" is returned.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>The content type</returns>
        public static string GetContentType(string filePath) {
            string contentType = "text/plain";

            // Expand path
            filePath = Path.GetFullPath(filePath);

            // If there's a file extension get the content type from the file type association mapper
            var ext = Path.GetExtension(filePath).ToLower();
            if (ext != string.Empty) {
                if (ModelLocator.Current.Associations.FilesAssociations.TryGetValue("*" + ext, out string ct)) {
                    contentType = ct;
                }
            }
            else {
                // Empty means no extension (e.g. .\.ssh\config) - use filename
                if (ModelLocator.Current.Associations.FilesAssociations.TryGetValue("*" + Path.GetFileName(filePath), out string ct)) {
                    contentType = ct;
                }
            }

            // If not text or html, is it a language?
            //if (!contentType.Equals("text/plain") && !contentType.Equals("text/html")) {
            //    // Technically, because we got the assocation from FilesAssocation, this should always work 
            //    if (!((List<Langauge>)ModelLocator.Current.Associations.Languages).Exists(lang => lang.Id == contentType))
            //        contentType = "text/plain";
            //}
            return contentType;
        }

        public abstract Task<bool> SetDocumentAsync(string document);
    }
}
