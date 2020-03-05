﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using Serilog;
using WinPrint.Core.Services;

namespace WinPrint.WinForms {
    public partial class UpdateDialog : Form {
        public UpdateDialog() {
            InitializeComponent();
        }

        private void downloadButton_Click(object sender, EventArgs args) {
            string url = ServiceLocator.Current.UpdateService.DownloadUri;
            Log.Debug($"Browsing to download: {url}");
            Process proc = null;
            try {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = true;   // This is important
                psi.FileName = url;
                proc = Process.Start(psi);
            }
            catch (Exception e) {
                ServiceLocator.Current.LogService.TrackException(e, false);
                // TODO: Better error message (output of stderr?)
                Log.Error(e, "Couldn't browse to {url}.", url);
            }
            finally {
                proc?.Dispose();
            }
            //Log.Debug("Exiting app.");
            //Application.Exit();
        }
    }
}
