﻿#pragma checksum "..\..\..\..\Pages\Navbar.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2934388A7B0A9F82CD5FC4E1DCD174742F593D67"
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
    /// Navbar
    /// </summary>
    public partial class Navbar : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 19 "..\..\..\..\Pages\Navbar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button homeButton;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\..\Pages\Navbar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button backupButton;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\..\Pages\Navbar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button settingsButton;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\..\..\Pages\Navbar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button zipButton;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\..\..\Pages\Navbar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button unzipButton;
        
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
            System.Uri resourceLocater = new System.Uri("/BackItUp;V1.0.0.0;component/pages/navbar.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Pages\Navbar.xaml"
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
            this.homeButton = ((System.Windows.Controls.Button)(target));
            
            #line 20 "..\..\..\..\Pages\Navbar.xaml"
            this.homeButton.Click += new System.Windows.RoutedEventHandler(this.homeButton_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.backupButton = ((System.Windows.Controls.Button)(target));
            
            #line 41 "..\..\..\..\Pages\Navbar.xaml"
            this.backupButton.Click += new System.Windows.RoutedEventHandler(this.backupButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.settingsButton = ((System.Windows.Controls.Button)(target));
            
            #line 62 "..\..\..\..\Pages\Navbar.xaml"
            this.settingsButton.Click += new System.Windows.RoutedEventHandler(this.settingsButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.zipButton = ((System.Windows.Controls.Button)(target));
            
            #line 82 "..\..\..\..\Pages\Navbar.xaml"
            this.zipButton.Click += new System.Windows.RoutedEventHandler(this.zipButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.unzipButton = ((System.Windows.Controls.Button)(target));
            
            #line 102 "..\..\..\..\Pages\Navbar.xaml"
            this.unzipButton.Click += new System.Windows.RoutedEventHandler(this.unzipButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

