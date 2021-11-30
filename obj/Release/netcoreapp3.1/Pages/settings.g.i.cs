﻿#pragma checksum "..\..\..\..\Pages\settings.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "F1DBB72EE3561B08A737D97D124CE12E3D7B5315"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using BackItUp.Pages;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using MaterialDesignThemes.Wpf.Transitions;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.DesignTime;
using ModernWpf.Markup;
using ModernWpf.Media.Animation;
using SourceChord.FluentWPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace BackItUp.Pages {
    
    
    /// <summary>
    /// settings
    /// </summary>
    public partial class settings : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 49 "..\..\..\..\Pages\settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ModernWpf.Controls.RadioButtons automaticBackupRB;
        
        #line default
        #line hidden
        
        
        #line 68 "..\..\..\..\Pages\settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox savePathTextBox;
        
        #line default
        #line hidden
        
        
        #line 80 "..\..\..\..\Pages\settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button openBackupFolderButton;
        
        #line default
        #line hidden
        
        
        #line 120 "..\..\..\..\Pages\settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox saveAsTextBox;
        
        #line default
        #line hidden
        
        
        #line 131 "..\..\..\..\Pages\settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button openBackupButton;
        
        #line default
        #line hidden
        
        
        #line 173 "..\..\..\..\Pages\settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MaterialDesignThemes.Wpf.TimePicker backupTimePicker;
        
        #line default
        #line hidden
        
        
        #line 189 "..\..\..\..\Pages\settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ignoreTextBox;
        
        #line default
        #line hidden
        
        
        #line 224 "..\..\..\..\Pages\settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CompressionLevelSelector;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.6.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/BackItUp;V1.0.0.0;component/pages/settings.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Pages\settings.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.6.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.automaticBackupRB = ((ModernWpf.Controls.RadioButtons)(target));
            return;
            case 2:
            this.savePathTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.openBackupFolderButton = ((System.Windows.Controls.Button)(target));
            
            #line 82 "..\..\..\..\Pages\settings.xaml"
            this.openBackupFolderButton.Click += new System.Windows.RoutedEventHandler(this.openBackupFolder);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 98 "..\..\..\..\Pages\settings.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.saveLocationSelector);
            
            #line default
            #line hidden
            return;
            case 5:
            this.saveAsTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.openBackupButton = ((System.Windows.Controls.Button)(target));
            
            #line 133 "..\..\..\..\Pages\settings.xaml"
            this.openBackupButton.Click += new System.Windows.RoutedEventHandler(this.openBackup);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 150 "..\..\..\..\Pages\settings.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.resetSaveAsName);
            
            #line default
            #line hidden
            return;
            case 8:
            this.backupTimePicker = ((MaterialDesignThemes.Wpf.TimePicker)(target));
            
            #line 174 "..\..\..\..\Pages\settings.xaml"
            this.backupTimePicker.SelectedTimeChanged += new System.Windows.RoutedPropertyChangedEventHandler<System.Nullable<System.DateTime>>(this.TimePicker_SelectedTimeChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.ignoreTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            
            #line 201 "..\..\..\..\Pages\settings.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.resetIgnore);
            
            #line default
            #line hidden
            return;
            case 11:
            this.CompressionLevelSelector = ((System.Windows.Controls.ComboBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

