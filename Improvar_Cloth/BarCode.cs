using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using Bytescout.BarCode;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;

namespace Improvar
{
    public class BarCode
    {
        public string BarcodeGeneratre(string Value)
        {
            Barcode barcode = new Barcode();
            barcode.RegistrationKey = "Improvar";
            barcode.RegistrationName = "Improvar";
            barcode.Symbology = SymbologyType.Code128;
            barcode.DrawCaption = true;
            barcode.Caption = Value;
            barcode.CaptionPosition = CaptionPosition.Below;
            barcode.CaptionFont = new Font("Arial", 12f, FontStyle.Regular);
            barcode.SmoothingMode = SmoothingMode.Default;
            barcode.BackColor = Color.White;
            barcode.ForeColor = Color.Black;
            barcode.Angle = RotationAngle.Degrees0;
            barcode.Margins = new Margins(5, 5, 5, 5);
            barcode.BarHeight = 50;
            barcode.NarrowBarWidth = 3;
            barcode.WideToNarrowRatio = 3;
            //float w = 3;
            //float h = 1;

            //barcode.FitInto(h, w, UnitOfMeasure.Inch ) ;
            barcode.AddChecksum = false;
            barcode.AddChecksumToCaption = false;
            barcode.RenderingHint = TextRenderingHint.SystemDefault;
            barcode.DrawQuietZones = true;
            barcode.Options.Code128Alphabet = Code128Alphabet.Auto;
            var ae = barcode.Version;
            // barcode.AdditionalCaption = "ll";
            barcode.Value = Value;

            barcode.SaveImage("C:\\ipsmart\\" + Value + ".jpg");

            Image barcodeImage = barcode.GetImage();
            var a = barcode.GetImageBytesJPG();
            string s = "data:image/png;base64," + Convert.ToBase64String(a);
            return s;
        }
        public string genBarCode(string barcode, bool genjpg=false)
        {
            using (System.IO.MemoryStream ms = new MemoryStream())
            {
                //The Image is drawn based on length of Barcode text.
                using (Bitmap bitMap = new Bitmap(barcode.Length * 40, 80))
                {
                    //The Graphics library object is generated for the Image.
                    using (Graphics graphics = Graphics.FromImage(bitMap))
                    {
                        //The installed Barcode font.
                        Font oFont = new Font("IDAutomationHC39M", 16);
                        PointF point = new PointF(2f, 2f);

                        //White Brush is used to fill the Image with white color.
                        SolidBrush whiteBrush = new SolidBrush(Color.White);
                        graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);

                        //Black Brush is used to draw the Barcode over the Image.
                        SolidBrush blackBrush = new SolidBrush(Color.Black);
                        graphics.DrawString("*" + barcode + "*", oFont, blackBrush, point);
                    }

                    //The Bitmap is saved to Memory Stream.
                    if (genjpg == true)
                    {
                        bitMap.Save(ms, ImageFormat.Png);
                        byte[] image = ms.ToArray();
                        Image image1 = Image.FromStream(new MemoryStream(image));
                        image1.Save("C:\\improvar\\" + barcode + ".jpg", ImageFormat.Jpeg);
                    }
                    return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}