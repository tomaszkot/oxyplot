﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellViewModel.cs" company="OxyPlot">
//   The MIT License (MIT)
//
//   Copyright (c) 2012 Oystein Bjorke
//
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Pdf;
using OxyPlot.Reporting;
using OxyPlot.Wpf;
using PropertyTools.Wpf;
using Plot = OxyPlot.Wpf.Plot;

namespace ExportDemo
{
    using OxyPlot.OpenXml;
    using OxyPlot.Xps;

    using DataPointSeries = OxyPlot.DataPointSeries;

    [Export(typeof(IShell))]
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        private PlotModel model;

        public Plot Plot { get; private set; }
        public Window Owner { get; private set; }

        public void Attach(Window owner, Plot plot)
        {
            Owner = owner;
            Plot = plot;
        }

        private ModelType currentModel;

        public ModelType CurrentModel
        {
            get { return currentModel; }
            set
            {
                currentModel = value;
                Model = PlotModelFactory.Create(currentModel);
            }
        }

        public ShellViewModel()
        {
            CurrentModel = ModelType.SineWave;
        }

        public PlotModel Model
        {
            get { return model; }
            set
            {
                if (model != value)
                {
                    model = value;
                    NotifyOfPropertyChange(() => Model);
                    NotifyOfPropertyChange(() => TotalNumberOfPoints);
                }
            }
        }

        public int TotalNumberOfPoints
        {
            get
            {
                if (Model == null) return 0;
                return Model.Series.Sum(ls => ((DataPointSeries)ls).Points.Count);
            }
        }

        public void SaveReport(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (ext == null)
            {
                return;
            }

            ext = ext.ToLower();

            var r = this.CreateReport(fileName);
            var reportStyle = new ReportStyle();

            if (ext == ".txt")
            {
                using (var w = new TextReportWriter(fileName))
                {
                    r.Write(w);
                }
            }

            if (ext == ".html")
            {
                using (var w = new HtmlReportWriter(fileName))
                {
                    w.WriteReport(r, reportStyle);
                }
            }

            if (ext == ".pdf")
            {
                using (var w = new PdfReportWriter(fileName))
                {
                    w.WriteReport(r, reportStyle);
                }
            }

            if (ext == ".rtf")
            {
                using (var w = new RtfReportWriter(fileName))
                {
                    w.WriteReport(r, reportStyle);
                }
            }

            if (ext == ".tex")
            {
                using (var w = new LatexReportWriter(fileName, "Example report", "oxyplot"))
                {
                    w.WriteReport(r, reportStyle);
                }
            }

            if (ext == ".xps")
            {
                using (var w = new FlowDocumentReportWriter())
                {
                    w.WriteReport(r, reportStyle);
                    w.Save(fileName);
                }
            }
            if (ext == ".docx")
            {
                using (var w = new WordDocumentReportWriter(fileName))
                {
                    w.WriteReport(r, reportStyle);
                    w.Save();
                }
            }
        }

        private Report CreateReport(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            ext = ext.ToLower();

            var r = new Report();
            r.Title = "Oxyplot example report";

            var main = new ReportSection();

            r.AddHeader(1, "Example report from OxyPlot");
            //r.AddHeader(2, "Content");
            //r.AddTableOfContents(main);
            r.Add(main);

            main.AddHeader(2, "Introduction");
            main.AddParagraph("The content in this file was generated by OxyPlot.");
            main.AddParagraph("See http://oxyplot.codeplex.com for more information.");

            var dir = Path.GetDirectoryName(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);

            string fileNameWithoutExtension = Path.Combine(dir, name);

            main.AddHeader(2, "Plot");
            main.AddParagraph("This plot was rendered to a file and included in the report as a plot.");
            main.AddPlot(Model, "Plot", 800, 500);

            main.AddHeader(2, "Drawing");
            main.AddParagraph("Not yet implemented.");

            /*            switch (ext)
                        {
                            case ".html":
                                {
                                    main.AddHeader(2, "Plot (svg)");
                                    main.AddParagraph("This plot was rendered to a .svg file and included in the report.");
                                    main.AddPlot(Model, "SVG plot", 800, 500);

                                    //main.AddHeader(2, "Drawing (vector)");
                                    //main.AddParagraph("This plot was rendered to SVG and included in the report as a drawing.");
                                    //var svg = Model.ToSvg(800, 500);
                                    //main.AddDrawing(svg, "SVG plot as a drawing.");
                                    break;
                                }
                            case ".pdf":
                            case ".tex":
                                {
                                    main.AddHeader(2, "Plot (vector)");
                                    main.AddParagraph("This plot was rendered to a .pdf file and included in the report.");
                                    main.AddPlot(Model, "PDF plot", 800, 500);
                                    //var pdfPlotFileName = fileNameWithoutExtension + "_plot.pdf";
                                    //PdfExporter.Export(Model, pdfPlotFileName, 800, 500);
                                    //main.AddParagraph("This plot was rendered to PDF and embedded in the report.");
                                    //main.AddImage(pdfPlotFileName, "PDF plot");
                                    break;
                                }
                            case ".docx":
                                {
                                    main.AddHeader(2, "Plot");
                                    main.AddParagraph("This plot was rendered to a .png file and included in the report.");
                                    main.AddPlot(Model, "Plot", 800, 500);
                                }
                                break;
                        }*/

            main.AddHeader(2, "Image");
            main.AddParagraph("The plot is rendered to a .png file and included in the report as an image.");

            string pngPlotFileName = fileNameWithoutExtension + "_Plot2.png";
            PngExporter.Export(this.Model, pngPlotFileName, 800, 500);
            main.AddImage(pngPlotFileName, "Plot as image");

            main.AddHeader(2, "Data");
            int i = 1;
            foreach (DataPointSeries s in Model.Series)
            {
                main.AddHeader(3, "Data series " + (i++));
                var pt = main.AddPropertyTable("Properties of the " + s.GetType().Name, new[] { s });
                pt.Fields[0].Width = 50;
                pt.Fields[1].Width = 100;

                var fields = new List<ItemsTableField>
                                 {
                                     new ItemsTableField("X", "X") { Width=60, StringFormat="0.00"},
                                     new ItemsTableField("Y", "Y") { Width=60, StringFormat="0.00"}
                                 };
                main.Add(new ItemsTable { Caption = "Data", Fields = fields, Items = s.Points });
            }

            //main.AddHeader(3, "Equations");
            //main.AddEquation(@"E = m \cdot c^2");
            //main.AddEquation(@"\oint \vec{B} \cdot d\vec{S} = 0");
            return r;
        }

