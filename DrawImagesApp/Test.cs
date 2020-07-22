using DrawImages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawImagesApp
{
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
        }

        private void BtnPdf_Click(object sender, EventArgs e)
        {
            System.Threading.Thread gambiarra = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                
            Metricas met = new Metricas();
            var pdf = met.GetPdfFile();
            var op = new FolderBrowserDialog();
            if (op.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllBytes(op.SelectedPath + "\\pdf_" + Guid.NewGuid().ToString() + ".pdf", pdf);
            }
            }));
            gambiarra.SetApartmentState(System.Threading.ApartmentState.STA);
            gambiarra.IsBackground = false;
            gambiarra.Start();
        }
    }
}
