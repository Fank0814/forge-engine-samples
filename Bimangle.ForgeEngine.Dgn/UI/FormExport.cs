﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.DgnPlatformNET;
using Bimangle.ForgeEngine.Dgn.Config;
using Bimangle.ForgeEngine.Dgn.Core;
using Bimangle.ForgeEngine.Dgn.UI.Controls;
using Newtonsoft.Json.Linq;
using Control = System.Windows.Forms.Control;
using Form = System.Windows.Forms.Form;

namespace Bimangle.ForgeEngine.Dgn.UI
{
    partial class FormExport : Form
    {
        private IExportControl _Exporter;
        private Viewport _View;

        public FormExport()
        {
            InitializeComponent();
        }

        public FormExport(Viewport view, AppConfig config, bool hasSelectElements, string target)
            : this()
        {
            _View = view;

            tabList.TabPages.Clear();

            var exporters = new List<IExportControl>();

            void AddPage(Control control, IExportControl exporter)
            {
                var tab = new TabPage();
                tabList.Controls.Add(tab);
                tab.Text = exporter.Title;
                tab.Name = $@"tabPage{exporter.Title}";
                tab.UseVisualStyleBackColor = true;
                tab.ImageKey = exporter.Icon;
                tab.Controls.Add(control);
                tab.Tag = exporter;
                control.Dock = DockStyle.Fill;

                if (exporter.Title == target)
                {
                    _Exporter = exporter;
                }

                exporters.Add(exporter);
            }

#if !EXPRESS
            #region 增加 SvfZip 导出
            {
                var control = new ExportSvfzip();
                var exporter = (IExportControl)control;
                AddPage(control, exporter);
            }
            #endregion
#endif

            #region 增加 glTF/glb 导出
            {
                var control = new ExportGltf();
                var exporter = (IExportControl)control;
                AddPage(control, exporter);
            }
#endregion

#region 增加 3D Tiles 导出
            {
                var control = new ExportCesium3DTiles();
                var exporter = (IExportControl)control;
                AddPage(control, exporter);
            }
#endregion

            if (_Exporter == null) _Exporter = exporters.First();

            foreach (var exporter in exporters)
            {
                exporter.Init(view, config, hasSelectElements);
            }

            foreach (TabPage tab in tabList.TabPages)
            {
                if (tab.Tag == _Exporter)
                {
                    tabList.SelectTab(tab);
                    break;
                }
            }
        }

        private void FormExport_Load(object sender, EventArgs e)
        {
            Icon =Icon.FromHandle(Properties.Resources.Converter_32px_1061192.GetHicon());
            Text = $@"{PackageInfo.ProductName} - Samples v{PackageInfo.ProductVersion}";
        }

        private void FormExportSvfzip_Shown(object sender, EventArgs e)
        {
        }

        private void FormExportSvfzip_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void btnResetOptions_Click(object sender, EventArgs e)
        {
            _Exporter.Reset();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                Enabled = false;

                if (_Exporter.Run() == false)
                {
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnLicense_Click(object sender, EventArgs e)
        {
            LicenseConfig.ShowDialog(this);
        }

        private void tabList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabList.SelectedTab.Tag is IExportControl exporter)
            {
                _Exporter = exporter;
            }
        }
    }
}
