﻿using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

namespace OSIRT.UI
{
    [DesignerSerializer("System.Windows.Forms.Design.ControlCodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class SpellBox : ElementHost
    {
        public SpellBox()
        {
            box = new TextBox();
            base.Child = box;
            box.TextChanged += (s, e) => OnTextChanged(EventArgs.Empty);
            //box.AcceptsReturn = UserSettings.Load().EnterInCaseNotesNewLine;

            box.SpellCheck.IsEnabled = true;
            box.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.Size = new System.Drawing.Size(100, 20);
           
        }

        public override string Text
        {
            get { return box.Text; }
            set { box.Text = value; }
        }
        [DefaultValue(false)]
        public bool Multiline
        {
            get { return box.AcceptsReturn; }
            set { box.AcceptsReturn = value; }
        }
        [DefaultValue(false)]
        public bool WordWrap
        {
            get { return box.TextWrapping != TextWrapping.NoWrap; }
            set { box.TextWrapping = value ? TextWrapping.Wrap : TextWrapping.NoWrap; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new System.Windows.UIElement Child
        {
            get { return base.Child; }
            set { /* Do nothing to solve a problem with the serializer !! */ }
        }

        public new event System.Windows.Input.KeyEventHandler KeyDown
        {

            add { box.KeyDown += value; }
            remove { box.KeyDown -= value; }
        }

        public new event System.Windows.Input.KeyEventHandler KeyUp
        {
           
            add { box.KeyUp += value; }
            remove { box.KeyUp -= value; }
        }

        private TextBox box;



    }
}
