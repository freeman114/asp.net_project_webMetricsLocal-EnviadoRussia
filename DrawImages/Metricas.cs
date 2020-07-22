using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;

namespace DrawImages
{
    public partial class Metricas : UserControl
    {
        public Metricas()
        {
            InitializeComponent();
        }
        public byte[] GetPdfFile()
        {
            var document = new PdfDocument();
            var prints = this.GetPrintImage();
            foreach (var print in prints)
            {
                var page = document.AddPage();
                using (var graphics = XGraphics.FromPdfPage(page))
                {
                    graphics.SmoothingMode = XSmoothingMode.HighQuality;
                    var textFormatter = new PdfSharp.Drawing.Layout.XTextFormatter(graphics);
                    var stream = new MemoryStream();
                    print.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    graphics.DrawImage(XImage.FromStream(stream), 0, 0, page.Width, page.Height);
                }
            };
            var pdfStream = new MemoryStream();
            document.Save(pdfStream, true);
            return pdfStream.ToArray();
        }

        public List<Image> GetPrintImage()
        {
            List<Image> imgs = new List<Image>();
            //foreach (Control x in this.Controls)//.flContainer.Controls)
            {
                var area = new Rectangle(0, 0, this.Width, this.Height);
                var bm = new Bitmap(area.Width, area.Height);
                this.Invalidate(true);
                this.Refresh();
                Application.DoEvents();
                this.DrawToBitmap(bm, area);
                imgs.Add(bm);
            };
            return imgs;
        }
    }
}