        public void SaveSvg()
        {
            var path = GetFilename(".svg files|*.svg", ".svg");
            if (path != null)
            {
                Model.SaveSvg(path, Plot.ActualWidth, Plot.ActualHeight);
                OpenContainingFolder(path);
            }
        }

        public void SavePng()
        {
            var path = GetFilename(".png files|*.png", ".png");
            if (path != null)
            {
                Plot.SaveBitmap(path);
                OpenContainingFolder(path);
            }
        }

        private static void OpenContainingFolder(string fileName)
        {
            // var folder = Path.GetDirectoryName(fileName);
            var psi = new ProcessStartInfo("Explorer.exe", "/select," + fileName);
            Process.Start(psi);
        }

        public void SavePdf()
        {
            var path = GetFilename(".pdf files|*.pdf", ".pdf");
            if (path != null)
            {
                PdfExporter.Export(Model, path, Plot.ActualWidth, Plot.ActualHeight);
                OpenContainingFolder(path);
            }
        }

        public void SaveRtfReport()
        {
            var path = GetFilename(".rtf files|*.rtf", ".rtf");
            if (path != null)
            {
                SaveReport(path);
                OpenContainingFolder(path);
            }
        }

        public void SaveTextReport()
        {
            var path = GetFilename("Text files (*.txt)|*.txt", ".txt");
            if (path != null)
            {
                SaveReport(path);
                OpenContainingFolder(path);
            }
        }

        public void SaveHtmlReport()
        {
            var path = GetFilename(".html files|*.html", ".html");
            if (path != null)
            {
                SaveReport(path);
                OpenContainingFolder(path);
            }
        }

        public void SaveLatexReport()
        {
            var path = GetFilename(".tex files|*.tex", ".tex");
            if (path != null)
            {
                SaveReport(path);
                OpenContainingFolder(path);
            }
        }

        public void SaveXaml()
        {
            var path = GetFilename(".xaml files|*.xaml", ".xaml");
            if (path != null)
            {
                Plot.SaveXaml(path);
                OpenContainingFolder(path);
            }
        }

        public void SaveXps()
        {
            var path = GetFilename(".xps files|*.xps", ".xps");
            if (path != null)
            {
                XpsExporter.Export(Plot.Model, path, Plot.ActualWidth, Plot.ActualHeight);
                OpenContainingFolder(path);
            }
        }

        public void Print()
        {
            XpsExporter.Print(model, Plot.ActualWidth, Plot.ActualHeight);
        }

        public void CopySvg()
        {
            Clipboard.SetText(Model.ToSvg(Plot.ActualWidth, Plot.ActualHeight, true));
        }

        public void CopyBitmap()
        {
            Clipboard.SetImage(Plot.ToBitmap());
        }

        public void CopyXaml()
        {
            Clipboard.SetText(Plot.ToXaml());
        }

        public void SavePdfReport()
        {
            var path = GetFilename(".pdf files|*.pdf", ".pdf");
            if (path != null)
            {
                SaveReport(path);
                OpenContainingFolder(path);
            }
        }

        public void SaveXpsReport()
        {
            var path = GetFilename(".xps files|*.xps", ".xps");
            if (path != null)
            {
                SaveReport(path);
                OpenContainingFolder(path);
            }
        }

        public void SaveDocxReport()
        {
            var path = GetFilename("Word document (.docx)|*.docx", ".docx");
            if (path != null)
            {
                SaveReport(path);
                OpenContainingFolder(path);
            }
        }

        private string GetFilename(string filter, string defaultExt)
        {
            // todo: this should probably move out of the viewmodel
            var dlg = new SaveFileDialog { Filter = filter, DefaultExt = defaultExt };
            return dlg.ShowDialog(Owner).Value ? dlg.FileName : null;
        }

        public void Exit()
        {
            Owner.Close();
        }

        public void HelpHome()
        {
            Process.Start("http://oxyplot.codeplex.com");
        }

        public void HelpDocumentation()
        {
            Process.Start("http://oxyplot.codeplex.com/documentation");
        }
        public void HelpAbout()
        {
            var dlg = new AboutDialog(Owner);
            dlg.Title = "About OxyPlot ExportDemo";
            dlg.Image = new BitmapImage(new Uri(@"pack://application:,,,/ExportDemo;component/Images/oxyplot.png"));
            dlg.Show();
        }
    }
}