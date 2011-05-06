﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class WaveFormUnDocked : Form
    {
        Main _mainForm = null;
        PositionsAndSizes _positionsAndSizes = null;

        public Panel PanelContainer
        {
            get
            {
                return panelContainer;
            }
        }

        public WaveFormUnDocked(Main mainForm, string title, PositionsAndSizes positionsAndSizes)
        {
            InitializeComponent();
            _mainForm = mainForm;
            _positionsAndSizes = positionsAndSizes;
            Text = title;
        }

        public WaveFormUnDocked()
        {
            // TODO: Complete member initialization
        }

        private void WaveFormUnDocked_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && panelContainer.Controls.Count > 0)
            {
                var controlWaveForm = panelContainer.Controls[0];
                var controlButtons = panelContainer.Controls[1];
                var controlTrackBar = panelContainer.Controls[2];
                panelContainer.Controls.Clear();
                _mainForm.ReDockWaveForm(controlWaveForm, controlButtons, controlTrackBar);
                _mainForm.SetWaveFormToggleOff();
            }
            _positionsAndSizes.SavePositionAndSize(this);
        }

        private void WaveFormUnDocked_KeyDown(object sender, KeyEventArgs e)
        {
            _mainForm.Main_KeyDown(sender, e);
        }
    }
}
