
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
namespace printer
{
    class printer_class
    {
        private Font printFont;
        private Font titleFont;
        private StringReader streamToPrint;
        private int leftMargin = 0;
        /// <summary>
        /// 设置PrintDocument 的相关属性
        /// </summary>
        /// <param name="str">要打印的文字</param>

        public void print(string str)
        {
            try
            {
                streamToPrint = new StringReader(str);
                printFont = new Font("宋体", 10);
                titleFont = new Font("宋体", 15);
                System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                pd.PrinterSettings.PrinterName = "Gprinter GP-9025T";
                pd.DocumentName = pd.PrinterSettings.MaximumCopies.ToString();
                pd.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.pd_PrintPage);

                pd.PrintController = new System.Drawing.Printing.StandardPrintController();
                pd.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = this.leftMargin;
            float topMargin = 0;
            String line = null;
            linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);
            while (count < linesPerPage &&
            ((line = streamToPrint.ReadLine()) != null))
            {
                if (count == 0)
                {
                    yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                    ev.Graphics.DrawString(line, titleFont, Brushes.Black, leftMargin + 10, yPos, new StringFormat());
                }
                else
                {
                    yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                    ev.Graphics.DrawString(line, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                }
                count++;
            }
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }

    }
}
