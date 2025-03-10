using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Text;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Duo
{
    /// <summary>
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _previewVisible = false;
        public bool PreviewVisible
        {
            get => _previewVisible;
            set
            {
                if (_previewVisible != value)
                {
                    _previewVisible = value;
                    OnPropertyChanged(nameof(PreviewVisible));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await MarkdownPreview.EnsureCoreWebView2Async();
        }

        private void TogglePreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewVisible)
            {
                PreviewVisible = false;
                TogglePreviewButton.Content = "Preview Markdown";
            }
            else
            {
                PreviewVisible = true;
                TogglePreviewButton.Content = "Hide Preview";
                RenderMarkdown();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MarkdownInput.Text = string.Empty;
            if (PreviewVisible)
            {
                RenderMarkdown();
            }
        }

        private void RenderMarkdown()
        {
            string markdownText = MarkdownInput.Text ?? string.Empty;
            
            string html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <title>Markdown Preview</title>
                    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/github-markdown-css/github-markdown.min.css'>
                    <script src='https://cdn.jsdelivr.net/npm/marked/marked.min.js'></script>
                    <style>
                        .markdown-body {{
                            box-sizing: border-box;
                            min-width: 200px;
                            max-width: 980px;
                            margin: 0 auto;
                            padding: 15px;
                        }}
                    </style>
                </head>
                <body>
                    <div id='content' class='markdown-body'></div>
                    <script>
                        document.getElementById('content').innerHTML = marked.parse(`{EscapeJavaScriptString(markdownText)}`);
                    </script>
                </body>
                </html>";

            MarkdownPreview.NavigateToString(html);
        }

        private string EscapeJavaScriptString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace("\\", "\\\\")
                .Replace("`", "\\`")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }
    }
}
