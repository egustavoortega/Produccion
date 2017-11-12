using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Recorte
{
    public static class Recortar
    {
        public static Image Recorta(this Image imagen, Rectangle areaParaRecortar)
        {
            try
            {
            
                using (Bitmap bmp = new Bitmap(imagen))
                {
                    Bitmap recortado = bmp.Clone(areaParaRecortar,
                    bmp.PixelFormat);
                    bmp.Dispose();
                    return (Image)(recortado);
                }
            }
            catch (Exception)
            {
                
                throw;
            }

        }

        public static void RecortaEn(this Image imagen, Rectangle areaParaRecortar, string archivo)
        {
            Recorta(imagen,areaParaRecortar).Save(archivo);
        }

        public static void RecortaEn(this Image imagen, Rectangle areaParaRecortar, string archivo, ImageFormat formato)
        {
            Recorta(imagen,areaParaRecortar).Save(archivo, formato);
        }

    }
}
